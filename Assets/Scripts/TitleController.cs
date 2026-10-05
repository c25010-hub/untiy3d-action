using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TitleController : SoundController
{
    [SerializeField] private Button _startButton;
    [SerializeField] private TextMeshProUGUI _startButtonText;

    private readonly float _startButtonFadeSpeed = 2.0f;

    private void Awake()
    {
        // Titleシーンの初期化
        SetTitleSceneCondition();

        // ロード画面の初期化
        SetLoadingCanvas();
    }

    void Start()
    {
        // スタートボタンを点滅させる
        StartCoroutine(StartButtonFadeLoopRoutine());
    }

    void Update()
    {
        
    }

    // Titleシーンの状態を整える
    private void SetTitleSceneCondition()
    {

    }

    // スタートボタンを点滅させる
    private IEnumerator StartButtonFadeLoopRoutine()
    {
        if (_startButton == null)
        {
            Debug.LogError("スタートボタンがセットされていません");
            yield break;
        }
        if (_startButtonText == null)
        {
            Debug.LogError("スタートボタンのテキストがセットされていません");
            yield break;
        }

        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * _startButtonFadeSpeed) + 1.0f) * 0.5f;

            Color buttonColor = _startButton.image.color;
            Color textColor = _startButtonText.color;

            buttonColor.a = alpha;
            textColor.a = alpha;

            // 計算した透明度をボタンに適用
            _startButton.image.color = buttonColor;
            _startButtonText.color = textColor;

            // 1フレーム待機してループを繰り返す
            yield return null;
        }
    }

    //=================================== ボタンインターフェース =====================================

    // ゲームスタート
    public void GameStart()
    {
        StartCoroutine(LoadScene("HomeScene", LoadState.Load));
    }
}
