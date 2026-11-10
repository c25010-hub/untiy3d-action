using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SliderType
{
    // NOTE:配列（_slider）には、こいつと同じ順番で格納していく
    PlayerHpSlider = 0,
    PlayerEnergySlider = 1,
    PlayerStaminaSlider = 2,
    BossHpSlider = 3,
    TurretRightHpSlider = 4,
    TurretRightEnergySlider = 5,
    TurretLeftHpSlider = 6,
    TurretLeftEnergySlider = 7,
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

    private int _difficultyType;
    private int _stageLevel;

    private float _itemsRotateSpeed = 2f;
    private float _itemSpawnInterval = 30;
    private float _itemSpawnTimer;
    private float _turretReGenerateInterval = 60;

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

        // ステージに難易度を適用する
        SetBattleScene();

        // ロード画面の初期化
        SetLoadingCanvas();
    }

    void Start()
    {
        // フェードアウト
        StartCoroutine(LoadScene("", LoadState.Unload));

        // Game シーンで使う音声を読み込む
        LoadAudio();

        // マウスカーソルを非表示
        LockCursor();

        // 不要なアイテムを非表示
        HideObjects();
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
    
    // ステージに難易度を適用する
    private void SetBattleScene()
    {
        // タレットスタンドにマテリアルを適用
        if (_stageLevel > 10)
        {
            Material newMaterial = Resources.Load<Material>("Materials/Unit_Selection_v1");

            if (newMaterial != null)
            {
                for (int i = 0; i < _turretStands.Length; i++)
                {
                    if (_turretStands[i] == null) continue;

                    // マテリアルをリソースフォルダから適用
                    if (_turretStands[i].TryGetComponent<Renderer>(out var renderer))
                    {
                        renderer.material = newMaterial;
                    }
                }
            }
            else Debug.LogError("リソースフォルダからマテリアルの読み込みに失敗しました。");
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

    //=================================== UI反映処理 =====================================
    /// <summary>
    /// 状態をスライダーに反映する
    /// </summary>
    public IEnumerator UpdateSlider(SliderType sliderTyp, int currentValue, int beforeValue, int maxValue)
    {
        if (_sliders == null) yield break;
        int index = (int)sliderTyp;

        // 配列の範囲チェック
        if (index < 0 || index >= _sliders.Length) yield break;
        UnityEngine.UI.Slider slider = _sliders[index];

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

            yield return null;
        }

        slider.value = target;
    }

    //=================================== 生成処理 =====================================
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

    /// <summary>
    /// 破壊されたタレットを再生する
    /// ReGenerateTurretAsync(TurretType turretType, SliderType energySliderType)
    /// TurretRightEnergySlider = 5
    /// TurretLeftEnergySlider = 7
    /// </summary>
    public IEnumerator ReGenerateTurretAsync(TurretType turretType, SliderType energySliderType)
    {
        int index = (int)turretType;

        if (index < 0 || index >= _turretControllers.Length)
            yield break;

        float turretReGenerateTimer = 0;

        // typeでインデックスを指定する
        // 破壊されたタレット別でタイマーを走らせる
        while (turretReGenerateTimer < _turretReGenerateInterval)
        {
            turretReGenerateTimer += Time.deltaTime;
            yield return null;
        }

        // クールタイムを切ったら TurretController側にある生成用コルーチンを実行する
        if (_turretControllers[index] != null)
        {
            _sliders[(int)energySliderType].value = 0;
            StartCoroutine(_turretControllers[index].GenerateTurretAsync());
        }
    }

    //=================================== バトル開始・終了処理 =====================================
    // 勝敗を受け取って、実行する処理を判断する
    public void GameSet(BattleResult result)
    {
        _gameSet = true;

        switch (result)
        {
            // 勝利
            case BattleResult.Win: 
                // 勝利時に処理

                break;
            
            // 敗北
            case BattleResult.Lose: 
                // 敗北時の処理

                break;
        }
    }

    // ゲームセット演出
    private IEnumerator GameSetSequence()
    {
        yield return null;
    }
}
