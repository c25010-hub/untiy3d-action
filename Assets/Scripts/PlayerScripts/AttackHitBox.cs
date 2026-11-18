using System.Collections.Generic;
using UnityEngine;

public enum HitBoxType
{
    FirstStrikeMultiplier = 0,
    SecondStrikeMultiplier = 1,
    ThirdStrikeMultiplier = 2,
}

public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private HitBoxType _hitBoxType;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerDataSO _playerData;
    [SerializeField] private Collider _collider;
    private float _damageMultiplier;

    // UŒ‚‚Ì‘ÎÛ‚ÌID
    private HashSet<int> _hitEnemies = new HashSet<int>();

    private void Awake()
    {
        // UŒ‚”{—¦‚ğ’è‚ß‚é
        switch (_hitBoxType)
        {
            case HitBoxType.FirstStrikeMultiplier:
                _damageMultiplier = _playerData.FirstShotMultiplier;
                break;

            case HitBoxType.SecondStrikeMultiplier:
                _damageMultiplier = _playerData.SecondStrikeMultiplier;
                break;

            case HitBoxType.ThirdStrikeMultiplier:
                _damageMultiplier = _playerData.ThirdStrikeMultiplier;
                break;

            default:
                Debug.LogError("‘I‘ğ‚µ‚½”{—¦‚ª‘¶İ‚µ‚Ü‚¹‚ñ");
                break;
        }
        
        Debug.Log($"ƒ_ƒ[ƒW”{—¦F{_damageMultiplier}");
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
        if (other.transform.root.CompareTag("Enemy"))
        {
            int enemyID = other.transform.root.gameObject.GetInstanceID();

            if (_hitEnemies.Add(enemyID))
            {
                IDamageable damageable = other.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    int damage = _playerController.CalculateDamage(_damageMultiplier);
                    damageable.TakeDamage(damage, DamageType.PhysDamage);

                    // ƒGƒlƒ‹ƒM[‰ñ•œ
                    _playerController.ChargeEnergy(_hitBoxType);
                }
            }
        }
    }
}
