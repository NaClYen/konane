using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class Game : MonoBehaviour
{
    #region inspector
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
    RectTransform m_IdleRoot = null;
    #endregion


    CellList mCells = null;

    ChessPool mChessPool = new ChessPool();
    HintPool mHintPool = new HintPool();
    InfoCenter mInfoCenter = new InfoCenter();

    GameStatus mCurrentStatus;

    HashSet<int> mFunctionalCells = new HashSet<int>();

    int mBleckPickedIndex = -1;
    int mAttackerSelection = -1;

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

        // init hint pool
        mHintPool.Init(m_HintPrefab, m_IdleRoot);

        SwitchGameStatus(GameStatus.BlackPickUp);
    }
    void InfoCenter_OnAnyEvent(string msg, object args)
    {
        Debug.Log($"[InfoCenter]{msg}, args: {args}");

        switch (msg)
        {
            case Options.kEvCellTouched:
                HandleEvCellTouched(args);
                break;
        }
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
            LinkCell(cell, LinkDirection.Up, x + 0, y - 1);
            LinkCell(cell, LinkDirection.UpRight, x + 1, y - 1);
            LinkCell(cell, LinkDirection.Right, x + 1, y + 0);
            LinkCell(cell, LinkDirection.BottomRight, x + 1, y + 1);
            LinkCell(cell, LinkDirection.Bottom, x + 0, y + 1);
            LinkCell(cell, LinkDirection.BottomLeft, x - 1, y + 1);
            LinkCell(cell, LinkDirection.Left, x - 1, y + 0);
            LinkCell(cell, LinkDirection.UpLeft, x - 1, y - 1);

            // debug
            cell.Layout.Info = i.ToString();
        }
    }

    void InitChess()
    {
        mChessPool.Init(m_ChessPrefab, m_IdleRoot);

        for (var i = 0; i < Options.CellCount; i++)
        {
            var chess = mChessPool.New();
            chess.ChessType = GetChessTypeByInitialIndex(i); // 設定初始陣營
            AppendChessToCell(chess, mCells.Get(i)); // 直接附加在對應的 cell 上
            chess.Layout.ChessType = chess.ChessType; // refresh UI
        }
    }

    void AppendChessToCell(IChessUnit chess, CellUnit cell)
    {
        var to = cell.Layout.Transform;
        // TODO: 應該要處理 `to` 為 null 的狀況
        chess.Layout.AppendTo(to);
        chess.Index = cell.Index;
    }


    ChessType GetChessTypeByInitialIndex(int index)
    {
        var columMod = index % 2;
        var rowMod = (index / Options.BoardSize) % 2;
        var totalMod = (columMod + rowMod) % 2;

        return totalMod == 0 ? ChessType.Black : ChessType.White;
    }

    void LinkCell(CellUnit cell, LinkDirection direction, int x, int y)
    {
        cell.Neighbors[direction] = GetCellByPos(x, y);
    }

    CellUnit GetCellByPos(int x, int y)
    {
        if (x < 0)
            return null;
        if (x >= Options.BoardSize)
            return null;
        if (y < 0)
            return null;
        if (y >= Options.BoardSize)
            return null;

        return mCells.GetByXy(x, y);
    }

    CellUnit GetJumpableCell(CellUnit cell, LinkDirection direction)
    {
        // 第一步要有格子且有棋子
        var cellStep1 = cell.Neighbors[direction];
        if (cellStep1 != null && mChessPool.Get(cellStep1.Index) != null)
        {
            // 第二步要有格子但不能有棋子
            var cellStep2 = cellStep1.Neighbors[direction];
            if (cellStep2 != null && mChessPool.Get(cellStep2.Index) == null)
            {
                return cellStep2;
            }
        }

        // 除此之外都NG
        return null;
    }

    void CleanAll()
    {
        mCells.ClearAllArgs();
        ClearHints();
        ClearFunctionalCells();
    }
    void ClearHints()
    {
        if (mHintPool.ActiveHints.Count <= 0)
            return;

        var hints = mHintPool.ActiveHints.ToArray();
        foreach (var hint in hints)
            mHintPool.Kill(hint);
    }
    void ClearFunctionalCells()
    {
        mFunctionalCells.Clear();
    }

    void ShowHintAt(CellUnit cell, HintType type)
    {
        // add to functional set
        mFunctionalCells.Add(cell.Index);

        var hint = mHintPool.CreateOrGetHint();
        hint.Layout.HintType = type;
        hint.Layout.AppendTo(cell.Layout.Transform);
        hint.Index = cell.Index;
    }
}