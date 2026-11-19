using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum CallType
{
    Performance = 0,
    Normal = 1
}

public enum TurretType
{
    LeftTurret = 0,
    RightTurret = 1
}

public class TurretController : MonoBehaviour, IDamageable
{
    // NOTE:タレットのパラメーターはボスのステータスを参照する
    //=================================== ステータス (Max値) =====================================
    private int _maxHp;
    private int _maxAtk;
    private int _maxDef;

    private float _maxPhysResist;  // 物理耐性
    private float _maxEnergyResist;// エネルギー耐性

    private float _maxCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _maxCritDamage;// 0.5 = 50%

    // NOTE:あらかじめ格納しておく
    private int _maxEnergy = 100;
    private float _maxFireCooldown = 5f;

    //=================================== ステータス (現在値) =====================================
    private int _currentHp;
    private int _currentAtk;
    private int _currentDef;

    private float _currentPhysResist;  // 物理耐性
    private float _currentEnergyResist;// エネルギー耐性

    private float _currentCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _currentCritDamage;// 0.5 = 50%

    private int _currentEnergy;
    private float _currentFireCooldown;

    //=================================== フィールド =====================================
    [SerializeField] private TurretType _turretType;
    [SerializeField] private SliderType _hpSliderType;
    [SerializeField] private SliderType _energySliderType;

    [SerializeField] private GameController _gameController;
    [SerializeField] private BossController _bossController;
    [SerializeField] private BossDataSO _bossData;
    [SerializeField] private GameObject _turretStand;// タレットの土台
    [SerializeField] private GameObject _turretHead; // タレットの頭部
    [SerializeField] private GameObject _turretMount;// 頭部の取付部位
    [SerializeField] private GameObject[] _muzzles = new GameObject[4];
    [SerializeField] private Image _turretSlider;
    [SerializeField] private Collider _turretCollider;

    [SerializeField] private LineRenderer[] _laserGlowLines = new LineRenderer[4];
    [SerializeField] private LineRenderer[] _laserCoreLines = new LineRenderer[4];

    // SliderTypの容量に合わせる
    private Coroutine[] _coroutines = new Coroutine[System.Enum.GetValues(typeof(SliderType)).Length];
    private Coroutine _reGenerateEnergy, _generateTurretAsync;
    private Camera _mainCamera;

    [SerializeField] private float _upperOffset, _rightOffset;
    private float _rotationSpeed  = 10;    // 回転速度
    private float _firstShotMultiplier = 1;// ダメージ倍率
    private float _laserSpeed = 17f;       // レーザーの速度

    private float[] _traveledDistances = new float[4];
    private float _gunfireRange = 10f;
    private float _energyRegenerationDelay = 5f;
    private float _turretReGenerationDelay = 10f;

    private int _stageLevel;
    private int _shotCost = 20;
    private int _energyReGenerateAmount = 5;

    private bool _readyToFire = false;
    private bool _startCooldown = true;
    bool[] isActive = new bool[4];

    private Transform _playerTransform;
    
    private Vector3[] _shootingPoints = new Vector3[4];  // レイの初期位置
    private Vector3[] _directions = new Vector3[4];      // レイの方角
    private Vector3[] _currentPositions = new Vector3[4];// レイの次の開始地点
    private Vector3[] muzzleDirection = new Vector3[4];

    private void Awake()
    {
        _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);

        // ボスのステータスを適用
        SetStatus();
    }

    private void Start()
    {
        _mainCamera = Camera.main;

        // ステージの状況を選択した項目に合わせる
        SetBattleScene();
    }

    void Update()
    {
        // プレイヤーを追従させる
        RotateTurret();
    }

    private void LateUpdate()
    {
        // スライダーの追従処理
        UpdateSliderPosition();
    }

    //=================================== 初期処理 =====================================
    // ボスのステータスを適用
    private void SetStatus()
    {
        int _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
        float _stageLevelMultiplier = PlayerPrefs.GetFloat("StageLevelMultiplier", 0.2f);

        int _baseHp = _bossData.BaseHp / 7;
        int _baseAtk = _bossData.BaseAtk / 10;
        int _baseDef = _bossData.BaseDef / 4;

        float _basePhysResist = _bossData.BasePhysResist / 2;    // 物理耐性
        float _baseEnergyResist = _bossData.BaseEnergyResist / 2;// エネルギー耐性
        float _baseCritRate = 0f;
        float _baseCritDamage = 0f;

        // 累乗倍率
        float _bossStatusExponent = _bossData.StatusExponent; // ステータスを累乗する値

        // Max値： 基礎ステータス = Bossステータス / 2 x (ステージレベル x 0.2) 0.6乗
        _maxHp = Mathf.RoundToInt(_baseHp * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent));
        _maxAtk = Mathf.RoundToInt(_baseAtk * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent));
        _maxDef = Mathf.RoundToInt(_baseDef * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent));
        _maxPhysResist = _basePhysResist;
        _maxEnergyResist = _baseEnergyResist;
        _maxCritRate = _baseCritRate;
        // 会心ダメージ = 基礎倍率 + (ステージレベル x 0.2) 0.6乗
        _maxCritDamage = _baseCritDamage * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent);

        // 現在値
        _currentHp = _maxHp;
        _currentAtk = _maxAtk;
        _currentDef = _maxDef;
        _currentPhysResist = _maxPhysResist;
        _currentEnergyResist = _maxEnergyResist;
        _currentCritRate = _maxCritRate;
        _currentCritDamage = _maxCritDamage;
    }

    // ステージの状況を選択した項目に合わせる
    private void SetBattleScene()
    {
        Material newMaterial = null;

        switch (_stageLevel)
        {
            case 10:
                Debug.Log($"ステージLv：{_stageLevel}");
                newMaterial = Resources.Load<Material>("Materials/Metal");
                _turretSlider.gameObject.SetActive(false);
                ; break;

            case 50:
                Debug.Log($"ステージLv：{_stageLevel}");
                newMaterial = Resources.Load<Material>("Materials/Unit_Selection_v1");
                ; break;

            case 100:
                Debug.Log($"ステージLv：{_stageLevel}");
                ; break;

            default:
                Debug.LogError("選択した難易度が適用しません");
                break;
        }

        // マテリアルをリソースフォルダから適用
        if (_turretStand.TryGetComponent<Renderer>(out var renderer)) renderer.material = newMaterial;

    }

    //=================================== 動作制御 =====================================
    // プレイヤーを追従させる
    private void RotateTurret()
    {
        if (!_readyToFire) return;

        // プレイヤー座標を取得
        _playerTransform = _bossController.PlayerTransform;

        if (_playerTransform != null)
        {
            Vector3 upperDirection = _playerTransform.position - _turretHead.transform.position;
            Vector3 lowerDirection = _playerTransform.position - _turretMount.transform.position;

            for (int i = 0; i < _muzzles.Length; i++)
            {
                muzzleDirection[i] = _playerTransform.position - _muzzles[i].transform.position;
            }

            // 上下の回転処理
            if (upperDirection != Vector3.zero)
            {
                // ローカル座標に変換
                Vector3 localDirection = _turretMount.transform.InverseTransformDirection(upperDirection);
                // 水平方向の距離（平方根）をラジアン型で求めた後、度数法に戻し、マイナスを掛けて格納
                float targetAngleX = -Mathf.Atan2(localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) * Mathf.Rad2Deg;
                // 計算した上下の回転を格納
                Quaternion targetLocalRot = Quaternion.Euler(targetAngleX, 0, 0);
                // Slerpを用いて滑らかに回転させる
                _turretHead.transform.localRotation = Quaternion.Slerp(_turretHead.transform.localRotation, targetLocalRot, _rotationSpeed * Time.deltaTime);
            }

            // 左右の回転処理
            lowerDirection.y = 0;

            if (lowerDirection != Vector3.zero)
            {
                Quaternion lowerTargetRot = Quaternion.LookRotation(lowerDirection);
                _turretMount.transform.rotation = Quaternion.Slerp(_turretMount.transform.rotation, lowerTargetRot, _rotationSpeed * Time.deltaTime);
            }

            // タレットの銃口を動かす
            if (muzzleDirection.Any(x => x != Vector3.zero))
            {
                for(int i = 0; i < _muzzles.Length; i++)
                {
                    Quaternion targetMuzzlesRot = Quaternion.LookRotation(muzzleDirection[i]);
                    _muzzles[i].transform.rotation = Quaternion.Slerp(_muzzles[i].transform.rotation, targetMuzzlesRot, _rotationSpeed * Time.deltaTime);
                }
            }
        }

        if (!_startCooldown) return;

        _currentFireCooldown -= Time.deltaTime;

        if (_currentFireCooldown <= 0)
        {
            _startCooldown = false;
            _currentFireCooldown = _maxFireCooldown;
            Attack();
        }
    }

    // 攻撃
    private void Attack()
    {
        // 外してもエネルギーは消費する
        int beforeValue = _currentEnergy;
        _currentEnergy -= _shotCost;

        // エネルギー不足は気合で乗り切る！
        if (_currentEnergy < 0) _currentEnergy = 0;

        // UIに反映
        if (_coroutines[(int)_energySliderType] != null) StopCoroutine(_coroutines[(int)_energySliderType]);
        _coroutines[(int)_energySliderType] = StartCoroutine(_gameController.UpdateSlider(_energySliderType, _currentEnergy, beforeValue, _maxEnergy));

        // レーザー攻撃
        StartCoroutine(AttackRayAsync());

        // エネルギー自動回復開始
        if (_currentEnergy <= 0)
        {
            _readyToFire = false;

            if (_reGenerateEnergy != null) StopCoroutine(_reGenerateEnergy);
            _reGenerateEnergy = StartCoroutine(ReGenerateEnergy(CallType.Normal));
        }
    }

    /// <summary>
    /// レーザーを発射する
    /// </summary>
    private IEnumerator AttackRayAsync()
    {
        for (int i = 0; i < isActive.Length; i++) isActive[i] = true;

        for (int i = 0; i < _muzzles.Length; i++)
        {
            _traveledDistances[i] = 0f;
            _shootingPoints[i] = _muzzles[i].transform.position;
            _directions[i] = _muzzles[i].transform.forward;
            _currentPositions[i] = _shootingPoints[i];
        }

        for (int i = 0; i < _laserGlowLines.Length; i ++)
        {
            _laserGlowLines[i].enabled = true;
            _laserGlowLines[i].positionCount = 2;
            _laserGlowLines[i].SetPosition(0, _shootingPoints[i]);
            _laserGlowLines[i].SetPosition(1, _shootingPoints[i]);

            _laserCoreLines[i].enabled = true;
            _laserCoreLines[i].positionCount = 2;
            _laserCoreLines[i].SetPosition(0, _shootingPoints[i]);
            _laserCoreLines[i].SetPosition(1, _shootingPoints[i]);
        }

        while (true)
        {
            if (_traveledDistances.Any(x => x >= _gunfireRange) || isActive.All(j => j == false)) break;

            for (int i = 0; i < _muzzles.Length; i++)
            {
                // 速度がそのまま移動距離になる
                float step = _laserSpeed * Time.deltaTime;
                // 移動距離が上限を超えないようにする
                float moveDistance = Mathf.Min(step, _gunfireRange - _traveledDistances[i]);

                Ray ray = new Ray(_currentPositions[i], _directions[i]);
                RaycastHit hit;

                // 進んだ距離の範囲内で衝突判定
                if (isActive[i] && Physics.Raycast(ray, out hit, moveDistance))
                {
                    Debug.Log("発砲！");

                    isActive[i] = false;
                    _laserGlowLines[i].enabled = false;
                    _laserCoreLines[i].enabled = false;

                    // プレイヤーに命中した場合
                    if (hit.collider.CompareTag("Player"))
                    {
                        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                        if (damageable != null) damageable.TakeDamage(CalculateDamage(_firstShotMultiplier), DamageType.EnergyDamage);
                    }
                }
                
                // 発射方角に対して打ったレイの移動量を加算
                _currentPositions[i] += _directions[i] * moveDistance;
                _traveledDistances[i] += moveDistance;

                // グラフィックに反映する
                if (isActive[i]) UpdateLaserPosition(i, _shootingPoints[i], _currentPositions[i], _directions[i], _traveledDistances[i]);
            }

            yield return null;
        }

        // レーザーを非表示にする
        foreach (var laserGlowLine in _laserGlowLines)
        {
            laserGlowLine.enabled = false;
        }
        foreach (var laserCoreLine in _laserCoreLines)
        {
            laserCoreLine.enabled = false;
        }

        // クールタイム開始
        _startCooldown = true;
    }

    /// <summary>
    /// レーザーの表示位置を更新する
    /// </summary>
    private void UpdateLaserPosition(int index, Vector3 origin, Vector3 currentPosition, Vector3 direction, float traveledDistance)
    {
        // レーザーの後端位置
        Vector3 startPosition = currentPosition - direction * traveledDistance;

        // レーザーの前端位置
        _laserGlowLines[index].SetPosition(0, startPosition);
        _laserGlowLines[index].SetPosition(1, currentPosition);

        _laserCoreLines[index].SetPosition(0, startPosition);
        _laserCoreLines[index].SetPosition(1, currentPosition);
    }

    // エネルギー自動回復処理
    private IEnumerator ReGenerateEnergy(CallType type)
    {
        if (type == CallType.Normal) yield return new WaitForSeconds(_energyRegenerationDelay);

        while (_currentEnergy < _maxEnergy)
        {
            int beforeValue = _currentEnergy;
            _currentEnergy += _energyReGenerateAmount;
            _currentEnergy = Mathf.Min(_currentEnergy, _maxEnergy);
            Debug.Log($"現在のエネルギー量：{_currentEnergy}");

            // UIに反映
            if (_coroutines[(int)_energySliderType] != null) StopCoroutine(_coroutines[(int)_energySliderType]);
            _coroutines[(int)_energySliderType] = StartCoroutine(_gameController.UpdateSlider(_energySliderType, _currentEnergy, beforeValue, _maxEnergy));

            yield return new WaitForSeconds(0.05f);
        }

        _currentEnergy = _maxEnergy;
        _readyToFire = true;
        Debug.Log("エネルギー：充填完了");
    }

    //=================================== スライダーの追従処理 =====================================
    private void UpdateSliderPosition()
    {
        Vector3 targetPosition = this.transform.position + Vector3.up * _upperOffset + _mainCamera.transform.transform.right * _rightOffset;
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(targetPosition);
        _turretSlider.rectTransform.position = screenPosition;
    }

    //=================================== ダメージ計算処理・取得処理 =====================================
    // ダメージ計算式：ボス攻撃力 / 2 x ダメージ倍率 x (1 + 会心ダメージ)
    private int CalculateDamage(float damageMultiplier)
    {
        bool isCritical = Random.value <= _currentCritRate;
        float finalDamage = _currentAtk * damageMultiplier * (isCritical ? (1 + _currentCritDamage) : 1.0f);
        return (int)finalDamage;
    }

    // ダメージを受け取る：(受けたダメージ - 防御力) x (1 - 耐性%)
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (_currentHp <= 0) return;
        Debug.Log("ダメージを受けました！");

        // ダメージタイプに応じた耐性を取得する
        float targetResist = 0f;

        if (damageType == DamageType.PhysDamage) targetResist = _currentPhysResist;
        else if (damageType == DamageType.EnergyDamage) targetResist = _currentEnergyResist;

        float baseDamage = Mathf.Max(damage - _currentDef, 1f);
        int finalDamage = Mathf.RoundToInt(baseDamage * (1f - targetResist));
        finalDamage = Mathf.Max(finalDamage, 1);
        int beforeValue = _currentHp;

        // HPの変動と死亡処理
        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);

        // UIに反映
        if (_coroutines[(int)_hpSliderType] != null) StopCoroutine(_coroutines[(int)_hpSliderType]);
        _coroutines[(int)_hpSliderType] = StartCoroutine(_gameController.UpdateSlider(_hpSliderType, _currentHp, beforeValue, _maxHp));

        if (_currentHp <= 0)
        {
            // _turretStandにアタッチされてる爆破エフェクトを実行


            // タレットを破壊
            BreakTurret();
        }
    }

    //=================================== 破壊・復活処理 =====================================
    // 破壊処理
    private void BreakTurret()
    {
        // 爆発エフェクト


        _turretSlider.gameObject.SetActive(false);
        _turretCollider.enabled = false;
        _readyToFire = false;
        this.gameObject.SetActive(false);

        _gameController.StartCoroutine(ReGenerateTurret(_turretType, _energySliderType));
    }

    // タレットの復活処理
    public IEnumerator ReGenerateTurret(TurretType turretType, SliderType energySliderType)
    {
        float turretReGenerateTimer = 0;

        // typeでインデックスを指定する
        // 破壊されたタレット別でタイマーを走らせる
        while (turretReGenerateTimer < _turretReGenerationDelay)
        {
            turretReGenerateTimer += Time.deltaTime;
            yield return null;
        }

        // クールタイムを切ったら TurretController側にある生成用コルーチンを実行する

        if (_generateTurretAsync != null) StopCoroutine(_generateTurretAsync);
            _generateTurretAsync = _gameController.StartCoroutine(GenerateTurretAsync());
    }

    //=================================== アニメーション処理 =====================================


    /// <summary>
    /// タレット生成時のアニメーション処理（実行は GameController側で行う！）
    /// </summary>
    public IEnumerator GenerateTurretAsync()
    {
        _turretSlider.gameObject.SetActive(true);

        // HP回復
        _currentHp = _maxHp;
        if (_coroutines[(int)_hpSliderType] != null) StopCoroutine(_coroutines[(int)_hpSliderType]);
        _coroutines[(int)_hpSliderType] = _gameController.StartCoroutine(_gameController.UpdateSlider(_hpSliderType, _currentHp, 1, _maxHp));

        // エネルギー充填
        _currentEnergy = 1;
        if (_coroutines[(int)_energySliderType] != null) StopCoroutine(_coroutines[(int)_energySliderType]);
        _coroutines[(int)_energySliderType] = _gameController.StartCoroutine(_gameController.UpdateSlider(_energySliderType, _currentEnergy, 1, _maxEnergy));

        float duration = 0.5f;
        float elapsedTime = 0f;

        _readyToFire = false;
        _currentFireCooldown = _maxFireCooldown;

        this.gameObject.SetActive(true);

        // 頭部の初期角度
        Quaternion startRot = Quaternion.Euler(-90f, _turretHead.transform.localEulerAngles.y, _turretHead.transform.localEulerAngles.z);
        Quaternion targetRot = Quaternion.identity;
        _turretHead.transform.localRotation = startRot;

        // 取付部位の初期位置
        Vector3 mountPos = _turretMount.transform.localPosition;
        Vector3 startPos = new Vector3(mountPos.x, mountPos.y - 3, mountPos.z);
        Vector3 targetPos = _turretMount.transform.localPosition;
        _turretMount.transform.localPosition = startPos;

        // 頭部の取付部位を上げる
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            _turretMount.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        _turretMount.transform.localPosition = targetPos;
        elapsedTime = 0f;

        // タレットの頭部を回転
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            _turretHead.transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        _turretHead.transform.localRotation = targetRot;

        Debug.Log("タレット：設置完了！");
        _turretCollider.enabled = true;

        // エネルギー充填
        if (_reGenerateEnergy != null) StopCoroutine(_reGenerateEnergy);
        _reGenerateEnergy = StartCoroutine(ReGenerateEnergy(CallType.Performance));
    }

    /// <summary>
    /// タレットを非表示にする
    /// </summary>
    public void TurretSetFalse()
    {
        this.gameObject.SetActive(false);
    }
}
