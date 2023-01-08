using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RideSencor : MonoBehaviour
{
    #region field
    /// <summary> �v���C���[���Z���T�[�ɐG��Ă���b�� </summary>
    private float _Count;

    /// <summary> �v���C���[������ԂɑJ�ڂ��Ă悢�̂� </summary>
    private bool _CanRiding = false;

    /// <summary> �Z���T�[�̂��Ă���c�� </summary>
    private GameObject _Afterimage;
    #endregion

    #region property
    /// <summary> �v���C���[������ԂɑJ�ڂ��Ă悢�̂��A�O������擾���� </summary>
    public bool CanRiding { get { return _CanRiding; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Count = 0.0f;
        _Afterimage = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        _CanRiding = TryRiding();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // ���ȏ�v���C���[�̕��������ʒu�ɂ��Ȃ���΁A����Ԃɂ͂Ȃ�Ȃ��B
            float y = _Afterimage.transform.position.y + 1.9f;
            
            if(other.gameObject.transform.position.y > y)
            {
                _Count += Time.deltaTime;
            }
            else
            {
                _Count = 0;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _Count = 0;
            //Debug.Log("�~�肽");
        }
    }
    #endregion

    #region private function
    /// <summary>
    /// �v���C���[�������b�ȏ�A�Z���T�[�ɐG�ꂢ�Ă��邩
    /// </summary>
    /// <returns></returns>
    private bool TryRiding()
    {
        if (_Count > 0.2f) return true;
        else return false;
    }
    #endregion
}
