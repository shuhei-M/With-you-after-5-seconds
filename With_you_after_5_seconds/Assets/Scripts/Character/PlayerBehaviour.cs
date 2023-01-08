using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

//using State = StateMachine<PlayerController>.State;

public partial class PlayerBehaviour : MonoBehaviour, IPlayGimmick
{
    #region serialize field
    /// <summary> �X�e�[�W���ɔz�u���ꂽ�c���̈ʒu�ȂǗL�p�ȏ����擾���� </summary>
    [SerializeField] private StageController _StageManager;

    /// <summary> �ړ��E��]���̃X�s�[�h�B�f�t�H���g�́i3, 5�j </summary>
    [SerializeField] private float _MoveSpeed = 3.0f;
    [SerializeField] private float _RotationSpeed = 5.0f;

    /// <summary> �t�H�[�J�X���ύX����virtual�J���� </summary>
    [SerializeField] private CinemachineVirtualCamera _MainVirtualCamera;

    public GameData gameData;   //�i�n粁j
    #endregion

    #region field
    /// <summary> �v���C���[�ɃA�^�b�`���ꂽ�R���|�[�l���g���擾���邽�߂̕ϐ��Q </summary>
    private CharacterController _CharaCon;
    private Animator _Animator;
    private AnimatorStateInfo _StateInfo;

    /// <summary> �ړ��E��]���s�����߂̕ϐ��Q </summary>
    private Camera _MyCamera;
    private Vector3 _Velocity;   // �A�i���O�X�e�B�b�N��3�����x�N�g���ɕύX
    private float _MoveBlend;
    private float _DeltaTime = 0f;
    private bool _IsInputable = true;   // �R���g���[������̓��͂��󂯕t���邩�ǂ���

    /// <summary> �v���C���[�̏�Ԃ̕ω������m���邽�߂̕ϐ��Q </summary>
    private PlayerState _CurrentState;
    private PlayerState _PrevState;
    private bool _IsAlive = true;
    private bool _IsLadder = false;   // ��q������Ă��邩

    /// <summary> virtual�J�����̃t�H�[�J�X���ύX���邽�߂̕ϐ��Q </summary>
    private float _LensVerticalFOV = 45.0f;
    private float _MinLensVerticalFOV = 45.0f;
    private float _MaxLensVerticalFOV = 70.0f;
    private bool _IsLensReset = false;

    /// <summary> �v���C���[�̕\���E��\�����s�����߂̕ϐ� </summary>
    private GameObject _BodyMeshs;

    /// <summary> �g�p����A�N�V�����{�^�� </summary>
    IButtonComponent _ActionButton;
    /// <summary> �q���g�M�~�b�N </summary>
    HintGimmick _HintGimmick;

    private bool isGoingOut = false;
    private bool canRotate = true;
    #endregion

    #region property
    /// <summary> �v���C���[�̍Đ�����Ă���A�j���[�V�����X�e�[�g���擾 </summary>
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
        HiddenPlayerBody();   // �Q�[���J�n�O�Ƀv���C���[���\���ɂ���B

        // �A�N�V�����{�^���̎擾
        var canvas = GameObject.Find("StageCanvas").gameObject;
        var defaultPanel = canvas.transform.Find("DefaultPanel").gameObject;
        _ActionButton = defaultPanel.transform.Find("ActionButton").gameObject.GetComponent<IButtonComponent>();
        // �q���g�M�~�b�N�̎擾
        var gimmicks = GameObject.Find("Gimmicks").gameObject;
        var hintGimmickSet = gimmicks.transform.Find("HintGimmick").gameObject;
        _HintGimmick = hintGimmickSet.transform.Find("HintArea").gameObject.GetComponent<HintGimmick>();

        // �f�o�b�O�p�Ȃ̂ŏ����Ă����Ȃ�
        _ActionType = ActionType.Default;

        // �X�e�[�g�}�V���̐ݒ���s��
        SetUpStateMachine();
    }

    // Update is called once per frame
    void Update()
    {
        _CurrentState = _State;
        DebugFunction();

        //�����Ȃ��ǂ̂ق��ɂ͕����Ȃ��悤�ɂ�����(��)
        BarrierPlayer();

        // �v���C���[���X�e�[�W�ɓ��ꂷ�鉉�o
        if (!_BodyMeshs.activeSelf && gameData.InGameState == InGame.EntryPlayer) DisplayPlayerBody();

        // ���S��Ԃł͈ȍ~�̏����͍s��Ȃ�
        if (!_IsAlive) return;

        _StateInfo = _Animator.GetCurrentAnimatorStateInfo(0);

        // �Q�[���v���C���ȊO�ȉ��̏������s��Ȃ��i�n粁j
        if (gameData.InGameState != InGame.PlayGame)
            return;

        _Velocity = GetInput_Move();

        _StateMachine.Update();

        // �����p�FBackspace�Ŏ��S��Ԃɋ����I�ɑJ��
        // if (Input.GetKeyDown(KeyCode.Backspace)) _StateMachine.Dispatch((int)Event.Dead);

        _PrevState = _CurrentState;
    }
    #endregion

    #region public function
    /// <summary>
    /// �v���C���[�̑̂��\���ɂ���
    /// </summary>
    public void HiddenPlayerBody()
    {
        if (!_BodyMeshs.activeSelf) return;
        _BodyMeshs.SetActive(false);
    }

    /// <summary>
    /// �v���C���[�̑̂�\��������
    /// </summary>
    public void DisplayPlayerBody()
    {
        if (_BodyMeshs.activeSelf) return;
        _BodyMeshs.SetActive(true);
    }
    #endregion

    #region private function
    /// <summary>
    /// �f�o�b�O�p
    /// </summary>
    protected void DebugFunction()
    {
        // �̂̕\���E��\���̐؂�ւ�
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
    /// virtual�J�����̃t�H�[�J�X���ύX����
    /// </summary>
    private void UpdateLensVerticalFOV()
    {
        float speed = (_MaxLensVerticalFOV - _MinLensVerticalFOV) * 0.5f;

        // �{�^���������݁i�������j
        if (Input.GetButton("Lens") && _LensVerticalFOV < _MaxLensVerticalFOV)
        {
            _IsLensReset = false;
            _LensVerticalFOV += (Time.deltaTime * speed);
            if (_LensVerticalFOV > _MaxLensVerticalFOV) _LensVerticalFOV = _MaxLensVerticalFOV;
            _MainVirtualCamera.m_Lens.FieldOfView = _LensVerticalFOV;
            //Debug.Log(_LensVerticalFOV);
        }

        // �{�^�����痣��
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
    /// ����ҁF��
    /// �����Ȃ��ǂ̂ق��ɂ͕����Ȃ��悤�ɂ�����
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
    /// <summary> �C���^�[�t�F�C�X�p�̊֐��Q </summary>

    /// <summary>
    /// �v���C���[�̓��͂��O������擾
    /// </summary>
    /// <returns></returns>
    public Vector3 Get_InputVelocity()
    {
        return _Velocity;
    }

    /// <summary>
    /// ���[���h���W�n�̃v���C���[�̈ړ��������擾
    /// </summary>
    /// <returns></returns>
    public Vector3 Get_WorldVelocity()
    {
        return _WorldVec;
    }

    /// <summary>
    /// �v���C���[�̍��W���擾
    /// </summary>
    /// <returns></returns>
    public Transform Get_Transform()
    {
        return this.transform;
    }

    /// <summary>
    /// �L�����N�^�[�R���g���[���[��ON�ɂ���
    /// </summary>
    public void OFF_CharacterController()
    {
        _CharaCon.enabled = false;
        _IsLadder = true;
    }

    /// <summary>
    /// �L�����N�^�[�R���g���[���[��OFF�ɂ���
    /// </summary>
    public void ON_CharacterController()
    {
        _CharaCon.enabled = true;
        _IsLadder = false;
    }

    /// <summary>
    /// �v���C���[��������ɖ������ړ�������B��q�p�B
    /// </summary>
    /// <param name="y"></param>
    public void Move_CharaCon(float y)
    {
        Vector3 up = new Vector3(0, y, 0);
        _CharaCon.Move(up);
    }

    /// <summary>
    /// �A�N�V�����^�C�v���O������A�擾�E�ύX����B
    /// </summary>
    public ActionType ActionTypeP
    {
        get { return _ActionType; }
        set { _ActionType = value; }
    }

    /// <summary>
    /// �v���C���[�̌��݂̏�ԁi�X�e�[�g�j���擾����B
    /// </summary>
    /// <returns></returns>
    public PlayerState Get_PlayerState()
    {
        return _CurrentState;
    }

    /// <summary>
    /// �R���g���[������̓��͂��󂯕t���邩�ǂ���
    /// </summary>
    /// <param name="enabled"></param>
    public void InputSetActive(bool enabled)
    {
        _IsInputable = enabled;
    }
    #endregion
}
