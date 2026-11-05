using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleResult
{
    Win = 0,
    Lose = 1
}

public class GameController : LoadingController
{   //=================================== フィールド =====================================
    // BGM
    private readonly List<AudioClip> _bgm = new List<AudioClip>();
    // 効果音（Sound effects）
    private readonly List<AudioClip> _se = new List<AudioClip>();

    [SerializeField] private List<GameObject> _items = new List<GameObject>();
    [SerializeField] private Slider _playerHpSlider, _playerEnergySlider, _playerStaminaSlider, _bossHpSlider;

    private float _ElapsedTime;
    private float _playerHpValue;
    private float _playerEnergyValue;
    private float _playerStaminaValue;
    private float _bossHpValue;

    private void Awake()
    {
        // ロード画面の初期化
        SetLoadingCanvas();

        // 必要な要素がそろっているか確認する
        CheckElement();
    }

    void Start()
    {
        // フェードアウト
        StartCoroutine(LoadScene("", LoadState.Unload));

        // Game シーンで使う音声を読み込む
        LoadAudio();
        
        // マウスカーソルを非表示
        LockCursor();
    }

    void Update()
    {
        _ElapsedTime += Time.deltaTime;
    }

    //=================================== 初期処理 =====================================
    // Game シーンで使う音声を読み込む
    private void LoadAudio()
    {

    }

    // 必要な要素がそろっているか確認する
    private void CheckElement()
    {
        if (_playerHpSlider == null) Debug.LogError("_playerHpSliderがセットされていません");
        if (_playerEnergySlider == null) Debug.LogError("_playerEnergySliderがセットされていません");
        if (_playerStaminaSlider == null) Debug.LogError("_playerStaminaSliderがセットされていません");
        if (_bossHpSlider == null) Debug.LogError("_bossHpSliderがセットされていません");
    }

    // マウスカーソルを表示
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // マウスカーソルを表示
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //=================================== UI処理 =====================================
    /// <summary>
    /// プレイヤーのHP量をUIに反映する
    /// </summary>
    public IEnumerator UpdatePlayerHpUI(int currentValue, int beforeValue)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        float startValue = beforeValue;
        float targetValue = currentValue;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            _playerHpValue = Mathf.Lerp(startValue, targetValue, t);
            _playerHpSlider.value = _playerHpValue;
            yield return null;
        }

        _playerHpValue = currentValue;
        _playerHpSlider.value = _playerHpValue;
    }

    /// <summary>
    /// プレイヤーのエネルギー量をUIに反映する
    /// </summary>
    public IEnumerator UpdatePlayerEnergyUI(int currentValue, int beforeValue)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        float startValue = beforeValue;
        float targetValue = currentValue;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            _playerEnergyValue = Mathf.Lerp(startValue, targetValue, t);
            _playerEnergySlider.value = _playerEnergyValue;
            yield return null;
        }

        _playerEnergyValue = currentValue;
        _playerEnergySlider.value = _playerEnergyValue;
    }

    /// <summary>
    ///プレイヤーのスタミナ量をUIに反映する
    /// </summary>
    public IEnumerator UpdatePlayerStaminaUI(int currentValue, int beforeValue)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        float startValue = beforeValue;
        float targetValue = currentValue;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            _playerStaminaValue = Mathf.Lerp(startValue, targetValue, t);
            _playerStaminaSlider.value = _playerStaminaValue;
            yield return null;
        }

        _playerStaminaValue = currentValue;
        _playerStaminaSlider.value = _playerStaminaValue;
    }

    /// <summary>
    /// ボスのHP量をUIに反映する
    /// </summary>
    public IEnumerator UpdateBossHpUI(int currentValue, int beforeValue)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        float startValue = beforeValue;
        float targetValue = currentValue;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            _bossHpValue = Mathf.Lerp(startValue, targetValue, t);
            _bossHpSlider.value = _bossHpValue;
            yield return null;
        }

        _bossHpValue = currentValue;
        _bossHpSlider.value = _bossHpValue;
    }

    //=================================== 生成処理 =====================================
    // アイテム生成
    private void GenerateItem()
    {

    }

    // タレット生成
    private void GenerateTurret()
    {

    }

    //=================================== バトル開始・終了処理 =====================================
    // 
    public void GameSet(BattleResult result)
    {
        switch (result)
        {
            // 勝利
            case BattleResult.Win: break;
            
            // 敗北
            case BattleResult.Lose: break;
        }
    }

}
