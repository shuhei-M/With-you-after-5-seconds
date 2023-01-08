using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3Dオブジェクトを（段階的に）（半）透明にする機能を提供する。
/// </summary>
[RequireComponent(typeof(Renderer))]
public class OccludeeBehaviour : MonoBehaviour
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define
    
    #endregion

    #region serialize field
    /// <summary>1フレームごとにどれくらいずつ alpha を変化させるか指定する</summary>
    [SerializeField] float _M_step = 0.01f;

    /// <summary> 元々ののマテリアル </summary>
    [SerializeField] Material _OrigintMaterial;

    /// <summary> 透けさせる用のマテリアル </summary>
    [SerializeField] Material _TransparentMaterial;

    ///// <summary> 子オブジェクトの数 </summary>
    //[SerializeField] int _ChildlenNum;
    #endregion

    #region field
    /// <summary>この alpha にするというターゲットの値</summary>
    float _M_targetAlpha = 1.0f;
    /// <summary>alpha の初期値</summary>
    float _M_originalAlpha = 1.0f;
    Material _M_material; 
    List<Renderer> _M_Renderers = new List<Renderer>();

    /// <summary> 子オブジェクトの数 </summary>
    int _ChildlenCount;
    #endregion

    #region property

    #endregion

    #region Unity function
    void Start()
    {
        _ChildlenCount = transform.childCount;

        // 当たり判定内の複数のオブジェトを透過させたいとき
        if (_ChildlenCount > 0)
        {
            for (int i = 0; i < _ChildlenCount; i++)
            {
                _M_Renderers.Add(transform.GetChild(i).gameObject.GetComponent<Renderer>());
            }

            return;
        }

        // 単一オブジェクトを透過させたい時
        // このオブジェクトのマテリアルを取得しておく
        Renderer r = GetComponent<Renderer>();
        if (r)
        {
            _M_material = r.material;
        }
    }
    #endregion

    #region public function
    /// <summary>
    /// alpha を初期値に戻す
    /// </summary>
    public void ChangeAlpha2Original()
    {
        ChangeAlpha(_M_originalAlpha);
    }

    /// <summary>
    /// alpha を変更する
    /// </summary>
    /// <param name="targetAlpha">ターゲットとなる alpha の値</param>
    public void ChangeAlpha(float targetAlpha)
    {
        _M_targetAlpha = targetAlpha;

        if (_M_material && _ChildlenCount == 0)
        {
            StartCoroutine(ChangeAlpha());
        }
        else if (_M_Renderers[_ChildlenCount - 1].material && _ChildlenCount >  0)
        {
            StartCoroutine(ChangeAlphas());
        }
    }
    #endregion

    #region private function
    /// <summary>
    /// 単一オブジェクト版。alpha を（徐々に）変更する
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeAlpha()
    {
        if (_M_material.color.a > _M_targetAlpha)
        {
            gameObject.GetComponent<Renderer>().material =_TransparentMaterial;
            _M_material = gameObject.GetComponent<Renderer>().material;

            while (_M_material.color.a > _M_targetAlpha)
            {
                Color c = _M_material.color;
                c.a -= _M_step;
                _M_material.color = c;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (_M_material.color.a < _M_targetAlpha)
            {
                Color c = _M_material.color;
                c.a += _M_step;
                _M_material.color = c;
                yield return new WaitForEndOfFrame();
            }

            gameObject.GetComponent<Renderer>().material = _OrigintMaterial;
            _M_material = gameObject.GetComponent<Renderer>().material;
        }
    }

    /// <summary>
    /// 複数オブジェクト一括版。alpha を（徐々に）変更する
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeAlphas()
    {
        if (_M_Renderers[0].material.color.a > _M_targetAlpha)
        {
            ChangeMaterials(_TransparentMaterial);

            while (_M_Renderers[0].material.color.a > _M_targetAlpha)
            {
                UpdateAlphas(-_M_step);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (_M_Renderers[0].material.color.a < _M_targetAlpha)
            {
                UpdateAlphas(_M_step);
                yield return new WaitForEndOfFrame();
            }

            ChangeMaterials(_OrigintMaterial);
        }
    }

    private void ChangeMaterials(Material m_material)
    {
        for (int i = 0; i < _ChildlenCount; i++)
        {
            _M_Renderers[i].material = m_material;
        }
    }

    private void UpdateAlphas(float m_step)
    {
        for (int i = 0; i < _ChildlenCount; i++)
        {
            Color c = _M_Renderers[i].material.color;
            c.a += m_step;
            _M_Renderers[i].material.color = c;
        }
    }
    #endregion
}
