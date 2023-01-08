using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI_RetryPanel : MonoBehaviour
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define

    #endregion

    #region serialize field

    #endregion

    #region field
    private StageUIScript _Canvas;
    private BookUI _BookUI;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Canvas = transform.parent.gameObject.GetComponent<StageUIScript>();
        _BookUI = GetComponent<BookUI>();
        _BookUI.GoToNextPage();
    }

    // Update is called once per frame
    void Update()
    {
        _BookUI.TurnPageUpdate();

        if(_BookUI.IsFinishEffect) _Canvas.RetryFunction();
        //if (Input.GetKeyDown(KeyCode.LeftShift)) _Canvas.RetryFunction();
    }
    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
