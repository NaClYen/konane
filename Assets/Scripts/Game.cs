using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField]
    int m_BoardSize = 8;
    [SerializeField]
    GridLayoutGroup m_BoardGridLayoutGroup = null;
    [SerializeField]
    CellLayout m_CellPrefab = null;
    [SerializeField]
    ChessLayout m_ChessPrefab = null;
    [SerializeField]
    HintLayout m_HintPrefab = null;
    [SerializeField]
    RectTransform m_TableRoot = null;
    [SerializeField]
    RectTransform m_IdleChessRoot = null;

    CellList mCells = null;

    Queue<IChessUnit> mIdleChesses = new Queue<IChessUnit>(Options.CellCount);
    Queue<IChessUnit> mActiveChesses = new Queue<IChessUnit>(Options.CellCount);

    Queue<IHintLayout> mIdleHint = new Queue<IHintLayout>(Options.CellCount);
    Queue<IHintLayout> mActiveHint = new Queue<IHintLayout>(Options.CellCount);



    InfoCenter mInfoCenter = new InfoCenter();

    void Start()
    {
        Options.BoardSize = m_BoardSize;
        Options.CellCount = Options.BoardSize * Options.BoardSize;

        // adjust
        m_BoardGridLayoutGroup.constraintCount = Options.BoardSize;

        mInfoCenter.OnAnyEvent += InfoCenter_OnAnyEvent;

        InitCells();

        // init chess
        InitChess();
    }

    private void InfoCenter_OnAnyEvent(string msg, object args)
    {
        Debug.Log($"[InfoCenter]{msg}, args: {args}");

        switch (msg)
        {
            case Options.kEvCellTouched:
                HandleEvCellTouched(args);
                break;
            default:
                break;
        }
    }

    void HandleEvCellTouched(object args)
    {
        var id = (int)args;

        Debug.Log($"[HandleEvCellTouched]id: {id}");

        var hint = Instantiate(m_HintPrefab, m_IdleChessRoot);
        hint.HintType = HintType.CanAttack;
        hint.AppendTo(mCells.Get(id).Layout.Transform);
    }

    void InitCells()
    {
        mCells = new CellList();

        for (int i = 0; i < Options.CellCount; i++)
        {
            var cell = mCells.Get(i);
            cell.Layout = Instantiate(m_CellPrefab, m_TableRoot.transform);
            cell.Layout.Init(mInfoCenter, i);
            cell.Index = i;

            // link cells - checked
            var x = i % Options.BoardSize;
            var y = i / Options.BoardSize;
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
        }
    }

    void InitChess()
    {
        mIdleChesses = new Queue<IChessUnit>(Options.CellCount);
        for (int i = 0; i < Options.CellCount; i++)
        {
            var chess = new ChessUnit();
            chess.ChessType = GetChessTypeByInitialIndex(i); // 設定初始陣營
            chess.Layout = Instantiate(m_ChessPrefab, m_IdleChessRoot);

            AppendChessToCell(chess, mCells.Get(i)); // 直接附加在對應的 cell 上
            chess.Layout.ChessType = chess.ChessType; // refresh UI
            mActiveChesses.Enqueue(chess); // 丟進工作中的池內
        }
    }

    void AppendChessToCell(IChessUnit chess, CellUnit cell)
    {
        var to = cell.Layout.Transform;
        // TODO: 應該要處理 `to` 為 null 的狀況
        chess.Layout.AppendTo(to);
    }


    ChessType GetChessTypeByInitialIndex(int index)
    {
        var columMod = index % 2;
        var rowMod = (index / Options.BoardSize) % 2;
        var totalMod = (columMod + rowMod) % 2;

        return totalMod == 0 ? ChessType.Black : ChessType.White;
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

class CellList
{

    CellUnit[] mCells = null;
    CellUnit[,] mCells2D = new CellUnit[Options.BoardSize, Options.BoardSize];

    public CellList()
    {
        mCells = new CellUnit[Options.CellCount];

        for (int i = 0; i < Options.CellCount; i++)
        {
            var cell = new CellUnit();
            mCells[i] = cell;
            mCells2D[i % Options.BoardSize, i / Options.BoardSize] = cell;
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