using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    const int kCellCount = 36;

    [SerializeField]
    CellLayout m_CellPrefab = null;
    [SerializeField]
    RectTransform m_TableRoot = null;

    CellUnit[] mCells = null;
    CellUnit[,] mCells2D = new CellUnit[6, 6]; // 連結用

    void Start()
    {
        mCells = new CellUnit[kCellCount];
        for (int i = 0; i < kCellCount; i++)
        {
            var cell = new CellUnit();
            mCells[i] = cell;
            mCells2D[i % 6, i / 6] = cell;
        }

        for (int i = 0; i < kCellCount; i++)
        {
            var cell = mCells[i];
            cell.Layout = Instantiate(m_CellPrefab, m_TableRoot.transform);
            cell.Index = i;
            cell.CellStatus = GetStatusByInitialIndex(i);

            // init layout - checked
            cell.Layout.SetStatus(cell.CellStatus);

            // link cells - checked
            var x = i % 6;
            var y = i / 6;
            LinkCell(cell, LinkPos.Up, x, y - 1);
            LinkCell(cell, LinkPos.UpRight, x + 1, y - 1);
            LinkCell(cell, LinkPos.Right, x + 1, y);
            LinkCell(cell, LinkPos.BottomRight, x + 1, y + 1);
            LinkCell(cell, LinkPos.Bottom, x, y + 1);
            LinkCell(cell, LinkPos.BottomLeft, x - 1, y + 1);
            LinkCell(cell, LinkPos.Left, x - 1, y);
            LinkCell(cell, LinkPos.UpLeft, x - 1, y - 1);
        }

    }
    CellStatus GetStatusByInitialIndex(int index)
    {
        var columMod = index % 2;
        var rowMod = (index / 6) % 2;
        var totalMod = (columMod + rowMod) % 2;

        return totalMod == 0 ? CellStatus.Black : CellStatus.White;
    }

    void LinkCell(CellUnit cell, LinkPos pos, int x, int y)
    {
        cell.Neighbors[pos] = GetCellByPos(x, y);
    }

    CellUnit GetCellByPos(int x, int y)
    {
        if (x < 0)
            return null;
        if (x >= 6)
            return null;
        if (y < 0)
            return null;
        if (y >= 6)
            return null;

        return mCells2D[x, y];
    }

    #region test

    public int Test_CellX = 0;
    public int Test_CellY = 0;

    [ContextMenu("test - Neighbors")]
    void Test_ShowIndexInfo()
    {
        var cell = GetCellByPos(Test_CellX, Test_CellY);

        SetNeighborInfo(cell, LinkPos.Up, "U");
        SetNeighborInfo(cell, LinkPos.UpRight, "UR");
        SetNeighborInfo(cell, LinkPos.Right, "R");
        SetNeighborInfo(cell, LinkPos.BottomRight, "BR");
        SetNeighborInfo(cell, LinkPos.Bottom, "B");
        SetNeighborInfo(cell, LinkPos.BottomLeft, "BL");
        SetNeighborInfo(cell, LinkPos.Left, "L");
        SetNeighborInfo(cell, LinkPos.UpLeft, "UL");
    }

    void SetNeighborInfo(CellUnit cell, LinkPos pos, string info)
    {
        var n = cell.Neighbors[pos];
        if(n != null)
            n.Layout.Info = info;
    }
    #endregion

}


public enum LinkPos
{
    Up = 0,
    UpRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
    UpLeft
}