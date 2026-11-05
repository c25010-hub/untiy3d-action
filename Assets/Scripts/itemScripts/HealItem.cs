using UnityEngine;

public enum HealType
{
    HealHp = 0,
    HealEnergy = 1,
}

public class HealItem : MonoBehaviour
{
    // 参照するデータ
    [SerializeField] private HealDataSO _healDataSO;
    // インスペクターでタイプを選択する
    [SerializeField] private HealType _healType;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが触れたか判定
        if (other.CompareTag("Player"))
        {
            // プレイヤーのステータス管理コンポーネントを取得
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController == null) return;

            switch (_healType)
            {
                case HealType.HealHp:
                    playerController.ApplyHeal(_healType, _healDataSO.HpHealAmount);
                    break;
                case HealType.HealEnergy:
                    playerController.ApplyHeal(_healType, _healDataSO.EnergyHealAmount);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
