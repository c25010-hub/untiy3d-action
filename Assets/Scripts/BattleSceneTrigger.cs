using UnityEngine;

public class BattleSceneTrigger : LoadingController
{
    private bool _active = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_active) return;

            _active = true;
            LoadScene("BattleScene", LoadState.Load);
        }
    }
}
