using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectGroundType : MonoBehaviour
{
    public enum GroundType
    {
        none = 0,
        @default = 1,
        stone = 2,
        wood = 3,
        water = 4,
        ruinDefault = 5,
        ruinWater = 6,
    }

    [SerializeField]GroundType groundType;
    
    public GroundType GetGroundType() { return groundType; }
}
