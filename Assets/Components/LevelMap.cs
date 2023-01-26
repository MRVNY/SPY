using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelMap : MonoBehaviour
{
    [Header("Maps")]
    public Vector3Int CharacPos;
    public Tilemap CharacMap;
    public Tilemap Map;
    public Tilemap Stars;

    [Header("Tiles")]
    public Tile Charac;
    public Tile LockedBase;
    public Tile Base;
    public Tile Castle;
    public Tile Road;
    public Tile Split;
    public Tile Merge;
    public Tile UpRight;
    public Tile DownRight;

    [Header("Stars")]
    public Tile Done;
    public Tile Undone;
    public Tile Code;
    public Tile Exec;
    public Tile All;

    [Header("UI")]
    public TextMeshProUGUI LevelName;
    public Button StartLevel;
}
