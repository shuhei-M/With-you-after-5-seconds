using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI.Extensions;
using System;

public class TitleScript : UI_Effect
{
    #region SERIALIZE FIELD MEMBER
    [Header("タイトルと目次")]
    [SerializeField] [Tooltip("タイトルと目次の画面")] private GameObject titleAndMenuScreen;
    [SerializeField] [Tooltip("タイトルのUIをまとめてる親オブジェクト")] private GameObject titleUIs;
    [SerializeField] [Tooltip("メインメニューのUIをまとめてる親オブジェクト")] private GameObject mainMenuUIs;
    [SerializeField] [Tooltip("「ボタンを押してください」の点滅させるImage")] private Image pressAnyButton;
    [SerializeField] [Tooltip("つづきからボタン")] private Button continueButton;
    [SerializeField] [Tooltip("シーン開始時のフェードインに欠ける時間")] private float startFadeSec = 3.0f;
    [SerializeField] [Tooltip("切り替えにかける時間")] private float fadeSec = 1.0f;
    [SerializeField] [Tooltip("切り替えにかける時間")] private float flashingRepetTime = 2.0f;

    [Space(3)]
    [Header("はじめから")]
    [SerializeField] [Tooltip("リセット画面")] private GameObject resetScreen;
    [SerializeField] [Tooltip("リセット確認画面")] private GameObject resetConfirmation;
    [SerializeField] [Tooltip("リセット確定画面")] private GameObject toReset;
    [SerializeField] [Tooltip("リセット確認画面の「いいえ」ボタン")] private Button noButton;

    [Space(3)]
    [Header("ステージセレクト")]
    [SerializeField] [Tooltip("ステージセレクト画面の親")] private GameObject stageSelectScreens;
    [SerializeField] [Tooltip("ステージセレクト画面の各ページ")] private GameObject[] stageSelectPageScreen;
    [SerializeField] [Tooltip("各ステージセレクト画面ページにあるボタンの数")] private int[] pageButtonNum;
    [SerializeField] [Tooltip("ステージ遷移するボタン")] private Button[] stageButton;
    [SerializeField] [Tooltip("ステージ01のボタン")] private Button stage01Button;

    [Space(3)]
    [Header("あそびかた")]
    [SerializeField] [Tooltip("あそびかた画面の親")] private GameObject howToPlayScreen;
    [SerializeField] [Tooltip("あそびかた画面の各ページ")] private GameObject[] howToPlayPageScreen;
    [SerializeField] [Tooltip("キャンセル用のボタン")] private Button cancelButton;

    [Space(3)]
    [Header("チート画面")]
    [SerializeField] [Tooltip("クリアチート使用表示画面")] private GameObject cheatOnImage;

    [Space(3)]
    [Header("その他")]
    [SerializeField] [Tooltip("イベントシステム")] private GameObject blackFadePanel;
    [SerializeField] [Tooltip("イベントシステム")] private EventSystem eventSystem;
    #endregion


    #region PUBLIC MEMBER
    public GameData gameData;
    #endregion


    #region PRIVATE MEMBER
    StageSelectPage[] stageSelectPage;
    GameObject selectedButton;

    GameObject nowActiveScreen;
    Button selectButton;
    FadeState fadeState = FadeState.Brack;
    float goTime; // マンガシーンに遷移するまでの経過時間
    bool nowEffecting = false;
    bool resetScreenFlag = false; // true: ResetScreen表示　false: ResetScreen非表示
    bool goNextScene = false;
    bool goShowStartStory = false; // マンガシーンに遷移するか
    bool stayScene = false; // デバッグ用、はじめからでタイトルシーンを再読み込みするかどうかを示すフラグ
    bool goSampleSceen = false; // デバッグ用、ステージ選択でサンプルシーンに行けるようにする。

    BookUI _BookUI;
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        gameData.Start();　// ローカルストレージに関するスタート処理

        gameData.StartData(); // データの読込・書込

        StageDataStorage(); // ステージセレクト画面のデータをクラスに格納
        
        ActiveScreenCheck();// ポーズ画面・クリア画面からステージセレクトへ遷移した時のUI処理

        SoundSet();

        _BookUI = stageSelectScreens.GetComponent<BookUI>();
    }

    // Update is called once per frame
    void Update()
    {
        gameData.Update();　// ローカルストレージに関するアップデート処理

        // タイトル画面のボタン処理
        if (gameData.OutGameState == OutGame.Title && Input.anyKey)
        {
            PressAnyKey();
        }

        // マンガシーンにフェードしながら遷移
        if (goShowStartStory)
        {
            GoShowStartStory();
            return;
        }

        // シーン遷移時ボタン処理を受け付けないようにする
        if (goNextScene)
        {
            eventSystem.enabled = false;
            return;
        }

        // フェードエフェクト使用するか判定
        FadeSelect();

        if (gameData.OutGameState != OutGame.Title && !nowEffecting)
        {
            // 今選択しているものを First Selected に随時上書きしつつ
            // 選択状態が解除されてしまった時に再度選択状態を取得できるようにする
            ReadCurrentSelectedButton();
            ReferenceSelectedButton();
        }

        // 各画面時のアップデート処理
        if (gameData.OutGameState == OutGame.Title)
        {
            FlashingUI(ref pressAnyButton, flashingRepetTime);
        }
        else if (gameData.OutGameState == OutGame.HowToPlay)
        {   // あそびかたの画面だった場合常にキャンセルできるようにしておく
            cancelButton.Select();
        }


        // チートで全ステージ解放
        if (Input.GetKeyDown(KeyCode.C))
        {
            gameData.AllClearData();
            cheatOnImage.SetActive(true);
            StartCoroutine("SceneReset");
        }

        // R を押したらクリア情報を初期化
        if (Input.GetKeyDown(KeyCode.R))
        {
            stayScene = !stayScene;
            Debug.Log("はじめからをしたらタイトルシーン再読み込み (" + stayScene + ")\n"
                       + "trueでタイトル再読み込み、falseで通常通りシーン遷移");
        }

        // F1 を押したらどのステージを選んでもサンプルシーンへ遷移
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!goSampleSceen)
            {
                goSampleSceen = true;
                Debug.Log("ステージ選択をするとサンプルシーンが起動されます。");
            }
            else
            {
                goSampleSceen = false;
                Debug.Log("ステージ選択で通常通り遷移します。");
            }
        }
    }
    #endregion


    #region Coroutine
    // チートで全ステージ解放したら、コルーチンで 2 秒だけ
    // テキストをアクティブ化しTitelSceneを呼びなおす
    IEnumerator SceneReset()
    {
        yield return new WaitForSeconds(2.0f);
        gameData.MeinMenuTransition();
        SceneManager.LoadScene("TitleScene");
    }

    // はじめからにした際にシーン遷移するまで少し待つ
    // 演出処理が入ればなくすかも
    IEnumerator ResetWaitTime()
    {
        yield return new WaitForSeconds(2.0f);
        gameData.StartStoryTransition();
        goShowStartStory = true;
    }
    #endregion


    #region PRIVATE METHOD
    /// <summary>
    /// ステージセレクト画面のデータをクラスに格納
    /// </summary>
    void StageDataStorage()
    {
        stageSelectPage = new StageSelectPage[stageSelectPageScreen.Length];
        int buttonCount = 0;
        for (int i = 0; i < stageSelectPage.Length; i++)
        {
            stageSelectPage[i] = new StageSelectPage();

            // stageSelectScreen へ格納
            stageSelectPage[i].stageSelectScreen = stageSelectPageScreen[i];

            // stageButton, buttonActive へ格納
            stageSelectPage[i].stageButton = new GameObject[pageButtonNum[i]];
            stageSelectPage[i].buttonActive = new bool[pageButtonNum[i]];
            for (int j = 0; j < pageButtonNum[i]; j++)
            {
                // stageButtonの処理
                if (buttonCount < stageButton.Length)
                {   // 各ページ内で表示するボタンのデータを格納
                    stageSelectPage[i].stageButton[j] = stageButton[buttonCount].gameObject;
                }
                else
                {
                    Debug.LogWarning("ステージセレクトのデータ格納中、範囲外にアクセスしようとしました。");
                }

                // buttonActiveの処理
                if (buttonCount == 0 || buttonCount <= gameData.notClearStageNum)
                {
                    stageSelectPage[i].buttonActive[j] = true;
                }
                else
                {
                    stageSelectPage[i].buttonActive[j] = false;
                }

                buttonCount++;
            }

            // active へ格納 (一番最初のぺージのみアクティブ化される)
            if (i == 0)
            {
                stageSelectPage[i].stageActive = true;
            }
            else
            {
                stageSelectPage[i].stageActive = false;
            }
        }
    }

    /// <summary>
    /// ポーズ画面・クリア画面からステージセレクトへ遷移した時の処理
    /// </summary>
    void ActiveScreenCheck()
    {
        if (gameData.OutGameState == OutGame.MeinMenu)
        {
            titleUIs.SetActive(false);
            mainMenuUIs.SetActive(true);
            ShowAlphaUI(ref mainMenuUIs);
            selectButton = continueButton;

            nowActiveScreen = titleAndMenuScreen;
        }
    }

    /// <summary>
    /// 使うサウンドを読み込む
    /// </summary>
    void SoundSet()
    {
        Sound.LoadBGM("TitleSceneBGM", "TitleSceneBGM");
        Sound.LoadSE("PencilSelect", "ButtonSelect");
        Sound.LoadSE("PencilPush", "ButtonPush");

        Sound.PlayBGM("TitleSceneBGM", 0.05f);
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
    /// フェードエフェクト使用するか判定
    /// </summary>
    void FadeSelect()
    {
        if (fadeState == FadeState.Brack)
        {   // 初期シーン起動時
            nowEffecting = true;

            FadeOutUI(ref blackFadePanel, startFadeSec);

            if (blackFadePanel.GetComponent<Image>().color.a == 0f)
            {
                fadeState = FadeState.None;
            }

            if (fadeState == FadeState.None && selectButton != null)
            {
                selectButton.Select();
            }
        }
        else if (fadeState == FadeState.Title)
        {   // もくじ画面遷移時
            nowEffecting = true;

            UIToFade(ref titleUIs, ref mainMenuUIs, ref fadeState, fadeSec);

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }
        }
        else if (fadeState == FadeState.StageSelect)
        {   // もくじ⇒ステージセレクト or ステージセレクト⇒もくじ
            if (!nowEffecting)
            {
                // 必要があれば1ページ目へ戻す（松島）
                ResetBookScript();
            }

            nowEffecting = true;            

            if (gameData.OutGameState != OutGame.MeinMenu)
            {
                PanelToFade(ref titleAndMenuScreen, ref stageSelectScreens, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }
            else
            {
                PanelToFade(ref stageSelectScreens, ref titleAndMenuScreen, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }

            if (!stageSelectScreens.activeInHierarchy)
            {
                StageSelectScreenEffectReset();
            }
        }
        else if (fadeState == FadeState.HowToPlay)
        {   // もくじ⇒あそびかた or あそびかた⇒もくじ
            nowEffecting = true;

            if (gameData.OutGameState != OutGame.MeinMenu)
            {
                PanelToFade(ref titleAndMenuScreen, ref howToPlayScreen, ref fadeState, titleAndMenuScreen, fadeSec, true);
            }
            else
            {
                PanelToFade(ref howToPlayScreen, ref titleAndMenuScreen, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }

            if (!howToPlayScreen.activeInHierarchy)
            {
                HowToPlayEffectReset(ref howToPlayScreen);
            }
        }
        else if (fadeState == FadeState.Reset)
        {   // もくじ⇒リセット確認画面 or リセット確認画面⇒もくじ
            nowEffecting = true;

            if (resetScreenFlag)
            {
                PanelToFade(ref titleAndMenuScreen, ref resetScreen, ref fadeState, titleAndMenuScreen, fadeSec, true);
            }
            else
            {
                PanelToFade(ref resetScreen, ref titleAndMenuScreen, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }
        }
        else
        {   // 遷移していない状態
            nowEffecting = false;
        }
    }

    void GoShowStartStory()
    {
        FadeInUI(ref blackFadePanel, fadeSec);
        Sound.VolumeDownBGM(Time.deltaTime, 0.2f);
        goTime += Time.deltaTime;

        if (goTime >= fadeSec / 2f)
        {
            Sound.BGMAndSEResets();
            gameData.StartStoryTransition();
            SceneManager.LoadScene("Comic01");
        }
    }

    /// <summary>
    /// 制作者：松島
    /// 2ページ目を選択したまま、他のメニュー画面へ遷った場合に、
    /// 1ページ目へ戻す処理
    /// </summary>
    private void ResetBookScript()
    {
        if (_BookUI.CurrentPage > 1) return;

        Image image = stageSelectPageScreen[1].GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1.0f);

        _BookUI.BackToPrevPage();
    }

    /// <summary>
    /// ステージセレクト画面のエフェクト処理をリセット
    /// </summary>
    void StageSelectScreenEffectReset()
    {
        for (int i = 0; i < stageSelectPage.Length; i++)
        {
            for (int j = 0; j < pageButtonNum[i]; j++)
            {
                // 演出処理
                if (i == 0)
                {
                    UIAlphaOffReset(ref stageSelectPage[i].stageButton[j], true);
                }
                else
                {
                    UIAlphaOnReset(ref stageSelectPage[i].stageButton[j], false);
                }
            }

            // active へ格納 (一番最初のぺージのみアクティブ化される)
            if (i == 0)
            {
                stageSelectPage[i].stageActive = true;
                UIAlphaOffReset(ref stageSelectPage[i].stageSelectScreen, true);
            }
            else
            {
                stageSelectPage[i].stageActive = false;
                UIAlphaOnReset(ref stageSelectPage[i].stageSelectScreen, false);
            }
        }

        //// 必要があれば1ページ目へ戻す（松島）
        //ResetBookScript();
    }
    #endregion


    #region PUBLIC BUTTON METHOD
    //---------- メインメニュー画面時の処理 ----------//
    public void ClickBeginStartButton()
    {
        fadeState = FadeState.Reset;
        resetScreenFlag = true;
        selectButton = noButton;
        Sound.PlaySE("PencilPush", 1f);
    }

    public void ClickContinueButton()
    {
        fadeState = FadeState.StageSelect;
        selectButton = stage01Button;
        nowActiveScreen = stageSelectScreens;
        Sound.PlaySE("PencilPush", 1f);
        gameData.StageSelectTransition();

        StageButtonActivity();
    }

    public void ClickHowToPlayButton()
    {
        fadeState = FadeState.HowToPlay;
        selectButton = cancelButton;
        nowActiveScreen = howToPlayScreen;
        Sound.PlaySE("PencilPush", 1f);
        gameData.HowToPlayTransition();
    }

    public void ClickStoryButton()
    {
        goShowStartStory = true;
        eventSystem.enabled = false;
    }

    public void ClickEndGameButton()
    {
        UnityEngine.Application.Quit();
    }

    //---------- ステージ選択画面時の処理 ----------//
    public void ClickStageButton()
    {
        // シーン遷移開始
        goNextScene = true;

        // サンプルシーン移動指示があった場合
        if (goSampleSceen)
        {
            gameData.ChangeCutInTransition();
            SceneManager.LoadScene("SampleScene");
            return;
        }

        // すべてのステージの用意が出来たらfor文処理を外すかも
        string scenename = eventSystem.currentSelectedGameObject.gameObject.name;
        for (int i = 0; i < stageButton.Length; i++)
        {
            if (gameData.stageData[i].sceneName == scenename)
            {
                if (gameData.stageData[i].selectOK)
                {
                    if (ComicCheck(scenename))
                        return;

                    gameData.ChangeCutInTransition();
                    Sound.BGMAndSEResets();
                    SceneManager.LoadScene(scenename);
                    return;
                }
                else
                {
                    StartCoroutine("NotSelectClick");
                    return;
                }
            }
        }

        // ステージが複数ない現段階では以下の処理をする
        gameData.ChangeCutInTransition();
        SceneManager.LoadScene("SampleScene");
    }

    //---------- リセット画面時の処理 ----------//
    public void ClickNoButton()
    {
        fadeState = FadeState.Reset;
        resetScreenFlag = false;
        selectButton = continueButton;
        Sound.PlaySE("PencilPush", 1f);
    }

    public void ClickYesButton()
    {
        resetConfirmation.SetActive(false);
        toReset.SetActive(true);
        goNextScene = true;
        gameData.ResetData();
        if (stayScene)
        {   // デバッグ用、ストーリーシーンに飛ばずにタイトルシーンを再読み込み
            StartCoroutine("SceneReset");
        }
        else
        {
            StartCoroutine("ResetWaitTime");
        }
    }

    //---------- 共通処理 ----------//
    public void MoveNextPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") > 0f || Input.GetAxis("R_Horizontal") > 0f || Input.GetAxis("D-Pad_Horizontal") > 0f)
        {
            if (nowActiveScreen == stageSelectScreens)
            {
                GoNextPageStageSelect();
            }
            else if (nowActiveScreen == howToPlayScreen)
            {
                GoNextPageHTP(ref howToPlayPageScreen);
            }
        }
    }

    public void MoveBackPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") < 0f || Input.GetAxis("R_Horizontal") < 0f || Input.GetAxis("D-Pad_Horizontal") < 0f)
        {
            if (nowActiveScreen == stageSelectScreens)
            {
                GoBackPageStageSelect();
            }
            else if (nowActiveScreen == howToPlayScreen)
            {
                GoBackPageHTP(ref howToPlayPageScreen);
            }
        }
    }

    public void ClickCancelButton()
    {
        if (nowEffecting)
            return;

        if (nowActiveScreen == stageSelectScreens)
        {
            StageSelectScreenReset();
            fadeState = FadeState.StageSelect;
        }
        else if (nowActiveScreen == howToPlayScreen)
        {
            fadeState = FadeState.HowToPlay;
        }

        selectButton = continueButton;
        nowActiveScreen = titleAndMenuScreen;
        gameData.MeinMenuTransition();
    }

    public void Select()
    {
        if (selectButton != null && eventSystem.GetComponent<EventSystem>().currentSelectedGameObject == selectButton.gameObject)
        {
            selectButton = null;
        }
        else
        {
            Sound.PlaySE("PencilSelect", 0.75f);
        }
    }
    #endregion


    #region PRIVATE BUTTON METHOD
    // 以下の各画面ごとの処理内容は演出の仕方によって要変更
    //---------- タイトル画面時の処理 ----------//
    void PressAnyKey()
    {
        if (nowEffecting)
            return;

        if (gameData.showStartStory[0])
        {
            fadeState = FadeState.Title;
            selectButton = continueButton;
            Sound.PlaySE("PencilPush", 1f);
            gameData.MeinMenuTransition();
        }
        else
        {
            goNextScene = true;
            goShowStartStory = true;
        }
    }

    //---------- ステージ選択画面時の処理 ----------//
    void GoNextPageStageSelect()
    {
        _BookUI.GoToNextPage();
        
        for (int i = 0; i < stageSelectPage.Length - 1; i++)
        {
            if (stageSelectPage[i].stageActive)
            {
                // 次の画面の一番若いステージが解放されてなければ、以下の処理はしない
                if (!stageSelectPage[i + 1].buttonActive[0])
                    return;

                // 次のページのUIを解放されている物はすべてアクティブ化
                stageSelectPage[i + 1].stageSelectScreen.SetActive(true);
                stageSelectPage[i + 1].stageActive = true;
                for(int j = 0; j < stageSelectPage[i + 1].stageButton.Length; j++)
                {
                    if(stageSelectPage[i + 1].buttonActive[j])
                    {
                        stageSelectPage[i + 1].stageButton[j].SetActive(true);
                    }
                    else
                    {
                        stageSelectPage[i + 1].stageButton[j].SetActive(false);
                    }
                }
                //// 今表示中のUIをすべて非アクティブ化
                //stageSelectPage[i].stageSelectScreen.SetActive(false);
                //stageSelectPage[i].stageActive = false;
                //for (int j = 0; j < stageSelectPage[i].stageButton.Length; j++)
                //{
                //    stageSelectPage[i].stageButton[j].SetActive(false);
                //}
                // ボタンの選択状態を指定
                stageSelectPage[i + 1].stageButton[0].GetComponent<Button>().Select();

                return;
            }
        }
    }

    void GoBackPageStageSelect()
    {
        _BookUI.BackToPrevPage();

        for (int i = 1; i < stageSelectPage.Length; i++)
        {
            if (stageSelectPage[i].stageActive)
            {
                // 次のページのUIをすべてアクティブ化
                stageSelectPage[i - 1].stageSelectScreen.SetActive(true);
                stageSelectPage[i - 1].stageActive = true;
                for (int j = 0; j < stageSelectPage[i - 1].stageButton.Length; j++)
                {
                    if (stageSelectPage[i - 1].buttonActive[j])
                    {
                        stageSelectPage[i - 1].stageButton[j].SetActive(true);
                    }
                    else
                    {
                        stageSelectPage[i - 1].stageButton[j].SetActive(false);
                    }
                }
                //// 今表示中のUIをすべて非アクティブ化
                //stageSelectPage[i].stageSelectScreen.SetActive(false);
                //stageSelectPage[i].stageActive = false;
                //for (int j = 0; j < stageSelectPage[i].stageButton.Length; j++)
                //{
                //    stageSelectPage[i].stageButton[j].SetActive(false);
                //}
                // ボタンの選択状態を指定
                var buttonNum = stageSelectPage[i - 1].stageButton.Length - 1;
                stageSelectPage[i - 1].stageButton[buttonNum].GetComponent<Button>().Select();

                return;
            }
        }
    }

    void StageSelectScreenReset()
    {
        int buttonCount = 0;
        for (int i = 0; i < stageSelectPage.Length; i++)
        {
            // stageSelectScreen へ格納
            stageSelectPage[i].stageSelectScreen = stageSelectPageScreen[i];

            // stageButton, buttonActive へ格納
            for (int j = 0; j < pageButtonNum[i]; j++)
            {
                // stageButtonの処理
                if (buttonCount < stageButton.Length)
                {   // 各ページ内で表示するボタンのデータを格納
                    stageSelectPage[i].stageButton[j] = stageButton[buttonCount].gameObject;
                }
                else
                {
                    Debug.LogWarning("ステージセレクトのデータ格納中、範囲外にアクセスしようとしました。");
                }

                // buttonActiveの処理
                if (buttonCount == 0 || buttonCount <= gameData.notClearStageNum)
                {
                    stageSelectPage[i].buttonActive[j] = true;
                }
                else
                {
                    stageSelectPage[i].buttonActive[j] = false;
                }

                buttonCount++;
            }
        }
    }

    // ステージセレクト時のステージボタンをアクティブにするかの処理
    void StageButtonActivity()
    {
        for (int i = 0; i < stageSelectPage[0].buttonActive.Length; i++)
        {
            if (stageSelectPage[0].buttonActive[i])
            {
                stageSelectPage[0].stageButton[i].SetActive(true);
            }
            else
            {
                stageSelectPage[0].stageButton[i].SetActive(false);
            }
        }
    }

    bool ComicCheck(string selectStage)
    {
        if (selectStage == gameData.stageScene[2] && !gameData.showStartStory[1])
        {
            Sound.BGMAndSEResets();
            SceneManager.LoadScene("Comic02");
            return true;
        }
        else if (selectStage == gameData.stageScene[4] && !gameData.showStartStory[2])
        {
            Sound.BGMAndSEResets();
            SceneManager.LoadScene("Comic03");
            return true;
        }
        else if (selectStage == gameData.stageScene[6] && !gameData.showStartStory[3])
        {
            Sound.BGMAndSEResets();
            SceneManager.LoadScene("Comic04");
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}

public class StageSelectPage
{
    public GameObject stageSelectScreen;
    public GameObject[] stageButton;      // そのステージセレクト画面で使うボタン
    public bool[] buttonActive;           // ステージボタンが選択可能か
    public bool stageActive;              // ステージ画面がアクティブ状態か
}