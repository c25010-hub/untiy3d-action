using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private Collider[] _colliders = new Collider[4];
    [SerializeField] private PlayerDataSO _playerData;

    private Coroutine[] _coroutines = new Coroutine[3];
    private Coroutine _rapidFireOn, _rapidFireOff;
    private Camera _mainCamera;
    private Rigidbody _playerRb;
    private Animator _playerAnim;

    private int _staminaConsumption = 35;
    private int _attackCount = 0;
    private int _hitCount = 0;
    private int _shotCost = 2;

    private float _gunfireRange = 500f;

    private float _shootingCooldown = 0.2f; // 銃撃のクールダウン
    private float _staminaRegenDelay = 0.1f;// スタミナ回復が始まるまでの遅延
    private float _comboTimeout = 2.0f;     // コンボ受付の猶予時間

    private float _shootingTimer;
    private float _staminaRegenTimer;
    private float _comboTimer;

    private float _playerRushSpeed = 150.0f;
    private float _playerRotateSpeed = 10.0f;
    private float _playerSpeed = 10.0f;

    private bool _switchToShootingMode = false;
    private bool _defeated = false;
    private bool _shootingMode = false;
    private bool _startAttackTimer = false;
    private bool _avoidance;
    private bool _isAttacking = false;

    private void Awake()
    {
        // ステータスをセット
        SetStats();

        // BuffType の要素数を自動で取得
        int buffTypeCount = Enum.GetValues(typeof(BuffType)).Length;
        // 取得した数で配列を初期化
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

        // スタミナ回復
        RegenerateStamina();

        // 挙動制御
        MovePlayer();

        // 回避処理
        Avoidance();

        // 通常攻撃
        PlayerAttack();

        // 通常攻撃コンボ・タイマー
        AttackTimer();

        // 銃撃が可能か確認する
        SwitchToShootingMode();

        // 銃撃
        RapidFire();
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
    /// （コライダーを表示）
    /// </summary>
    public void Attack1()
    {
        _hitCount = 1;
        _colliders[0].gameObject.SetActive(true);
    }

    /// <summary>
    /// 通常攻撃二段目
    /// （コライダーを表示）
    /// </summary>
    public void Attack2()
    {
        _hitCount = 2;
        _colliders[1].gameObject.SetActive(true);
    }

    /// <summary>
    /// 通常攻撃三段目
    /// （コライダーを表示）
    /// </summary>
    public void Attack3()
    {
        _hitCount = 3;
        _colliders[2].gameObject.SetActive(true);
    }

    /// <summary>
    /// 攻撃に使ったコライダーを非アクティブ化する
    /// （剣を振り下ろすタイミングに合わせて実行）
    /// </summary>
    public void AttackColliderOff()
    {
        for (int i = 0; i < _colliders.Length;i++)
        {
            _colliders[i].gameObject.SetActive(false);
        }

        // 攻撃判定が消えたら、コンボ猶予（リキャスト）のカウントを開始   
        _comboTimer = 0f;
    }

    /// <summary>
    /// 通常攻撃時から次の攻撃につなげるまでにに残された時間をカウント
    /// </summary>
    public void StartAttackRecast()
    {
        _startAttackTimer = true;
    }

    /// <summary>
    /// 攻撃終了フラグを下ろす（これで次の入力や移動が可能になる）
    /// </summary>
    public void EndAttack()
    {
        _isAttacking = false; 
    }

    /// <summary>
    /// 弾丸（Ray）を発砲
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
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                float multiplier = _playerData.FirstShotMultiplier;

                damageable.TakeDamage(CalculateDamage(multiplier), DamageType.EnergyDamage);
            }

            int beforeValue = _currentEnergy;
            _currentEnergy -= _shotCost;

            // プレイヤーのエネルギー量をUIに反映する（GameController に通知を送る）
            if (_coroutines[(int)SliderTyp.PlayerEnergySlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerEnergySlider]);
            _coroutines[(int)SliderTyp.PlayerEnergySlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerEnergySlider, _currentEnergy, beforeValue, _maxEnergy));
        }
    }
    
    /// <summary>
    /// 回避終了
    /// </summary>
    public void AvoidanceOff()
    {
        _avoidance = false;
        Time.timeScale = 1.0f;
    }

    //=================================== プレイヤーの動作制御 =====================================
    // 移動制御
    private void MovePlayer()
    {
        float t = Time.deltaTime;
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveDirection != Vector3.zero)
        {
            if (_avoidance == false)
            {
                // 移動
                _playerRb.linearVelocity = new Vector3(
                    moveDirection.x * _playerSpeed * (1 + Buffs[(int)BuffType.SpeedUp]),
                    _playerRb.linearVelocity.y,
                    moveDirection.z * _playerSpeed * (1 + Buffs[(int)BuffType.SpeedUp])
                    );
            }

            // 回転
            // 向きたい方向への回転を作る（LookRotatio）
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            // 滑らかに回転させる（Slerp）
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t * _playerRotateSpeed);
        }

        if (_avoidance == false)
        {
            int animValue = (Mathf.Abs((int)horizontal) > 0 || Mathf.Abs((int)vertical) > 0) ? 1 : 0;
            _playerAnim.SetInteger("Run", animValue);
        }
        else
        {
            _playerAnim.SetInteger("Run", 0);
        }
    }

    // 回避処理
    private void Avoidance()
    {
        // 回避
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (_currentStamina > 0 && _avoidance == false)
            {
                if (_shootingMode)
                {
                    // 銃撃モード終了
                    RapidFireOff();
                }

                _avoidance = true;
                int beforeValue = _currentStamina;
                _currentStamina -= _staminaConsumption;
                _playerAnim.SetTrigger("Dodge");

                // 現在の速度を一度リセット
                _playerRb.linearVelocity = Vector3.zero;
                // 自身の前方向へ突進
                _playerRb.AddForce(transform.forward * _playerRushSpeed, ForceMode.VelocityChange);

                if (_currentStamina < 0)
                {
                    _currentStamina = 0;
                }

                //プレイヤーのスタミナ量をUIに反映する
                if (_coroutines[(int)SliderTyp.PlayerStaminaSlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerStaminaSlider]);
                _coroutines[(int)SliderTyp.PlayerStaminaSlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerStaminaSlider, _currentStamina, beforeValue, _maxStamina));
            }
        }
    }

    // スタミナ自動回復
    private void RegenerateStamina()
    {
        if (_currentStamina < _maxStamina)
        {
            _staminaRegenTimer += Time.deltaTime;

            if (_staminaRegenTimer >= _staminaRegenDelay)
            {
                _staminaRegenTimer = 0;
                int beforeValue = _currentStamina;
                _currentStamina++;

                if (_currentStamina > _maxStamina) _currentStamina = _maxStamina;

                //プレイヤーのスタミナ量をUIに反映する
                if (_coroutines[(int)SliderTyp.PlayerStaminaSlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerStaminaSlider]);
                _coroutines[(int)SliderTyp.PlayerStaminaSlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerStaminaSlider, _currentStamina, beforeValue, _maxStamina));
            }
        }
    }

    // プレイヤーの通常攻撃システム
    private void PlayerAttack()
    {
        // 銃撃モードの時、通常攻撃を無視
        if (_shootingMode) return;
        // 回避中は、通常攻撃を無視
        if (_avoidance) return;

        // 通常攻撃
        if (Mouse.current.leftButton.wasPressedThisFrame && _isAttacking == false)
        {
            _isAttacking = true;        // 攻撃中フラグを立てる
            _startAttackTimer = false;  // タイマーを止める

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

    // 銃撃が可能か確認する
    private void SwitchToShootingMode()
    {
        // 銃撃可能か確認
        if (!_switchToShootingMode) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
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

        // 銃撃
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _shootingTimer = 0;
            _playerAnim.SetTrigger("Fire");
        }
    }

    //=================================== 銃撃開始・終了アクション =====================================
    // 開始演出
    private IEnumerator RapidFireOn()
    {
        // 一人称視点に切り替えて、UIを開く
        _cameraController.ChangeToFirstPerson();

        // _cameraController側の準備が整うまで、銃撃モードには入れない
        yield return new WaitUntil(() => _cameraController.ReadyToFire == true);

        // スローモーション ON
        Time.timeScale = 0.1f;
        _shootingTimer = _shootingCooldown;

        // 銃撃開始
        _shootingMode = true;
    }

    // 終了演出
    private void  RapidFireOff()
    {
        // 銃撃停止
        _shootingMode = false;
        _currentEnergy = 0;

        // 三人称視点に戻して、UIを閉じる
        _cameraController.ChangeToThirdPerson();

        // スローモーション OFF
        Time.timeScale = 1.0f;
    }

    //=================================== ダメージ計算式・取得処理 =====================================
    // ダメージ計算処理
    private int CalculateDamage(float damageMultiplier)
    {
        // 攻撃力 x (1 + 攻撃力バフ) x 倍率 x (1 + 会心ダメージ) x (1+ 会心ダメージバフ)
        bool isCritical = UnityEngine.Random.value <= _currentCritRate;
        float finalDamage = _currentAtk * (1 + Buffs[(int)BuffType.AttackUp]) * damageMultiplier * (isCritical ? (1 + _currentCritDamage) * (1 + Buffs[(int)BuffType.CritDamageUp]) : 1.0f);
        return(int)finalDamage;
    }

    // ダメージを受け取る
    public void TakeDamage(int damage, DamageType damageType)
    {
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

        // (受けたダメージ - 防御力 x (1 + 防御力バフ)) x (1 - 耐性%)
        float baseDamage = Mathf.Max(damage - _currentDef * (1 + Buffs[(int)BuffType.DefUp]), 1f);
        int finalDamage = Mathf.RoundToInt(baseDamage * (1.0f - targetResist));
        finalDamage = Mathf.Max(finalDamage, 1);

        int beforeValue = _currentHp;

        // HPの変動と死亡処理
        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);

        // プレイヤーの HP量を UIに反映する（GameController に通知を送る）
        if (_coroutines[(int)SliderTyp.PlayerHpSlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerHpSlider]);
        _coroutines[(int)SliderTyp.PlayerHpSlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerHpSlider, _currentHp, beforeValue, _maxHp));

        if (_currentHp == 0)
        {
            Defeated();
        }
    }

    // 負け演出実行関数
    private void Defeated()
    {
        // アニメーション


        // 負け演出は GameController に通知を出す
        _gameController.GameSet(BattleResult.Lose);
    }

    //=================================== 回復・バフ処理 =====================================
    // 回復を受け取る関数
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
                if (_coroutines[(int)SliderTyp.PlayerHpSlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerHpSlider]);
                _coroutines[(int)SliderTyp.PlayerHpSlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerHpSlider, _currentHp, beforeValue, _maxHp));
                break;

            // エネルギ補充
            case HealType.HealEnergy:
                beforeValue = _currentEnergy;
                _currentEnergy += amount;
                if (_currentEnergy > _maxEnergy) _currentEnergy = _maxEnergy;
                // プレイヤーのエネルギー量をUIに反映する（GameController に通知を送る）
                if (_coroutines[(int)SliderTyp.PlayerEnergySlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerEnergySlider]);
                _coroutines[(int)SliderTyp.PlayerEnergySlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerEnergySlider, _currentEnergy, beforeValue, _maxEnergy));
                break;
        }
    }
    
    // バフを受け取る関数
    public void ApplyBuff(BuffType type, float amount, float duration, int limit)
    {
        if (BuffStacks[(int)type] >= limit) return;
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
        //=================================== 通常攻撃の当たり判定 =====================================
        if (_hitCount > 0 && _colliders[_hitCount - 1].gameObject.activeSelf)
        {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

            if (damageable != null)
            {
                float multiplier = 1.0f;

                if (_hitCount == 1) multiplier = _playerData.FirstStrikeMultiplier;
                else if (_hitCount == 2) multiplier = _playerData.SecondStrikeMultiplier;
                else if (_hitCount == 3) multiplier = _playerData.ThirdStrikeMultiplier;

                Debug.Log($"通常攻撃{_hitCount}段目が当たった");
                damageable.TakeDamage(CalculateDamage(multiplier), DamageType.PhysDamage);

                int beforeValue = _currentEnergy;

                // エネルギー回復
                if (_currentEnergy < _maxEnergy) _currentEnergy += 5 * _hitCount;
                // 超過分を割く
                if (_currentEnergy > _maxEnergy) _currentEnergy = _maxEnergy;
                // 銃撃許可
                if (_currentEnergy == _maxEnergy) _switchToShootingMode = true;

                // エネルギー回復量を UI に反映（GameController に通知を出す）
                if (_coroutines[(int)SliderTyp.PlayerEnergySlider] != null) StopCoroutine(_coroutines[(int)SliderTyp.PlayerEnergySlider]);
                _coroutines[(int)SliderTyp.PlayerEnergySlider] = StartCoroutine(_gameController.UpdateSlider(SliderTyp.PlayerEnergySlider, _currentEnergy, beforeValue, _maxEnergy));
            }
        }

        //=================================== 極限回避の当たり判定 =====================================
        if (_colliders[3].gameObject.activeSelf &&
            other.gameObject.CompareTag("Enemy"))
        {
            // 極限回避処理
            Time.timeScale = 0.4f;
        }
    }
}
