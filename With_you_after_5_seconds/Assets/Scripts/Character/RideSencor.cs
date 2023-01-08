using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RideSencor : MonoBehaviour
{
    #region field
    /// <summary> プレイヤーがセンサーに触れている秒数 </summary>
    private float _Count;

    /// <summary> プレイヤーが乗り状態に遷移してよいのか </summary>
    private bool _CanRiding = false;

    /// <summary> センサーのついている残像 </summary>
    private GameObject _Afterimage;
    #endregion

    #region property
    /// <summary> プレイヤーが乗り状態に遷移してよいのか、外部から取得する </summary>
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
            // 一定以上プレイヤーの方が高い位置にいなければ、乗り状態にはなれない。
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
            //Debug.Log("降りた");
        }
    }
    #endregion

    #region private function
    /// <summary>
    /// プレイヤーが条件秒以上、センサーに触れいているか
    /// </summary>
    /// <returns></returns>
    private bool TryRiding()
    {
        if (_Count > 0.2f) return true;
        else return false;
    }
    #endregion
}
