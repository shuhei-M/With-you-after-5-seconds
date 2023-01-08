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
            // プレイヤーのインターフェイスを取得
            IPlayGimmick playGimmick = other.gameObject.GetComponent<IPlayGimmick>();
            // プレイヤーのアクションタイプを変更
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.PushOrPull;

            // プレイヤーの入力を基に、このオブジェクトを動かす
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
            // プレイヤーのアクションタイプをデフォルトに戻す
            if (playGimmick != null) playGimmick.ActionTypeP = ActionType.Default;
        }
    }
    #endregion
}
