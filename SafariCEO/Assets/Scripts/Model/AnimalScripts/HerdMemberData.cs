using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using static Tile;
public class HerdMemberData : MonoBehaviour
{
    public int HerdMemberID { get; set; }
    public float HungerTimer { get; set; } 
    public float ThirstTimer { get; set; }
    public float MateTimer { get; set; }
    public float Age { get; set; }
    public float MaxAge { get; set; }
}