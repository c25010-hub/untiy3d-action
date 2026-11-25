using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;

    // ”ÍˆÍ“à‚É‚¢‚é“G‚ğŠÇ—
    private HashSet<Transform> _enemies = new HashSet<Transform>();
    private void Update()
    {
        // Destroy‚³‚ê‚½“G‚ğ“o˜^ƒŠƒXƒg‚©‚çíœ
        _enemies.RemoveWhere(enemy => enemy == null);
    }

    // Å‚à‹ß‚¢“G‚Ì•ûŒü‚ğæ“¾
    public bool TryGetNearestEnemy(out Vector3 direction)
    {
        direction = Vector3.zero;

        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Transform enemy in _enemies)
        {
            // ”O‚Ì‚½‚ßnullƒ`ƒFƒbƒN
            if (enemy == null) continue;

            Vector3 offset = enemy.position - _playerController.transform.position;

            // …•½•ûŒü‚¾‚¯‚ğ‘ÎÛ‚É‚·‚é
            offset.y = 0f;

            float distance = offset.sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null) return false;

        direction = nearestEnemy.position - _playerController.transform.position;
        direction.y = 0f;

        return direction.sqrMagnitude > 0.0001f;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform enemy = other.transform.root;

        if (enemy.CompareTag("Enemy"))
        {
            // “G‚ğ“o˜^
            _enemies.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform enemy = other.transform.root;

        if (enemy.CompareTag("Enemy"))
        {
            // ”ÍˆÍŠO‚Éo‚½“G‚ğ“o˜^‰ğœ
            _enemies.Remove(enemy);
        }
    }


}
