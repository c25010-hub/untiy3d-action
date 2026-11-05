using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterType
{
    Player = 0,
    Boss = 1
}

public class HomeController : LoadingController
{
    //=================================== プレイヤーのステータス =====================================
    // 基礎ステータス
    private int _playerHp;
    private int _playerAtk;
    private int _playerDef;
    private int _playerEnergy;
    private int _playerStamina;

    // 耐性
    private float _playerPhysResist;  // 物理耐性
    private float _playerEnergyResist;// エネルギー耐性

    // クリティカル
    private float _playerCritRate;
    private float _playerCritDamage;

    // 計算結果

    private int _playerResultHp;
    private int _playerResultAtk;
    private int _playerResultDef;
    private int _playerResultEnergy;
    private int _playerResultStamina;

    private float _playerResultPhysResist;
    private float _playerResultEnergyResist;

    private float _playerResultCritRate;
    private float _playerResultCritDamage;

    //=================================== ボスのステータス =====================================
    // 基礎ステータス
    private int _bossHp;
    private int _bossAtk;
    private int _bossDef;

    // 耐性
    private float _bossPhysResist;  // 物理耐性
    private float _bossEnergyResist;// エネルギー耐性

    // クリティカル
    private float _bossCritRate;
    private float _bossCritDamage;

    // 計算結果
    private int _bossResultHp;
    private int _bossResultAtk;
    private int _bossResultDef;

    private float _bossResultPhysResist;
    private float _bossResultEnergyResist;

    private float _bossResultCritRate;
    private float _bossResultCritDamage;

    //=================================== フィールド =====================================
    [SerializeField] private RawImage _homePageBG;
    [SerializeField] private RawImage _homePage;

    /// <summary>プレイヤーの基礎ステータスデータ</summary>
    [SerializeField] private PlayerDataSO _playerData;
    /// <summary>ボスの基礎ステータスデータ</summary>
    [SerializeField] private BossDataSO _bossData;

    [SerializeField] private RawImage[] _characterCursor = new RawImage[2];
    [SerializeField] private RawImage[] _difficultyCursor = new RawImage[3];
    [SerializeField] private RawImage[] _homeUIs = new RawImage[10];
    [SerializeField] private RawImage[] _statusUIs = new RawImage[2];
    [SerializeField] private TextMeshProUGUI[] _statusTexts = new TextMeshProUGUI[9];
    [SerializeField] private RawImage _startButton;
    [SerializeField] private TextMeshProUGUI Expltext;
    [SerializeField] private GameObject[] Doors = new GameObject[2];

    // BGMz
    private readonly List<AudioClip> _bgm = new List<AudioClip>();
    // 効果音（Sound effects）
    private readonly List<AudioClip> _se = new List<AudioClip>();

    private CharacterType _characterType = CharacterType.Player;
    private int _difficultyType = 0;
    private int _stageLevel = 10;

    // ステータスを累乗する値
    private float _statusExponent = 1.2f;
    // レベル値の倍率
    private float _levelMultiplier = 0.25f;

    private void Awake()
    {
        // ロード画面の初期化
        SetLoadingCanvas();

        // フェードアウト
        StartCoroutine(LoadScene("", LoadState.Unload));
        
        // 基礎ステータスを格納する
        SetBaseStatus();
    }

    void Start()
    {
        // Homeシーンで使う音声を読み込む
        LoadAudio();

        // 難易度ボタンの上に表示する文
        Expltext.text = "<color=orange>難易度</color>を<color=orange>選択</color>してください";

        // 保存したデータを適用
        SetSaveData();

        // ホーム画面のUIアニメーションを開始する
        StartCoroutine(HomeUIsAnimation());

        // カーソルアニメーションを実行
        //StartCoroutine(CursorMotion(_characterType, _difficultyType));

        // スタートボタンを点滅させる
        StartCoroutine(StartButtonFadeLoop());

        // UIを更新
        UpdateUI();
    }

    //=================================== 初期処理 =====================================
    // Homeシーンで使う音声を読み込む
    private void LoadAudio()
    {

    }

    // 基礎ステータスを格納する
    private void SetBaseStatus()
    {
        // プレイヤー
        if (!_playerData)
        {
            Debug.LogError("プレイヤーのステータスがセットされていません");
            return;
        }

        // 基礎ステータス
        _playerHp = Mathf.RoundToInt(_playerData.BaseHp * _playerData.HpMultiplier);
        _playerAtk = Mathf.RoundToInt(_playerData.BaseAtk * _playerData.AtkMultiplier);
        _playerDef = Mathf.RoundToInt(_playerData.BaseDef * _playerData.DefMultiplier);
        _playerEnergy = _playerData.BaseEnergy;
        _playerStamina = _playerData.BaseStamina;

        // 耐性
        _playerPhysResist = _playerData.BasePhysResist;
        _playerEnergyResist = _playerData.BaseEnergyResist;

        // クリティカル
        _playerCritRate = _playerData.BaseCritRate;
        _playerCritDamage = _playerData.BaseCritDamage;

        // ボス
        if (!_bossData)
        {
            Debug.LogError("ボスのステータスがセットされていません");
            return;
        }

        // 基礎ステータス
        _bossHp = Mathf.RoundToInt(_bossData.BaseHp * _bossData.HpMultiplier);
        _bossAtk = Mathf.RoundToInt(_bossData.BaseAtk * _bossData.AtkMultiplier);
        _bossDef = Mathf.RoundToInt(_bossData.BaseDef * _bossData.DefMultiplier);

        // 耐性
        _bossPhysResist = _bossData.BasePhysResist ;
        _bossEnergyResist = _bossData.BaseEnergyResist;

        // クリティカル
        _bossCritRate = _bossData.BaseCritRate;
        _bossCritDamage = _bossData.BaseCritDamage;
    }

    // 保存した難易度を適用
    private void SetSaveData()
    {
        _stageLevel = PlayerPrefs.GetInt("StageLevel", 10);

        _difficultyType = PlayerPrefs.GetInt("DifficultyType", 0);
    }

    //=================================== ボタンインターフェース処理 =====================================
    //////////////////// 対象キャラの変更 ////////////////////
    /// <summary>
    /// /// 調整対象をプレイヤーに切り替える
    /// </summary>
    public void ChangeToPlayer()
    {
        _characterType = CharacterType.Player;
        UpdateUI();
    }

    /// <summary>
    /// /// 調整対象をボスに切り替える
    /// </summary>
    public void ChangeToBoss()
    {
        _characterType = CharacterType.Boss;
        UpdateUI();
    }

    //////////////////// 難易度調整 ////////////////////
    /// <summary>
    /// 難易度を「イージー：Lv10」に変更
    /// </summary>
    public void SetStageLevelToEasy()
    {
        _stageLevel = 10;
        _difficultyType = 0;
        UpdateUI();
    }

    /// <summary>
    /// 難易度を「ノーマル：Lv50」に変更
    /// </summary>
    public void SetStageLevelToNormal()
    {
        _stageLevel = 50;
        _difficultyType = 1;
        UpdateUI();
    }

    /// <summary>
    /// 難易度を「ハード：Lv100」に変更
    /// </summary>
    public void SetStageLevelToHard()
    {
        _stageLevel = 100;
        _difficultyType = 2;
        UpdateUI();
    }

    // 重複していた呼び出しをひとまとめにする共通関数
    private void UpdateUI()
    {
        SetCursor((int)_characterType, _difficultyType);
        SetStatus(_characterType);
    }

    // ステータスを反映する
    private void SetStatus(CharacterType characterType)
    {
        if (characterType == CharacterType.Player)
        {
            // 必要なテキストを表示
            foreach (RawImage image in _statusUIs)
            {
                image.gameObject.SetActive(true);
            }

            // ステータスを計算
            _playerResultHp = Mathf.RoundToInt(_playerHp * Mathf.Pow(_stageLevel * _levelMultiplier, _statusExponent));
            _playerResultAtk = Mathf.RoundToInt(_playerAtk * Mathf.Pow(_stageLevel * _levelMultiplier, _statusExponent));
            _playerResultDef = Mathf.RoundToInt(_playerDef * Mathf.Pow(_stageLevel * _levelMultiplier, _statusExponent));
            _playerResultEnergy = _playerEnergy;
            _playerResultStamina = _playerStamina;

            _playerResultPhysResist = _playerPhysResist * _stageLevel * _levelMultiplier;
            _playerResultEnergyResist = _playerEnergyResist * _stageLevel * _levelMultiplier;

            _playerResultCritRate = Mathf.Min(_playerCritRate * _stageLevel * _levelMultiplier * 100, 100.0f);
            _playerResultCritDamage = _playerCritDamage * _stageLevel * _levelMultiplier * 100 * 0.5f;

            // テキストに代入
            _statusTexts[0].text = $"HP：{_playerResultHp}";
            _statusTexts[1].text = $"攻撃力：{_playerResultAtk}";
            _statusTexts[2].text = $"防御力：{_playerResultDef}";
            _statusTexts[3].text = $"エネルギー量：{_playerResultEnergy}";
            _statusTexts[4].text = $"スタミナ：{_playerResultStamina}";

            _statusTexts[5].text = $"物理耐性：{_playerResultPhysResist:F1}";
            _statusTexts[6].text = $"エネルギー耐性：{_playerResultEnergyResist:F1}";
            _statusTexts[7].text = $"会心率：{_playerResultCritRate:F1}";
            _statusTexts[8].text = $"会心ダメージ：{_playerResultCritDamage:F1}";
        }
        else
        {
            // 不要なテキストを非表示
            foreach (RawImage image in _statusUIs)
            {
                image.gameObject.SetActive(false);
            }

            // ステータスを計算
            _bossResultHp = Mathf.RoundToInt(_bossHp * Mathf.Pow(_stageLevel * _levelMultiplier, _statusExponent));
            _bossResultAtk = Mathf.RoundToInt(_bossAtk * Mathf.Pow(_stageLevel * _levelMultiplier, _statusExponent));
            _bossResultDef = Mathf.RoundToInt(_bossDef * Mathf.Pow(_stageLevel * _levelMultiplier, _statusExponent));

            _bossResultPhysResist = _bossPhysResist * _stageLevel * _levelMultiplier;
            _bossResultEnergyResist = _bossEnergyResist * _stageLevel * _levelMultiplier;

            _bossResultCritRate = Mathf.Min(_bossCritRate * _stageLevel * _levelMultiplier * 100, 100.0f);
            _bossResultCritDamage = _bossCritDamage * _stageLevel * _levelMultiplier * 100 *0.5f;

            // テキストに代入
            _statusTexts[0].text = $"HP：{_bossResultHp}";
            _statusTexts[1].text = $"攻撃力：{_bossResultAtk}";
            _statusTexts[2].text = $"防御力：{_bossResultDef}";

            _statusTexts[5].text = $"物理耐性：{_bossResultPhysResist:F1}";
            _statusTexts[6].text = $"エネルギー耐性：{_bossResultEnergyResist:F1}";
            _statusTexts[7].text = $"会心率：{_bossResultCritRate:F1}";
            _statusTexts[8].text = $"会心ダメージ：{_bossResultCritDamage:F1}";
        }
    }

    // カーソルをセット
    private void SetCursor(int Index1, int Index2)
    {
        float cursorScaleX = 2.5f;
        float cursorScaleY = 1.0f;

        for (int c = 0; c < _characterCursor.Length; c++)
            _characterCursor[c].gameObject.SetActive(false);

        for (int d = 0; d < _difficultyCursor.Length; d++)
            _difficultyCursor[d].gameObject.SetActive(false);

        _characterCursor[Index1].transform.localScale = new Vector3(cursorScaleX, cursorScaleY, 1);
        _difficultyCursor[Index2].transform.localScale = new Vector3(cursorScaleX, cursorScaleY, 1);

        _characterCursor[Index1].gameObject.SetActive(true);
        _difficultyCursor[Index2].gameObject.SetActive(true);
    }

    //////////////////// 試合開始 ////////////////////
    /// <summary>
    /// 戦闘を開始する
    /// </summary>
    public void BattleStart()
    {
        SaveStatusLevel();
        StartCoroutine(ClosePage());
    }

    // ステータスを保存する処理
    private void SaveStatusLevel()
    {
        // カーソルのインデックス番号
        PlayerPrefs.SetInt("DifficultyType", _difficultyType);
        PlayerPrefs.SetInt("StageLevel", _stageLevel);

        // プレイヤー
        PlayerPrefs.SetInt("PlayerHP", _playerResultHp);
        PlayerPrefs.SetInt("PlayerATK", _playerResultAtk);
        PlayerPrefs.SetInt("PlayerDEF", _playerResultDef);
        PlayerPrefs.SetInt("PlayerEnergy", _playerResultEnergy);
        PlayerPrefs.SetInt("PlayerStamina", _playerResultStamina);

        PlayerPrefs.SetFloat("PlayerPhysResist", _playerResultPhysResist);
        PlayerPrefs.SetFloat("PlayerEnergyResist", _playerResultEnergyResist);
        PlayerPrefs.SetFloat("PlayerCritRate", _playerResultCritRate);
        PlayerPrefs.SetFloat("PlayerCritDamage", _playerResultCritDamage);

        // ボス
        PlayerPrefs.SetInt("BossHP", _bossResultHp);
        PlayerPrefs.SetInt("BossATK", _bossResultAtk);
        PlayerPrefs.SetInt("BossDEF", _bossResultDef);

        PlayerPrefs.SetFloat("BossPhysResist", _bossResultPhysResist);
        PlayerPrefs.SetFloat("BossEnergyResist", _bossResultEnergyResist);
        PlayerPrefs.SetFloat("BossCritRate", _bossResultCritRate);
        PlayerPrefs.SetFloat("BossCritDamage", _bossResultCritDamage);
    }

    //=================================== モーション処理 =====================================
    // Index1 = _characterCursor, Index2 = _difficultyCursor
    IEnumerator CursorMotion(int Index1, int Index2)
    {
        //for (int c = 0; c < _characterCursor.Length; c++)
        //    _characterCursor[c].gameObject.SetActive(false);

        //for (int d = 0; d < _difficultyCursor.Length; d++)
        //    _difficultyCursor[d].gameObject.SetActive(false);

        //_characterCursor[Index1].gameObject.SetActive(true);
        //_difficultyCursor[Index2].gameObject.SetActive(true);

        //RawImage[] cursor = new RawImage[2];
        //cursor[0] = _characterCursor[Index1];
        //cursor[1] = _difficultyCursor[Index2];

        //// 最大値が「横250 : 縦100」になるように、基準(Base)と振り幅(Amp)を横縦で分ける
        //float baseX = 2.35f;// 横の基準（2.2 〜 2.5 の真ん中）
        //float ampX = 0.15f; // 横の振り幅（最大 2.5 になる）

        //float baseY = 0.9f;// 縦の基準（0.8 〜 1.0 の真ん中）
        //float ampY = 0.1f; // 縦の振り幅（最大 1.0 になる）

        //float speed = 2.25f;// 拡大縮小のスピード
        //float startTime = Time.time;

        //// アニメーションのループ
        //while (true)
        //{
        //    float elapsedTime = Time.time - startTime;
        //    float sinValue = Mathf.Sin(elapsedTime * speed);// -1.0 〜 +1.0 を行き来する

        //    // 横と縦のスケールを別々に計算（sinValueが1のとき、Xは2.5、Yは1.0の最大値になります）
        //    float currentScaleX = baseX + (sinValue * ampX);
        //    float currentScaleY = baseY + (sinValue * ampY);

        //    // それぞれ計算したXとYを代入する（Z軸は元のまま）
        //    cursor[0].transform.localScale = new Vector3(currentScaleX, currentScaleY, cursor[0].transform.localScale.z);
        //    cursor[1].transform.localScale = new Vector3(currentScaleX, currentScaleY, cursor[1].transform.localScale.z);

        yield return null;
        //}
    }

    // スタートボタンを点滅させる
    IEnumerator StartButtonFadeLoop()
    {
        float startButtonFadeSpeed = 2.0f;

        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * startButtonFadeSpeed) + 1.0f) * 0.25f + 0.5f;

            Color buttonColor = _startButton.color;

            buttonColor.a = alpha;

            // 計算した透明度をボタンに適用
            _startButton.color = buttonColor;

            // 1フレーム待機してループを繰り返す
            yield return null;
        }
    }

    // ホーム画面のUIアニメーション処理
    IEnumerator HomeUIsAnimation()
    {
        int count = 0;
        float delta = 0.0f;

        for (int i = 0; i < _homeUIs.Length; i++)
        {
            if (i != 0 && i != 3 && i != 6 && i != 9)
            {
                _homeUIs[i].gameObject.SetActive(false);
            }
        }

        while (true)
        {
            delta += Time.deltaTime;

            if (delta >= 1 && count <= 1)
            {
                _homeUIs[count + 1].gameObject.SetActive(true);
                _homeUIs[count + 4].gameObject.SetActive(true);
                _homeUIs[count + 7].gameObject.SetActive(true);

                count++;
                delta = 0.0f;
            }
            else if (delta >= 1 && count == 2)
            {
                for (int i = 0; i <= 1; i++)
                {
                    _homeUIs[i + 1].gameObject.SetActive(false);
                    _homeUIs[i + 4].gameObject.SetActive(false);
                    _homeUIs[i + 7].gameObject.SetActive(false);
                }

                count = 0;
                delta = 0.0f;
            }

            _homeUIs[9].transform.localRotation *= Quaternion.Euler(0f, 0f, -1.5f);

            yield return null;
        }
    }

    // ホームページをしまう処理
    IEnumerator ClosePage()
    {
        float speed = 4.0f;
        float delta = 0.0f;

        _homePageBG.gameObject.SetActive(false);

        foreach (Transform item in _homePage.transform)
        {
            item.gameObject.SetActive(false);
        }

        Vector3 startScale = _homePage.transform.localScale;
        Vector3 startPos = _homePage.transform.position;

        float targetPageScaleX = startScale.x * 5.0f;

        while (_homePage.transform.localScale.x < targetPageScaleX)
        {
            delta += Time.deltaTime;

            float scaleProgress = 1.0f + (delta * speed);
            _homePage.transform.localScale = startScale * scaleProgress;

            _homePage.transform.position = startPos + new Vector3(scaleProgress * 200.0f, 0, 0);

            yield return null;
        }

        _homePage.gameObject.SetActive(false);

        StartCoroutine(OpenDoor());
    }

    // 扉を開く処理
    IEnumerator OpenDoor()
    {
        float speed = 2.0f;
        float nowPosZ = 0.0f;
        float targetPosZ = (nowPosZ + 1) * 3.0f;

        while (nowPosZ < targetPosZ)
        {
            float step = speed * Time.deltaTime;

            Doors[0].transform.localPosition += new Vector3(0, 0, step);
            Doors[1].transform.localPosition -= new Vector3(0, 0, step);

            nowPosZ = Doors[0].transform.localPosition.z;

            yield return null;
        }

        foreach (GameObject item in Doors)
        {
            item.gameObject.SetActive(false);
        }
    }
}
