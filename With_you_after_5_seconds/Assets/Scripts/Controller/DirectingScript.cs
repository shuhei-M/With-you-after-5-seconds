using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;  // Cinemachineを追加でusing
using UnityEngine.SceneManagement;

public class DirectingScript : MonoBehaviour
{
    #region Serialize Field
    [Header("VirtualCamera")]
    [SerializeField] private CinemachineVirtualCamera[] startViewVC;
    [SerializeField] private CinemachineVirtualCamera mainVirtualCinemachine;
    [SerializeField] private CinemachineVirtualCamera startVirtualCinemachine;
    [SerializeField] private CinemachineVirtualCamera goalVirtualCinemachine;
    [SerializeField] private CinemachineVirtualCamera sectionPointCinemachine;

    [Header("演出共通")]
    [SerializeField] private bool bird = true;
    [SerializeField] private float[] changeBirdPosision; // 最後の数値が鳥の終了値
    [SerializeField] private float changeCameraSec = 2.0f; // 鳥を飛ばさない場合のカメラ遷移時間
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject afterimage;
    

    [Header("スタート演出")]
    [SerializeField] private GameObject gameStartPoint;
    [SerializeField] private float startRunSec = 3.0f;
    [SerializeField] private float startTurnSec = 1.0f;
    [SerializeField] private bool startCamSkip;

    [Header("ゴール演出")]
    [SerializeField] private GameObject goalArea;
    [SerializeField] private GameObject gatePoint;
    [SerializeField] private GameObject goalTouchWall;
    [SerializeField] private float goalRunSec = 1.5f;
    [SerializeField] private float goalTurnSec = 1.5f;
    #endregion


    #region Public Field
    public GameData gameData;
    public static GameObject playerOnTrigger;
    #endregion


    #region Field
    // 演出共通
    MyCinemachineDollyCart myCinemachineDollyCart;
    CharacterController charaCon_p;
    Animator animator_p;
    string nowStageName;
    float step = -1;
    float goGateTime = 0;
    bool viewing = false;

    // スタート演出
    Vector3 startVec;
    bool cameraCycleOK = false;
    Stage01LookAtPlayerCameraController LookAtPlayerController;

    // ゴール演出
    Vector3 goalHeadingVec;

    /// <summary> warpエフェクトを再生するかどうか（松島） </summary>
    private bool _IsStartWarpEffect = false;
    #endregion


    #region Propaty
    public bool IsStartWarpEffect { get { return _IsStartWarpEffect; } }
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        // 初期化処理
        SetUp();

        // VCカメラの初期優先順位を設定
        StartVCPrioritySet();

#if UNITY_EDITOR
        // 実行時アウトゲームの状態がタイトル、メインメニュー、ステージセレクト以外ならタイトル状態にする
        LastTimeStateCheck();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // スタート時の演出
        if (gameData.InGameState == InGame.ChangeStartView)
        {
            StartView();
        }
        if (gameData.InGameState == InGame.EntryPlayer)
        {
            StartDirecting();
            return;
        }
        if (gameData.InGameState == InGame.PlayGame && startVirtualCinemachine.Priority != 10)
        {
            StartCameraControll();
        }

        // ゴール時の処理
        if (playerOnTrigger == goalArea && gameData.InGameState == InGame.PlayGame)
        {
            gameData.EntryGoalTransition();
            goalHeadingVec = gatePoint.transform.position - player.transform.position;
            goalHeadingVec.y = 0;
            player.transform.rotation = Quaternion.LookRotation(goalHeadingVec);
        }
        else if (gameData.InGameState == InGame.EntryGoal)
        {
            EntryGoalDirecting();
        }
        else if(gameData.InGameState == InGame.InGoal)
        {
            InGoalDirecting();
        }
    }
    #endregion


    #region Method
    // Start内で呼ぶ初期化処理
    void SetUp()
    {
        // スタートとゴールどちらにも共通する初期処理
        charaCon_p = player.GetComponent<CharacterController>();
        animator_p = player.GetComponent<Animator>();

        // スタート時の演出に関する初期処理
        startVec = gameStartPoint.transform.position - player.transform.position;
        startVec.y = 0f;
        player.transform.rotation = Quaternion.LookRotation(startVec);
        if (bird)
        {
            myCinemachineDollyCart = GameObject.Find("Bird").GetComponent<MyCinemachineDollyCart>();
        }

        // 今のシーンの名前を読み込む
        nowStageName = SceneManager.GetActiveScene().name;
        if (nowStageName == "Stage01")
        {
            //stage01でプレイヤーを見回すカメラのコントローラースクリプトの取得
            LookAtPlayerController = startVirtualCinemachine.GetComponent<Stage01LookAtPlayerCameraController>();
        }
    }

    // VCカメラの初期優先順位を設定処理
    void StartVCPrioritySet()
    {
        if (!startCamSkip)
        {
            for (int i = 0; i < startViewVC.Length; i++)
            {
                if (i == 0)
                {
                    startViewVC[i].Priority = 100;
                }
                else
                {
                    startViewVC[i].Priority = 10;
                }
            }
        }

        mainVirtualCinemachine.Priority = 50;
        startVirtualCinemachine.Priority = 10;
        goalVirtualCinemachine.Priority = 10;
    }

    // 実行時アウトゲームの状態がタイトル、メインメニュー、ステージセレクト以外ならタイトル状態にする
    // exeファイルにするときは消した方がいいかも
    void LastTimeStateCheck()
    {
        if (gameData.OutGameState != OutGame.None ||
            gameData.InGameState != InGame.CutIn)
        {
            //gameData.ChangeStartViewTransition();
            gameData.ChangeCutInTransition();
        }
    }

    // スタート時のカメラ処理
    void StartView()
    {
        if (!viewing)
            StartCoroutine("StartViewChange");
    }

    void StartCameraControll()
    {
        startVirtualCinemachine.Priority = 10;
    }

    IEnumerator StartViewChange()
    {
        // カメラ旋回時プレイヤーを非表示
        player.SetActive(false);
        viewing = true;
        if (!startCamSkip)
        {
            for (int i = 0; i < startViewVC.Length; i++)
            {
                if (bird)
                {
                    yield return new WaitUntil(() => changeBirdPosision[i] <= myCinemachineDollyCart.BirdPosition);
                }
                else
                {
                    yield return new WaitForSeconds(changeCameraSec);
                }
                startViewVC[i].Priority = 10;
                if (i < startViewVC.Length - 1)
                {
                    startViewVC[i + 1].Priority = 100;
                }
                else
                {
                    startVirtualCinemachine.Priority = 100;
                }
            }
        }
        else
        {
            mainVirtualCinemachine.Priority = 50;
        }
        gameData.EntryPlayerTransition();
        // カメラ旋回終了後プレイヤーと残像を表示
        player.SetActive(true);
        afterimage.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);

        //if (bird)
        //{   // 鳥生存時のカメラ遷移（ステージ１～４）
        //    // カメラ旋回時プレイヤーを非表示
        //    player.SetActive(false);
        //    viewing = true;
        //    for (int i = 0; i < startViewVC.Length; i++)
        //    {
        //        yield return new WaitUntil(() => changeBirdPosision[i] <= myCinemachineDollyCart.BirdPosition);
        //        startViewVC[i].Priority = 10;
        //        if (i < startViewVC.Length - 1)
        //        {
        //            startViewVC[i + 1].Priority = 100;
        //        }
        //        else
        //        {
        //            startVirtualCinemachine.Priority = 100;
        //        }
        //    }
        //    gameData.EntryPlayerTransition();
        //    // カメラ旋回終了後プレイヤーと残像を表示
        //    player.SetActive(true);
        //    afterimage.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        //}
        //else
        //{

        //}
    }


    // スタート演出処理関数
    void StartDirecting()
    {
        // プレイヤーがステージ内に入ってくる演出処理
        if (gameStartPoint != playerOnTrigger)
        {
            charaCon_p.Move(startVec * Time.deltaTime / startRunSec);
            animator_p.SetFloat("DeltaTime", 1.0f);
            animator_p.SetFloat("MoveBlend", 1.0f);
        }
        else if (PlayerTurn(gameStartPoint, startTurnSec))
        {
            if (nowStageName == "Stage01" && !cameraCycleOK)
            {
                animator_p.SetFloat("DeltaTime", 0.0f);
                animator_p.SetFloat("MoveBlend", 0.0f);

                LookAtPlayerController.LookAtPlayerCameraStart();
                if (LookAtPlayerController.EndPathPos())
                    cameraCycleOK = true;
            }
            else
            {
                Debug.Log("スタート地点到着");
                animator_p.SetFloat("DeltaTime", 0.0f);
                animator_p.SetFloat("MoveBlend", 0.0f);
                gameData.PlayGameTransition();
            }
        }
    }

    // ゴールゲートに移動しきるまでの処理
    void EntryGoalDirecting()
    {
        //演出用のバーチャルシネマシーンに変える
        if (goalVirtualCinemachine.Priority != 100)
        {
            goalVirtualCinemachine.Priority = 100;
        }

        // 門の前に来たらいったん止まって旋回
        if (playerOnTrigger == gatePoint)
        {
            if (PlayerTurn(gatePoint, goalTurnSec))
            {
                // 旋回し終わったらまっすぐ門を潜り抜けるベクトルを取得
                gameData.InGoalTransition();
                goalHeadingVec = gatePoint.transform.forward;
                player.transform.rotation = Quaternion.LookRotation(goalHeadingVec);
                goalTouchWall.SetActive(false);
            }
        }
        else
        {
            charaCon_p.Move(goalHeadingVec * Time.deltaTime / goalRunSec);
        }
    }

    //ゴールゲートを通り抜ける処理
    void InGoalDirecting()
    {
        float speed = 3.0f;
        
        //animator_p.SetFloat("DeltaTime", 0.0f);
        //animator_p.SetFloat("MoveBlend", 0.0f);
        if (goGateTime >= goalRunSec)
        {
            sectionPointCinemachine.Priority = 100;
            goalVirtualCinemachine.Priority = 10;
            player.SetActive(false);
            _IsStartWarpEffect = true;
        }
        else
        {
            charaCon_p.Move(goalHeadingVec * Time.deltaTime * speed / goalRunSec);
            goGateTime += Time.deltaTime;
        }
    }

    // 旋回中は false を返す。旋回が終われば true を返す関数
    bool PlayerTurn(GameObject pointObj, float sec)
    {
        var pointEuler = pointObj.transform.rotation.eulerAngles;

        if (step == -1)
        {
            var playerEuler = player.transform.rotation.eulerAngles;
            var speed = Mathf.Abs(pointEuler.y - playerEuler.y);
            if (speed > 180)
            {
                speed = 360 - speed;
            }
            step = speed * Time.deltaTime / sec;
        }

        player.transform.rotation = Quaternion.RotateTowards
            (player.transform.rotation, Quaternion.Euler(0, pointEuler.y, 0), step);

        if (player.transform.rotation.eulerAngles.y == pointEuler.y)
        {
            step = -1;
            return true;
        }

        return false;
    }
    #endregion
}
