using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelEditor : MonoBehaviour
{

    public Tile Road;
    public Tile Wall;
    public Tile Start;
    public Tile Goal;

    public List<Vector3> availablePlaces;

    public bool haveTwoAgent = false;
    
    public List<String> convo = new List<String>();
}
