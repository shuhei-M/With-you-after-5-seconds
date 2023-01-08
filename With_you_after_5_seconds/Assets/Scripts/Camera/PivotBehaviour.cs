using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// m_start と m_end を繋ぐようなコライダーを作る機能を提供する。
/// 四角い棒のようなコライダーになるが、その太さを変えたい場合は Box Collider の Size.x, sixe.y を編集すること。
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PivotBehaviour : MonoBehaviour
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define

    #endregion

    #region serialize field
    /// <summary>コライダーの始点</summary>
    [SerializeField] Transform _M_start;
    /// <summary>コライダーの終点</summary>
    [SerializeField] Transform _M_end;
    #endregion

    #region field

    #endregion

    #region property

    #endregion

    #region Unity function
    void Start()
    {
        if (!_M_start || !_M_end)
        {
            Debug.LogError(name + " needs both Start and End.");
        }
    }

    void Update()
    {
        if (_M_start && _M_end)
        {
            //Vector3 endPos = new Vector3(_M_end.position.x, _M_end.position.y + 1.0f, _M_end.position.z);
            //Vector3 startPos = new Vector3(_M_start.position.x, _M_start.position.y + 1.0f, _M_start.position.z);
            // 始点と終点の中間に移動し、角度を調整し、コライダーの長さを計算して設定する
            Vector3 pivotPosition = (_M_end.position + _M_start.position) / 2;
            transform.position = pivotPosition;
            Vector3 dir = _M_end.position - transform.position;
            transform.forward = dir;
            BoxCollider col = GetComponent<BoxCollider>();
            float distance = Vector3.Distance(_M_start.position, _M_end.position);
            col.size = new Vector3(col.size.x, col.size.y, distance);
        }
    }
    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
