using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SliderType
{
    PlayerHpSlider = 0,
    PlayerEnergySlider = 1,
    BossHpSlider = 2,
    TurretLeftHpSlider = 3,
    TurretLeftEnergySlider = 4,
    TurretRightHpSlider = 5,
    TurretRightEnergySlider = 6,
}

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

    [SerializeField] private Transform _buffIconsParent;
    // NOTE: BuffTypeと同じ順番で格納しろ！
    [SerializeField] private GameObject[] _buffIcons = new GameObject[System.Enum.GetValues(typeof(BuffType)).Length];
    [SerializeField] private Transform _itemsParent;
    [SerializeField] private List<GameObject> _items = new List<GameObject>();// プレファブ化されたアイテム
    [SerializeField] private Transform[] _circleCenters = new Transform[3];   // アイテム生成可能範囲
    [SerializeField] private TurretController[] _turretControllers = new TurretController[System.Enum.GetValues(typeof(TurretType)).Length];
    // NOTE: SliderTypeと同じ順番で格納しろ！
    [SerializeField] private Slider[] _sliders = new Slider[System.Enum.GetValues(typeof(SliderType)).Length];
    [SerializeField] private Slider[] _trailSliders = new Slider[System.Enum.GetValues(typeof(SliderType)).Length];
    //[SerializeField] private Image[] _glowImages = new Image[System.Enum.GetValues(typeof(SliderType)).Length];
    [SerializeField] private Image _staminaBackground;
    [SerializeField] private Image _staminaFillArea;
    [SerializeField] private TextMeshProUGUI[] _sliderValueTexts = new TextMeshProUGUI[System.Enum.GetValues(typeof(SliderType)).Length];

    private GameObject[] _iconInstance = new GameObject[System.Enum.GetValues(typeof(BuffType)).Length];
    private TextMeshProUGUI[] _buffIStackText = new TextMeshProUGUI[System.Enum.GetValues(typeof(BuffType)).Length];
    private int _stageLevel;

    private float _itemsRotateSpeed = 20f;
    private float _itemSpawnInterval = 10;
    private float _itemSpawnTimer;
    private float _spawnRadius = 0.7f;// 円の半径

    private bool _gameSet = false;

    private void Awake()
    {
        _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
    }

    void Start()
    {
        // ステージの状態を選択した難易度に合わせる
        SetBattleScene();

        // ロード画面の初期化
        SetLoadingCanvas();

        // フェードアウト
        StartCoroutine(LoadScene("", LoadState.Unload));

        // Game シーンで使う音声を読み込む
        LoadAudio();

        // マウスカーソルを非表示
        LockCursor();

        // 不要なアイテムを非表示
        HideObjects();

        // ゲーム開始演出
        StartCoroutine(GameStart());
    }

    void Update()
    {
        // アイテム生成
        GenerateItem();
    }

    //=================================== 初期処理 =====================================
    // Game シーンで使う音声を読み込む
    private void LoadAudio()
    {

    }

    // ステージの状態を選択した難易度に合わせる
    private void SetBattleScene()
    {
        switch (_stageLevel)
        {
            case 10:
                Debug.Log($"ステージLv：{_stageLevel}");
                ; break;

            case 50:
                Debug.Log($"ステージLv：{_stageLevel}");
                ; break;

            case 100:
                Debug.Log($"ステージLv：{_stageLevel}");
                ; break;
            default:
                Debug.LogError("選択した難易度が適用しません");
                break;
        }
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

    // 不要な用途を非表示
    private void HideObjects()
    {
        // タレット
        foreach (var controller in _turretControllers)
        {
            if (controller == null) continue;
            controller.TurretSetFalse();
        }

        // アイテム生成の基準点
        foreach (var item in _circleCenters)
        {
            if (item == null) continue;
            item.gameObject.SetActive(false);
        }
    }

    //=================================== スライダー処理・反映 =====================================
    /// <summary>
    /// 状態をスライダーに反映する
    /// </summary>
    public IEnumerator UpdateSlider(SliderType sliderTyp, int currentValue, int beforeValue, int maxValue)
    {
        if (_sliders == null) yield break;

        int index = (int)sliderTyp;

        if (index < 0 || index >= _sliders.Length) yield break;

        Slider slider = _sliders[index];
        TextMeshProUGUI valueText = _sliderValueTexts[index];

        if (!slider || maxValue <= 0) yield break;

        // HPゲージかどうか
        bool isHpSlider =
            sliderTyp == SliderType.PlayerHpSlider ||
            sliderTyp == SliderType.BossHpSlider ||
            sliderTyp == SliderType.TurretLeftHpSlider ||
            sliderTyp == SliderType.TurretRightHpSlider;

        float duration = 0.5f;
        float elapsedTime = 0f;

        float start = Mathf.Clamp01((float)beforeValue / maxValue);
        float target = Mathf.Clamp01((float)currentValue / maxValue);

        bool isDamage = currentValue < beforeValue;

        // HP回復時は遅延ゲージも一緒に動かす
        if (isHpSlider && !isDamage)
        {
            _trailSliders[index].value = start;
        }

        // 赤ゲージの減少
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            float value = Mathf.SmoothStep(start, target, t);

            slider.value = value;

            // ダメージ以外は遅延ゲージも同時に更新
            if (!isDamage)
            {
                _trailSliders[index].value = value;
            }

            //// 発光エフェクト
            //if (isDamage)
            //{
            //    Color color = _glowImages[index].color;
            //    color.a = Mathf.Sin(t * Mathf.PI) * 0.9f;
            //    _glowImages[index].color = color;
            //}

            switch (sliderTyp)
            {
                case SliderType.PlayerHpSlider:
                case SliderType.TurretLeftHpSlider:
                case SliderType.TurretRightHpSlider:
                    valueText.SetText("{0:0}", value * maxValue);
                    break;

                case SliderType.BossHpSlider:
                    valueText.SetText("");
                    break;

                default:
                    valueText.SetText("{0:0}%", value * 100f);
                    break;
            }

            yield return null;
        }

        slider.value = target;

        // ダメージを受けた場合
        if (isDamage)
        {
            // 0.1秒待機
            yield return new WaitForSeconds(0.1f);

            // 遅延ゲージを減少させる
            float trailStart = _trailSliders[index].value;
            elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime * 2;

                float t = elapsedTime / duration;

                _trailSliders[index].value =
                    Mathf.SmoothStep(trailStart, target, t);

                yield return null;
            }

            _trailSliders[index].value = target;
        }

        //if (isDamage)
        //{
        //    // 発光を終了
        //    Color color = _glowImages[index].color;
        //    color.a = 0f;
        //    _glowImages[index].color = color;
        //}
    }

    /// <summary>
    /// プレイヤーのスタミナに反映する
    /// </summary>
    public IEnumerator UpdatePlayerStamina(int currentValue, int beforeValue, int maxValue)
    {
        if (_staminaFillArea == null) yield break;

        float duration = 0.5f;
        float elapsedTime = 0f;

        float start = Mathf.Clamp01((float)beforeValue / maxValue);
        float target = Mathf.Clamp01((float)currentValue / maxValue);

        // スタミナを徐々に増減させる
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            _staminaFillArea.fillAmount = Mathf.SmoothStep(start, target, t);
            yield return null;
        }

        _staminaFillArea.fillAmount = target;
    }

    /// <summary>
    /// スライダーのvalueを1にする
    /// </summary>
    public void SliderToOnePercent(SliderType sliderTyp, int maxValue)
    {
        if (_sliders == null) return;
        int index = (int)sliderTyp;

        // 配列の範囲チェック
        if (index < 0 || index >= _sliders.Length) return;
        Slider slider = _sliders[index];

        slider.value = 1 / maxValue;
    }

    //=================================== アイテム生成処理 =====================================
    // アイテム生成
    private void GenerateItem()
    {
        if (_gameSet) return;

        _itemSpawnTimer += Time.deltaTime;

        if (_itemSpawnTimer < _itemSpawnInterval) return;
        _itemSpawnTimer = 0;

        int _itemIndex = Random.Range(0, _items.Count);
        int centerIndex = Random.Range(0, _circleCenters.Length);

        Vector3 _itemSize = _items[_itemIndex].transform.localScale;
        Vector3 center = _circleCenters[centerIndex].position;
        Vector2 randomPosition = Random.insideUnitCircle * _spawnRadius;

        Vector3 spawnPosition = new Vector3(
            center.x + randomPosition.x,
            0,
            center.z + randomPosition.y
            );

        StartCoroutine(ItemRotateLoop(Instantiate(_items[_itemIndex], spawnPosition, Quaternion.identity, _itemsParent)));
    }

    // 回転処理
    private IEnumerator ItemRotateLoop(GameObject item)
    {
        while (item != null)
        {
            item.transform.Rotate(Vector3.up, _itemsRotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// バフアイコンを表示
    /// </summary>
    public void DisplayBuffIcon(BuffType type, int stack)
    {
        int index = (int)type;

        if (_iconInstance[index] == null)
        {
            _iconInstance[index] = Instantiate(_buffIcons[index], _buffIconsParent);
            _buffIStackText[index] = _iconInstance[index].GetComponentInChildren<TextMeshProUGUI>();
        }

        _buffIStackText[index].SetText("x" + stack);
    }

    /// <summary>
    /// バフの効果切れをUIに反映
    /// </summary>
    public void BuffExpired(BuffType type, int stack)
    {
        int index = (int)type;
        TextMeshProUGUI stackText = _buffIStackText[index];

        if (stack < 1)
        {
            Destroy(_iconInstance[index]);

            _iconInstance[index] = null;
            _buffIStackText[index] = null;
        }
        else
        {
            _buffIStackText[index].SetText("x" + stack);
        }
    }

    //=================================== バトル開始・終了処理 =====================================
    // 開始演出
    public IEnumerator GameStart()
    {
        // タレットを両方生成
        if (_stageLevel > 10)
        {
            foreach (var turretController in _turretControllers)
            {
                StartCoroutine(turretController.GenerateTurretAsync());
            }
        }

        yield return null;
    }
    
    // 勝敗を受け取って、実行する処理を判断する
    public void GameSet(BattleResult result)
    {
        _gameSet = true;

        switch (result)
        {
            // 勝利
            case BattleResult.Win:
                Debug.Log("勝利！");

                break;
            
            // 敗北
            case BattleResult.Lose:
                Debug.Log("敗北...");

                break;
        }
    }

    // ゲームセット演出
    private IEnumerator GameSetSequence()
    {
        yield return null;
    }
}
