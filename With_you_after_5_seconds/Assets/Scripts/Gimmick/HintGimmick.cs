using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class HintGimmick : MonoBehaviour
{
    /// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>
    /// <summary>
    /// �q���g�M�~�b�N�̏��
    /// </summary>
    public enum StateEnum
    {
        Locked,
        Disabled,
        Useable,
        Finish,
    }

    #region define
    private enum HintViewEdge : int
    {
        PlayerSide,
        HintObjSide
    }
    #endregion

    #region serialize field
    [SerializeField] private GameObject _HintObject;

    [SerializeField] private CinemachineVirtualCamera _HintVcamera;

    [SerializeField] private GameObject[] _HintViewEdge;

    [Header("�q���g��p�̈�����𒲐�")]
    [SerializeField, Range(0, 10)] private float _EdgeDistance = 5.0f;

    [Header("�q���g�@�\�J���܂ł̑҂�����")]
    [SerializeField, Range(0, 120)] private float _WaitTime = 60.0f;
    #endregion

    #region field
    private PlayerBehaviour _Player;
    private Transform _PlayerTransform;

    private GameObject _HintBody;

    private bool _CanShowHint;
    private bool _IsAliveHint;

    private StateEnum _CurrentHintState;
    private StateEnum _PrevHintState;
    private float _Time;
    #endregion

    #region property
    public bool CanShowHint { get { return _CanShowHint; } }
    public bool IsAliveHint { get { return _IsAliveHint; } }

    public StateEnum State { get { return _CurrentHintState; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _CanShowHint = false;
        _IsAliveHint = false;
        _HintBody = _HintObject.transform.GetChild(0).gameObject;
        _HintBody.SetActive(false);

        GameObject playerObj = GameObject.Find("Player").gameObject;
        _Player = playerObj.GetComponent<PlayerBehaviour>();
        _PlayerTransform = playerObj.GetComponent<Transform>();

        _CurrentHintState = StateEnum.Locked;
        _Time = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();

        _PrevHintState = _CurrentHintState;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _CanShowHint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _CanShowHint = false;
        }
    }
    #endregion

    #region public function

    #endregion

    #region private function
    /// <summary>
    /// ��Ԃ̕ύX
    /// </summary>
    private void ChangeState(StateEnum stateEnum)
    {
        _CurrentHintState = stateEnum;
        Debug.Log(_CurrentHintState);
        
        switch (_CurrentHintState)
        {
            case StateEnum.Locked:
                {
                }
                break;
            case StateEnum.Disabled:
                {
                }
                break;
            case StateEnum.Useable:
                {
                }
                break;
            case StateEnum.Finish:
                {
                }
                break;
        }
    }

    /// <summary>
    /// ��Ԗ��̖��t���[���Ă΂�鏈��
    /// </summary>
    private void UpdateState()
    {
        switch (_CurrentHintState)
        {
            case StateEnum.Locked:
                {
                    _Time += Time.deltaTime;
                    if (_Time > _WaitTime) ChangeState(StateEnum.Disabled);
                }
                break;
            case StateEnum.Disabled:
                {
                    if(_CanShowHint) ChangeState(StateEnum.Useable);
                }
                break;
            case StateEnum.Useable:
                {
                    if (!_CanShowHint) { ChangeState(StateEnum.Disabled); return; }

                    // �v���C���[��Idle�EMove��ԈȊO�ł���΍s��Ȃ�
                    if(_Player.State != PlayerState.Idle
                        && _Player.State != PlayerState.Move)
                    {
                        return;
                    }

                    // �q���g�𐶐�
                    if (Input.GetButtonDown("Hint")) 
                    {
                        UpdateHintViewEdge();
                        GenerateHintObj(); 
                        //ChangeState(StateEnum.Finish); 
                    }
                }
                break;
            case StateEnum.Finish:
                {
                }
                break;
        }
    }

    /// <summary>
    /// �q���g�p�I�u�W�F�N�g��\������
    /// </summary>
    private void GenerateHintObj()
    {
        if (!_CanShowHint) return;
        if (_IsAliveHint) return;

        _HintBody.SetActive(true);
        _IsAliveHint = true;
        _HintVcamera.Priority += 10;

        StartCoroutine(DelayCoroutine());
    }

    /// <summary>
    /// �q���g�I�u�W�F�N�g�𐶐����Ă���5�b��ɁA��\���ɂ���
    /// </summary>
    private IEnumerator DelayCoroutine()
    {
        // 5�b�ԑ҂�
        yield return new WaitForSeconds(5);

        // 5�b��Ƀq���g�I�u�W�F�N�g������
        _IsAliveHint = false;
        _HintBody.SetActive(false);
        _HintVcamera.Priority -= 10;
    }

    /// <summary>
    /// �q���g��p�̒[���X�V
    /// </summary>
    private void UpdateHintViewEdge()
    {
        _HintViewEdge[(int)HintViewEdge.PlayerSide].transform.position = _PlayerTransform.position;
        _HintViewEdge[(int)HintViewEdge.HintObjSide].transform.position = _HintObject.transform.position;

        Vector3 gapN = (_PlayerTransform.position - _HintObject.transform.position).normalized;

        _HintViewEdge[(int)HintViewEdge.PlayerSide].transform.position += (gapN * _EdgeDistance);
        _HintViewEdge[(int)HintViewEdge.HintObjSide].transform.position -= (gapN * _EdgeDistance);
    }
    #endregion
}
