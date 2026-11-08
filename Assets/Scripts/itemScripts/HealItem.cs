using UnityEngine;

public enum HealType
{
    HealHp = 0,
    HealEnergy = 1,
}

public class HealItem : MonoBehaviour
{
    // 参照するデータ
    [SerializeField] private HealDataSO _healData;
    // インスペクターでタイプを選択する
    [SerializeField] private HealType _healType;

    private int _healAmount;

    private void Awake()
    {
        if (_healData == null)
        {
            Debug.LogError("バフデータを反映できません。", this);
            enabled = false;
            return;
        }

        switch (_healType)
        {
            case HealType.HealHp:
                _healAmount = _healData.HpHealAmount;
                break;

            case HealType.HealEnergy:
                _healAmount = _healData.EnergyHealAmount;
                break;

            default:
                Debug.LogError("未対応の回復タイプです。", this);
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
            if (!other.TryGetComponent<PlayerController>(out PlayerController playerController)) return;

            playerController.ApplyHeal(_healType, _healAmount);

            Destroy(gameObject);
        }
    }
}
