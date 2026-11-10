using System.Collections;
using UnityEngine;

public enum TurretType
{
    RightTurret = 0,
    LeftTurret = 1
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

    // SliderTypの容量に合わせる
    private Coroutine[] _coroutines = new Coroutine[System.Enum.GetValues(typeof(SliderType)).Length];
    private Coroutine _reGenerateEnergy;

    [SerializeField] private float _rotationSpeed;      // 回転速度
    [SerializeField] private float _firstShotMultiplier;// ダメージ倍率

    private float _gunfireRange = 1000f;

    private int _shotCost = 10;

    private bool _readyToFire = false;

    [SerializeField] private Transform _attackOrigin;// レイの発射基準点
    private Transform _playerTransform;

    // タレットの初期角度（右側：-140, 左側：140）
    [SerializeField] private Quaternion _headInitialRot;
    [SerializeField] private Vector3 _mountInitialPos;

    void Start()
    {
        // ボスのステータスを適用
        SetStatus();
    }

    void Update()
    {
        // プレイヤーを追従させる
        RotateTurret();
    }

    //=================================== 初期処理 =====================================
    // ボスのステータスを適用
    private void SetStatus()
    {
        int _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
        float _stageLevelMultiplier = PlayerPrefs.GetFloat("StageLevelMultiplier", 0.2f);

        int _baseHp = _bossData.BaseHp / 2;
        int _baseAtk = _bossData.BaseAtk / 2;
        int _baseDef = _bossData.BaseDef / 2;

        float _basePhysResist = _bossData.BasePhysResist / 2;    // 物理耐性
        float _baseEnergyResist = _bossData.BaseEnergyResist / 2;// エネルギー耐性
        float _baseCritRate = _bossData.BaseCritRate / 2;
        float _baseCritDamage = _bossData.BaseCritDamage / 2;

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

            // 上下の回転処理
            if (upperDirection != Vector3.zero)
            {
                Quaternion targetLocalRot = Quaternion.LookRotation(upperDirection);
                _turretHead.transform.localRotation = Quaternion.Euler(targetLocalRot.eulerAngles.x, 0, 0);
            }

            // 左右の回転処理
            if (lowerDirection != Vector3.zero)
            {
                Quaternion lowerTargetRotation = Quaternion.Euler(0, Quaternion.LookRotation(lowerDirection).eulerAngles.y, 0);
                _turretMount.transform.rotation = Quaternion.Slerp(_turretMount.transform.rotation, lowerTargetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        // 攻撃実行処理
        _currentFireCooldown -= Time.deltaTime;

        if (_currentFireCooldown <= 0)
        {
            _currentFireCooldown = _maxFireCooldown;
            // 必須：発砲音・エフェクト

            Attack();
        }
    }

    // 攻撃
    private void Attack()
    {
        // 外してもエネルギーは消費する
        int beforeValue = _currentEnergy;
        _currentEnergy -= _shotCost;

        // エネルギー不足は気合で乗り切る
        if (_currentEnergy < 0) _currentEnergy = 0;

        // UIに反映
        if (_coroutines[(int)_energySliderType] != null) StopCoroutine(_coroutines[(int)_energySliderType]);
        _coroutines[(int)_energySliderType] = StartCoroutine(_gameController.UpdateSlider(_energySliderType, _currentEnergy, beforeValue, _maxEnergy));

        // 攻撃はレイを飛ばす
        Ray ray = new Ray(_attackOrigin.position, _attackOrigin.forward);
        RaycastHit hit;

        // デバッグ用にシーン画面に赤い線を表示
        Debug.DrawRay(ray.origin, ray.direction * _gunfireRange, Color.red, 0.5f);

        // レイを飛ばして何かに当たったら
        if (Physics.Raycast(ray, out hit, _gunfireRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();

                if (damageable != null)
                {
                    float multiplier = _firstShotMultiplier;
                    damageable.TakeDamage(CalculateDamage(multiplier), DamageType.EnergyDamage);
                }
            }
        }

        // エネルギー自動回復開始
        if (_currentEnergy <= 0)
        {
            _readyToFire = false;
            if (_reGenerateEnergy != null) StopCoroutine(_reGenerateEnergy);
            _reGenerateEnergy = StartCoroutine(ReGenerateEnergy());
        }
    }

    // エネルギー自動回復処理
    private IEnumerator ReGenerateEnergy()
    {
        float time = 0f;

        while (_currentEnergy < _maxEnergy)
        {
            time += Time.deltaTime;

            if (time > 0.02f)
            {
                time = 0;
                int beforeValue = _currentEnergy;
                _currentEnergy += _shotCost;
                _currentEnergy = Mathf.Min(_currentEnergy, _maxEnergy);

                // UIに反映
                if (_coroutines[(int)_energySliderType] != null) StopCoroutine(_coroutines[(int)_energySliderType]);
                _coroutines[(int)_energySliderType] = StartCoroutine(_gameController.UpdateSlider(_energySliderType, _currentEnergy, beforeValue, _maxEnergy));
            }
            
            yield return null;
        }

        _currentEnergy = _maxEnergy;
        _readyToFire = true;
        Debug.Log("エネルギー：充填完了");
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

        // ダメージタイプに応じた耐性を取得する
        float targetResist = 0f;

        if (damageType == DamageType.PhysDamage)
        {
            targetResist = _currentPhysResist;
        }
        else if (damageType == DamageType.EnergyDamage)
        {
            targetResist = _currentEnergyResist;
        }

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

    //=================================== 破壊処理 =====================================
    private void BreakTurret()
    {
        _readyToFire = false;
        this.gameObject.SetActive(false);

        // GameController側で処理を行う
        _gameController.StartCoroutine(_gameController.ReGenerateTurretAsync(_turretType, _energySliderType));
    }

    //=================================== アニメーション処理 =====================================
    /// <summary>
    /// タレット生成時のアニメーション処理（実行は GameController側で行う！）
    /// </summary>
    public IEnumerator GenerateTurretAsync()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        _readyToFire = false;

        this.gameObject.SetActive(true);

        // 頭部の初期角度
        Quaternion startRot = Quaternion.Euler(-90f, _headInitialRot.eulerAngles.y, _headInitialRot.eulerAngles.z);
        Quaternion targetRot = _headInitialRot;
        _turretHead.transform.rotation = startRot;

        // 取付部位の初期位置
        Vector3 startPos = _mountInitialPos + Vector3.down * 1.5f;
        Vector3 targetPos = _mountInitialPos;
        _turretMount.transform.position = startPos;

        // 頭部の取付部位を上げる
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            _turretMount.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        _turretMount.transform.position = targetPos;
        elapsedTime = 0f;

        // タレットの頭部を回転
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            _turretHead.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        _turretHead.transform.rotation = targetRot;

        _currentEnergy = 0;
        _currentFireCooldown = _maxFireCooldown;
        
        Debug.Log("タレット：起動準備完了！");

        if (_reGenerateEnergy != null) StopCoroutine(_reGenerateEnergy);
        _reGenerateEnergy = StartCoroutine(ReGenerateEnergy());
    }

    /// <summary>
    /// タレットを非表示にする
    /// </summary>
    public void TurretSetFalse()
    {
        this.gameObject.SetActive(false);
    }
}
