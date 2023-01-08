using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �A�j���[�V�����X�e�[�g�}�V���r�w�C�r�A
/// PlayerAnimatorController�́APushOrPull�X�e�[�g�ȊO�̃X�e�[�g�ɃA�^�b�`
/// </summary>
public class ASMB_Actions : StateMachineBehaviour
{
    /// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>

    #region define

    #endregion

    #region serialize field
    private PlayerBehaviour _Player;
    #endregion

    #region field

    #endregion

    #region property

    #endregion

    #region Unity function
    private void Awake()
    {
        _Player = GameObject.Find("Player").gameObject.GetComponent<PlayerBehaviour>();
    }

    //��Ԃ��ς�������Ɏ��s
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    //���t���[�����s(���ŏ��ƍŌ�̃t���[��������)
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    //��Ԃ��I��鎞(�ς�钼�O)�Ɏ��s
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _Player.OnAttackFinish();
    }

    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
