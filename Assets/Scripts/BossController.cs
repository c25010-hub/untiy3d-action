using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    //=================================== ステータス (Max値) =====================================
    private int _maxHp;
    private int _maxAtk;
    private int _maxDef;

    private float _maxPhysResist;  // 物理耐性
    private float _maxEnergyResist;// エネルギー耐性

    private float _maxCritRate;  // 0.0〜1.0で管理 (0.05 = 5%)
    private float _maxCritDamage;// 0.5 = 50%

    //=================================== ステータス (現在値) =====================================
    private int _currentHp;
    private int _currentAtk;
    private int _currentDef;

    private float _currentPhysResist;  // 物理耐性
    private float _currentEnergyResist;// エネルギー耐性

    private float _currentCritRate;
    private float _currentCritDamage;

    // 参照用プロパティ

    //=================================== フィールド =====================================
    [SerializeField] private GameController _gameController;

    private void Awake()
    {
        //  データの読み込み
        LoadBossPrefs();

        // 初期ステータスの設定
        SetMaxStatus();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    //=================================== 初期処理 =====================================
    // セーブデータやマスターデータからの読み込み
    private void LoadBossPrefs()
    {
        _maxHp = PlayerPrefs.GetInt("BossHP", 1000);
        _maxAtk = PlayerPrefs.GetInt("BossATK", 1000);
        _maxDef = PlayerPrefs.GetInt("BossDEF", 1000);

        _maxPhysResist = PlayerPrefs.GetFloat("BossPhysResist", 1.0f);
        _maxEnergyResist = PlayerPrefs.GetFloat("BossEnergyResist", 1.0f);

        _maxCritRate = PlayerPrefs.GetFloat("BossCritRate", 0.05f);
        _maxCritDamage = PlayerPrefs.GetFloat("BossCritDamage", 0.5f);
    }

    // 現在値にMax値を格納
    private void SetMaxStatus()
    {
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
        // 攻撃力 x 倍率 x 会心ダメージ
        bool isCritical = Random.value <= _currentCritRate;
        float finalDamage = _currentAtk * damageMultiplier * (isCritical ? _currentCritDamage : 1.0f);

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

        // ( 受けたダメージ - 防御力 ) x ( 1 - 耐性% )
        float baseDamage = Mathf.Max(damage - _currentDef, 1f);
        int finalDamage = Mathf.RoundToInt(baseDamage * (1.0f - targetResist / 100.0f));
        finalDamage = Mathf.Max(finalDamage, 1);

        // HPの変動と死亡処理
        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);
    }

    //=================================== 負け演出 =====================================


    //=================================== 当たり判定処理 =====================================
    public void OnTriggerEnter(Collider other)
    {
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
