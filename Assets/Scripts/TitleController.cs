using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : LoadingController
{
    [SerializeField] private Button _startButton;
    [SerializeField] private TextMeshProUGUI _startButtonText;

    // BGM
    private readonly List<AudioClip> _bgm = new List<AudioClip>();
    // 効果音（Sound effects）
    private readonly List<AudioClip> _se = new List<AudioClip>();

    private readonly float _startButtonFadeSpeed = 2.0f;

    private void Awake()
    {
        // ロード画面の初期化
        SetLoadingCanvas();
    }

    void Start()
    {
        // Titleシーンで使う音声を読み込む
        LoadAudio();

        // スタートボタンを点滅させる
        StartCoroutine(StartButtonFadeLoopRoutine());
    }

    void Update()
    {
        
    }

    //=================================== 読み込み処理 =====================================

    // Titleシーンで使う音声を読み込む
    private void LoadAudio()
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
            float alpha = (Mathf.Sin(Time.time * _startButtonFadeSpeed) + 1.0f) * 0.25f + 0.5f;

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

    /// <summary>
    /// ゲームスタート
    /// </summary>
    public void GameStart()
    {
        // フェードイン
        StartCoroutine(LoadScene("HomeScene", LoadState.Load));
    }
}
