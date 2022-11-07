using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class LevelEditor : MonoBehaviour
{
    public GameObject InputAgent;
    public GameObject InputId;
    public Transform Canvas;

    [Header("Tilemap Layers")]
    public Tilemap Objects;
    public Tilemap Map;
    
    // [Header("Tile Prefabs")]
    // public Tile Road;
    // public Tile Obstacle;
    // public Tile Start;
    // public Tile Goal;

    //public bool haveTwoAgent = false;
    
    public bool AgentsAutoNameing = true;
    public bool ConsoleAutoLinking = true;
    
    public bool dragdropDisabled = false;
    public bool fog = false;

    public int executionLimit = 1;

    [Header("Block Limits")] 
    //public List<Limit> blockLimits;
    
    public int Forward = 1;
    public int TurnLeft = 0;
    public int TurnRight = 0;
    public int Wait = 0;
    public int Activate = 0;
    public int TurnBack = 0;
    public int If = 0;
    public int IfElse = 0;
    public int For = 0;
    public int While = 0;
    public int Forever = 0;
    public int AndOperator = 0;
    public int OrOperator = 0;
    public int NotOperator = 0;
    public int Wall = 0;
    public int Enemie = 0;
    public int RedArea = 0;
    public int FieldGate = 0;
    public int Terminal = 0;
    public int Exit = 0;

    public int TwoStars = 0;
    public int ThreeStars = 10000;
}
