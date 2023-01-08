using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BirdBehaviour : MonoBehaviour
{
    #region define

    #endregion

    #region serialize field
    /// <summary> ドリーカートの最大値 </summary>
    [SerializeField] private float _MaxRange = 42.0f;

    /// <summary> 透明マテリアル </summary>
    [SerializeField] private Material _MaterialT;

    /// <summary> ゲームの状態を取得 </summary>
    public GameData _GameData;
    #endregion

    #region field
    private MyCinemachineDollyCart _DollyCart;
    private float _DollyPosition;
    private float _Speed;

    /// <summary> 鳥の子オブジェクトを取得 </summary>
    private GameObject _BodyMeshs;

    /// <summary> 門をくぐったら消滅させる機能用 </summary>
    private Renderer _Renderer;
    private bool _IsWarp = false;
    private float _WarpSpeed = 4.0f;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _DollyCart = GetComponent<MyCinemachineDollyCart>();
        _DollyPosition = _DollyCart._M_Position;
        _Speed = 4.0f;

        _BodyMeshs = transform.GetChild(0).gameObject.transform.GetChild(3).gameObject;
        _Renderer = _BodyMeshs.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // レールの上を移動させる処理
        if (_DollyPosition < _MaxRange && _GameData.InGameState == InGame.ChangeStartView)
        {
            _DollyPosition += (Time.deltaTime * _Speed);
            _DollyCart._M_Position = _DollyPosition;
        }
        else if(_GameData.InGameState == InGame.EntryPlayer)
        {
            _BodyMeshs.SetActive(false);
        }

        // ワープゲートをくぐっていたら
        if(_IsWarp)
        {
            Color color = _Renderer.material.color;
            color.a -= (Time.deltaTime * _WarpSpeed);
            if (color.a <= 0.0f) color.a = 0.0f;
            _Renderer.material.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ゴールゲートをくぐったら
        if(other.gameObject.tag == "WarpHole")
        {
            Debug.Log("通過");
            _Renderer.material = _MaterialT;
            _IsWarp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ゴールゲートをくぐったら
        if (other.gameObject.tag == "WarpHole")
        {
            Color color = _Renderer.material.color;
            color.a = 0.0f;
            _Renderer.material.color = color;
        }
    }
    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
