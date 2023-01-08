using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    #region field
    private GameObject _MyParent;
    #endregion

    #region Unity function
    private void Start()
    {
        _MyParent = transform.parent.gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            // �v���C���[�̃C���^�[�t�F�C�X���擾
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            // �v���C���[�̃A�N�V�����^�C�v��ύX
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.PushOrPull;

            // �v���C���[�̓��͂���ɁA���̃I�u�W�F�N�g�𓮂���
            Vector3 vector = Vector3.zero;
            if (playGimmick.Get_PlayerState() == PlayerState.Action_PushOrPull)
            {
                if (playGimmick != null) vector = playGimmick.Get_WorldVelocity();
                _MyParent.transform.Translate(vector * Time.deltaTime);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            // �v���C���[�̃A�N�V�����^�C�v���f�t�H���g�ɖ߂�
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.Default;
        }
    }
    #endregion
}
