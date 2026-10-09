using System.Collections.Generic;
using UnityEngine;

public class HomeController : LoadingController
{
    // BGM
    private readonly List<AudioClip> _bgm = new List<AudioClip>();
    // 効果音（Sound effects）
    private readonly List<AudioClip> _se = new List<AudioClip>();

    private void Awake()
    {
        // ロード画面の初期化
        SetLoadingCanvas();

        // フェードアウト
        StartCoroutine(LoadScene("", LoadState.Unload));
    }

    void Start()
    {
        // Homeシーンで使う音声を読み込む
        LoadAudio();
    }

    void Update()
    {
        
    }

    //=================================== 読み込み処理 =====================================

    // Homeシーンで使う音声を読み込む

    private void LoadAudio()
    {

    }
}
