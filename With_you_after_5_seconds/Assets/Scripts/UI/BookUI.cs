using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
    [DisallowMultipleComponent]
    public class BookUI : MonoBehaviour
    {
        /// <summary> ソースを書くときのレンプレート </summary>

        #region define
        private enum ModeEnum : int
        {
            ClearEffect,
            CutInEffect,
            RetryEffect,
            StageSelect,
        }    
        #endregion

        #region serialize field
        [SerializeField, Range(0, 3)] float _TurnTime = 0.5f;
        [SerializeField, Range(-2, 2)] float _TurnPageTilt = 1f;
        [SerializeField] Shader _Shader;
        [SerializeField] public UnityEvent _OnPageChanged = new UnityEvent();

        [SerializeField] private ModeEnum _Mode;
        #endregion

        #region field
        private int _InputPageNumber;
        private int _MaxPage;

        int _PageID;
        float _CurrentPosition = 0;

        int _CurrentPage;

        Material _Mat;

        Vector2? _Resolution = null;

        float _CurrentTime = -1;
        float _StartPosition;

        private EventSystem _EventSystem;
        private GameObject _SelectGameObject;
        private GameObject _PaperT;
        private Image _ImagePaperT;

        private bool _IsFinishTurnPaper = false;
        private float _EffectTime = 0.0f;

        private bool _IsFinishFadeOut = false;
        private float _CutInWaitTime;
        private bool _IsStartFirstCutIn = false;

        //private Material _ClearPaperMat;
        #endregion

        #region property
        public float TurnTime { get { return _TurnTime; } }

        float CurrentPosition
        {
            get { return _CurrentPosition; }
            set
            {
                _CurrentPosition = value;
                material.SetFloat(_PageID, _CurrentPosition);
            }
        }

        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                if (_CurrentPage == value) return;
                _CurrentPage = value;
                _StartPosition = CurrentPosition;
                _CurrentTime = 0;
                if (_OnPageChanged != null)
                    _OnPageChanged.Invoke();
                //Debug.Log("OnPageChanged" + _CurrentPage);
            }
        }

        Material material
        {
            get
            {
                if (_Mat == null)
                {
                    _Mat = new Material(_Shader);
                    var matrix = Matrix4x4.Scale(new Vector3(1f / Resolution.x, 1f / Resolution.y, 1)) * CalcCanvas2LocalMatrix();
                    _Mat.SetMatrix("_Canvas2Local", matrix);
                    _Mat.SetMatrix("_Local2Canvas", matrix.inverse);
                    _Mat.SetFloat("_Tilt", _TurnPageTilt);
                }
                return _Mat;
            }
        }

        public Vector2 Resolution
        {
            get
            {
                if (_Resolution == null) _Resolution = CalcResolution();
                return _Resolution.Value;
            }
        }

        public bool IsFinishEffect 
        { 
            get 
            {
                bool isFinishEffect = false;
                //if(_Mode == ModeEnum.ClearEffect)
                //{
                //    isFinishEffect = _IsFinishTurnPaper;
                //}
                //else if(_Mode == ModeEnum.CutInEffect)
                //{
                //    isFinishEffect = _IsFinishFadeOut;
                //}
                //else if()
                //else
                //{
                //    Debug.Log(_Mode);
                //    Debug.Log("不正な値が入力されています。");
                //}
                switch (_Mode)
                {
                    case ModeEnum.ClearEffect:
                        {
                            isFinishEffect = _IsFinishTurnPaper;
                        }
                        break;
                    case ModeEnum.CutInEffect:
                        {
                            isFinishEffect = _IsFinishFadeOut;
                        }
                        break;
                    case ModeEnum.RetryEffect:
                        {
                            isFinishEffect = _IsFinishTurnPaper;
                        }
                        break;
                    default:
                        {
                            Debug.Log(_Mode);
                            Debug.Log("不正な値が入力されています。");
                        }
                        break;
                }
                return isFinishEffect; 
            } 
        }
        #endregion


        #region Unity function
        void Awake()
        {
            _PageID = Shader.PropertyToID("_Page");
            foreach (var g in GetComponentsInChildren<Graphic>(true))
                g.material = material;

            // Start関数内で、モードに応じた変数の設定を行う
            SetUpComponent();

            _InputPageNumber = 0;

            Sound.LoadSE("TurnPage", "TurnPage");
        }

        void Update()
        {
            // DebugKeyDown();

            BookUpdate();
        }
        #endregion

        #region public function
        public void ResetParameter()
        {
            //_CurrentPage = 0;
            _CurrentPosition = 0.0f;
            //_CurrentTime = -1.0f;
        }    

        /// <summary>
        /// 無地の紙を徐々に出現させる
        /// </summary>
        public void FadeInPlainPage()
        {
            //if (_Mode != ModeEnum.ClearEffect) return;
            //if (_ImagePaperT.color.a >= 0.4) return;

            //var color = _ImagePaperT.color;
            //color.a += (0.4f * Time.deltaTime);
            //_ImagePaperT.color = color;
            //if (_ImagePaperT.color.a >= 0.4f)
            //{
            //    color.a = 0.4f;
            //    _ImagePaperT.color = color;
            //}
        }

        /// <summary>
        /// 無地の紙を徐々に消滅させる
        /// </summary>
        public void FadeOutPlainPage()
        {
            if (_Mode != ModeEnum.CutInEffect) return;
            if (_ImagePaperT.color.a <= 0.0f) return;

            var color = _ImagePaperT.color;
            color.a -= (0.5f * Time.deltaTime);
            _ImagePaperT.color = color;
            if (_ImagePaperT.color.a <= 0.0f)
            {
                color.a = 0.0f;
                _ImagePaperT.color = color;
                _IsFinishFadeOut = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void TurnPageUpdate()
        {
            if (_CurrentTime < 0) return;
            _CurrentTime += Time.unscaledDeltaTime;
            float t = _CurrentTime / _TurnTime;
            if (_CurrentTime >= _TurnTime)
            {
                _CurrentTime = -1;
                t = 1f;
            }
            CurrentPosition = Mathf.SmoothStep(_StartPosition, CurrentPage, t);

            if (CurrentPage == _MaxPage) _EffectTime += Time.deltaTime;
            if (!_IsFinishTurnPaper && _EffectTime >= _TurnTime) _IsFinishTurnPaper = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GoToNextPage()
        {
            if (_InputPageNumber >= _MaxPage) return;
            Debug.Log(_InputPageNumber + "  ->  "+ (_InputPageNumber + 1));

            _InputPageNumber++;
            CurrentPage = _InputPageNumber;

            Sound.PlaySE("TurnPage", 1f);
        }

        /// <summary>
        /// 
        /// </summary>
        public void BackToPrevPage()
        {
            if (_InputPageNumber <= 0) return;
            Debug.Log(_InputPageNumber + "  ->  "+ (_InputPageNumber - 1));

            _InputPageNumber--;
            CurrentPage = _InputPageNumber;

            Sound.PlaySE("TurnPage", 1f);
        }

        public void ToUseableButton()
        {
            _EventSystem = EventSystem.current;
            _EventSystem.SetSelectedGameObject(_SelectGameObject);
        }

        public void DisplayBGImage()
        {
            var color = _ImagePaperT.color;
            color.a = 1.0f;
            _ImagePaperT.color = color;
        }
        #endregion

        #region private function
        private Matrix4x4 CalcCanvas2LocalMatrix()
        {
            var canvaslist = GetComponentsInParent<Canvas>();
            return transform.worldToLocalMatrix * canvaslist[canvaslist.Length - 1].transform.worldToLocalMatrix.inverse;
        }

        private Vector2 CalcResolution()
        {
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize)
                {
                    var canvas = GetComponent<Canvas>();
                    if (canvas.isRootCanvas)
                        return scaler.referenceResolution;
                }
            var rect = GetComponent<RectTransform>().rect;
            return new Vector2(rect.width, rect.height);
        }

        /// <summary>
        /// Start関数内で、モードに応じた変数の設定を行う
        /// </summary>
        private void SetUpComponent()
        {
            switch (_Mode)
            {
                case ModeEnum.ClearEffect:
                    {
                        _PaperT = transform.Find("RawImage/PlainPageR/PlainPageImage").gameObject;
                        _ImagePaperT = _PaperT.GetComponent<Image>();

                        _SelectGameObject = transform.Find("RawImage/ClearPageR/ClearPanel/NextStageButton").gameObject;
                        var color = _ImagePaperT.color;
                        color.a = 0.0f;
                        _ImagePaperT.color = color;

                        _MaxPage = (transform.GetChild(0).gameObject.transform.childCount / 2) - 1;
                    }
                    break;
                case ModeEnum.CutInEffect:
                    {
                        _PaperT = transform.Find("RawImage/FadeOutPageR/FadeOutPanel/ClearPageImage").gameObject;
                        _ImagePaperT = _PaperT.GetComponent<Image>();
                        var color = _ImagePaperT.color;
                        color.a = 0.5f;
                        _ImagePaperT.color = color;

                        _MaxPage = (transform.GetChild(0).gameObject.transform.childCount / 2) - 1;
                        //Debug.Log(_Mode+" MaxPage : "+ _MaxPage);
                    }
                    break;
                case ModeEnum.RetryEffect:
                    {
                        _MaxPage = (transform.GetChild(0).gameObject.transform.childCount / 2) - 1;
                    }
                    break;
                case ModeEnum.StageSelect:
                    {
                        _MaxPage = (transform.GetChild(0).gameObject.transform.childCount / 2) - 1;
                    }
                    break;
                default:
                    {
                        Debug.Log("不正な値が入力されています。");
                    }
                    break;
            }
        }

        /// <summary>
        /// BookUI更新用関数
        /// </summary>
        private void BookUpdate()
        {
            switch (_Mode)
            {
                case ModeEnum.CutInEffect:
                    {
                        if (!_IsStartFirstCutIn)
                        {
                            _CutInWaitTime += (0.5f * Time.deltaTime);
                            if (_CutInWaitTime > 0.5f)
                            {
                                GoToNextPage();
                                _IsStartFirstCutIn = true;
                            }
                            return;
                        }
                        TurnPageUpdate();
                        if (_IsFinishTurnPaper) FadeOutPlainPage();
                    }
                    break;
                case ModeEnum.ClearEffect:
                    {
                        FadeInPlainPage();
                    }
                    break;
                case ModeEnum.RetryEffect:
                    {
                    }
                    break;
                case ModeEnum.StageSelect:
                    {
                        TurnPageUpdate();
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }

        /// <summary>
        /// UIが動くかどうか、キー入力でチェック
        /// </summary>
        private void DebugKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.V)) GoToNextPage();
            if (Input.GetKeyDown(KeyCode.C)) BackToPrevPage();
        }

        public virtual void DebugLog()
        {
            Debug.Log("親");
        }
        #endregion
    }
}
