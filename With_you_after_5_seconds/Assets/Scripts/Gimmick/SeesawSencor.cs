using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region define
public enum SeesawSencorType : int
{
    Plus,
    Minus
}
#endregion

public class SeesawSencor : MonoBehaviour
{
    #region serialize field
    /// <summary> シーソーのどちら側に設置されているか </summary>
    [SerializeField] public SeesawSencorType _Type;
    #endregion

    #region field
    private float _Count;
    private bool _CanRiding = false;
    private bool _IsPlayerRide = false;
    private bool _IsAfterimageRide = false;
    #endregion

    #region property
    public bool CanRiding { get { return _CanRiding; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Count = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!(_IsPlayerRide || _IsAfterimageRide)) _Count = 0;
        _CanRiding = TryRiding();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _Count += Time.deltaTime;
            _IsPlayerRide = true;
        }
        if (other.gameObject.tag == "Afterimage")
        {
            _Count += Time.deltaTime;
            _IsAfterimageRide = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _IsPlayerRide = false;
        }
        if (other.gameObject.tag == "Afterimage")
        {
            _IsAfterimageRide = false;
        }
    }
    #endregion

    #region private function
    private bool TryRiding()
    {
        if (_Count > 0.1f) return true;
        else return false;
    }
    #endregion
}
