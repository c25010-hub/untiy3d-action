using UnityEngine;

// プロジェクトウィンドウの右クリックメニューからデータファイルを作れるようにする
[CreateAssetMenu(fileName = "NewPlayerParameter", menuName = "ScriptableObjects/PlayerParameter")]

public class PlayerParameterSO : ScriptableObject
{
    [Header("基礎ステータス")]
    public int baseHp = 1000;
    public int baseAtk = 1000;
    public int baseDef = 1000;
    public int basePhysRes = 1000;

    [Header("クリティカル")]
    public float critRate = 0.05f;
    public float critDamage = 0.5f;
}
