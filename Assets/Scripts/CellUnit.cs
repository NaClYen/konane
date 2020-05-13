using System.Collections.Generic;

public class CellUnit
{
    public int Index { get; set; }

    public ICellLayout Layout { get; set; }

    public Dictionary<LinkDirection, CellUnit> Neighbors = new Dictionary<LinkDirection, CellUnit>();

    public object Args { get; set; }
}
