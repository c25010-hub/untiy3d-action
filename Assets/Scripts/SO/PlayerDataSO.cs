using UnityEngine;

// プロジェクトウィンドウの右クリックメニューからデータファイルを作れるようにする
[CreateAssetMenu(fileName = "NewPlayerData", menuName = "ScriptableObjects/PlayerData")]

public class PlayerDataSO : ScriptableObject
{
    [Header("基礎ステータス")]
    [SerializeField] private int _baseHp;
    [SerializeField] private int _baseAtk;
    [SerializeField] private int _baseDef;
    [SerializeField] private int _baseEnergy;
    [SerializeField] private int _baseStamina;

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
    [SerializeField] private float _firstStrikeMultiplier; // 通常攻撃一段目の倍率
    [SerializeField] private float _secondStrikeMultiplier;// 通常攻撃二段目の倍率
    [SerializeField] private float _thirdStrikeMultiplier; // 通常攻撃三段目の倍率
    [SerializeField] private float _firstShotMultiplier;   // 銃撃の倍率

    [Header("累乗倍率")]
    [SerializeField] private float _statusExponent;// ステータスを累乗する値（1.2f）

    // 外部のスクリプトからはこのプロパティを通じて「読み取り専用」で安全に取得
    // 基礎ステータス x 基礎倍率
    public int BaseHp => Mathf.RoundToInt(_baseHp * _hpBaseMultiplier);
    public int BaseAtk => Mathf.RoundToInt(_baseAtk * _atkBaseMultiplier);
    public int BaseDef => Mathf.RoundToInt(_baseDef * _defBaseMultiplier);
    public int BaseEnergy => _baseEnergy;
    public int BaseStamina => _baseStamina;

    // 耐性
    public float BasePhysResist => _basePhysResist;    // 物理耐性
    public float BaseEnergyResist => _baseEnergyResist;// エネルギー耐性

    // クリティカル
    public float BaseCritRate => _baseCritRate;
    public float BaseCritDamage => _baseCritDamage;

    // 攻撃ダメージ倍率
    public float FirstStrikeMultiplier => _firstStrikeMultiplier;
    public float SecondStrikeMultiplier => _secondStrikeMultiplier;
    public float ThirdStrikeMultiplier => _thirdStrikeMultiplier;
    public float FirstShotMultiplier => _firstShotMultiplier;

    // 累乗倍率
    public float StatusExponent => _statusExponent;  // ステータスを累乗する値
}
