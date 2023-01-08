using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//=== 移動処理管理用 ===//
public partial class PlayerBehaviour
{
    #region field
    /// <summary> アナログスティックで入力されたベクトルを、ワールド座標に変換 </summary>
    private Vector3 _WorldVec = Vector3.zero;
    #endregion

    #region public function
    /// <summary>
    /// プレイヤーの回転をONにする
    /// </summary>
    public void ON_PlayerRotate()
    {
        canRotate = true;
    }

    /// <summary>
    /// プレイヤーの回転をOFFにする
    /// </summary>
    public void OFF_PlayerRotate()
    {
        canRotate = false;
    }
    #endregion

    #region private function
    /// <summary>
    /// プレイヤーの入力を移動用のベクトルに変換する
    /// </summary>
    /// <returns> 変換後のベクトル（最大：1） </returns>
    private Vector3 GetInput_Move()
    {
        float xDelta = 0.0f;
        float zDelta = 0.0f;

        // コントローラからの入力を受け付ける場合
        if (_IsInputable)
        {
            xDelta = Input.GetAxis("L_Horizontal");
            zDelta = Input.GetAxis("L_Vertical");
        }

        Vector3 moveDelta = new Vector3(xDelta, 0, zDelta);

        // ベクトルの大きさが1より大きければ、ベクトルを正規化
        if (moveDelta.magnitude > 1.0f) moveDelta = moveDelta.normalized;

        return moveDelta;
    }

    /// <summary>
    /// プレイヤーをカメラの向きに合わせて回転する。（MovePlayerで呼ばれる）
    /// </summary>
    /// <param name="horizontalRotation">カメラの回転</param>
    private void RotatePlayer(Quaternion horizontalRotation)
    {
        Vector3 rotateVec = horizontalRotation * _Velocity.normalized;
        Quaternion look = Quaternion.LookRotation(rotateVec);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, _RotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// プレイヤーを移動回転させる
    /// </summary>
    private void MovePlayer()
    {
        var horizontalRotation = Quaternion.AngleAxis(_MyCamera.transform.eulerAngles.y, Vector3.up);

        _MoveBlend = _Velocity.magnitude;

        //プレイヤーが外に向かっているときは動かないようにしたいので!isGoingOut追加しました
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

        //外に向かっているときにmoveVecをゼロにするようにしました。
        Vector3 moveVec;
        if (isGoingOut)
            moveVec = Vector3.zero;
        else
            moveVec = _WorldVec * _MoveSpeed * Time.deltaTime;

        if (!_CharaCon.isGrounded) moveVec.y += Physics.gravity.y * Time.deltaTime;

        // コントローラからの入力を受け付ける場合
        if (_IsInputable)
        {
            _Animator.SetFloat("DeltaTime", _DeltaTime);
            _Animator.SetFloat("MoveBlend", _MoveBlend);
        }

        //回転をオンオフできるようにしました
        if (_Velocity.normalized.magnitude > 0.0f && canRotate)
        {
            RotatePlayer(horizontalRotation);
        }

        if (!_CharaCon.enabled) return;

        _CharaCon.Move(moveVec);
    }
    #endregion
}
