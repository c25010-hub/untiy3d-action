using UnityEngine;

public enum BuffType
{
    AttackUp = 0,
    DefUp = 1,
    CritDamageUp = 2,
    SpeedUp = 3
}

public class BuffItem : MonoBehaviour
{
    // 参照するデータ
    [SerializeField] private BuffDataSO _buffData;
    // インスペクターでタイプを選択する
    [SerializeField] private BuffType _buffType;

    private float _buffAmount;  // バフ倍率
    private float _buffDuration;// バフ持続時間
    private int _buffStackLimit;// バフ最大スタック数

    private void Awake()
    {
        if (_buffData == null)
        {
            Debug.LogError("バフデータを反映できません。", this);
            enabled = false;
            return;
        }

        switch (_buffType)
        {
            case BuffType.AttackUp:
                _buffAmount = _buffData.AtkBuffAmount;
                _buffDuration = _buffData.AtkBuffDuration;
                _buffStackLimit = _buffData.AtkBuffStackLimit;
                break;

            case BuffType.DefUp:
                _buffAmount = _buffData.DefBuffAmount;
                _buffDuration = _buffData.DefBuffDuration;
                _buffStackLimit = _buffData.DefBuffStackLimit;
                break;

            case BuffType.CritDamageUp:
                _buffAmount = _buffData.CritDamageBuffAmount;
                _buffDuration = _buffData.CritDamageBuffDuration;
                _buffStackLimit = _buffData.CritDamageBuffStackLimit;
                break;

            case BuffType.SpeedUp:
                _buffAmount = _buffData.SpeedUpBuffAmount;
                _buffDuration = _buffData.SpeedUpBuffDuration;
                _buffStackLimit = _buffData.SpeedUpBuffStackLimit;
                break;

            default:
                Debug.LogError("未対応のバフタイプです。", this);
                enabled = false;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが触れたか判定
        if (other.CompareTag("Player"))
        {
            // プレイヤーのステータス管理コンポーネントを取得
            if (!other.TryGetComponent(out PlayerController playerController)) return;

            playerController.ApplyBuff(_buffType, _buffAmount, _buffDuration, _buffStackLimit);

            Destroy(gameObject);
        }
    }
}
