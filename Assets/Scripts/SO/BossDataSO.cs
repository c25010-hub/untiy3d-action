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
    [SerializeField] private float _hpBaseMultiplier;
    [SerializeField] private float _atkBaseMultiplier;
    [SerializeField] private float _defBaseMultiplier;

    [Header("攻撃ダメージ倍率")]


    [Header("累乗倍率")]
    [SerializeField] private float _statusExponent; // ステータスを累乗する値

    // 外部のスクリプトからはこのプロパティを通じて「読み取り専用」で安全に取得
    // 基礎ステータス
    public int BaseHp => Mathf.RoundToInt(_baseHp * _hpBaseMultiplier);
    public int BaseAtk => Mathf.RoundToInt(_baseAtk * _atkBaseMultiplier);
    public int BaseDef => Mathf.RoundToInt(_baseDef * _defBaseMultiplier);

    // 耐性
    public float BasePhysResist => _basePhysResist;    // 物理耐性
    public float BaseEnergyResist => _baseEnergyResist;// エネルギー耐性

    // クリティカル
    public float BaseCritRate => _baseCritRate;
    public float BaseCritDamage => _baseCritDamage;

    // 攻撃ダメージ倍率


    // ステージレベル倍率
    public float StatusExponent => _statusExponent;  // ステータスを累乗する値
}
