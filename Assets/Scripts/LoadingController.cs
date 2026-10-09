using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ロード画面の種類（シーンを読み込むか、ロード画面を片付けるか）
/// </summary>
public enum LoadState
{
    Load,
    Unload
}

/// <summary>
/// 非同期のシーン遷移と、それに伴うフェード・進捗バーの表示を制御するクラス
/// </summary>
public class LoadingController : MonoBehaviour
{
    [SerializeField] private Canvas _loadingCanvas;
    [SerializeField] private RawImage _loadingBG;
    [SerializeField] private Slider _loadingBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    // 読み込み中のシーンとその進行状況を管理するオブジェクト
    private AsyncOperation _loadingScene;

    // Loadingテキストの後ろにつくドットのアニメーション用配列
    private readonly string[] _dots = new string[4] { "", ".", "..", "..." };

    /// <summary>
    /// ロード画面に使用するUIを一度すべて非表示にする
    /// </summary>
    public void SetLoadingCanvas()
    {
        // 一度 LoadingCanvas の中身を個別で非表示
        _loadingCanvas.gameObject.SetActive(false);
        _loadingBar.gameObject.SetActive(false);
        _loadingText.gameObject.SetActive(false);

        _loadingText.text = "Loading" + _dots[3];
    }

    /// <summary>
    /// ロード画面を開く（Load）と、ロード画面を閉じる（Unload）を渡すことで処理してくれる
    /// </summary>
    public IEnumerator LoadScene(string sceneName, LoadState loadState)
    {
        ////////// 必要な要素がそろっているか確認する ////////
        if (_loadingCanvas == null)
        {
            Debug.LogError("ロード画面に必要なキャンバスがセットされていません");
            yield break;
        }
        if (_loadingBG == null)
        {
            Debug.LogError("ロード画面に必要な背景イメージがセットされていません");
            yield break;
        }
        if (_loadingBar == null)
        {
            Debug.LogError("ロード画面に必要なスライダーがセットされていません");
            yield break;
        }
        if (_loadingText == null)
        {
            Debug.LogError("ロード画面に必要なテキストがセットされていません");
            yield break;
        }

        // ロード画面を表示
        _loadingCanvas.gameObject.SetActive(true);
        _loadingBG.gameObject.SetActive(true);// ← 念のため!

        if (loadState == LoadState.Load)
        {
            _loadingBar.gameObject.SetActive(true);
            _loadingText.gameObject.SetActive(true);

            ////////// ロード画面を表示 ////////
            Color color = _loadingBG.color;
            color.a = 0f;
            _loadingBG.color = color;

            while (true)
            {
                if (_loadingBG.color.a >= 1) break;

                color = _loadingBG.color;
                color.a += Time.deltaTime * 3.0f;
                _loadingBG.color = color;

                yield return null;
            }

            // 最後に完全に不透明（1.0）にする
            color = _loadingBG.color;
            color.a = 1.0f;
            _loadingBG.color = color;

            ////////// ロード開始 ////////
            _loadingScene = SceneManager.LoadSceneAsync(sceneName);

            int count = 0;
            float textTimer = 0f;

            while (!_loadingScene.isDone)
            {
                float progress = Mathf.Clamp01(_loadingScene.progress / 0.9f);
                _loadingBar.value = progress;

                textTimer += Time.deltaTime;
                if (textTimer >= 0.5f)
                {
                    textTimer = 0f;

                    // 0, 1, 2, 3 を綺麗にループさせる
                    count = (count + 1) % 4;
                    _loadingText.text = "Loading" + _dots[count];
                }

                yield return null;
            }
        }
        else
        {
            ////////// ロード画面を非表示 ////////
            Color color = _loadingBG.color;
            color.a = 1f;
            _loadingBG.color = color;

            while (true)
            {
                if (_loadingBG.color.a <= 0) break;

                color = _loadingBG.color;
                color.a -= Time.deltaTime * 3.0f;
                _loadingBG.color = color;

                yield return null;
            }

            // 最後に完全に不透明(0.0)にする
            color = _loadingBG.color;
            color.a = 0.0f;
            _loadingBG.color = color;

            // ロード画面を非表示
            _loadingCanvas.gameObject.SetActive(false);
        }
    }
}
