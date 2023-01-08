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
    /// ���[�����⓹�ɂȂ��Ă���ہA�I�u�W�F�N�g���s���R�ɌX���Ȃ��悤�ɂ���
    /// </summary>
    void SetCartPosition(float distanceAlongPath)
    {
        if (_M_Path != null)
        {
            _M_Position = _M_Path.StandardizeUnit(distanceAlongPath, _M_PositionUnits);
            transform.position = _M_Path.EvaluatePositionAtUnit(_M_Position, _M_PositionUnits);
            var tangent = _M_Path.EvaluateTangentAtUnit(_M_Position, _M_PositionUnits);
            tangent.y = 0; // Y�����𖳌�
            transform.rotation = Quaternion.LookRotation(tangent);
        }
    }
    #endregion
}
