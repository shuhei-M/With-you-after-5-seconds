using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region IPlayGimmickComponent
/// <summary>
/// �M�~�b�N������A�v���C���[�̃����o�֐����Ăяo�����߂̃C���^�[�t�F�C�X
/// </summary>
public interface IPlayGimmick
{
    Vector3 Get_InputVelocity();
    Vector3 Get_WorldVelocity();
    Transform Get_Transform();
    void OFF_CharacterController();
    void ON_CharacterController();
    public ActionType ActionTypeP { get; set; }
    PlayerState Get_PlayerState();
    void InputSetActive(bool enabled);
}
#endregion

#region IPlayGimmickComponent
/// <summary>
/// �M�~�b�N������A�v���C���[�̃����o�֐����Ăяo�����߂̃C���^�[�t�F�C�X
/// </summary>
public interface IButtonComponent
{
    /// <summary>
    /// �{�^�����������Ƃ�
    /// </summary>
    public void GetButtonDown();

    /// <summary>
    /// �{�^����b�����Ƃ�
    /// </summary>
    public void GetButtonUp();
}
#endregion
