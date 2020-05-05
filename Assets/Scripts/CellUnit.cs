using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellUnit
{
    public int Index { get; set; }

    public CellStatus CellStatus { get; set; }

    public CellLayout Layout { get; set; }

    public Dictionary<LinkPos, CellUnit> Neighbors = new Dictionary<LinkPos, CellUnit>();
}

public enum CellStatus
{
    Empty = 0,
    Black = 1,
    White = 2
}