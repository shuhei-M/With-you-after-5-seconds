using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class MyCinemachineDollyCart : MonoBehaviour
{
    #region serialize field
    [SerializeField] CinemachinePathBase _M_Path;

    [SerializeField] CinemachinePathBase.PositionUnits _M_PositionUnits = CinemachinePathBase.PositionUnits.Distance;

    [FormerlySerializedAs("m_CurrentDistance")]
    public float _M_Position;
    #endregion

    #region property
    public float BirdPosition { get { return _M_Position; } }
    #endregion

    #region Unity function
    void Update()
    {
        SetCartPosition(_M_Position);
    }
    #endregion

    #region private function
    /// <summary>
    /// レールが坂道になっている際、オブジェクトが不自然に傾かないようにする
    /// </summary>
    void SetCartPosition(float distanceAlongPath)
    {
        if (_M_Path != null)
        {
            _M_Position = _M_Path.StandardizeUnit(distanceAlongPath, _M_PositionUnits);
            transform.position = _M_Path.EvaluatePositionAtUnit(_M_Position, _M_PositionUnits);
            var tangent = _M_Path.EvaluateTangentAtUnit(_M_Position, _M_PositionUnits);
            tangent.y = 0; // Y方向を無効
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }
    #endregion
}
