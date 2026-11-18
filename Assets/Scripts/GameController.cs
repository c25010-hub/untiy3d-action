using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SliderType
{
    // NOTE:配列（_slider）には、こいつと同じ順番で格納していく
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

    [SerializeField] private Transform _itemsParent;
    [SerializeField] private List<GameObject> _items = new List<GameObject>();// プレファブ化されたアイテム
    [SerializeField] private GameObject[] _turretStands = new GameObject[System.Enum.GetValues(typeof(TurretType)).Length];
    [SerializeField] private TurretController[] _turretControllers = new TurretController[System.Enum.GetValues(typeof(TurretType)).Length];
    [SerializeField] private Slider[] _sliders = new Slider[System.Enum.GetValues(typeof(SliderType)).Length];
    [SerializeField] private Image _staminaBackground;
    [SerializeField] private Image _staminaFillArea;
    [SerializeField] private TextMeshProUGUI[] _sliderValueTexts = new TextMeshProUGUI[System.Enum.GetValues(typeof(SliderType)).Length];

    private int _difficultyType;
    private int _stageLevel;

    private float _itemsRotateSpeed = 2f;
    private float _itemSpawnInterval = 30;
    private float _itemSpawnTimer;

    // アイテム生成可能範囲
    [SerializeField] private Transform _minSpawnPosX;
    [SerializeField] private Transform _maxSpawnPosX;
    [SerializeField] private Transform _minSpawnPosZ;
    [SerializeField] private Transform _maxSpawnPosZ;

    private bool _gameSet = false;

    private void Awake()
    {
        _difficultyType = PlayerPrefs.GetInt("DifficultyType", 0);
        _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);
    }

    void Start()
    {
        // ステージに難易度を適用する
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

    // ステージの状況を選択した項目に合わせる
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

    // 不要なアイテムを非表示
    private void HideObjects()
    {
        // アイテム生成の制限区域
        if (_minSpawnPosX != null) _minSpawnPosX.gameObject.SetActive(false);
        if (_maxSpawnPosX != null) _maxSpawnPosX.gameObject.SetActive(false);
        if (_minSpawnPosZ != null) _minSpawnPosZ.gameObject.SetActive(false);
        if (_maxSpawnPosZ != null) _maxSpawnPosZ.gameObject.SetActive(false);

        // タレット
        foreach (var controller in _turretControllers)
        {
            if (controller == null) continue;
            controller.TurretSetFalse();
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

        // 配列の範囲チェック
        if (index < 0 || index >= _sliders.Length) yield break;
        Slider slider = _sliders[index];
        TextMeshProUGUI valueText = _sliderValueTexts[index];

        // Sliderと最大値のチェック
        if (!slider || maxValue <= 0) yield break;

        float duration = 0.5f;
        float elapsedTime = 0f;

        float start = Mathf.Clamp01((float)beforeValue / maxValue);
        float target = Mathf.Clamp01((float)currentValue / maxValue);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            slider.value = Mathf.SmoothStep(start, target, t);
            valueText.SetText("{0:0}%", slider.value * 100f);

            yield return null;
        }

        slider.value = target;
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
        if (_gameSet == true) return;

        _itemSpawnTimer += Time.deltaTime;
        if (_itemSpawnTimer < _itemSpawnInterval) return;
        
        _itemSpawnTimer = 0;

        if (_items == null || _items.Count == 0) return;

        int itemIndex = Random.Range(0, _items.Count);
        GameObject originalItem = _items[itemIndex];

        if (originalItem == null)
        {
            Debug.LogError("生成するアイテムが設定されていません。", this);
            return;
        }

        float minX = _minSpawnPosX.position.x;
        float maxX = _maxSpawnPosX.position.x;

        float minZ = _minSpawnPosZ.position.z;
        float maxZ = _maxSpawnPosZ.position.z;

        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        float spawnY = originalItem.transform.localScale.y / 1.5f;

        Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

        // アイテムを生成
        GameObject item = Instantiate(originalItem, spawnPosition, Quaternion.identity, _itemsParent);

        StartCoroutine(ItemRotateLoop(item));
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
