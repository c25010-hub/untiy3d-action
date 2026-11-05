using UnityEngine;

// プロジェクトウィンドウの右クリックメニューからデータファイルを作れるようにする
[CreateAssetMenu(fileName = "NewHealData", menuName = "ScriptableObjects/HealData")]


public class HealDataSO : ScriptableObject
{
    [Header("回復量")]
    [SerializeField] private int _hpHealAmount;
    [SerializeField] private int _energyHealAmount;

    // 外部のスクリプトからはこのプロパティを通じて「読み取り専用」で安全に取得

    public int HpHealAmount => _hpHealAmount;
    public int EnergyHealAmount => _energyHealAmount;
}
