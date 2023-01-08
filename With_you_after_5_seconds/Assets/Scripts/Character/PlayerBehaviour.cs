using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

//using State = StateMachine<PlayerController>.State;

public partial class PlayerBehaviour : MonoBehaviour, IPlayGimmick
{
    #region serialize field
    /// <summary> ステージ内に配置された残像の位置など有用な情報を取得する </summary>
    [SerializeField] private StageController _StageManager;

    /// <summary> 移動・回転時のスピード。デフォルトは（3, 5） </summary>
    [SerializeField] private float _MoveSpeed = 3.0f;
    [SerializeField] private float _RotationSpeed = 5.0f;

    /// <summary> フォーカス具合を変更するvirtualカメラ </summary>
    [SerializeField] private CinemachineVirtualCamera _MainVirtualCamera;

    public GameData gameData;   //（渡邊）
    #endregion

    #region field
    /// <summary> プレイヤーにアタッチされたコンポーネントを取得するための変数群 </summary>
    private CharacterController _CharaCon;
    private Animator _Animator;
    private AnimatorStateInfo _StateInfo;

    /// <summary> 移動・回転を行うための変数群 </summary>
    private Camera _MyCamera;
    private Vector3 _Velocity;   // アナログスティックを3次元ベクトルに変更
    private float _MoveBlend;
    private float _DeltaTime = 0f;
    private bool _IsInputable = true;   // コントローラからの入力を受け付けるかどうか

    /// <summary> プレイヤーの状態の変化を感知するための変数群 </summary>
    private PlayerState _CurrentState;
    private PlayerState _PrevState;
    private bool _IsAlive = true;
    private bool _IsLadder = false;   // 梯子を上っているか

    /// <summary> virtualカメラのフォーカス具合を変更するための変数群 </summary>
    private float _LensVerticalFOV = 45.0f;
    private float _MinLensVerticalFOV = 45.0f;
    private float _MaxLensVerticalFOV = 70.0f;
    private bool _IsLensReset = false;

    /// <summary> プレイヤーの表示・非表示を行うための変数 </summary>
    private GameObject _BodyMeshs;

    /// <summary> 使用するアクションボタン </summary>
    IButtonComponent _ActionButton;
    /// <summary> ヒントギミック </summary>
    HintGimmick _HintGimmick;

    private bool isGoingOut = false;
    private bool canRotate = true;
    #endregion

    #region property
    /// <summary> プレイヤーの再生されているアニメーションステートを取得 </summary>
    public AnimatorStateInfo animatorStateInfo { get { return _StateInfo; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _CharaCon = GetComponent<CharacterController>();

        _MyCamera = Camera.main;
        _Animator = GetComponent<Animator>();

        _BodyMeshs = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        HiddenPlayerBody();   // ゲーム開始前にプレイヤーを非表示にする。

        // アクションボタンの取得
        var canvas = GameObject.Find("StageCanvas").gameObject;
        var defaultPanel = canvas.transform.Find("DefaultPanel").gameObject;
        _ActionButton = defaultPanel.transform.Find("ActionButton").gameObject.GetComponent<IButtonComponent>();
        // ヒントギミックの取得
        var gimmicks = GameObject.Find("Gimmicks").gameObject;
        var hintGimmickSet = gimmicks.transform.Find("HintGimmick").gameObject;
        _HintGimmick = hintGimmickSet.transform.Find("HintArea").gameObject.GetComponent<HintGimmick>();

        // デバッグ用なので消しても問題なし
        _ActionType = ActionType.Default;

        // ステートマシンの設定を行う
        SetUpStateMachine();
    }

    // Update is called once per frame
    void Update()
    {
        _CurrentState = _State;
        DebugFunction();

        //見えない壁のほうには歩かないようにしたい(芝)
        BarrierPlayer();

        // プレイヤーがステージに入場する演出
        if (!_BodyMeshs.activeSelf && gameData.InGameState == InGame.EntryPlayer) DisplayPlayerBody();

        // 死亡状態では以降の処理は行わない
        if (!_IsAlive) return;

        _StateInfo = _Animator.GetCurrentAnimatorStateInfo(0);

        // ゲームプレイ時以外以下の処理を行わない（渡邊）
        if (gameData.InGameState != InGame.PlayGame)
            return;

        _Velocity = GetInput_Move();

        _StateMachine.Update();

        // 実験用：Backspaceで死亡状態に強制的に遷移
        // if (Input.GetKeyDown(KeyCode.Backspace)) _StateMachine.Dispatch((int)Event.Dead);

        _PrevState = _CurrentState;
    }
    #endregion

    #region public function
    /// <summary>
    /// プレイヤーの体を非表示にする
    /// </summary>
    public void HiddenPlayerBody()
    {
        if (!_BodyMeshs.activeSelf) return;
        _BodyMeshs.SetActive(false);
    }

    /// <summary>
    /// プレイヤーの体を表示させる
    /// </summary>
    public void DisplayPlayerBody()
    {
        if (_BodyMeshs.activeSelf) return;
        _BodyMeshs.SetActive(true);
    }
    #endregion

    #region private function
    /// <summary>
    /// デバッグ用
    /// </summary>
    protected void DebugFunction()
    {
        // 体の表示・非表示の切り替え
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_BodyMeshs.activeSelf)
            {
                HiddenPlayerBody();
            }
            else
            {
                DisplayPlayerBody();
            }
        }
        
        if (_PrevState == _CurrentState) return;

        Debug.Log(_CurrentState);
    }

    /// <summary>
    /// virtualカメラのフォーカス具合を変更する
    /// </summary>
    private void UpdateLensVerticalFOV()
    {
        float speed = (_MaxLensVerticalFOV - _MinLensVerticalFOV) * 0.5f;

        // ボタン押し込み（長押し）
        if (Input.GetButton("Lens") && _LensVerticalFOV < _MaxLensVerticalFOV)
        {
            _IsLensReset = false;
            _LensVerticalFOV += (Time.deltaTime * speed);
            if (_LensVerticalFOV > _MaxLensVerticalFOV) _LensVerticalFOV = _MaxLensVerticalFOV;
            _MainVirtualCamera.m_Lens.FieldOfView = _LensVerticalFOV;
            //Debug.Log(_LensVerticalFOV);
        }

        // ボタンから離す
        if (Input.GetButtonUp("Lens") && _LensVerticalFOV > _MinLensVerticalFOV)
        {
            _IsLensReset = true;
        }

        if(_IsLensReset)
        {
            _LensVerticalFOV -= (Time.deltaTime * speed);
            if (_LensVerticalFOV < _MinLensVerticalFOV) 
            { 
                _LensVerticalFOV = _MinLensVerticalFOV;
                _IsLensReset = false;
            }
            _MainVirtualCamera.m_Lens.FieldOfView = _LensVerticalFOV;
        }
    }

    /// <summary>
    /// 制作者：芝
    /// 見えない壁のほうには歩かないようにしたい
    /// </summary>
    private void BarrierPlayer()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up + transform.forward / 4f, transform.forward / 2f);
        if (Physics.Raycast(ray, out hit, 1f) && hit.collider.tag == "Wall")
            isGoingOut = true;
        else
            isGoingOut = false;

        Debug.DrawLine(ray.origin, ray.origin + ray.direction);
    }
    #endregion

    #region IPlayGimmick Interface fanction
    /// <summary> インターフェイス用の関数群 </summary>

    /// <summary>
    /// プレイヤーの入力を外部から取得
    /// </summary>
    /// <returns></returns>
    public Vector3 Get_InputVelocity()
    {
        return _Velocity;
    }

    /// <summary>
    /// ワールド座標系のプレイヤーの移動方向を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 Get_WorldVelocity()
    {
        return _WorldVec;
    }

    /// <summary>
    /// プレイヤーの座標を取得
    /// </summary>
    /// <returns></returns>
    public Transform Get_Transform()
    {
        return this.transform;
    }

    /// <summary>
    /// キャラクターコントローラーをONにする
    /// </summary>
    public void OFF_CharacterController()
    {
        _CharaCon.enabled = false;
        _IsLadder = true;
    }

    /// <summary>
    /// キャラクターコントローラーをOFFにする
    /// </summary>
    public void ON_CharacterController()
    {
        _CharaCon.enabled = true;
        _IsLadder = false;
    }

    /// <summary>
    /// プレイヤーを上方向に無理やり移動させる。梯子用。
    /// </summary>
    /// <param name="y"></param>
    public void Move_CharaCon(float y)
    {
        Vector3 up = new Vector3(0, y, 0);
        _CharaCon.Move(up);
    }

    /// <summary>
    /// アクションタイプを外部から、取得・変更する。
    /// </summary>
    public ActionType ActionTypeP
    {
        get { return _ActionType; }
        set { _ActionType = value; }
    }

    /// <summary>
    /// プレイヤーの現在の状態（ステート）を取得する。
    /// </summary>
    /// <returns></returns>
    public PlayerState Get_PlayerState()
    {
        return _CurrentState;
    }

    /// <summary>
    /// コントローラからの入力を受け付けるかどうか
    /// </summary>
    /// <param name="enabled"></param>
    public void InputSetActive(bool enabled)
    {
        _IsInputable = enabled;
    }
    #endregion
}
