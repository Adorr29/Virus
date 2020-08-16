using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    //public new string name; // ???
    public string type; // ?
    public Vector2Int position;
    public Entity entity;

    // variables for A* pathfinding
    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
    public Tile parent;
}