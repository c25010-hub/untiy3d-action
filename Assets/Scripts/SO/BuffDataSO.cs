using UnityEngine;

// プロジェクトウィンドウの右クリックメニューからデータファイルを作れるようにする
[CreateAssetMenu(fileName = "NewBuffData", menuName = "ScriptableObjects/BuffData")]

public class BuffDataSO : ScriptableObject
{
    // NOTE: 倍率は 1 → 0.01, 10 → 0.1 とする
    [Header("バフ倍率")]
    [SerializeField] private float _atkBuffAmount;
    [SerializeField] private float _defBuffAmount;
    [SerializeField] private float _critDamageBuffAmount;
    [SerializeField] private float _speedUpBuffAmount;

    [Header("バフ持続時間")]
    [SerializeField] private float _atkBuffDuration;
    [SerializeField] private float _defBuffDuration;
    [SerializeField] private float _critDamageBuffDuration;
    [SerializeField] private float _speedUpBuffDuration;

    [Header("スタック上限")]
    [SerializeField] private int _atkBuffStackLimit;
    [SerializeField] private int _defBuffStackLimit;
    [SerializeField] private int _critDamageBuffStackLimit;
    [SerializeField] private int _speedUpBuffStackLimit;

    // 外部のスクリプトからはこのプロパティを通じて「読み取り専用」で安全に取得

    public float AtkBuffAmount => _atkBuffAmount;
    public float DefBuffAmount => _defBuffAmount;
    public float CritDamageBuffAmount => _critDamageBuffAmount;
    public float SpeedUpBuffAmount => _speedUpBuffAmount;
    public float AtkBuffDuration => _atkBuffDuration;
    public float DefBuffDuration => _defBuffDuration;
    public float CritDamageBuffDuration => _critDamageBuffDuration;
    public float SpeedUpBuffDuration => _speedUpBuffDuration;

    public int AtkBuffStackLimit => _atkBuffStackLimit;
    public int DefBuffStackLimit => _defBuffStackLimit;
    public int CritDamageBuffStackLimit => _critDamageBuffStackLimit;
    public int SpeedUpBuffStackLimit => _speedUpBuffStackLimit;
}
