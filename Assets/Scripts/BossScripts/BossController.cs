using UnityEngine;
using System.Collections.Generic;

public class BossController : MonoBehaviour, IDamageable
{
    //=================================== ステータス (Max値) =====================================
    private int _maxHp;
    private int _maxAtk;
    private int _maxDef;

    private float _maxPhysResist;  // 物理耐性
    private float _maxEnergyResist;// エネルギー耐性

    private float _maxCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _maxCritDamage;// 0.5 = 50%

    //=================================== ステータス (現在値) =====================================
    private int _currentHp;
    private int _currentAtk;
    private int _currentDef;

    private float _currentPhysResist;  // 物理耐性
    private float _currentEnergyResist;// エネルギー耐性

    private float _currentCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _currentCritDamage;// 0.5 = 50%

    //=================================== 参照用プロパティ =====================================
    public int MaxHp => _maxHp;
    public int MaxAtk => _maxAtk;
    public int MaxDef => _maxDef;
    public float MaxPhysResist => _maxPhysResist;
    public float MaxEnergyResist => _maxEnergyResist;
    public float MaxCritRate => _maxCritRate;
    public float MaxCritDamage => _maxCritDamage;

    //=================================== フィールド =====================================
    [SerializeField] private GameController _gameController;
    [SerializeField] private BossDataSO _bossData;
    [SerializeField] private BossAttackHitBox[] _attackHitBoxes = new BossAttackHitBox[3];
    [SerializeField] private Transform _playerTransform;
    public Transform PlayerTransform => _playerTransform;

    // 攻撃時の対象のID
    private HashSet<int> _hitPlayer = new HashSet<int>();

    // SliderTypの容量に合わせる
    private Coroutine[] _coroutines = new Coroutine[System.Enum.GetValues(typeof(SliderType)).Length];

    private float _rotationSpeed = 2f;

    private bool _defeated = false;

    private void Awake()
    {
        //  ステータスをセット
        SetStatus();
    }

    void Start()
    {

    }

    void Update()
    {
        if (_defeated) return;

        // 回転制御
        RotateBoss();

        // 攻撃パターンを管理
        ControlAttackPattern();
    }

    //=================================== 初期処理 =====================================
    // ステータスをセット
    private void SetStatus()
    {
        int _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
        float _stageLevelMultiplier = PlayerPrefs.GetFloat("StageLevelMultiplier", 0.2f);

        int _baseHp = PlayerPrefs.GetInt("BossBaseHp", _bossData.BaseHp);   // HP
        int _baseAtk = PlayerPrefs.GetInt("BossBaseAtk", _bossData.BaseAtk);// 攻撃力
        int _baseDef = PlayerPrefs.GetInt("BossBaseDef", _bossData.BaseDef);// 防御力

        float _basePhysResist = PlayerPrefs.GetFloat("BossBasePhysResist", _bossData.BasePhysResist);      // 物理耐性
        float _baseEnergyResist = PlayerPrefs.GetFloat("BossBaseEnergyResist", _bossData.BaseEnergyResist);// エネルギー耐性
        float _baseCritRate = PlayerPrefs.GetFloat("BossBaseCritRate", _bossData.BaseCritRate);      // 会心率
        float _baseCritDamage = PlayerPrefs.GetFloat("BossBaseCritDamage", _bossData.BaseCritDamage);// 会心ダメージ

        // Max
        _maxHp = _baseHp;
        _maxAtk = _baseAtk;
        _maxDef = _baseDef;

        _maxPhysResist = _basePhysResist;    // 物理耐性
        _maxEnergyResist = _baseEnergyResist;// エネルギー耐性
        _maxCritRate = _baseCritRate;
        _maxCritDamage = _baseCritDamage;

        // 現在値
        _currentHp = _maxHp;
        _currentAtk = _maxAtk;
        _currentDef = _maxDef;
        _currentPhysResist = _maxPhysResist;
        _currentEnergyResist = _maxEnergyResist;
        _currentCritRate = _maxCritRate;
        _currentCritDamage = _maxCritDamage;

        // HPをUIに反映する
        int beforeValue = _currentHp;
        _coroutines[(int)SliderType.BossHpSlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.BossHpSlider, _currentHp, beforeValue, _maxHp));
    }

    //=================================== イベント関数 =====================================
    /// <summary>
    /// 攻撃 1
    /// </summary>
    public void Attack1()
    {
        _attackHitBoxes[0].HitBoxOn();
    }

    /// <summary>
    /// 攻撃 2
    /// </summary>
    public void Attack2()
    {
        _attackHitBoxes[1].HitBoxOn();
    }

    /// <summary>
    /// 攻撃 3
    /// </summary>
    public void Attack3()
    {
        _attackHitBoxes[2].HitBoxOn();
    }

    /// <summary>
    /// 攻撃 4
    /// </summary>
    public void Attack4()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public void AttackColliderAllOff()
    {

    }

    //=================================== ボスの回転制御 =====================================
    // 回転制御
    private void RotateBoss()
    {

    }

    //=================================== ボスの攻撃制御 =====================================
    // 攻撃パターンを管理
    private void ControlAttackPattern()
    {

    }

    //=================================== ダメージ計算処理・取得処理 =====================================
    // ダメージ計算式：基礎攻撃力 x ダメージ倍率 x (1 + 会心ダメージ)
    public int CalculateDamage(float damageMultiplier)
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

        // プレイヤーの HP量を UIに反映する（GameController に通知を送る）
        if (_coroutines[(int)SliderType.BossHpSlider] != null) StopCoroutine(_coroutines[(int)SliderType.BossHpSlider]);
        _coroutines[(int)SliderType.BossHpSlider] = StartCoroutine(_gameController.UpdateSlider(SliderType.BossHpSlider, _currentHp, beforeValue, _maxHp));

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

        // リタイア演出は GameController に通知を出す
        _gameController.GameSet(BattleResult.Lose);
    }
}
