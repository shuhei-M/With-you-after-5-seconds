using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Storage;

// ゲームのクリア状況やステージ名を管理するクラス
[CreateAssetMenu(fileName = "game_data", menuName =
        "ScriptableObjects/GameData", order = 1)]
public partial class GameData : ScriptableObject
{
    #region DEFINE
    /// <summary>
	/// 処理状態
	/// </summary>
	private enum STATE
    {
        IDLE,
        SAVING,
        LOADING,
        ASYNC_WAIT,
        FINISH,
    }
    #endregion


    #region DATA
    public string[] stageScene = {"Stage01", "Stage02", "Stage03", "Stage04", "Stage05",
                                  "Stage06", "Stage07", "Stage08"};
    public int notClearStageNum;
    public StageData[] stageData = new StageData[8];
    public bool Editor_StartGame = false;      // ゲームを開始した際に立てるフラグ
    public bool[] showStartStory = new bool[5]; // スタートストーリーを見たか
    #endregion


    #region MEMBER
    private StorageManager storageManager = null;   // ストレージ制御
    private UserSettings usedSettings = null;       // 現在のデータ
    private UserSettings procSettings = null;       // 処理中のデータ
    private FinishHandler ioHandler = null;         // ストレージアクセスコールバック
    private STATE state = STATE.IDLE;               // 処理状態
    private IO_RESULT result = IO_RESULT.NONE;      // 結果
    private float ioTime = 0f;                      // 処理開始時刻
    private bool loadFlag;
    #endregion


    #region STORAGE MAIN FUNCTION
    /// <summary>
	/// 初期化
	/// </summary>
	public void Start()
    {   // TitleScript.cs の Start() で呼ぶ
        this.ioHandler = new FinishHandler(this.IOHandler);
        this.storageManager = new StorageManager();
        this.usedSettings = this.procSettings = new UserSettings();

        // 例外
        this.UpdateDataInfo((IO_RESULT)999);

        loadFlag = false;
    }

    /// <summary>
	/// 更新
	/// </summary>
	public void Update()
    {   // TitleScript.cs の Update() で呼ぶ
        // キャプチャ
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.P))
            ScreenCapture.CaptureScreenshot("capture.png");
#endif

        switch (this.state)
        {
            case STATE.IDLE:
                break;
            case STATE.ASYNC_WAIT:
                IEnumerator iEnumerator = AsyncWait();
                iEnumerator.MoveNext();                 //MonoBehaviour.StartCoroutine(this.AsyncWait()); の代わり
                this.state = STATE.FINISH;
                break;
            case STATE.FINISH:
                break;
        }
    }
    #endregion


    #region STORAGE PRIVATE FUNCTION
    /// <summary>
	/// 完了コールバック
	/// </summary>
	/// <param name="ret">結果</param>
	/// <param name="dataInfo">結果情報</param>
	private void IOHandler(IO_RESULT ret, ref DataInfo dataInfo)
    {
        this.result = ret;
        switch (ret)
        {
            case IO_RESULT.SAVE_SUCCESS:
                // 保存成功
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.UpdateDataInfo(ret);
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.SAVE_FAILED:
                // 保存失敗
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.LOAD_SUCCESS:
                // 読込成功
                this.procSettings = dataInfo.serializer as UserSettings;
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.UpdateDataInfo(ret);
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.LOAD_FAILED:
                // 読込失敗
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.NONE:
                // データ不備
                Debug.LogWarning("データに不備あり...。");
                this.state = STATE.IDLE;
                break;
        }
    }

    /// <summary>
	/// 非同期完了待ち
	/// </summary>
	private IEnumerator AsyncWait()
    {
        // 非同期最低待ち時間（※保存や読込のアニメーションを一定時間表示させるため）
        while ((Time.realtimeSinceStartup - this.ioTime) < 1.8f)
            yield return null;

        this.UpdateDataInfo(this.result);
    }

    /// <summary>
	/// データ情報更新
	/// </summary>
	/// <param name="result">結果</param>
	private void UpdateDataInfo(IO_RESULT result)
    {
        UserSettings us = this.usedSettings = this.procSettings;
        //us.format = this.control.saveJson.isOn ? FORMAT.JSON : FORMAT.BINARY;
        //us.encrypt = this.control.saveEncrypt.isOn;
        //us.backup = this.control.saveBackup.isOn;
        if (loadFlag)
        {
            for(int i = 0; i < stageData.Length; i++)
            {
                stageData[i] = us.stageData[i];
            }
            notClearStageNum = us.notClearStageNum;
            for (int i = 0; i < showStartStory.Length; i++)
                showStartStory[i] = us.showStartStory[i];
            loadFlag = false;
        }
        this.state = STATE.IDLE;

        switch (result)
        {
            case IO_RESULT.NONE:
                Debug.LogWarning("未実行終了（データ不備）");
                break;
            case IO_RESULT.SAVE_SUCCESS:
            case IO_RESULT.LOAD_SUCCESS:
                Debug.Log("成功");
                break;
            case IO_RESULT.SAVE_FAILED:
            case IO_RESULT.LOAD_FAILED:
                Debug.LogError("失敗（保存ファイルの破損等、データがおかしい）");
                break;
            default:
                break;
        }
    }
    #endregion


    #region DATA PUBLIC METHOD
    // シーン開始時データの読込・書込を行う処理
    public void StartData()
    {
#if UNITY_EDITOR
        // エディター上で初回起動時のフラグを false にしたら、
        // 実行ファイル起動時を想定して実行される
        if (!Editor_StartGame)
        {
            SetFirstData();
            Save();
        }
#endif
        if (this.storageManager.Exists(usedSettings))
        {
            Load();
        }
        else
        {
            // 初期データを作って、ローカルストレージを作成
            SetFirstData();
            Save();
        }

#if UNITY_EDITOR
        // エディター上で初回起動時のフラグを false にしたら、
        // 実行ファイル起動時を想定して実行される
        if (!Editor_StartGame)
        {
            TitleTransition();
            Editor_StartGame = true;
            return;
        }
#endif
        if(OutGameState != OutGame.MeinMenu)
        {
            TitleTransition();
        }
    }

    // エディタ内でタイトル以外からスタートするとエラーが発生する
    // 場合があるので、それを防ぐための関数
    // StageUIScript.cs で呼び出ししている
    public void EditorStart()
    {
#if UNITY_EDITOR
        if (storageManager == null)
        {
            Start();
            Load();
        }
#endif
    }

    // ステージのクリア状態とアイテム取得状態をリセット
    public void ResetData() 
    {
        SetFirstData();
        Save();
    }

    // ステージのクリア状態とアイテム取得状態を全てできている状態にアウル
    public void AllClearData()
    {
        notClearStageNum = 8;

        for (int i = 0; i < stageData.Length; i++)
        {
            stageData[i].selectOK = true;
            stageData[i].itemGetFlag = true;
        }

        for(int i = 0; i < showStartStory.Length; i++)
        {
            showStartStory[i] = true;
        }

        // データの保存
        Save();
    }

    // スタートストーリーを見た時の処理
    public void ShowStartStoryComp(int num)
    {
        showStartStory[num] = true;
        Save();
    }

    // クリアしたステージの次のステージができるように格納
    // また、アイテムをとっていたらそのデータも格納
    public void ClearStageDataStorage(int stageNum, bool itemGet)
    {
        // 次が解放されていないステージ以外は処理を行わない
        if (stageNum != notClearStageNum)
            return;

        notClearStageNum++;
        stageData[stageNum + 1].selectOK = true;
        if (itemGet)
        {
            stageData[stageNum].itemGetFlag = true;
        }

        // データの保存
        Save();
    }
    #endregion


    #region DATA PRIVATE METHOD
    // 初期データを作る
    void SetFirstData()
    {
        notClearStageNum = 0;

        for (int i = 0; i < stageData.Length; i++)
        {
            if (stageData[i] == null)
            {
                stageData[i] = new StageData();
            }

            if (i == 0)
            {
                stageData[i].SetData(true, false, stageScene[i]);
            }
            else
            {
                stageData[i].SetData(false, false, stageScene[i]);
            }
        }

        for(int i = 0; i < showStartStory.Length; i++)
            showStartStory[i] = false;
    }
#endregion


    #region CONTROL FUNCTION
    // データを保存する関数
    void Save()
    {
        // 保存設定
        this.ioTime = Time.realtimeSinceStartup;

        // 保存設定（※デモ用の設定変更）
        this.procSettings = this.usedSettings.Clone() as UserSettings;
        UserSettings us = this.procSettings;

        // 内容更新
        for(int i = 0; i < us.stageData.Length; i++)
        {
            us.stageData[i] = this.stageData[i];
        }
        us.notClearStageNum = this.notClearStageNum;
        for (int i = 0; i < showStartStory.Length; i++)
            us.showStartStory[i] = this.showStartStory[i];

        // 保存（※FinishHandlerはnullでも可）
        bool async = false; // 非同期処理はしない
        this.storageManager.Save(this.procSettings, this.ioHandler, async);
    }

    // データを読み込む関数
    public void Load()
    {
        this.ioTime = Time.realtimeSinceStartup;

        loadFlag = true;

        // 読込
        bool async = false; // 非同期処理はしない
        this.storageManager.Load(this.usedSettings, this.ioHandler, async);
    }
    #endregion
}

[System.Serializable]
public class StageData
{
    public bool selectOK;
    public bool itemGetFlag;
    public string sceneName;

    public void ClearData()
    {
        selectOK = false;
        itemGetFlag = false;
        sceneName = null;
    }

    public void SetData(bool select, bool item, string scene)
    {
        selectOK = select;
        itemGetFlag = item;
        sceneName = scene;
    }
}
