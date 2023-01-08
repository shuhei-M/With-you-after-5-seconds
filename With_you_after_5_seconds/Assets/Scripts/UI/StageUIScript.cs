using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI.Extensions;

public class StageUIScript : UI_Effect
{
    #region serialize field
    [SerializeField] private GameData gameData;
    [Space(3)]

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pausePlayButton;
    [Space(3)]

    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject[] howToPlayPagePanel;
    [SerializeField] private Button cancelButton;
    [SerializeField] float fadeSec = 2f;
    [Space(3)]

    [SerializeField] private GameObject retryPanel;
    [SerializeField] private Button retryNoButton;
    [Space(3)]

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float time = 500f;
    [Space(3)]

    [SerializeField] private GameObject clearPanel;
    [SerializeField] private Button nextStageButton;
    [SerializeField] private float clearStayTime = 2.0f;
    [Space(3)]

    [SerializeField] private GameObject brackFadePanel;
    #endregion


    #region private member
    FadeState fadeState = FadeState.None;
    GameObject selectButton;
    GameObject selectedButton; // マウスなどが原因でコントローラー操作が出来なくなるのを防ぐために保管しておく用
    bool howToPlayFlag = false;
    bool retryFlag = false;
    bool nowEffecting = false;

    float timeLimit;

    Scene nowStage;

    EventSystem eventSystem;

    /// <summary> インゲーム時のデフォルトのパネル（松島） </summary>
    private GameObject _DefaultPanel;
    private InGame _CurrentInGameState;
    private InGame _PrevInGameState;

    /// <summary> クリア画面表示前のページめくり演出のパネル（松島） </summary>
    private BookUI _ClearBookUI;
    private GameObject _WarpEffectObj;
    private UI_WarpEffect _WarpEffect;
    private DirectingScript _DirectingScript;

    /// <summary> Stageへの遷移時のページめくり演出（松島） </summary>
    //private bool _IsStartFirstCutIn = false;
    private GameObject _FirstCutInPanel;
    private BookUI _FirstCutInUI;
    //private float _FirstCutInTime;

    /// <summary> リトライ時のページめくり演出（松島） </summary>RetryPanel
    private UI_RetryPanel _RetryPanel;
    #endregion


    #region propaty
    public float ClearStayTime { get { return clearStayTime; } }
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        gameData.EditorStart();
        
        // 効果音を一括ロード
        SoundSet();

        pausePanel.GetComponent<PauseUIAnimation>().PauseUISetUp();

        timeLimit = time;

        nowStage = SceneManager.GetActiveScene();

        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        selectButton = pausePlayButton.gameObject;

        // 松島
        _FirstCutInPanel = transform.Find("FirstCutInPanel").gameObject;
        _FirstCutInUI = _FirstCutInPanel.GetComponent<BookUI>();
        _DefaultPanel = transform.Find("DefaultPanel").gameObject;
        _DefaultPanel.SetActive(false);
        _ClearBookUI = clearPanel.GetComponent<BookUI>();

        _CurrentInGameState = InGame.None;
        _PrevInGameState = _CurrentInGameState;

        _WarpEffectObj = transform.Find("WarpEffectPanel").gameObject;
        _WarpEffect = _WarpEffectObj.GetComponent<UI_WarpEffect>();

        _DirectingScript = GameObject.Find("DirectingObj").GetComponent<DirectingScript>();

        _RetryPanel = transform.Find("RetryPanel").gameObject.GetComponent<UI_RetryPanel>();
        _RetryPanel.gameObject.SetActive(false);

        // ステージ09の場合
        if(nowStage.name == "Stage09")
        {
            _FirstCutInPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _CurrentInGameState = gameData.InGameState;
        UpdateState();

        FadeEffect();

        if (gameData.InGameState == InGame.Pause || gameData.InGameState == InGame.GoalCompletion)
        {
            // 今選択しているものを First Selected に随時上書きしつつ
            // 選択状態が解除されてしまった時に再度選択状態を取得できるようにする
            ReadCurrentSelectedButton();
            ReferenceSelectedButton();
        }

        if (howToPlayFlag)
        {
            cancelButton.Select();
        }
        
        if(!howToPlayFlag && !retryFlag)
        {
            // ゲームクリア条件やゲーム中の演出処理を追加した際に
            // ポーズ画面に行くかどうかの条件文変更の可能性大
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown("joystick button 7"))
            {
                PauseActiveChange();
            }
        }

        // ひとつ前のインゲームステートとして保存
        _PrevInGameState = _CurrentInGameState;
    }
    #endregion


    #region Coroutine
    /// <summary>
    /// リトライ用コルーチン
    /// </summary>
    private IEnumerator RetryCoroutine()
    {
        yield return new WaitForSeconds(_ClearBookUI.TurnTime);

        // 現在のシーンを再読み込み
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator MainMenuTransitionCorutine()
    {
        float time = 0f;

        eventSystem.enabled = false;

        while(time <= fadeSec)
        {
            FadeInUI(ref brackFadePanel, fadeSec, true);
            if (_CurrentInGameState == InGame.Pause)
            {
                Sound.VolumeDownBGM(Time.unscaledDeltaTime, 0.3f);
            }
            yield return new WaitForEndOfFrame();
            time += Time.unscaledDeltaTime;
        }

        Time.timeScale = 1f;
        gameData.MeinMenuTransition();
        SceneManager.LoadScene("TitleScene");
    }
    #endregion


    #region private function
    /// <summary>
    /// 状態の変更
    /// </summary>
    private void ChangeState()
    {
        // ログを出す
        Debug.Log("ChangeState " + _PrevInGameState + "-> " + _CurrentInGameState);

        switch (_CurrentInGameState)
        {
            case InGame.CutIn:
                {
                    _DefaultPanel.SetActive(false);
                }
                break;
            case InGame.ChangeStartView:
                {
                    _FirstCutInPanel.SetActive(false);
                }
                break;
            case InGame.EntryPlayer:
                {
                }
                break;
            case InGame.PlayGame:
                {
                    _DefaultPanel.SetActive(true);
                }
                break;
            case InGame.Pause:
                {
                }
                break;
            case InGame.EntryGoal:
                {
                    _DefaultPanel.SetActive(false);
                }
                break;
            case InGame.InGoal:
                {
                    Sound.PlaySE("Warp", 0.5f);

                    ////演出追加のため、この時点でクリアパネルをオンにする（松島）
                    //clearPanel.SetActive(true);
                    //_WarpEffectObj.SetActive(true);
                    //// _WarpEffect.StartEffect();
                    //StartCoroutine(StayClearUI());
                }
                break;
            case InGame.GoalCompletion:
                {
                    Sound.StopBGM();
                    if (nowStage.name != "Stage08")
                    {
                        Sound.PlaySE("StageClear", 1f);
                    }
                }
                break;
        }

    }

    /// <summary>
    /// 状態毎の毎フレーム呼ばれる処理
    /// </summary>
    private void UpdateState()
    {
        if (IsEntryThisState()) { ChangeState(); return; }

        switch (_CurrentInGameState)
        {
            case InGame.CutIn:
                {
                    // ステージ09の場合
                    if (nowStage.name == "Stage09")
                    {
                        gameData.ChangeStartViewTransition();
                        return;
                    }

                    // それ以外のシーン
                    CutInEffectUpdate();
                }
                break;
            case InGame.ChangeStartView:
                {
                }
                break;
            case InGame.EntryPlayer:
                {
                }
                break;
            case InGame.PlayGame:
                {
                    // チートコマンド
                    if (Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.Return))
                    {
                        StageClear();
                    }

                    // 制限時間を更新する
                    timeLimit -= Time.deltaTime;
                    timerText.text = "Time:" + (int)timeLimit;
                }
                break;
            case InGame.Pause:
                {
                }
                break;
            case InGame.EntryGoal:
                {
                    Sound.VolumeDownBGM(Time.deltaTime, 0.6f);
                }
                break;
            case InGame.InGoal:
                {
                    Sound.VolumeDownBGM(Time.deltaTime, 0.6f);

                    // カメラが上を向いていたら
                    if (_DirectingScript.IsStartWarpEffect && !_WarpEffectObj.activeSelf)
                    {
                        //演出追加のため、この時点でクリアパネルをオンにする（松島）
                        clearPanel.SetActive(true);
                        _WarpEffectObj.SetActive(true);
                        // _WarpEffect.StartEffect();
                        StartCoroutine(StayClearUI());
                    }
                }
                break;
            case InGame.GoalCompletion:
                {
                    if (nowStage.name == "Stage08")
                    {   // ファイナルマンガへ！
                        FadeInUI(ref brackFadePanel, fadeSec * 2);

                        if (brackFadePanel.GetComponent<Image>().color.a == 1f)
                        {
                            //gameData.MeinMenuTransition();
                            //SceneManager.LoadScene("TitleScene");

                            gameData.ChangeCutInTransition();
                            SceneManager.LoadScene("Stage09");
                        }
                    }
                    else
                    {
                        // ページをめくる演出を行う（松島）
                        _ClearBookUI.TurnPageUpdate();
                    }

                    // マウスを押した際に、進行不能になることを防ぐ。
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                        _ClearBookUI.ToUseableButton();
                }
                break;
        }
    }

    /// <summary>
    /// ちょうどそのステートに入った所かどうか
    /// </summary>
    /// <returns></returns>
    private bool IsEntryThisState()
    {
        return (_PrevInGameState != _CurrentInGameState);
    }

    void SoundSet()
    {
        Sound.LoadSE("PencilSelect", "ButtonSelect");
        Sound.LoadSE("PencilPush", "ButtonPush");
        Sound.LoadSE("StageClear", "StageClear");
        Sound.LoadSE("Warp", "warp");
    }

    /// <summary>
    /// 万が一選択状態のボタンが無くなってしまったときにスティック操作で
    /// 選択中のボタンを再度選択状態できるよう保管する関数
    /// また、随時選択状態が何かを取得し、InputModule操作が切り替わっても正常に操作できるよう保存
    /// </summary>
    void ReadCurrentSelectedButton()
    {
        if (eventSystem.currentSelectedGameObject != null)
        {
            selectedButton = eventSystem.currentSelectedGameObject.gameObject;
            eventSystem.firstSelectedGameObject = selectedButton;
        }
    }

    /// <summary>
    /// 選択状態のボタンがないときにスティック操作で再度選択状態にする関数
    /// </summary>
    void ReferenceSelectedButton()
    {
        if (eventSystem.GetComponent<EventSystem>().currentSelectedGameObject == null)
        {
            if (Input.GetAxis(eventSystem.GetComponent<StandaloneInputModule>().horizontalAxis) != 0 ||
                Input.GetAxis(eventSystem.GetComponent<StandaloneInputModule>().verticalAxis) != 0)
            {
                selectedButton.GetComponent<Button>().Select();
            }
        }
    }

    /// <summary>
    /// カットインエフェクトを更新する
    /// </summary>
    private void CutInEffectUpdate()
    {
        // 終わっていたら
        if (_FirstCutInUI.IsFinishEffect)
        {
            gameData.ChangeStartViewTransition();
            _FirstCutInPanel.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// ステージクリアまで一定時間待つ
    /// </summary>
    /// <returns></returns>
    IEnumerator StayClearUI()
    {
        yield return new WaitForSeconds(clearStayTime);
        _ClearBookUI.DisplayBGImage();
        if(nowStage.name != "Stage08")
        {
            // ページをめくる演出を開始させる（松島）
            _ClearBookUI.GoToNextPage();
        }
        StageClear();
    }

    // クリアした時の処理
    void StageClear()
    {
        gameData.GoalCompletionTransition();
        clearPanel.SetActive(true);
        selectButton = nextStageButton.gameObject;
        nextStageButton.Select();

        if (nowStage.name == "SampleScene")
            return;

        for (int i = 0; i < gameData.stageData.Length; i++)
        {
            if (gameData.stageData[i].sceneName == nowStage.name)
            {
                // クリアしたステージの情報を保存する
                // アイテム機能はまだ入れていないので false を入れるようしている（要変更）
                gameData.ClearStageDataStorage(i, false);
                return;
            }
        }
    }


    bool ComicCheck()
    {
        if (nowStage.name == gameData.stageScene[1] && !gameData.showStartStory[1])
        {
            SceneManager.LoadScene("Comic02");
            return true;
        }
        else if(nowStage.name == gameData.stageScene[3] && !gameData.showStartStory[2])
        {
            SceneManager.LoadScene("Comic03");
            return true;
        }
        else if (nowStage.name == gameData.stageScene[5] && !gameData.showStartStory[3])
        {
            SceneManager.LoadScene("Comic04");
            return true;
        }
        else if (nowStage.name == gameData.stageScene[7] && !gameData.showStartStory[4])
        {
            //SceneManager.LoadScene("");
            //return true;

            return false;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// フェードエフェクト処理
    /// </summary>
    void FadeEffect()
    {
        if (fadeState == FadeState.HowToPlay)
        {
            nowEffecting = true;

            if (howToPlayFlag)
            {
                PanelToFade(ref pausePanel, ref howToPlayPanel, ref fadeState, pausePanel, fadeSec, true, true);
            }
            else
            {
                PanelToFade(ref howToPlayPanel, ref pausePanel, ref fadeState, pausePanel, fadeSec, false, true);
            }


            if (!howToPlayPanel.activeInHierarchy)
            {
                selectButton = pausePlayButton.gameObject;
                pausePlayButton.Select();
                HowToPlayEffectReset(ref howToPlayPanel);
            }
        }
        else if (fadeState == FadeState.Reset)
        {
            nowEffecting = true;

            if (retryFlag)
            {
                PanelToFade(ref pausePanel, ref retryPanel, ref fadeState, pausePanel, fadeSec, true, true);

                if (retryPanel.GetComponent<Image>().color.a >= 1f)
                {
                    selectButton = retryNoButton.gameObject;
                    retryNoButton.Select();
                }
            }
            else
            {
                PanelToFade(ref retryPanel, ref pausePanel, ref fadeState, pausePanel, fadeSec, false, true);
            }


            if (!retryPanel.activeInHierarchy)
            {
                selectButton = pausePlayButton.gameObject;
                pausePlayButton.Select();
            }
        }
        else
        {
            nowEffecting = false;
        }
    }
    #endregion


    #region Button function
    // ポーズボタンを押したときの処理
    void PauseActiveChange()
    {
        // プレイ中もしくはポーズ画面中なら処理を行わない
        if (gameData.InGameState != InGame.PlayGame && gameData.InGameState != InGame.Pause)
            return;

        var pauseUIAnim = pausePanel.GetComponent<PauseUIAnimation>();

        // カット処理中なら処理を行わない
        if (!pauseUIAnim.CutOK)
            return;

        // ポーズ画面を表示したときの処理
        if (gameData.InGameState != InGame.Pause)
        {
            gameData.PauseTransition();
            pausePanel.SetActive(true);
            pauseUIAnim.CutOK = false;
            pauseUIAnim.CutInPause();
        }
        else
        {
            pauseUIAnim.CutOK = false;
            pauseUIAnim.CutOutPause();
        }
    }

    // ---------- ポーズ画面中のボタン処理 ---------- //
    public void ClickPlayButton()
    {
        Sound.PlaySE("PencilPush", 1f);
        pausePanel.GetComponent<PauseUIAnimation>().CutOutPause();
    }

    public void ClickHowToPlayButton()
    {
        Sound.PlaySE("PencilPush", 1f);
        fadeState = FadeState.HowToPlay;
        howToPlayFlag = true;
        cancelButton.Select();
    }

    public void MoveNextPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") > 0f || Input.GetAxis("R_Horizontal") > 0f || Input.GetAxis("D-Pad_Horizontal") > 0f)
        {
            GoNextPageHTP(ref howToPlayPagePanel);
        }
    }

    public void MoveBackPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") < 0f || Input.GetAxis("R_Horizontal") < 0f || Input.GetAxis("D-Pad_Horizontal") < 0f)
        {
            GoBackPageHTP(ref howToPlayPagePanel);
        }
    }

    public void ClickCancelButton()
    {
        if (nowEffecting)
            return;

        fadeState = FadeState.HowToPlay;
        howToPlayFlag = false;
    }

    //if(_PaperFlip == -_PaperRange)
    // ---------- クリア画面中のボタン処理 ---------- //
    public void ClickNextStageButton()
    {
        Sound.PlaySE("PencilPush", 1f);

        // ページめくり演出が終わっていなければ、以下は実行しない（松島）
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            return;
        }

        if (nowStage.name == "SampleScene")
        {
            Time.timeScale = 1f;
            gameData.ChangeCutInTransition();
            SceneManager.LoadScene(nowStage.name);
            return;
        }

        // マンガのムービーに行くか
        if (ComicCheck())
        {
            return;
        }

        for (int i = 0; i < gameData.stageData.Length; i++)
        {
            if (nowStage.name == gameData.stageData[i].sceneName)
            {
                Time.timeScale = 1f;
                gameData.ChangeCutInTransition();
                SceneManager.LoadScene(gameData.stageData[i + 1].sceneName);
                return;
            }
        }
    }

    // ---------- リトライ確認画面のボタン処理 ---------- //
    public void RetryNoButton()
    {
        retryFlag = false;
        fadeState = FadeState.Reset;
        Sound.PlaySE("PencilPush", 1f);
    }

    public void RetryYesButton()
    {
        RetryFunction();
    }


    // ---------- ポーズ・クリア画面共通のボタン処理 ---------- //
    public void ClickStageSelectButton()
    {
        // ページめくり演出が終わっていなければ、以下は実行しない（松島）
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            return;
        }

        Time.timeScale = 1f;
        gameData.StageSelectTransition();
        Sound.BGMAndSEResets();
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// メインメニューへ移動する
    /// ステージクリア時に表示
    /// </summary>
    public void ClickMeinMenuButton()
    {
        // ページめくり演出が終わっていなければ、以下は実行しない（松島）
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            return;
        }

        Sound.PlaySE("PencilPush", 1f);
        StartCoroutine(MainMenuTransitionCorutine());

        //Time.timeScale = 1f;
        //gameData.MeinMenuTransition();
        //SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// 現在のステージに再挑戦する
    /// ステージクリア時に表示
    /// </summary>
    public void ClickRetryButton()
    {
        if (gameData.InGameState == InGame.Pause)
        {
            retryFlag = true;
            fadeState = FadeState.Reset;
            Sound.PlaySE("PencilPush", 1f);

            //Time.timeScale = 1f;
            //gameData.ChangeCutInTransition();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // ページめくり演出が終わっていなければ、以下は実行しない
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            Debug.Log("wwwwwwwwwwwwwwwwwwww");
            return;
        }

        _RetryPanel.gameObject.SetActive(true);
        clearPanel.SetActive(false);
        Sound.BGMAndSEResets();

        //_ClearBookUI.GoToNextPage();

        //// リトライコルーチンを起動
        //StartCoroutine(RetryCoroutine());
    }

    public void RetryFunction()
    {
        Time.timeScale = 1f;
        gameData.ChangeCutInTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Select()
    {
        if (selectButton != null && eventSystem.GetComponent<EventSystem>().currentSelectedGameObject == selectButton)
        {
            selectButton = null;
        }
        else
        {
            Sound.PlaySE("PencilSelect", 0.75f);
        }
    }
    #endregion
}
