using UnityEngine;

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
    [SerializeField] private Transform _playerTransform;
    public Transform PlayerTransform => _playerTransform;

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
        _maxHp = _bossData.BaseHp;
        _baseAtk = _bossData.BaseAtk;
        _baseDef = _bossData.BaseDef;

        _basePhysResist = _bossData.BasePhysResist;    // 物理耐性
        _baseEnergyResist = _bossData.BaseEnergyResist;// エネルギー耐性
        _baseCritRate = _bossData.BaseCritRate;
        _baseCritDamage = _bossData.BaseCritDamage;

        // 現在値
        _currentHp = _maxHp;
        _currentAtk = _maxAtk;
        _currentDef = _maxDef;
        _currentPhysResist = _maxPhysResist;
        _currentEnergyResist = _maxEnergyResist;
        _currentCritRate = _maxCritRate;
        _currentCritDamage = _maxCritDamage;
    }

    //=================================== イベント関数 =====================================


    //=================================== ボスの制御関数 =====================================
    // 動作制御
    private void MoveBoss()
    {

    }

    //=================================== ダメージ計算式・取得処理 =====================================
    private int CalculateDamage(float damageMultiplier)
    {
        // 攻撃力 x 倍率 x (1 + 会心ダメージ)
        bool isCritical = Random.value <= _currentCritRate;
        float finalDamage = _currentAtk * damageMultiplier * (isCritical ? (1 + _currentCritDamage) : 1.0f);

        return (int)finalDamage;
    }

    // ダメージを受け取る
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

        // (受けたダメージ - 防御力) x (1 - 耐性%)
        float baseDamage = Mathf.Max(damage - _currentDef, 1f);
        int finalDamage = Mathf.RoundToInt(baseDamage * (1.0f - targetResist));
        finalDamage = Mathf.Max(finalDamage, 1);

        // HPの変動と死亡処理
        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);
    }

    //=================================== 負け演出 =====================================


    //=================================== 当たり判定処理 =====================================
    public void OnTriggerEnter(Collider other)
    {
        // 必須：プレイヤーに当たった場合ではなく、あくまで攻撃が当たった場合に改変しろ

        if (other.gameObject.CompareTag("Player"))
        {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

            if (damageable != null)
            {
                // ダメージを与える

            }
        }
    }
}
