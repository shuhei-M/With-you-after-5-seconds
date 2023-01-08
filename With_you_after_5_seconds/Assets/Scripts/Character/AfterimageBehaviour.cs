using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region define
/// <summary>
/// 残像がプレイ中のギミック（残像に影響を与えるもののみ）
/// </summary>
enum PlayGimmickType : int
{
    None,
    Seesaw,
    SunnySpot,
}
#endregion

/// <summary>
/// 残像を制御するクラス
/// </summary>
public class AfterimageBehaviour : MonoBehaviour
{
    #region define
    enum MeshType : int
    {
        Skin,
        Clothes,
        Bag,
        Guard,   // 番兵
    }

    enum MaterialState : int
    {
        Nomal,
        ToTransparent,
        Transparent,
        ToNomal,
    }
    #endregion

    #region serialize field
    /// <summary> ステージ内のプレイヤーなどのオブジェクトの情報を取得する </summary>
    [SerializeField] private StageController _StageManager;

    /// <summary> プレイヤーの挙動を記録したゲームオブジェクト。5秒間遅らせて再現する。 </summary>
    [SerializeField] private Recorder _Recorder;

    /// <summary> 残像の元々のマテリアル </summary>
    [SerializeField] private Material[] _OriginMaterials;

    /// <summary> 残像の透明化用のマテリアル </summary>
    [SerializeField] private Material[] _TransparentMaterials;

    #endregion

    #region field
    /// <summary> レコーダーの情報を基に動きをトレースするための変数群 </summary>
    private float _RecordDuration = 0.005f;
    private int _DataSwitcher = 0;
    private int _MaxDataNum = 2000;
    private bool _IsPlayRecord = true;

    /// <summary> 残像にアタッチされたコンポーネントを取得ための変数群 </summary>
    private Animator _Animator;
    private AnimatorStateInfo _AimStateInfo;
    //private Renderer _Renderer;
    private CharacterController _CharacterController;

    /// <summary> 残像のステートを管理するための変数群 </summary>
    private PlayerState _CurrentState;
    private PlayerState _PrevState;
    private float _MoveDeltaTime = 0.0f;
    private float _MoveBlend = 0.0f;
    private bool _IsLadder = false;
    private int _ActionType;

    /// <summary> シーソーギミック中の残像の動きを制御するための変数群 </summary>
    private PlayGimmickType _PlayGimmick;
    private float _RayDistance = 6.0f;
    private RaycastHit[] _RaycastHits = new RaycastHit[10];
    private float _SeesawX = 0.0f;

    /// <summary> プレイヤーと重なった際に、残像を透けさせるための変数群 </summary>
    private float _OverlapTime = 0.0f;
    private Transform _PlayerTransform;
    private float _LimitDistance = 0.75f;
    // 変更するメッシュをマテリアルごとに分ける
    List<Renderer>[] _M_Renderers = new List<Renderer>[(int)MeshType.Guard];
    MaterialState _MaterialState = MaterialState.Nomal;   // マテリアルの状態
    private GameObject _AfterimageBody;

    /// <summary> ゲーム開始最初の５秒を固定のスタート地点にいさせるための変数群 </summary>
    private Vector3 startPos;
    private Quaternion startRote;
    private float recordDeltaTime = 0.0f;

    private GameObject _FloatEffectOjb = null;

    public GameData gameData;

    /// <summary> ゲーム開始時エフェクトを作動させないためのフラグ </summary>
    bool isFirstPos;
    #endregion

    #region property
    /// <summary> 残像のステートを外部から取得する </summary>
    public PlayerState AfterimageState { get { return _CurrentState; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        _PlayGimmick = PlayGimmickType.None;
        _CharacterController = GetComponent<CharacterController>();

        isFirstPos = true;

        // 重なり判定用のプレイヤーの位置取得
        _PlayerTransform = _StageManager._PlayerTransform;

        // 重なり時の透過用に、メッシュごとのレンダラーを取得
        RenderersSetUp();
        SetMaterials(_OriginMaterials);
        _AfterimageBody = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameData.InGameState != InGame.ChangeStartView && 
            gameData.InGameState != InGame.EntryPlayer &&
            recordDeltaTime < 5.1f)
        {
            recordDeltaTime += Time.deltaTime;
        }
           

        // ゲーム開始五秒後に、プレイヤーの挙動のトレースを開始する
        if (_IsPlayRecord)
        {
            StartAfterimage();
            _IsPlayRecord = false;
        }

        // 残像のステートを基にアニメーションを更新
        AnimatorUpdate();

        if(_PlayGimmick != PlayGimmickType.SunnySpot)
        {
            // 条件秒以上プレイヤーと重なっていれば、残像を透けさせる。
            CheckOverlap();
            ChangeAlphas();
        }
        //ToTransparent(isOverlap);

        //DebugLog();

        // 更新前の残像のステートを保存
        _PrevState = _CurrentState;
    }

    private void OnTriggerStay(Collider other)
    {
        // シーソーギミックのエリアに入ったら、残像のY座標を調整する
        if (other.gameObject.tag == "SeesawArea")
        {
            float otherY = other.gameObject.transform.position.y;
            float gap = otherY - transform.position.y;
            if (gap > 2.4f/* || gap < 0.0f*/) return;

            _PlayGimmick = PlayGimmickType.Seesaw;
            _SeesawX = other.gameObject.transform.position.x;
        }

        // 日向エリアに入ったら残像を消す
        if (other.gameObject.tag == "SunnySpot")
        {
            _PlayGimmick = PlayGimmickType.SunnySpot;
            _AfterimageBody.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // シーソーギミックエリアから出たら、ステートをNoneにする。
        if (other.gameObject.tag == "SeesawArea")
        {
            _PlayGimmick = PlayGimmickType.None;
        }

        // 日向エリアから出たら、ステートをNoneにし、見た目を戻す。
        if (other.gameObject.tag == "SunnySpot")
        {
            _PlayGimmick = PlayGimmickType.None;
            _AfterimageBody.SetActive(true);
        }
    }
    #endregion

    #region public function
    #endregion

    #region private function
    /// <summary>
    /// 変更するレンダラーのリストの配列をセットアップ
    /// </summary>
    private void RenderersSetUp()
    {
        for (int i = 0; i < (int)MeshType.Guard; i++)
        {
            _M_Renderers[i] = new List<Renderer>();
        }
        GameObject bodyMeshs = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        int childlenCount = bodyMeshs.transform.childCount;
        for (int i = 0; i < childlenCount; i++)
        {
            GameObject mesh = bodyMeshs.transform.GetChild(i).gameObject;
            MeshType materialType = MeshType.Guard;
            if (mesh.tag == "Skin")
            {
                materialType = MeshType.Skin;
            }
            else if (mesh.tag == "Clothes")
            {
                materialType = MeshType.Clothes;
            }
            else if(mesh.tag == "Bag")
            {
                materialType = MeshType.Bag;
            }
            else
            {
                Debug.Log("エラー。マテリアルの種類が取得でいていません。");
            }

            _M_Renderers[(int)materialType].Add(mesh.GetComponent<Renderer>());
        }

        // 透明化用のマテリアルのアルファ値を初期化
        for(int i = 0; i < (int)MeshType.Guard; i++)
        {
            Color c = _TransparentMaterials[i].color;
            c.a = 1.0f;
            _TransparentMaterials[i].color = c;
        }
    }

    /// <summary>
    /// 残像の全てのメッシュに特定のマテリアルをセットする。
    /// </summary>
    private void SetMaterials(Material[] materials)
    {
        for (int i = 0; i < (int)MeshType.Guard; i++)
        {
            for(int j = 0; j < _M_Renderers[i].Count; j++)
            {
                _M_Renderers[i][j].material = materials[i];
            }
        }
    }

    /// <summary>
    /// 1秒以上重なっているか
    /// </summary>
    private void CheckOverlap()
    {
        float distance = Vector3.Distance(transform.position, _PlayerTransform.position);
        float overlapCheckTime = 1.0f;

        // 重なっていれば
        if (distance < _LimitDistance/*transform.position == _playerTransform.position*/)
        {
            
        }
        else
        {
            // 重なっていなければ見た目を元に戻す
            _OverlapTime = 0.0f;
            if(_MaterialState != MaterialState.Nomal) _MaterialState = MaterialState.ToNomal;
            return;
        }

        // 重なってからの経過時間を計算
        _OverlapTime += Time.deltaTime;

        // 重なってから一秒経った場合、透明化開始
        if (_MaterialState != MaterialState.ToTransparent && _MaterialState != MaterialState.Transparent
            && _OverlapTime > overlapCheckTime
            && _PlayGimmick != PlayGimmickType.SunnySpot)
        {
            SetMaterials(_TransparentMaterials);
            _MaterialState = MaterialState.ToTransparent;
        }
        else
        {

        }
    }

    /// <summary>
    /// 各マテリアルのアルファ値を変更する
    /// ToTransparent、ToNomal状態の時のみ使用出来る。
    /// </summary>
    private void ChangeAlphas()
    {
        float m_step = (Time.deltaTime / 2.0f);
        float minAlpha = 0.1f;
        float maxAlpha = 1.0f;

        // 徐々に透明になっていく
        if (_MaterialState == MaterialState.ToTransparent)
        {
            for (int i = 0; i < (int)MeshType.Guard; i++)
            {
                for (int j = 0; j < _M_Renderers[i].Count; j++)
                {
                    Color c = _M_Renderers[i][j].material.color;
                    c.a -= m_step;
                    _M_Renderers[i][j].material.color = c;
                }
            }

            //alphaが最小値に到達していたら
            if(_M_Renderers[0][0].material.color.a < minAlpha)
            {
                for (int i = 0; i < (int)MeshType.Guard; i++)
                {
                    for (int j = 0; j < _M_Renderers[i].Count; j++)
                    {
                        Color c = _M_Renderers[i][j].material.color;
                        c.a = minAlpha;
                        _M_Renderers[i][j].material.color = c;
                    }
                }
                _MaterialState = MaterialState.Transparent;
            }
        }
        // 徐々に元に戻っていく
        else if(_MaterialState == MaterialState.ToNomal)
        {
            for (int i = 0; i < (int)MeshType.Guard; i++)
            {
                for (int j = 0; j < _M_Renderers[i].Count; j++)
                {
                    Color c = _M_Renderers[i][j].material.color;
                    c.a += m_step;
                    _M_Renderers[i][j].material.color = c;
                }
            }

            //alphaが最大値に到達していたら
            if (_M_Renderers[0][0].material.color.a > maxAlpha - 0.0001f)
            {
                for (int i = 0; i < (int)MeshType.Guard; i++)
                {
                    for (int j = 0; j < _M_Renderers[i].Count; j++)
                    {
                        Color c = _M_Renderers[i][j].material.color;
                        c.a = maxAlpha;
                        _M_Renderers[i][j].material.color = c;
                    }
                }
                _MaterialState = MaterialState.Nomal;
                SetMaterials(_OriginMaterials);
            }
        }
    }

    /// <summary>
    /// デバッグ用
    /// </summary>
    private void DebugLog()
    {
        if (_PrevState == _CurrentState) return;

        Debug.Log(_CurrentState);
    }

    /// <summary>
    /// 残像による動きのトレースを開始する
    /// </summary>
    private void StartAfterimage()
    {
        Debug.Log("StartAfterimage");
        if (_Recorder[_DataSwitcher] == null)
        {
            Debug.Log("ゴーストデータがありません");
        }
        else
        {
            transform.position = AjustPosition(_Recorder[_DataSwitcher].Get_PosLists(0));
            transform.rotation =_Recorder[_DataSwitcher].Get_RotLists(0);
            _CurrentState = _Recorder[_DataSwitcher].Get_PlayerStateLists(0);
            StartCoroutine(PlayBack());
        }
    }

    /// <summary>
    /// 0.005f秒毎に、5秒前のプレイヤーのデータをセットする
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayBack()
    {
        int i = 0;

        while (true)
        {
            yield return new WaitForSeconds(_RecordDuration);

            Vector3 prevVec = transform.position;

            if(gameData.InGameState == InGame.ChangeStartView || gameData.InGameState == InGame.EntryPlayer)
            {
                this.gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false) ;
            }
            else if (recordDeltaTime <= 5.1f)
            {   // 開始5秒間はスタート位置で停止させる
                this.gameObject.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
                if (startPos == Vector3.zero)
                {
                    startPos = _PlayerTransform.position;
                    startRote = _PlayerTransform.rotation;
                }
                this.transform.position = startPos;
                this.transform.rotation = startRote;
            }
            else
            {
                transform.position = AjustPosition(_Recorder[_DataSwitcher].Get_PosLists(i));
                transform.rotation = _Recorder[_DataSwitcher].Get_RotLists(i);
                _MoveDeltaTime = _Recorder[_DataSwitcher].Get_DeltaTimeLists(i);
                _MoveBlend = _Recorder[_DataSwitcher].Get_BlendLists(i);
                _IsLadder = _Recorder[_DataSwitcher].Get_IsLadderLists(i);
                _ActionType = _Recorder[_DataSwitcher].Get_ActionTypeLists(i);

                _CurrentState = _Recorder[_DataSwitcher].Get_PlayerStateLists(i);
            }
            i++;

            // 一回の更新で条件距離以上移動していれば、エフェクトを発生させる
            TryGanarateTeleportationEffect(transform.position, prevVec);

            //　保存データ数を超えたら次の保存データを再生
            if (i >= _MaxDataNum)
            {
                _Recorder[_DataSwitcher].Clear_AllLists();
                _DataSwitcher++;
                _DataSwitcher %= 2;
                i = 0;
            }
        }
    }

    /// <summary>
    /// 残像にセットする位置情報を適切な形に調節する（高さ制限、シーソー）
    /// </summary>
    /// <param name="position">レコーダーに記されたプレイヤーの座標</param>
    /// <returns>シーソーの高さに沿うように調節した残像の座標</returns>
    private Vector3 AjustPosition(Vector3 position)
    {
        Vector3 temp = position;

        if (position.y > _StageManager._HighestPosition)
        {
            temp = new Vector3(
                temp.x,
                _StageManager._HighestPosition,
                temp.z);
        }

        if (_PlayGimmick == PlayGimmickType.Seesaw)
        {
            bool hitSeesaw = false;

            Vector3 rayPosition = new Vector3(
                transform.position.x,
                transform.position.y + 4.0f,
                transform.position.z);
            //Debug.DrawRay(rayPosition, -transform.up * _rayDistance, Color.red);
            int hitCount = Physics.RaycastNonAlloc(rayPosition, -transform.up, _RaycastHits, _RayDistance);
            for (int i = 0; i < hitCount; i++)
            {
                if (_RaycastHits[i].collider.gameObject.tag == "Seesaw")
                {
                    hitSeesaw = true;
                    break;
                }
            }
            if (hitSeesaw)
            {
                var hitList = _RaycastHits.ToList();
                RaycastHit seesawPos = hitList.FirstOrDefault(item => (item.collider.gameObject.tag == "Seesaw"));
                temp = new Vector3(
                    temp.x,
                    seesawPos.point.y,
                    temp.z);
            }
        }

        return temp;
    }

    /// <summary>
    /// 一回の更新で条件距離以上移動していれば、エフェクトを発生させる
    /// </summary>
    /// <param name="currentPos">現在の座標</param>
    /// <param name="prevPos">更新前の座標</param>
    private void TryGanarateTeleportationEffect(Vector3 currentPos, Vector3 prevPos)
    {
        float distance = (currentPos - prevPos).magnitude;

        // 移動距離が1以下であれば以下の処理は行わない
        if (distance < 0.75f) return;

        // 0.0f (current)  ⇔  1.0f (prev)
        Vector3 genaratePos = Vector3.Lerp(currentPos, prevPos, 0.75f) + transform.up;

        //最初だけエフェクトを再生したくない
        if (!isFirstPos)
        {
            GameObject teleportationEffectObj = EffectManager.Instance.Play(EffectManager.EffectID.Teleportation, genaratePos);
            Destroy(teleportationEffectObj, 1.0f);
        }
        isFirstPos = false;
    }

    /// <summary>
    /// 残像のアニメーションステートを管理
    /// </summary>
    private void AnimatorUpdate()
    {
        // 移動入力の経過時間と、Lスティックの倒し具合をアニメーターにセット
        _Animator.SetFloat("DeltaTime", _MoveDeltaTime);
        _Animator.SetFloat("MoveBlend", _MoveBlend);
        _Animator.SetBool("IsLadder", _IsLadder);
        _Animator.SetInteger("ActionType", _ActionType);

        // ステートが変わっていなければ、基本的にはすぐ戻る。トリガーのリセット以外は行わない。
        if (_PrevState == _CurrentState)
        {
            if (_CurrentState == PlayerState.Idle) _Animator.ResetTrigger("ToMove");
            else if (_CurrentState == PlayerState.Ride) _Animator.ResetTrigger("ToMove");
            else if(_CurrentState == PlayerState.Move) _Animator.ResetTrigger("ToRide");
            return;
        }

        switch (_CurrentState)
        {
            case PlayerState.Idle:
                {
                    if (_PrevState == PlayerState.Ride) _Animator.SetTrigger("ToMove");
                    else if (_PrevState == PlayerState.Action_PushOrPull) _Animator.SetTrigger("ToIdle");
                }
                break;
            case PlayerState.Action:
                {
                    _Animator.SetTrigger("ToAction");
                }
                break;
            case PlayerState.Action_PushOrPull:
                {
                    if (_PrevState == PlayerState.Move || _PrevState == PlayerState.Idle) _Animator.SetTrigger("ToAction");
                }
                break;
            case PlayerState.Ride:
                {
                    if (_PrevState == PlayerState.Idle || _PrevState == PlayerState.Move) _Animator.SetTrigger("ToRide");

                    // 浮遊エフェクトの生成・座標調整
                    _FloatEffectOjb = EffectManager.Instance.Play(EffectManager.EffectID.Float, transform.position);
                }
                break;
            case PlayerState.Move:
                {
                    if (_PrevState == PlayerState.Ride) 
                    { 
                        _Animator.SetTrigger("ToMove"); _Animator.ResetTrigger("ToRide");
                    }
                    else if (_PrevState == PlayerState.Action_PushOrPull) 
                    { 
                        _Animator.SetTrigger("ToIdle"); 
                    }
                }
                break;
            default:
                break;
        }
    }
    #endregion
}
