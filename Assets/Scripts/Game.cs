using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    const int kCellCount = 36;

    [SerializeField]
    CellLayout m_CellPrefab = null;
    [SerializeField]
    ChessLayout m_ChessPrefab = null;
    [SerializeField]
    RectTransform m_TableRoot = null;
    [SerializeField]
    RectTransform m_IdleChessRoot = null;

    CellList mCells = null;
    IChessUnit[] mChesses = null;

    void Start()
    {
        InitCells();

        // init chess
        InitChess();
    }

    void InitCells()
    {
        mCells = new CellList();

        for (int i = 0; i < Options.kCellCount; i++)
        {
            var cell = mCells.Get(i);
            cell.Layout = Instantiate(m_CellPrefab, m_TableRoot.transform);
            cell.Index = i;

            // link cells - checked
            var x = i % Options.kColumn;
            var y = i / Options.kRow;
            LinkCell(cell, LinkPos.Up, x, y - 1);
            LinkCell(cell, LinkPos.UpRight, x + 1, y - 1);
            LinkCell(cell, LinkPos.Right, x + 1, y);
            LinkCell(cell, LinkPos.BottomRight, x + 1, y + 1);
            LinkCell(cell, LinkPos.Bottom, x, y + 1);
            LinkCell(cell, LinkPos.BottomLeft, x - 1, y + 1);
            LinkCell(cell, LinkPos.Left, x - 1, y);
            LinkCell(cell, LinkPos.UpLeft, x - 1, y - 1);

            // debug
            cell.Layout.Info = i.ToString();


            //cell.CellStatus = GetStatusByInitialIndex(i);
            //cell.Layout.SetStatus(cell.CellStatus);
        }
    }

    void InitChess()
    {
        mChesses = new IChessUnit[Options.kCellCount];
        for (int i = 0; i < Options.kCellCount; i++)
        {
            var chess = new ChessUnit();
            mChesses[i] = chess;

            chess.Layout = Instantiate(m_ChessPrefab, m_IdleChessRoot);
        }
    }


    CellStatus GetStatusByInitialIndex(int index)
    {
        var columMod = index % 2;
        var rowMod = (index / Options.kRow) % 2;
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

        return mCells.GetByXy(x, y);
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

public static class Options
{
    public const int kCellCount = 36;
    public const int kRow = 6;
    public const int kColumn = 6;

}


class CellList
{

    CellUnit[] mCells = null;
    CellUnit[,] mCells2D = new CellUnit[6, 6];

    public CellList()
    {
        mCells = new CellUnit[Options.kCellCount];

        for (int i = 0; i < Options.kCellCount; i++)
        {
            var cell = new CellUnit();
            mCells[i] = cell;
            mCells2D[i % Options.kColumn, i / Options.kRow] = cell;
        }
    }


    public CellUnit Get(int index)
    {
        return mCells[index];
    }

    public CellUnit GetByXy(int x, int y)
    {
        return mCells2D[x, y];
    }
}

public interface IChessUnit
{
    IChessLayout Layout { get; set; }
}

public interface IChessLayout
{
    ChessType ChessType { get; set; }
    void AppendTo(Transform t);
}

public interface IHintLayout
{
    HintType HintType { get; set; }
}


public interface ICellLayout
{
    string Info { get; set; }
    ICellLayout Init(IInfoCenter ic, int id);
}

public interface IInfoCenter
{
    void InvokeEvent(string msg, object args);
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


public enum ChessType
{
    Black,
    White
}


public enum HintType
{
    CanAttack,  // 可以進攻
    CanGoTo,    // 可到達
}