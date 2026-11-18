using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour, IDamageable
{
    //=================================== ステータス (Max値) =====================================
    private int _maxHp;
    private int _maxAtk;
    private int _maxDef;
    private int _maxEnergy;
    private int _maxStamina;

    private float _maxPhysResist;  // 物理耐性
    private float _maxEnergyResist;// エネルギー耐性

    private float _maxCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _maxCritDamage;// 0.5 = 50%

    //=================================== ステータス (現在値) =====================================
    private int _currentHp;
    private int _currentAtk;
    private int _currentDef;
    private int _currentEnergy;
    private int _currentStamina;

    private float _currentPhysResist;  // 物理耐性
    private float _currentEnergyResist;// エネルギー耐性

    private float _currentCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _currentCritDamage;// 0.5 = 50%

    //=================================== 参照用プロパティ =====================================
    public int CurrentHp => _currentHp;
    public int CurrentEnergy => _currentEnergy;
    public int CurrentStamina => _currentStamina;

    //=================================== バフ増加値 =====================================
    private float [] Buffs;

    //=================================== スタック上限 =====================================
    private int[] BuffStacks;

    //=================================== フィールド =====================================
    [SerializeField] private GameController _gameController;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private PlayerDataSO _playerData;
    [SerializeField] private GameObject[] _guns = new GameObject[2];
    [SerializeField] private AttackHitBox[] _hitBoxs = new AttackHitBox[3];
    [SerializeField] private CanvasGroup _staminaCanvasGroup;

    // SliderTypの容量に合わせる
    private Coroutine[] _coroutines = new Coroutine[System.Enum.GetValues(typeof(SliderType)).Length];
    private Coroutine _rapidFireOn, _updatePlayerStamina, _regenerateStamina;
    private Camera _mainCamera;// 発砲時のRayの基準点になる
    private Rigidbody _playerRb;
    private Animator _playerAnim;

    private int _staminaConsumption = 20;
    private int _attackCount = 0;
    private int _shotCost = 2;

    private float _gunfireRange = 500f;
    private float _shootingCooldown = 0.2f;// 銃撃のクールダウン
    private float _comboTimeout = 1.0f;    // コンボ受付の猶予時間
    private float _staminaRegenerationDelay = 1.4f;// スタミナ回復が始まるまでの遅延

    private float _shootingTimer;
    private float _comboTimer;

    private float _playerRushSpeed = 3.2f;
    private float _playerRotateSpeed = 10.0f;
    private float _playerSpeed = 2.0f;

    private bool _switchToShootingMode = false;
    private bool _defeated = false;
    private bool _shootingMode = false;
    private bool _startAttackTimer = false;
    private bool _avoidance = false;
    private bool _isAttacking = false;

    private void Awake()
    {
        SetStats();
        AttackColliderAllOff();

        _staminaCanvasGroup.alpha = 0f;

        foreach (var gun in _guns) if (gun != null) gun.gameObject.Serialize(false);
        foreach (var hitBox in _hitBoxs) if (hitBox != null) hitBox.gameObject.Serialize(false);

        int buffTypeCount = Enum.GetValues(typeof(BuffType)).Length;
        Buffs = new float[buffTypeCount];
        BuffStacks = new int[buffTypeCount];

        _mainCamera = Camera.main;
        _playerRb = GetComponent<Rigidbody>();
        _playerAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        // ダウンしたら操作不可
        if (_defeated) return;

        // 挙動制御
        MovePlayer();

        // 回避処理
        Avoidance();

        // 通常攻撃
        PlayerAttack();

        // 通常攻撃コンボ・タイマー
        AttackTimer();

        // 銃撃モード切り替え処理
        SwitchToShootingMode();

        // 銃撃
        RapidFire();

        // スタミナゲージの追従処理
        MoveStaminaGauge();
    }

    //=================================== 初期処理 =====================================
    // ステータスをセット
    private void SetStats()
    {
        int _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
        float _stageLevelMultiplier = PlayerPrefs.GetFloat("StageLevelMultiplier", 0.2f);

        int _baseHp = PlayerPrefs.GetInt("PlayerBaseHp", _playerData.BaseHp);   // HP
        int _baseAtk = PlayerPrefs.GetInt("PlayerBaseAtk", _playerData.BaseAtk);// 攻撃力
        int _baseDef = PlayerPrefs.GetInt("PlayerBaseDef", _playerData.BaseDef);// 防御力
        int _baseEnergy = PlayerPrefs.GetInt("PlayerBaseEnergy", _playerData.BaseEnergy);   // 最大エネルギー量
        int _baseStamina = PlayerPrefs.GetInt("PlayerBaseStamina", _playerData.BaseStamina);// 最大スタミナ量

        float _basePhysResist = PlayerPrefs.GetFloat("PlayerBasePhysResist", _playerData.BasePhysResist);      // 物理耐性
        float _baseEnergyResist = PlayerPrefs.GetFloat("PlayerBaseEnergyResist", _playerData.BaseEnergyResist);// エネルギー耐性
        float _baseCritRate = PlayerPrefs.GetFloat("PlayerBaseCritRate", _playerData.BaseCritRate);      // 会心率
        float _baseCritDamage = PlayerPrefs.GetFloat("PlayerBaseCritDamage", _playerData.BaseCritDamage);// 会心ダメージ

        // Max
        _maxHp = _baseHp;
        _maxAtk = _baseAtk;
        _maxDef = _baseDef;
        _maxEnergy = _baseEnergy;
        _maxStamina = _baseStamina;

        _maxPhysResist = _basePhysResist;
        _maxEnergyResist = _baseEnergyResist;
        _maxCritRate = _baseCritRate;
        _maxCritDamage = _baseCritDamage;

        // 現在値
        _currentHp = _maxHp;
        _currentAtk = _maxAtk;
        _currentDef = _maxDef;
        _currentEnergy = _maxEnergy;
        _currentStamina = _maxStamina;
        _currentPhysResist = _maxPhysResist;
        _currentEnergyResist = _maxEnergyResist;
        _currentCritRate = _maxCritRate;
        _currentCritDamage = _maxCritDamage;
    }

    //=================================== イベント関数 =====================================
    /// <summary>
    /// 通常攻撃一段目
    /// （コライダーをON）
    /// </summary>
    public void Attack1()
    {
        if (_avoidance || _shootingMode || _defeated) return;

        _hitBoxs[0].HitBoxOn();
    }

    /// <summary>
    /// 通常攻撃二段目
    /// （コライダーをON）
    /// </summary>
    public void Attack2()
    {
        if (_avoidance || _shootingMode || _defeated) return;

        _hitBoxs[1].HitBoxOn();
    }

    /// <summary>
    /// 通常攻撃三段目
    /// （コライダーをON）
    /// </summary>
    public void Attack3()
    {
        if (_avoidance || _shootingMode || _defeated) return;

        _hitBoxs[2].HitBoxOn();
    }

    /// <summary>
    /// 攻撃用コライダーをすべて OFF
    /// </summary>
    public void AttackColliderAllOff()
    {
        for (int i = 0; i < 3; i++)
        {
            if (_hitBoxs[i] != null) _hitBoxs[i].HitBoxOff();
        }
    }

    /// <summary>
    /// 通常攻撃時から次の攻撃につなげるまでにに残された時間をカウントを開始
    /// と同時に攻撃終了のフラグ
    /// </summary>
    public void StartAttackTimer()
    {   
        _isAttacking = false;
        _startAttackTimer = true;
    }

    /// <summary>
    /// 発砲処理
    /// </summary>
    public void Fire()
    {
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // デバッグ用にシーン画面に赤い線を表示
        Debug.DrawRay(ray.origin, ray.direction * _gunfireRange, Color.red, 0.5f);

        // レイを飛ばして何かに当たったら
        if (Physics.Raycast(ray, out hit, _gunfireRange))
        {
            if (hit.collider.transform.root.CompareTag("Enemy"))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    float multiplier = _playerData.FirstShotMultiplier;

                    damageable.TakeDamage(CalculateDamage(multiplier), DamageType.EnergyDamage);
                }
            }
        }

        int beforeValue = _currentEnergy;
        _currentEnergy -= _shotCost;

        if (_currentEnergy < 0) _currentEnergy = 0;

        // プレイヤーのエネルギー量をUIに反映する（GameController に通知を送る）
        if (_coroutines[(int)SliderType.PlayerEnergySlider] != null) StopCoroutine(_coroutines[(int)SliderType.PlayerEnergySlider]);
        _coroutines[(int)SliderType.PlayerEnergySlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.PlayerEnergySlider, _currentEnergy, beforeValue, _maxEnergy));
    }
    
    /// <summary>
    /// 回避終了
    /// </summary>
    public void AvoidanceOff()
    {
        _avoidance = false;
        Time.timeScale = 1.0f;
        
        // 回避後は通常攻撃を発動可能
        _isAttacking = false;
    }

    //=================================== プレイヤーの動作制御 =====================================
    // 移動制御
    private void MovePlayer()
    {
        if (_isAttacking || _shootingMode || _defeated || _avoidance) return;

        float t = Time.deltaTime;
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        // 移動
        _playerRb.linearVelocity = new Vector3(
            moveDirection.x * _playerSpeed * (1 + Buffs[(int)BuffType.SpeedUp]),
            _playerRb.linearVelocity.y,
            moveDirection.z * _playerSpeed * (1 + Buffs[(int)BuffType.SpeedUp])
            );

        // 回転
        if (moveDirection != Vector3.zero)
        {
            // 向きたい方向への回転を作る（LookRotatio）
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            // 滑らかに回転させる（Slerp）
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t * _playerRotateSpeed);
        }

        int animValue = (Mathf.Abs((int)horizontal) > 0 || Mathf.Abs((int)vertical) > 0) ? 1 : 0;
        _playerAnim.SetInteger("Run", animValue);
    }

    // 移動を強制的に止める
    private void StopMovement()
    {
        _playerRb.linearVelocity = new Vector3(0, _playerRb.linearVelocity.y, 0);
        _playerAnim.SetInteger("Run", 0);
    }

    // 回避処理
    // NOTE:回避後は時間外でなければ、次の攻撃につなげられる
    private void Avoidance()
    {
        // 回避
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (_currentStamina > 0 && _avoidance == false)
            {
                _staminaCanvasGroup.alpha = 1f;

                // 移動を強制的に止める
                StopMovement();
                _avoidance = true;

                if (_isAttacking)
                {
                    AttackColliderAllOff();
                    StartAttackTimer();
                }

                // 銃撃処理を中断
                if (_rapidFireOn != null || _shootingMode)
                {
                    StopCoroutine(_rapidFireOn);
                    _rapidFireOn = null;

                    RapidFireOff();
                }

                // 回避アニメション
                _playerAnim.SetTrigger("Dodge");
                int beforeValue = _currentStamina;
                _currentStamina -= _staminaConsumption;

                // 現在の速度を一度リセット
                _playerRb.linearVelocity = Vector3.zero;
                // 自身の前方向へ突進
                _playerRb.linearVelocity =transform.forward * _playerRushSpeed;

                if (_currentStamina < 0) _currentStamina = 0;

                //プレイヤーのスタミナ量をUIに反映する
                if (_updatePlayerStamina != null) StopCoroutine(_updatePlayerStamina);
                _updatePlayerStamina = StartCoroutine(_gameController.UpdatePlayerStamina(_currentStamina, beforeValue, _maxStamina));

                // スタミナ自動回復
                if (_regenerateStamina != null) StopCoroutine(_regenerateStamina);
                _regenerateStamina = StartCoroutine(RegenerateStamina());
            }
        }
    }

    // スタミナ自動回復
    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(_staminaRegenerationDelay);

        while (_currentStamina < _maxStamina)
        {
            int beforeValue = _currentStamina;
            _currentStamina++;

            if (_currentStamina > _maxStamina) _currentStamina = _maxStamina;

            // プレイヤーのスタミナ量をUIに反映する
            if (_updatePlayerStamina != null) StopCoroutine(_updatePlayerStamina);
            _updatePlayerStamina = StartCoroutine(_gameController.UpdatePlayerStamina(_currentStamina, beforeValue, _maxStamina));

            yield return new WaitForSeconds(0.05f);
        }

        float timer = 0;

        while (timer < 1.4)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        while (_staminaCanvasGroup.alpha > 0)
        {
            _staminaCanvasGroup.alpha -= Time.deltaTime;
            yield return null;
        }

        _staminaCanvasGroup.alpha = 0f;
    }

    // 通常攻撃処理
    private void PlayerAttack()
    {
        // 銃撃モードの時、通常攻撃を無視
        if (_shootingMode) return;
        // 回避中は、通常攻撃を無視
        if (_avoidance) return;
        // マウスが接続されてるか確認
        if (Mouse.current == null) return;

        // 通常攻撃
        if (Input.GetButton("Fire1") && _isAttacking == false)
        {
            // 移動を強制的に止める
            StopMovement();
            AttackColliderAllOff();

            _isAttacking = true;      // 攻撃中フラグを立てる
            _startAttackTimer = false;// タイマーを止める
            _comboTimer = 0f;         // タイマーをリセット

            if (_attackCount == 0)
            {
                _attackCount++;
                _playerAnim.SetTrigger("FirstStrike");
            }
            else if (_attackCount == 1)
            {
                _attackCount++;
                _playerAnim.SetTrigger("SecondStrike");
            }
            else if (_attackCount == 2)
            {
                _attackCount = 0;// 3段目が終わったらコンボ頭に戻す
                _playerAnim.SetTrigger("ThirdStrike");
            }
        }
    }

    // 攻撃のコンボ・タイマーを開始
    private void AttackTimer()
    {
        if (_startAttackTimer)
        {
            _comboTimer += Time.deltaTime;

            if (_comboTimer >= _comboTimeout)
            {
                _startAttackTimer = false; // カウント停止
                _attackCount = 0;
                _comboTimer = 0f;
            }
        }
    }

    // 銃撃モード切り替え処理
    private void SwitchToShootingMode()
    {
        // 銃撃可能か確認
        if (!_switchToShootingMode) return;
        if (_avoidance || _isAttacking) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 移動を強制的に止める
            StopMovement();
            _switchToShootingMode = false;

            // 銃撃モード ON
            if (_rapidFireOn != null) StopCoroutine(_rapidFireOn);
            _rapidFireOn = StartCoroutine(RapidFireOn());
        }
    }

    // 銃撃処理
    private void RapidFire()
    {
        if (!_shootingMode) return;

        // カメラをマウスで動かす処理
        _cameraController.MoveCamera();

        // クールダウンの更新とチェック
        _shootingTimer += Time.deltaTime;
        if (_shootingTimer < _shootingCooldown) return;

        // エネルギ残高が足りない場合
        if (_currentEnergy <= 0)
        {
            RapidFireOff();
            return;
        }

        // マウスが接続されてるか確認
        if (Mouse.current == null) return;

        // 銃撃
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _shootingTimer = 0;
            _playerAnim.SetTrigger("Fire");
        }
    }

    //=================================== スタミナゲージの追従処理 =====================================
    private void MoveStaminaGauge()
    {

    }

    //=================================== 銃撃開始・終了アクション =====================================
    // 開始演出
    private IEnumerator RapidFireOn()
    {
        // 一人称視点に切り替えて、UIを開く
        _cameraController.ChangeToFirstPerson();

        // 銃撃開始モーション
        _playerAnim.SetTrigger("FireOn");

        // _cameraController側の準備が整うまで、銃撃モードには入れない
        yield return new WaitUntil(() => _cameraController.ReadyToFire == true);

        // スローモーション ON
        Time.timeScale = 0.1f;
        _shootingTimer = _shootingCooldown;

        // 銃撃開始
        _shootingMode = true;
        _rapidFireOn = null;
    }

    // 銃撃終了
    private void  RapidFireOff()
    {
        // レイヤー0（Base Layer）の現在のアニメーション情報を取得
        AnimatorStateInfo stateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Dodge")) _playerAnim.SetTrigger("FireOff");

        _shootingMode = false;// 銃撃停止
        Time.timeScale = 1.0f;// スローモーション OFF
        
        _cameraController.ChangeToThirdPerson();// 三人称視点に戻して、UIを閉じる
    }

    //=================================== ダメージ計処理・取得処理 =====================================
    // ダメージ計算算式：基礎攻撃力 x (1 + 攻撃力バフ) x ダメージ倍率 x(1 + 会心ダメージ)
    public int CalculateDamage(float damageMultiplier)
    {
        bool isCritical = UnityEngine.Random.value <= _currentCritRate;
        float finalDamage = _currentAtk * (1 + Buffs[(int)BuffType.AttackUp]) * damageMultiplier * (isCritical ? (1 + _currentCritDamage) * (1 + Buffs[(int)BuffType.CritDamageUp]) : 1.0f);
        return(int)finalDamage;
    }

    // ダメージを受け取る：(受けたダメージ - 防御力 x (1 + 防御力バフ)) x (1 - 耐性%)
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (_defeated) return;
        if (_currentHp <= 0) return;
        if (_avoidance) return;

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

        float baseDamage = Mathf.Max(damage - _currentDef * (1 + Buffs[(int)BuffType.DefUp]), 1f);
        int finalDamage = Mathf.RoundToInt(baseDamage * (1f - targetResist));
        finalDamage = Mathf.Max(finalDamage, 1);

        int beforeValue = _currentHp;

        // HPの変動と死亡処理
        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);
        Debug.Log($"残りHP：{_currentHp}");

        // プレイヤーの HP量を UIに反映する（GameController に通知を送る）
        if (_coroutines[(int)SliderType.PlayerHpSlider] != null) StopCoroutine(_coroutines[(int)SliderType.PlayerHpSlider]);
        _coroutines[(int)SliderType.PlayerHpSlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.PlayerHpSlider, _currentHp, beforeValue, _maxHp));

        if (_currentHp == 0)
        {
            Defeated();
        }
    }

    //=================================== リタイア処理 =====================================
    // リタイア演出実行関数
    private void Defeated()
    {
        _defeated = true;
        _playerAnim.SetTrigger("Death");
        AttackColliderAllOff();
        _gameController.GameSet(BattleResult.Lose);// GameControllerに通知を出す
    }

    //=================================== 回復・バフ処理 =====================================
    /// <summary>
    /// 回復を受け取る関数
    /// </summary>
    public void ApplyHeal(HealType type, int amount)
    {
        if (_currentHp <= 0) return;
        int beforeValue;

        switch (type)
        {
            // HP 回復
            case HealType.HealHp:
                beforeValue = _currentHp;
                _currentHp += amount;
                if (_currentHp > _maxHp) _currentHp = _maxHp;
                // プレイヤーの HP量を UIに反映する（GameController に通知を送る）
                if (_coroutines[(int)SliderType.PlayerHpSlider] != null) StopCoroutine(_coroutines[(int)SliderType.PlayerHpSlider]);
                _coroutines[(int)SliderType.PlayerHpSlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.PlayerHpSlider, _currentHp, beforeValue, _maxHp));
                break;

            // エネルギ補充
            case HealType.HealEnergy:
                beforeValue = _currentEnergy;
                _currentEnergy += amount;
                if (_currentEnergy > _maxEnergy) _currentEnergy = _maxEnergy;
                // プレイヤーのエネルギー量をUIに反映する（GameController に通知を送る）
                if (_coroutines[(int)SliderType.PlayerEnergySlider] != null) StopCoroutine(_coroutines[(int)SliderType.PlayerEnergySlider]);
                _coroutines[(int)SliderType.PlayerEnergySlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.PlayerEnergySlider, _currentEnergy, beforeValue, _maxEnergy));
                break;
        }
    }

    /// <summary>
    /// エネルギー回復
    /// </summary>
    public void ChargeEnergy(HitBoxType type)
    {
        int beforeValue = _currentEnergy;

        // エネルギー回復
        if (_currentEnergy < _maxEnergy) _currentEnergy += 5 * (1 + (int)type);
        // 超過分を割く
        if (_currentEnergy > _maxEnergy) _currentEnergy = _maxEnergy;
        // 銃撃許可
        if (_currentEnergy == _maxEnergy) _switchToShootingMode = true;

        // エネルギー回復量を UI に反映（GameController に通知を出す）
        if (_coroutines[(int)SliderType.PlayerEnergySlider] != null) StopCoroutine(_coroutines[(int)SliderType.PlayerEnergySlider]);
        _coroutines[(int)SliderType.PlayerEnergySlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.PlayerEnergySlider, _currentEnergy, beforeValue, _maxEnergy));
    }

    /// <summary>
    /// バフを受け取る関数
    /// </summary>
    public void ApplyBuff(BuffType type, float amount, float duration, int limit)
    {
        if (BuffStacks[(int)type] >= limit) return;
        if (amount <= 0 || duration <= 0) return;
        BuffStacks[(int)type]++;

        switch (type)
        {
        　　// 攻撃力
            case BuffType.AttackUp: Buffs[(int)BuffType.AttackUp] += amount; break;
            // 防御力
            case BuffType.DefUp: Buffs[(int)BuffType.DefUp] += amount; break;
            // 会心ダメージ
            case BuffType.CritDamageUp: Buffs[(int)BuffType.CritDamageUp] += amount; break;
            // 移動速度
            case BuffType.SpeedUp: Buffs[(int)BuffType.SpeedUp] += amount; break;
            // 未対応
            default:
                Debug.LogError("未対応のバフタイプです。", this);
                enabled = false;
                break;
        }

        Debug.Log($"{type}：{amount * BuffStacks[(int)type]}増加 持続時間：{duration}");

        // カウントダウン開始
        StartCoroutine(BuffCountdown(type, amount, duration));
    }

    // バフ持続時間の管理（独立してカウントする）
    private IEnumerator BuffCountdown(BuffType type, float amount, float duration)
    {
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            yield return null;
        }

        BuffStacks[(int)type]--;
        Buffs[(int)type] -= amount;
    }

    //=================================== 当たり判定処理 =====================================
    public void OnTriggerEnter(Collider other)
    {
        //=================================== 極限回避の当たり判定 =====================================
        if (_avoidance && other.transform.root.CompareTag("Enemy"))
        {
            // 極限回避処理
            Time.timeScale = 0.2f;
        }
    }
}
