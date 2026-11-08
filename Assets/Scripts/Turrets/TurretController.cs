using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour, IDamageable
{
    // NOTE: タレットのパラメーターはボスのステータスを参照する
    //=================================== ステータス (Max値) =====================================
    private int _maxHp;
    private int _maxAtk;
    private int _maxDef;

    private float _maxPhysResist;  // 物理耐性
    private float _maxEnergyResist;// エネルギー耐性

    private float _maxCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _maxCritDamage;// 0.5 = 50%

    //=================================== ステータス (現在値) =====================================
    private int _currentHp;
    private int _currentAtk;
    private int _currentDef;
    
    private float _currentPhysResist;  // 物理耐性
    private float _currentEnergyResist;// エネルギー耐性

    private float _currentCritRate;  // 0.0～1.0で管理 (0.05 = 5%)
    private float _currentCritDamage;// 0.5 = 50%

    //=================================== フィールド =====================================
    [SerializeField] private BossController _bossController;
    [SerializeField] private BossDataSO _bossData;

    // インスペクターから入力
    [SerializeField] private float _rotationSpeed;// 回転速度

    // ダメージ倍率
    private float _damageMultiplier;
    // 累乗倍率
    private float _bossStatusExponent; // ステータスを累乗する値

    private Transform _playerTransform;

    void Start()
    {
        // ボスのステータスを適用
        SetStatus();
    }

    void Update()
    {
        // プレイヤーを追従させる
        MoveTurret();
    }

    //=================================== 初期処理 =====================================
    // ボスのステータスを適用
    private void SetStatus()
    {
        int _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
        float _stageLevelMultiplier = PlayerPrefs.GetFloat("StageLevelMultiplier", 0.2f);

        int _baseHp = _bossData.BaseHp / 2;
        int _baseAtk = _bossData.BaseAtk / 2;
        int _baseDef = _bossData.BaseDef / 2;

        float _basePhysResist = _bossData.BasePhysResist / 2;    // 物理耐性
        float _baseEnergyResist = _bossData.BaseEnergyResist / 2;// エネルギー耐性
        float _baseCritRate = _bossData.BaseCritRate / 2;
        float _baseCritDamage = _bossData.BaseCritDamage / 2;

        // 累乗倍率
        float _bossStatusExponent = _bossData.StatusExponent; // ステータスを累乗する値

        // Max値： 基礎ステータス = Bossステータス / 2 x (ステージレベル x 0.2) 0.6乗
        _maxHp = Mathf.RoundToInt(_baseHp * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent));
        _maxAtk = Mathf.RoundToInt(_baseAtk * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent));
        _maxDef = Mathf.RoundToInt(_baseDef * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent));
        _maxPhysResist = _basePhysResist;
        _maxEnergyResist = _baseEnergyResist;
        _maxCritRate = Mathf.Min(_baseCritRate, 1.0f);
        // 会心ダメージ = 基礎倍率 + (ステージレベル x 0.2) 0.6乗
        _maxCritDamage = _baseCritDamage * Mathf.Pow(_stageLevel * _stageLevelMultiplier, _bossStatusExponent);

        // 現在値
        _currentHp = _maxHp;
        _currentAtk = _maxAtk;
        _currentDef = _maxDef;
        _currentPhysResist = _maxPhysResist;
        _currentEnergyResist = _maxEnergyResist;
        _currentCritRate = _maxCritRate;
        _currentCritDamage = _maxCritDamage;
    }

    //=================================== 動作制御 =====================================
    // プレイヤーを追従させる
    private void MoveTurret()
    {
        if (_playerTransform != null)
        {
            Vector3 direction = _playerTransform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                // 目標とする回転（クォータニオン）を計算
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // 現在の回転から目標の回転へ、時間経過に合わせて滑らかに補間
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    // 攻撃
    private void Attack()
    {

    }

    //=================================== ダメージ計算式・取得処理 =====================================
    // ダメージ計算式
    private int CalculateDamage(float damageMultiplier)
    {
        bool isCritical = UnityEngine.Random.value <= _currentCritRate;
        float finalDamage = _currentAtk * damageMultiplier * (isCritical ? (1 + _currentCritDamage) : 1.0f);
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

        // (受けたダメージ - 防御力) x (1 - 耐性%)
        float baseDamage = Mathf.Max(damage - _currentDef, 1f);
        int finalDamage = Mathf.RoundToInt(baseDamage * (1.0f - targetResist));
        finalDamage = Mathf.Max(finalDamage, 1);

        int beforeValue = _currentHp;

        // HPの変動と死亡処理
        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);

        // プレイヤーの HP量を UIに反映する（GameController に通知を送る）

        if (_currentHp == 0)
        {
            // 破壊する
            BreakTurret();
        }
    }

    //=================================== 破壊処理 =====================================
    private void BreakTurret()
    {
        this.gameObject.SetActive(false);

        // エフェクト実行
    }

    //=================================== アニメーション処理 =====================================
    /// <summary>
    /// タレット生成時のアニメーション処理（実行は GameController側で行う！）
    /// </summary>
    public IEnumerator GenerateTurretAsync()
    {
        this.gameObject.SetActive(true);



        yield return null;
    }
}
