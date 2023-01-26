using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelMap : MonoBehaviour
{
    
    public Vector3Int CharacPos;
    public Tilemap CharacMap;
    public Tilemap Map;
    public Tilemap Stars;

    public Tile Charac;
    public Tile LockedBase;
    public Tile Base;
    public Tile Castle;
    public Tile Road;
    public Tile Split;
    public Tile Merge;
    public Tile UpRight;
    public Tile DownRight;

    public Tile Done;
    public Tile Undone;
    public Tile Code;
    public Tile Exec;
    public Tile All;

    public TextMeshProUGUI LevelName;
    public Button StartLevel;
}
