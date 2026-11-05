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
    [SerializeField] private int _baseCost;

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
    [SerializeField] private float _firstStrikeMultiplier; // 通常攻撃一段目の倍率
    [SerializeField] private float _secondStrikeMultiplier;// 通常攻撃二段目の倍率
    [SerializeField] private float _thirdStrikeMultiplier; // 通常攻撃三段目の倍率
    [SerializeField] private float _firstShotMultiplier;   // 銃撃の倍率

    // 外部のスクリプトからはこのプロパティを通じて「読み取り専用」で安全に取得
    public int BaseHp => _baseHp;
    public int BaseAtk => _baseAtk;
    public int BaseDef => _baseDef;
    public int BaseEnergy => _baseEnergy;
    public int BaseStamina => _baseStamina;
    public int BaseCost => _baseCost;

    public float BasePhysResist => _basePhysResist;    // 物理耐性
    public float BaseEnergyResist => _baseEnergyResist;// エネルギー耐性
    public float BaseCritRate => _baseCritRate;
    public float BaseCritDamage => _baseCritDamage;
    public float HpMultiplier => _hpMultiplier;
    public float AtkMultiplier => _atkMultiplier;
    public float DefMultiplier => _defMultiplier;

    // NOTE: PlayerController で直接受け取っている
    public float FirstStrikeMultiplier => _firstStrikeMultiplier;
    public float SecondStrikeMultiplier => _secondStrikeMultiplier;
    public float ThirdStrikeMultiplier => _thirdStrikeMultiplier;
    public float FirstShotMultiplier => _firstShotMultiplier;
}
