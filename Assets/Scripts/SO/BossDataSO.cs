using UnityEngine;

// プロジェクトウィンドウの右クリックメニューからデータファイルを作れるようにする
[CreateAssetMenu(fileName = "NewBossData", menuName = "ScriptableObjects/BossData")]

public class BossDataSO : ScriptableObject
{
    [Header("基礎ステータス")]
    [SerializeField] private int _baseHp;
    [SerializeField] private int _baseAtk;
    [SerializeField] private int _baseDef;

    [Header("耐性")]
    [SerializeField] private float _basePhysResist;  // 物理耐性
    [SerializeField] private float _baseEnergyResist;// エネルギー耐性

    [Header("クリティカル")]
    [SerializeField] private float _baseCritRate;
    [SerializeField] private float _baseCritDamage;

    [Header("基礎倍率")]
    [SerializeField] private float _hpMultiplier;
    [SerializeField] private float _atkMultiplier;
    [SerializeField] private float _defMultiplier;

    [Header("攻撃ダメージ倍率")]

    // 外部のスクリプトからはこのプロパティを通じて「読み取り専用」で安全に取得
    public int BaseHp => _baseHp;
    public int BaseAtk => _baseAtk;
    public int BaseDef => _baseDef;

    public float BasePhysResist => _basePhysResist;    // 物理耐性
    public float BaseEnergyResist => _baseEnergyResist;// エネルギー耐性
    public float BaseCritRate => _baseCritRate;
    public float BaseCritDamage => _baseCritDamage;
    public float HpMultiplier => _hpMultiplier;
    public float AtkMultiplier => _atkMultiplier;
    public float DefMultiplier => _defMultiplier;
}
