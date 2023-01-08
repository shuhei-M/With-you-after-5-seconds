using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;  // Cinemachine��ǉ���using
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

    [Header("���o����")]
    [SerializeField] private bool bird = true;
    [SerializeField] private float[] changeBirdPosision; // �Ō�̐��l�����̏I���l
    [SerializeField] private float changeCameraSec = 2.0f; // �����΂��Ȃ��ꍇ�̃J�����J�ڎ���
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject afterimage;
    

    [Header("�X�^�[�g���o")]
    [SerializeField] private GameObject gameStartPoint;
    [SerializeField] private float startRunSec = 3.0f;
    [SerializeField] private float startTurnSec = 1.0f;
    [SerializeField] private bool startCamSkip;

    [Header("�S�[�����o")]
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
    // ���o����
    MyCinemachineDollyCart myCinemachineDollyCart;
    CharacterController charaCon_p;
    Animator animator_p;
    string nowStageName;
    float step = -1;
    float goGateTime = 0;
    bool viewing = false;

    // �X�^�[�g���o
    Vector3 startVec;
    bool cameraCycleOK = false;
    Stage01LookAtPlayerCameraController LookAtPlayerController;

    // �S�[�����o
    Vector3 goalHeadingVec;

    /// <summary> warp�G�t�F�N�g���Đ����邩�ǂ����i�����j </summary>
    private bool _IsStartWarpEffect = false;
    #endregion


    #region Propaty
    public bool IsStartWarpEffect { get { return _IsStartWarpEffect; } }
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        // ����������
        SetUp();

        // VC�J�����̏����D�揇�ʂ�ݒ�
        StartVCPrioritySet();

#if UNITY_EDITOR
        // ���s���A�E�g�Q�[���̏�Ԃ��^�C�g���A���C�����j���[�A�X�e�[�W�Z���N�g�ȊO�Ȃ�^�C�g����Ԃɂ���
        LastTimeStateCheck();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // �X�^�[�g���̉��o
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

        // �S�[�����̏���
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
    // Start���ŌĂԏ���������
    void SetUp()
    {
        // �X�^�[�g�ƃS�[���ǂ���ɂ����ʂ��鏉������
        charaCon_p = player.GetComponent<CharacterController>();
        animator_p = player.GetComponent<Animator>();

        // �X�^�[�g���̉��o�Ɋւ��鏉������
        startVec = gameStartPoint.transform.position - player.transform.position;
        startVec.y = 0f;
        player.transform.rotation = Quaternion.LookRotation(startVec);
        if (bird)
        {
            myCinemachineDollyCart = GameObject.Find("Bird").GetComponent<MyCinemachineDollyCart>();
        }

        // ���̃V�[���̖��O��ǂݍ���
        nowStageName = SceneManager.GetActiveScene().name;
        if (nowStageName == "Stage01")
        {
            //stage01�Ńv���C���[�����񂷃J�����̃R���g���[���[�X�N���v�g�̎擾
            LookAtPlayerController = startVirtualCinemachine.GetComponent<Stage01LookAtPlayerCameraController>();
        }
    }

    // VC�J�����̏����D�揇�ʂ�ݒ菈��
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

    // ���s���A�E�g�Q�[���̏�Ԃ��^�C�g���A���C�����j���[�A�X�e�[�W�Z���N�g�ȊO�Ȃ�^�C�g����Ԃɂ���
    // exe�t�@�C���ɂ���Ƃ��͏�����������������
    void LastTimeStateCheck()
    {
        if (gameData.OutGameState != OutGame.None ||
            gameData.InGameState != InGame.CutIn)
        {
            //gameData.ChangeStartViewTransition();
            gameData.ChangeCutInTransition();
        }
    }

    // �X�^�[�g���̃J��������
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
        // �J�������񎞃v���C���[���\��
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
        // �J��������I����v���C���[�Ǝc����\��
        player.SetActive(true);
        afterimage.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);

        //if (bird)
        //{   // ���������̃J�����J�ځi�X�e�[�W�P�`�S�j
        //    // �J�������񎞃v���C���[���\��
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
        //    // �J��������I����v���C���[�Ǝc����\��
        //    player.SetActive(true);
        //    afterimage.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        //}
        //else
        //{

        //}
    }


    // �X�^�[�g���o�����֐�
    void StartDirecting()
    {
        // �v���C���[���X�e�[�W���ɓ����Ă��鉉�o����
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
                Debug.Log("�X�^�[�g�n�_����");
                animator_p.SetFloat("DeltaTime", 0.0f);
                animator_p.SetFloat("MoveBlend", 0.0f);
                gameData.PlayGameTransition();
            }
        }
    }

    // �S�[���Q�[�g�Ɉړ�������܂ł̏���
    void EntryGoalDirecting()
    {
        //���o�p�̃o�[�`�����V�l�}�V�[���ɕς���
        if (goalVirtualCinemachine.Priority != 100)
        {
            goalVirtualCinemachine.Priority = 100;
        }

        // ��̑O�ɗ����炢������~�܂��Đ���
        if (playerOnTrigger == gatePoint)
        {
            if (PlayerTurn(gatePoint, goalTurnSec))
            {
                // ���񂵏I�������܂����������蔲����x�N�g�����擾
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

    //�S�[���Q�[�g��ʂ蔲���鏈��
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

    // ���񒆂� false ��Ԃ��B���񂪏I���� true ��Ԃ��֐�
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
