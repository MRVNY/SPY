using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelEditor : MonoBehaviour
{

    public Tile Road;
    public Tile Obstacle;
    public Tile Start;
    public Tile Goal;
    
    public bool haveTwoAgent = false;
    
    public List<String> convo = new List<String>();

    public bool dragdropDisabled = false;
    public bool fog = false;

    public int executionLimit = 1;
    
    [Header("blockLimits")]
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
    //
    // public List<Vector2> Coins = new List<Vector2>();
    // public List<Vector4> Consoles = new List<Vector4>();

    public Script StartPositions;
    
}

[System.Serializable]
public class Script
{
    public string one;
    public Script child;
    public string two;
}
