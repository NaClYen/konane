using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellUnit
{
    public int Index { get; set; }

    public ICellLayout Layout { get; set; }

    public Dictionary<LinkPos, CellUnit> Neighbors = new Dictionary<LinkPos, CellUnit>();
}
