using System.Collections.Generic;
using UnityEngine;

public enum BossHitBoxType
{
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2
}

public class BossAttackHitBox : MonoBehaviour
{
    [SerializeField] private BossHitBoxType _hitBoxType;
    [SerializeField] private BossController _bossController;
    [SerializeField] private BossDataSO _bossData;
    [SerializeField] private Collider _collider;
    private float _damageMultiplier;

    // çUåÇéûÇÃëŒè€ÇÃID
    private HashSet<int> _hitEnemies = new HashSet<int>();

    private void Awake()
    {
        // çUåÇî{ó¶ÇíËÇﬂÇÈ
        switch (_hitBoxType)
        {
            case BossHitBoxType.Attack1:
                _damageMultiplier = _bossData.AttackMultiplier1;
                break;

            case BossHitBoxType.Attack2:
                _damageMultiplier = _bossData.AttackMultiplier2;
                break;

            case BossHitBoxType.Attack3:
                _damageMultiplier = _bossData.AttackMultiplier3;
                break;

            default:
                Debug.LogError("ëIëÇµÇΩî{ó¶Ç™ë∂ç›ÇµÇ‹ÇπÇÒ");
                break;
        }

        Debug.Log($"É_ÉÅÅ[ÉWî{ó¶ÅF{_damageMultiplier}");
        HitBoxOff();
    }

    public void HitBoxOn()
    {
        _hitEnemies.Clear();
        _collider.enabled = true;
    }

    public void HitBoxOff()
    {
        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            int enemyID = other.transform.root.gameObject.GetInstanceID();

            if (_hitEnemies.Add(enemyID))
            {
                IDamageable damageable = other.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    int damage = _bossController.CalculateDamage(_damageMultiplier);
                    damageable.TakeDamage(damage, DamageType.PhysDamage);
                }
            }
        }
    }
}
