using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//=== �ړ������Ǘ��p ===//
public partial class PlayerBehaviour
{
    #region field
    /// <summary> �A�i���O�X�e�B�b�N�œ��͂��ꂽ�x�N�g�����A���[���h���W�ɕϊ� </summary>
    private Vector3 _WorldVec = Vector3.zero;
    #endregion

    #region public function
    /// <summary>
    /// �v���C���[�̉�]��ON�ɂ���
    /// </summary>
    public void ON_PlayerRotate()
    {
        canRotate = true;
    }

    /// <summary>
    /// �v���C���[�̉�]��OFF�ɂ���
    /// </summary>
    public void OFF_PlayerRotate()
    {
        canRotate = false;
    }
    #endregion

    #region private function
    /// <summary>
    /// �v���C���[�̓��͂��ړ��p�̃x�N�g���ɕϊ�����
    /// </summary>
    /// <returns> �ϊ���̃x�N�g���i�ő�F1�j </returns>
    private Vector3 GetInput_Move()
    {
        float xDelta = 0.0f;
        float zDelta = 0.0f;

        // �R���g���[������̓��͂��󂯕t����ꍇ
        if (_IsInputable)
        {
            xDelta = Input.GetAxis("L_Horizontal");
            zDelta = Input.GetAxis("L_Vertical");
        }

        Vector3 moveDelta = new Vector3(xDelta, 0, zDelta);

        // �x�N�g���̑傫����1���傫����΁A�x�N�g���𐳋K��
        if (moveDelta.magnitude > 1.0f) moveDelta = moveDelta.normalized;

        return moveDelta;
    }

    /// <summary>
    /// �v���C���[���J�����̌����ɍ��킹�ĉ�]����B�iMovePlayer�ŌĂ΂��j
    /// </summary>
    /// <param name="horizontalRotation">�J�����̉�]</param>
    private void RotatePlayer(Quaternion horizontalRotation)
    {
        Vector3 rotateVec = horizontalRotation * _Velocity.normalized;
        Quaternion look = Quaternion.LookRotation(rotateVec);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, _RotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// �v���C���[���ړ���]������
    /// </summary>
    private void MovePlayer()
    {
        var horizontalRotation = Quaternion.AngleAxis(_MyCamera.transform.eulerAngles.y, Vector3.up);

        _MoveBlend = _Velocity.magnitude;

        //�v���C���[���O�Ɍ������Ă���Ƃ��͓����Ȃ��悤�ɂ������̂�!isGoingOut�ǉ����܂���
        if (_Velocity != Vector3.zero && !isGoingOut)
        {
            _DeltaTime += Time.deltaTime;
            if (_DeltaTime > 1.0f) _DeltaTime = 1.0f;
        }
        else
        {
            _DeltaTime = 0f;
        }

        _WorldVec = horizontalRotation * _Velocity;

        //�O�Ɍ������Ă���Ƃ���moveVec���[���ɂ���悤�ɂ��܂����B
        Vector3 moveVec;
        if (isGoingOut)
            moveVec = Vector3.zero;
        else
            moveVec = _WorldVec * _MoveSpeed * Time.deltaTime;

        if (!_CharaCon.isGrounded) moveVec.y += Physics.gravity.y * Time.deltaTime;

        // �R���g���[������̓��͂��󂯕t����ꍇ
        if (_IsInputable)
        {
            _Animator.SetFloat("DeltaTime", _DeltaTime);
            _Animator.SetFloat("MoveBlend", _MoveBlend);
        }

        //��]���I���I�t�ł���悤�ɂ��܂���
        if (_Velocity.normalized.magnitude > 0.0f && canRotate)
        {
            RotatePlayer(horizontalRotation);
        }

        if (!_CharaCon.enabled) return;

        _CharaCon.Move(moveVec);
    }
    #endregion
}
