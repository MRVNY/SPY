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

    public Tile Charac;
    public Tile Base;
    public Tile Road;
    public Tile Split;
    public Tile Merge;
}
