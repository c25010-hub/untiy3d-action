using UnityEngine;

public class BossController : MonoBehaviour
{
    // ここにインスペクターから上記のデータファイルをアタッチする
    [SerializeField] private BossParameterSO _bossParameter;

    // 外部から値を「見る」ことだけができるプロパティ（カプセル化）
    public int MaxHp => _maxHp;
    public int CurrentHp => _currentHp;
    public int CurrentAtk => _currentAtk;
    public int CurrentDef => _currentDef;
    public int CurrentPhysRes => _currentPhysRes;

    // ボスの現在のステータス上限値（現在値がこれを超えて増えることはありません）
    private int _maxHp = 1000;     // 生命力（Hit Point）
    private int _maxAtk = 1000;    // 攻撃力（Attack）
    private int _maxDef = 1000;    // 防御力（Defense）
    private int _maxPhysRes = 1000;// 物理耐性（Physical Resistance）

    // ボスの今現在のステータス（画面上のHPバーの残りゲージなどに直結する値）
    private int _currentHp = 1000;     // 生命力
    private int _currentAtk = 1000;    // 攻撃力
    private int _currentDef = 1000;    // 防御力
    private int _currentPhysRes = 1000;// 物理耐性

    void Start()
    {
        SetStatus();
    }

    void Update()
    {

    }

    // 調節したパラメーターをボスのステータスに反映させる
    private void SetStatus()
    {
        if (_bossParameter == null)
        {
            Debug.LogError("スクリプタブルオブジェクト（BossParameter）がセットされていません");
            return;
        }

        // 上限値の同期
        _maxHp = _bossParameter.baseHp;
        _maxAtk = _bossParameter.baseAtk;
        _maxDef = _bossParameter.baseDef;
        _maxPhysRes = _bossParameter.basePhysRes;

        // 現在値の初期化
        _currentHp = _maxHp;
        _currentAtk = _maxAtk;
        _currentDef = _maxDef;
        _currentPhysRes = _maxPhysRes;

        // ログを1行にまとめて可読性を向上
        Debug.Log($"[Boss生成] HP:{_currentHp}, ATK:{_currentAtk}, DEF:{_currentDef}, RES:{_currentPhysRes}");
    }
}
