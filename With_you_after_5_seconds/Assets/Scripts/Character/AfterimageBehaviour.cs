using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region define
/// <summary>
/// �c�����v���C���̃M�~�b�N�i�c���ɉe����^������̂̂݁j
/// </summary>
enum PlayGimmickType : int
{
    None,
    Seesaw,
    SunnySpot,
}
#endregion

/// <summary>
/// �c���𐧌䂷��N���X
/// </summary>
public class AfterimageBehaviour : MonoBehaviour
{
    #region define
    enum MeshType : int
    {
        Skin,
        Clothes,
        Bag,
        Guard,   // �ԕ�
    }

    enum MaterialState : int
    {
        Nomal,
        ToTransparent,
        Transparent,
        ToNomal,
    }
    #endregion

    #region serialize field
    /// <summary> �X�e�[�W���̃v���C���[�Ȃǂ̃I�u�W�F�N�g�̏����擾���� </summary>
    [SerializeField] private StageController _StageManager;

    /// <summary> �v���C���[�̋������L�^�����Q�[���I�u�W�F�N�g�B5�b�Ԓx�点�čČ�����B </summary>
    [SerializeField] private Recorder _Recorder;

    /// <summary> �c���̌��X�̃}�e���A�� </summary>
    [SerializeField] private Material[] _OriginMaterials;

    /// <summary> �c���̓������p�̃}�e���A�� </summary>
    [SerializeField] private Material[] _TransparentMaterials;

    #endregion

    #region field
    /// <summary> ���R�[�_�[�̏�����ɓ������g���[�X���邽�߂̕ϐ��Q </summary>
    private float _RecordDuration = 0.005f;
    private int _DataSwitcher = 0;
    private int _MaxDataNum = 2000;
    private bool _IsPlayRecord = true;

    /// <summary> �c���ɃA�^�b�`���ꂽ�R���|�[�l���g���擾���߂̕ϐ��Q </summary>
    private Animator _Animator;
    private AnimatorStateInfo _AimStateInfo;
    //private Renderer _Renderer;
    private CharacterController _CharacterController;

    /// <summary> �c���̃X�e�[�g���Ǘ����邽�߂̕ϐ��Q </summary>
    private PlayerState _CurrentState;
    private PlayerState _PrevState;
    private float _MoveDeltaTime = 0.0f;
    private float _MoveBlend = 0.0f;
    private bool _IsLadder = false;
    private int _ActionType;

    /// <summary> �V�[�\�[�M�~�b�N���̎c���̓����𐧌䂷�邽�߂̕ϐ��Q </summary>
    private PlayGimmickType _PlayGimmick;
    private float _RayDistance = 6.0f;
    private RaycastHit[] _RaycastHits = new RaycastHit[10];
    private float _SeesawX = 0.0f;

    /// <summary> �v���C���[�Əd�Ȃ����ۂɁA�c���𓧂������邽�߂̕ϐ��Q </summary>
    private float _OverlapTime = 0.0f;
    private Transform _PlayerTransform;
    private float _LimitDistance = 0.75f;
    // �ύX���郁�b�V�����}�e���A�����Ƃɕ�����
    List<Renderer>[] _M_Renderers = new List<Renderer>[(int)MeshType.Guard];
    MaterialState _MaterialState = MaterialState.Nomal;   // �}�e���A���̏��
    private GameObject _AfterimageBody;

    /// <summary> �Q�[���J�n�ŏ��̂T�b���Œ�̃X�^�[�g�n�_�ɂ������邽�߂̕ϐ��Q </summary>
    private Vector3 startPos;
    private Quaternion startRote;
    private float recordDeltaTime = 0.0f;

    private GameObject _FloatEffectOjb = null;

    public GameData gameData;

    /// <summary> �Q�[���J�n���G�t�F�N�g���쓮�����Ȃ����߂̃t���O </summary>
    bool isFirstPos;
    #endregion

    #region property
    /// <summary> �c���̃X�e�[�g���O������擾���� </summary>
    public PlayerState AfterimageState { get { return _CurrentState; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        _PlayGimmick = PlayGimmickType.None;
        _CharacterController = GetComponent<CharacterController>();

        isFirstPos = true;

        // �d�Ȃ蔻��p�̃v���C���[�̈ʒu�擾
        _PlayerTransform = _StageManager._PlayerTransform;

        // �d�Ȃ莞�̓��ߗp�ɁA���b�V�����Ƃ̃����_���[���擾
        RenderersSetUp();
        SetMaterials(_OriginMaterials);
        _AfterimageBody = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameData.InGameState != InGame.ChangeStartView && 
            gameData.InGameState != InGame.EntryPlayer &&
            recordDeltaTime < 5.1f)
        {
            recordDeltaTime += Time.deltaTime;
        }
           

        // �Q�[���J�n�ܕb��ɁA�v���C���[�̋����̃g���[�X���J�n����
        if (_IsPlayRecord)
        {
            StartAfterimage();
            _IsPlayRecord = false;
        }

        // �c���̃X�e�[�g����ɃA�j���[�V�������X�V
        AnimatorUpdate();

        if(_PlayGimmick != PlayGimmickType.SunnySpot)
        {
            // �����b�ȏ�v���C���[�Əd�Ȃ��Ă���΁A�c���𓧂�������B
            CheckOverlap();
            ChangeAlphas();
        }
        //ToTransparent(isOverlap);

        //DebugLog();

        // �X�V�O�̎c���̃X�e�[�g��ۑ�
        _PrevState = _CurrentState;
    }

    private void OnTriggerStay(Collider other)
    {
        // �V�[�\�[�M�~�b�N�̃G���A�ɓ�������A�c����Y���W�𒲐�����
        if (other.gameObject.tag == "SeesawArea")
        {
            float otherY = other.gameObject.transform.position.y;
            float gap = otherY - transform.position.y;
            if (gap > 2.4f/* || gap < 0.0f*/) return;

            _PlayGimmick = PlayGimmickType.Seesaw;
            _SeesawX = other.gameObject.transform.position.x;
        }

        // �����G���A�ɓ�������c��������
        if (other.gameObject.tag == "SunnySpot")
        {
            _PlayGimmick = PlayGimmickType.SunnySpot;
            _AfterimageBody.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �V�[�\�[�M�~�b�N�G���A����o����A�X�e�[�g��None�ɂ���B
        if (other.gameObject.tag == "SeesawArea")
        {
            _PlayGimmick = PlayGimmickType.None;
        }

        // �����G���A����o����A�X�e�[�g��None�ɂ��A�����ڂ�߂��B
        if (other.gameObject.tag == "SunnySpot")
        {
            _PlayGimmick = PlayGimmickType.None;
            _AfterimageBody.SetActive(true);
        }
    }
    #endregion

    #region public function
    #endregion

    #region private function
    /// <summary>
    /// �ύX���郌���_���[�̃��X�g�̔z����Z�b�g�A�b�v
    /// </summary>
    private void RenderersSetUp()
    {
        for (int i = 0; i < (int)MeshType.Guard; i++)
        {
            _M_Renderers[i] = new List<Renderer>();
        }
        GameObject bodyMeshs = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        int childlenCount = bodyMeshs.transform.childCount;
        for (int i = 0; i < childlenCount; i++)
        {
            GameObject mesh = bodyMeshs.transform.GetChild(i).gameObject;
            MeshType materialType = MeshType.Guard;
            if (mesh.tag == "Skin")
            {
                materialType = MeshType.Skin;
            }
            else if (mesh.tag == "Clothes")
            {
                materialType = MeshType.Clothes;
            }
            else if(mesh.tag == "Bag")
            {
                materialType = MeshType.Bag;
            }
            else
            {
                Debug.Log("�G���[�B�}�e���A���̎�ނ��擾�ł��Ă��܂���B");
            }

            _M_Renderers[(int)materialType].Add(mesh.GetComponent<Renderer>());
        }

        // �������p�̃}�e���A���̃A���t�@�l��������
        for(int i = 0; i < (int)MeshType.Guard; i++)
        {
            Color c = _TransparentMaterials[i].color;
            c.a = 1.0f;
            _TransparentMaterials[i].color = c;
        }
    }

    /// <summary>
    /// �c���̑S�Ẵ��b�V���ɓ���̃}�e���A�����Z�b�g����B
    /// </summary>
    private void SetMaterials(Material[] materials)
    {
        for (int i = 0; i < (int)MeshType.Guard; i++)
        {
            for(int j = 0; j < _M_Renderers[i].Count; j++)
            {
                _M_Renderers[i][j].material = materials[i];
            }
        }
    }

    /// <summary>
    /// 1�b�ȏ�d�Ȃ��Ă��邩
    /// </summary>
    private void CheckOverlap()
    {
        float distance = Vector3.Distance(transform.position, _PlayerTransform.position);
        float overlapCheckTime = 1.0f;

        // �d�Ȃ��Ă����
        if (distance < _LimitDistance/*transform.position == _playerTransform.position*/)
        {
            
        }
        else
        {
            // �d�Ȃ��Ă��Ȃ���Ό����ڂ����ɖ߂�
            _OverlapTime = 0.0f;
            if(_MaterialState != MaterialState.Nomal) _MaterialState = MaterialState.ToNomal;
            return;
        }

        // �d�Ȃ��Ă���̌o�ߎ��Ԃ��v�Z
        _OverlapTime += Time.deltaTime;

        // �d�Ȃ��Ă����b�o�����ꍇ�A�������J�n
        if (_MaterialState != MaterialState.ToTransparent && _MaterialState != MaterialState.Transparent
            && _OverlapTime > overlapCheckTime
            && _PlayGimmick != PlayGimmickType.SunnySpot)
        {
            SetMaterials(_TransparentMaterials);
            _MaterialState = MaterialState.ToTransparent;
        }
        else
        {

        }
    }

    /// <summary>
    /// �e�}�e���A���̃A���t�@�l��ύX����
    /// ToTransparent�AToNomal��Ԃ̎��̂ݎg�p�o����B
    /// </summary>
    private void ChangeAlphas()
    {
        float m_step = (Time.deltaTime / 2.0f);
        float minAlpha = 0.1f;
        float maxAlpha = 1.0f;

        // ���X�ɓ����ɂȂ��Ă���
        if (_MaterialState == MaterialState.ToTransparent)
        {
            for (int i = 0; i < (int)MeshType.Guard; i++)
            {
                for (int j = 0; j < _M_Renderers[i].Count; j++)
                {
                    Color c = _M_Renderers[i][j].material.color;
                    c.a -= m_step;
                    _M_Renderers[i][j].material.color = c;
                }
            }

            //alpha���ŏ��l�ɓ��B���Ă�����
            if(_M_Renderers[0][0].material.color.a < minAlpha)
            {
                for (int i = 0; i < (int)MeshType.Guard; i++)
                {
                    for (int j = 0; j < _M_Renderers[i].Count; j++)
                    {
                        Color c = _M_Renderers[i][j].material.color;
                        c.a = minAlpha;
                        _M_Renderers[i][j].material.color = c;
                    }
                }
                _MaterialState = MaterialState.Transparent;
            }
        }
        // ���X�Ɍ��ɖ߂��Ă���
        else if(_MaterialState == MaterialState.ToNomal)
        {
            for (int i = 0; i < (int)MeshType.Guard; i++)
            {
                for (int j = 0; j < _M_Renderers[i].Count; j++)
                {
                    Color c = _M_Renderers[i][j].material.color;
                    c.a += m_step;
                    _M_Renderers[i][j].material.color = c;
                }
            }

            //alpha���ő�l�ɓ��B���Ă�����
            if (_M_Renderers[0][0].material.color.a > maxAlpha - 0.0001f)
            {
                for (int i = 0; i < (int)MeshType.Guard; i++)
                {
                    for (int j = 0; j < _M_Renderers[i].Count; j++)
                    {
                        Color c = _M_Renderers[i][j].material.color;
                        c.a = maxAlpha;
                        _M_Renderers[i][j].material.color = c;
                    }
                }
                _MaterialState = MaterialState.Nomal;
                SetMaterials(_OriginMaterials);
            }
        }
    }

    /// <summary>
    /// �f�o�b�O�p
    /// </summary>
    private void DebugLog()
    {
        if (_PrevState == _CurrentState) return;

        Debug.Log(_CurrentState);
    }

    /// <summary>
    /// �c���ɂ�铮���̃g���[�X���J�n����
    /// </summary>
    private void StartAfterimage()
    {
        Debug.Log("StartAfterimage");
        if (_Recorder[_DataSwitcher] == null)
        {
            Debug.Log("�S�[�X�g�f�[�^������܂���");
        }
        else
        {
            transform.position = AjustPosition(_Recorder[_DataSwitcher].Get_PosLists(0));
            transform.rotation =_Recorder[_DataSwitcher].Get_RotLists(0);
            _CurrentState = _Recorder[_DataSwitcher].Get_PlayerStateLists(0);
            StartCoroutine(PlayBack());
        }
    }

    /// <summary>
    /// 0.005f�b���ɁA5�b�O�̃v���C���[�̃f�[�^���Z�b�g����
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayBack()
    {
        int i = 0;

        while (true)
        {
            yield return new WaitForSeconds(_RecordDuration);

            Vector3 prevVec = transform.position;

            if(gameData.InGameState == InGame.ChangeStartView || gameData.InGameState == InGame.EntryPlayer)
            {
                this.gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false) ;
            }
            else if (recordDeltaTime <= 5.1f)
            {   // �J�n5�b�Ԃ̓X�^�[�g�ʒu�Œ�~������
                this.gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
                if (startPos == Vector3.zero)
                {
                    startPos = _PlayerTransform.position;
                    startRote = _PlayerTransform.rotation;
                }
                this.transform.position = startPos;
                this.transform.rotation = startRote;
            }
            else
            {
                transform.position = AjustPosition(_Recorder[_DataSwitcher].Get_PosLists(i));
                transform.rotation = _Recorder[_DataSwitcher].Get_RotLists(i);
                _MoveDeltaTime = _Recorder[_DataSwitcher].Get_DeltaTimeLists(i);
                _MoveBlend = _Recorder[_DataSwitcher].Get_BlendLists(i);
                _IsLadder = _Recorder[_DataSwitcher].Get_IsLadderLists(i);
                _ActionType = _Recorder[_DataSwitcher].Get_ActionTypeLists(i);

                _CurrentState = _Recorder[_DataSwitcher].Get_PlayerStateLists(i);
            }
            i++;

            // ���̍X�V�ŏ��������ȏ�ړ����Ă���΁A�G�t�F�N�g�𔭐�������
            TryGanarateTeleportationEffect(transform.position, prevVec);

            //�@�ۑ��f�[�^���𒴂����玟�̕ۑ��f�[�^���Đ�
            if (i >= _MaxDataNum)
            {
                _Recorder[_DataSwitcher].Clear_AllLists();
                _DataSwitcher++;
                _DataSwitcher %= 2;
                i = 0;
            }
        }
    }

    /// <summary>
    /// �c���ɃZ�b�g����ʒu����K�؂Ȍ`�ɒ��߂���i���������A�V�[�\�[�j
    /// </summary>
    /// <param name="position">���R�[�_�[�ɋL���ꂽ�v���C���[�̍��W</param>
    /// <returns>�V�[�\�[�̍����ɉ����悤�ɒ��߂����c���̍��W</returns>
    private Vector3 AjustPosition(Vector3 position)
    {
        Vector3 temp = position;

        if (position.y > _StageManager._HighestPosition)
        {
            temp = new Vector3(
                temp.x,
                _StageManager._HighestPosition,
                temp.z);
        }

        if (_PlayGimmick == PlayGimmickType.Seesaw)
        {
            bool hitSeesaw = false;

            Vector3 rayPosition = new Vector3(
                transform.position.x,
                transform.position.y + 4.0f,
                transform.position.z);
            //Debug.DrawRay(rayPosition, -transform.up * _rayDistance, Color.red);
            int hitCount = Physics.RaycastNonAlloc(rayPosition, -transform.up, _RaycastHits, _RayDistance);
            for (int i = 0; i < hitCount; i++)
            {
                if (_RaycastHits[i].collider.gameObject.tag == "Seesaw")
                {
                    hitSeesaw = true;
                    break;
                }
            }
            if (hitSeesaw)
            {
                var hitList = _RaycastHits.ToList();
                RaycastHit seesawPos = hitList.FirstOrDefault(item => (item.collider.gameObject.tag == "Seesaw"));
                temp = new Vector3(
                    temp.x,
                    seesawPos.point.y,
                    temp.z);
            }
        }

        return temp;
    }

    /// <summary>
    /// ���̍X�V�ŏ��������ȏ�ړ����Ă���΁A�G�t�F�N�g�𔭐�������
    /// </summary>
    /// <param name="currentPos">���݂̍��W</param>
    /// <param name="prevPos">�X�V�O�̍��W</param>
    private void TryGanarateTeleportationEffect(Vector3 currentPos, Vector3 prevPos)
    {
        float distance = (currentPos - prevPos).magnitude;

        // �ړ�������1�ȉ��ł���Έȉ��̏����͍s��Ȃ�
        if (distance < 0.75f) return;

        // 0.0f (current)  ��  1.0f (prev)
        Vector3 genaratePos = Vector3.Lerp(currentPos, prevPos, 0.75f) + transform.up;

        //�ŏ������G�t�F�N�g���Đ��������Ȃ�
        if (!isFirstPos)
        {
            GameObject teleportationEffectObj = EffectManager.Instance.Play(EffectManager.EffectID.Teleportation, genaratePos);
            Destroy(teleportationEffectObj, 1.0f);
        }
        isFirstPos = false;
    }

    /// <summary>
    /// �c���̃A�j���[�V�����X�e�[�g���Ǘ�
    /// </summary>
    private void AnimatorUpdate()
    {
        // �ړ����͂̌o�ߎ��ԂƁAL�X�e�B�b�N�̓|������A�j���[�^�[�ɃZ�b�g
        _Animator.SetFloat("DeltaTime", _MoveDeltaTime);
        _Animator.SetFloat("MoveBlend", _MoveBlend);
        _Animator.SetBool("IsLadder", _IsLadder);
        _Animator.SetInteger("ActionType", _ActionType);

        // �X�e�[�g���ς���Ă��Ȃ���΁A��{�I�ɂ͂����߂�B�g���K�[�̃��Z�b�g�ȊO�͍s��Ȃ��B
        if (_PrevState == _CurrentState)
        {
            if (_CurrentState == PlayerState.Idle) _Animator.ResetTrigger("ToMove");
            else if (_CurrentState == PlayerState.Ride) _Animator.ResetTrigger("ToMove");
            else if(_CurrentState == PlayerState.Move) _Animator.ResetTrigger("ToRide");
            return;
        }

        switch (_CurrentState)
        {
            case PlayerState.Idle:
                {
                    if (_PrevState == PlayerState.Ride) _Animator.SetTrigger("ToMove");
                    else if (_PrevState == PlayerState.Action_PushOrPull) _Animator.SetTrigger("ToIdle");
                }
                break;
            case PlayerState.Action:
                {
                    _Animator.SetTrigger("ToAction");
                }
                break;
            case PlayerState.Action_PushOrPull:
                {
                    if (_PrevState == PlayerState.Move || _PrevState == PlayerState.Idle) _Animator.SetTrigger("ToAction");
                }
                break;
            case PlayerState.Ride:
                {
                    if (_PrevState == PlayerState.Idle || _PrevState == PlayerState.Move) _Animator.SetTrigger("ToRide");

                    // ���V�G�t�F�N�g�̐����E���W����
                    _FloatEffectOjb = EffectManager.Instance.Play(EffectManager.EffectID.Float, transform.position);
                }
                break;
            case PlayerState.Move:
                {
                    if (_PrevState == PlayerState.Ride) 
                    { 
                        _Animator.SetTrigger("ToMove"); _Animator.ResetTrigger("ToRide");
                    }
                    else if (_PrevState == PlayerState.Action_PushOrPull) 
                    { 
                        _Animator.SetTrigger("ToIdle"); 
                    }
                }
                break;
            default:
                break;
        }
    }
    #endregion
}
