using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BirdBehaviour : MonoBehaviour
{
    #region define

    #endregion

    #region serialize field
    /// <summary> �h���[�J�[�g�̍ő�l </summary>
    [SerializeField] private float _MaxRange = 42.0f;

    /// <summary> �����}�e���A�� </summary>
    [SerializeField] private Material _MaterialT;

    /// <summary> �Q�[���̏�Ԃ��擾 </summary>
    public GameData _GameData;
    #endregion

    #region field
    private MyCinemachineDollyCart _DollyCart;
    private float _DollyPosition;
    private float _Speed;

    /// <summary> ���̎q�I�u�W�F�N�g���擾 </summary>
    private GameObject _BodyMeshs;

    /// <summary> ���������������ł�����@�\�p </summary>
    private Renderer _Renderer;
    private bool _IsWarp = false;
    private float _WarpSpeed = 4.0f;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _DollyCart = GetComponent<MyCinemachineDollyCart>();
        _DollyPosition = _DollyCart._M_Position;
        _Speed = 4.0f;

        _BodyMeshs = transform.GetChild(0).gameObject.transform.GetChild(3).gameObject;
        _Renderer = _BodyMeshs.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // ���[���̏���ړ������鏈��
        if (_DollyPosition < _MaxRange && _GameData.InGameState == InGame.ChangeStartView)
        {
            _DollyPosition += (Time.deltaTime * _Speed);
            _DollyCart._M_Position = _DollyPosition;
        }
        else if(_GameData.InGameState == InGame.EntryPlayer)
        {
            _BodyMeshs.SetActive(false);
        }

        // ���[�v�Q�[�g���������Ă�����
        if(_IsWarp)
        {
            Color color = _Renderer.material.color;
            color.a -= (Time.deltaTime * _WarpSpeed);
            if (color.a <= 0.0f) color.a = 0.0f;
            _Renderer.material.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �S�[���Q�[�g������������
        if(other.gameObject.tag == "WarpHole")
        {
            Debug.Log("�ʉ�");
            _Renderer.material = _MaterialT;
            _IsWarp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �S�[���Q�[�g������������
        if (other.gameObject.tag == "WarpHole")
        {
            Color color = _Renderer.material.color;
            color.a = 0.0f;
            _Renderer.material.color = color;
        }
    }
    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
