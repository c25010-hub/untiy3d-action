using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 外部から値を「見る」ことだけができるプロパティ（カプセル化）
    public int MaxHp => _maxHp;
    public int MaxCost => _maxCost;
    public int CurrentHp => _currentHp;
    public int CurrentAtk => _currentAtk;
    public int CurrentDef => _currentDef;
    public int CurrentPhysRes => _currentPhysRes;
    public int CurrentEnergy => _currentEnergy;
    public int CurrentStamina => _currentStamina;
    public int CurrentCost => _currentCost;

    // プレイヤーの現在のステータス上限値（現在値がこれを超えて増えることはありません）
    private int _maxHp = 1000;     // 生命力（Hit Point）
    private int _maxAtk = 1000;    // 攻撃力（Attack）
    private int _maxDef = 1000;    // 防御力（Defense）
    private int _maxPhysRes = 1000;// 物理耐性（Physical Resistance）

    private int _maxEnergy = 1000; // エネルギー（銃発砲する際に消費する）
    private int _maxStamina = 1000;// スタミナ（回避時や走る際に消費する）
    private int _maxCost = 10;     // コスト（ギミックを設置する際に消費する）

    // プレイヤーの今現在のステータス（画面上のHPバーの残りゲージなどに直結する値）
    private int _currentHp = 1000;     // 生命力
    private int _currentAtk = 1000;    // 攻撃力
    private int _currentDef = 1000;    // 防御力
    private int _currentPhysRes = 1000;// 物理耐性

    private int _currentEnergy = 1000; // エネルギー
    private int _currentStamina = 1000;// スタミナ
    private int _currentCost = 10;     // コスト

    void Start()
    {
        SetStatus();
    }

    void Update()
    {
        
    }

    // 調節したパラメーターをプレイヤーのステータスに反映させる
    private void SetStatus()
    {

    }
}
