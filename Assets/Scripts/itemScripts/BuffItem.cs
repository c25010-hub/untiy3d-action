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
    [SerializeField] private BuffDataSO _buffDataSO;
    // インスペクターでタイプを選択する
    [SerializeField] private BuffType _buffType;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーが触れたか判定
        if (other.CompareTag("Player"))
        {
            // プレイヤーのステータス管理コンポーネントを取得
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController == null) return;

            switch (_buffType)
            {
                case BuffType.AttackUp:
                    playerController.ApplyBuff(_buffType, _buffDataSO.AtkBuffAmount, _buffDataSO.AtkBuffDuration, _buffDataSO.AtkBuffStackLimit);
                    break;
                case BuffType.DefUp:
                    playerController.ApplyBuff(_buffType, _buffDataSO.DefBuffAmount, _buffDataSO.DefBuffDuration, _buffDataSO.DefBuffStackLimit);
                    break;
                case BuffType.CritDamageUp:
                    playerController.ApplyBuff(_buffType, _buffDataSO.CritDamageBuffAmount, _buffDataSO.CritDamageBuffDuration, _buffDataSO.CritDamageBuffStackLimit);
                    break;
                case BuffType.SpeedUp:
                    playerController.ApplyBuff(_buffType, _buffDataSO.SpeedUpBuffAmount, _buffDataSO.SpeedUpBuffDuration, _buffDataSO.SpeedUpBuffStackLimit);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
