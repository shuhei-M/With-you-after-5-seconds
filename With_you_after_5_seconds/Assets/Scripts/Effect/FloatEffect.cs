using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatEffect : MonoBehaviour
{
    /// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>

    #region define

    #endregion

    #region serialize field

    #endregion

    #region field
    AfterimageBehaviour _Afterimage;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Afterimage = GameObject.Find("Afterimage").GetComponent<AfterimageBehaviour>();
        this.transform.parent = _Afterimage.gameObject.transform;
        this.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        // �c��������Ԃł͖����Ȃ����ꍇ
        if(_Afterimage.AfterimageState != PlayerState.Ride)
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
