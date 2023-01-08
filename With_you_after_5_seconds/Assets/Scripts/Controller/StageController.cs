using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageController : MonoBehaviour
{
    #region serialize field
    /// <summary> 外部から取得するオブジェクトをまとめておく </summary>
    [SerializeField] public GameObject _Player;
    [SerializeField] public GameObject _Afterimage;

    /// <summary> 残像の高度制限 </summary>
    [SerializeField] private GameObject _HighestPoint;

    public GameData gameData;
    #endregion

    #region field
    private RideSencor _RideSencor;
    private string nowSceneName;
    #endregion

    #region property
    public Transform _PlayerTransform { get; private set; }
    public Transform _AfterimageTransform { get; private set; }

    public RideSencor RideSencor_ { get { return _RideSencor; } }

    public float _HighestPosition { get; private set; }
    #endregion

    #region Unity function
    private void Start()
    {
        _PlayerTransform = _Player.GetComponent<Transform>();
        _AfterimageTransform = _Afterimage.GetComponent<Transform>();

        _RideSencor = _Afterimage.transform.GetChild(1).gameObject.GetComponent<RideSencor>();

        _HighestPosition = _HighestPoint.GetComponent<Transform>().position.y;

        // 渡邊 追記
        nowSceneName = SceneManager.GetActiveScene().name;
        if (nowSceneName == gameData.stageScene[0] || nowSceneName == gameData.stageScene[1])
        {
            Sound.LoadBGM("StageBGM", "StageBGM1_2");
            Sound.PlayBGM("StageBGM", 0.03f);
        }
        else if (nowSceneName == gameData.stageScene[2] || nowSceneName == gameData.stageScene[3])
        {
            Sound.LoadBGM("StageBGM", "StageBGM3_4");
            Sound.PlayBGM("StageBGM", 0.03f);
        }
        else if (nowSceneName == gameData.stageScene[4] || nowSceneName == gameData.stageScene[5])
        {
            Sound.LoadBGM("StageBGM", "StageBGM5_6");
            Sound.PlayBGM("StageBGM", 0.03f);
        }
        else if (nowSceneName == gameData.stageScene[6] || nowSceneName == gameData.stageScene[7])
        {
            Sound.LoadBGM("StageBGM", "StageBGM7_8");
            Sound.PlayBGM("StageBGM", 0.03f);
        }
        else
        {
            Sound.LoadBGM("StageBGM", "StageBGM1_2");
            Debug.LogWarning("BGMがセットできてないよ！！");
        }
    }
    #endregion
}
