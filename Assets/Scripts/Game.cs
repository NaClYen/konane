using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    HashSet<IChessUnit> mActiveChesses = new HashSet<IChessUnit>();

    Queue<IHintLayout> mIdleHint = new Queue<IHintLayout>(Options.CellCount);
    HashSet<IHintLayout> mActiveHint = new HashSet<IHintLayout>();



    InfoCenter mInfoCenter = new InfoCenter();

    GameStatus mCurrentStatus;

    HashSet<int> mFunctionalCells = new HashSet<int>();

    int mBleckPickedIndex = -1;

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

        SwitchGameStatus(GameStatus.BlackPickUp);
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
        var isTouchFunctionCell = mFunctionalCells.Contains(id);

        Debug.Log($"[HandleEvCellTouched]mCurrentStatus:{mCurrentStatus}, id: {id}, isTouchFunctionCell: {isTouchFunctionCell}");



        switch (mCurrentStatus)
        {
            case GameStatus.None:
                break;
            case GameStatus.BlackPickUp:
                {
                    if (isTouchFunctionCell)
                        SwitchGameStatus(GameStatus.BlackPickUpConfirm, id);
                }
                break;
            case GameStatus.BlackPickUpConfirm:
                {
                    if (isTouchFunctionCell)
                    {
                        // remove chess 
                        KillChess(id);
                        mBleckPickedIndex = id; // 為了白棋拿棋用
                        SwitchGameStatus(GameStatus.WhitePickUp); // next
                    }
                    else
                        SwitchGameStatus(GameStatus.BlackPickUp); // above
                }
                break;
            case GameStatus.WhitePickUp:
                {
                    if (isTouchFunctionCell)
                        SwitchGameStatus(GameStatus.WhitePickUpConfirm, id);
                }
                break;
            case GameStatus.WhitePickUpConfirm:
                {
                    if (isTouchFunctionCell)
                    {
                        // remove chess 
                        KillChess(id);
                        SwitchGameStatus(GameStatus.BlackAttackFrom); // next
                    }
                    else
                        SwitchGameStatus(GameStatus.WhitePickUp); // above
                }
                break;
            case GameStatus.BlackAttackFrom:
                break;
            case GameStatus.BlackAttackTo:
                break;
            case GameStatus.WhiteAttackFrom:
                break;
            case GameStatus.WhiteAttackTo:
                break;
            case GameStatus.End:
                break;
            default:
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
            LinkCell(cell, LinkPos.Up,          x + 0, y - 1);
            LinkCell(cell, LinkPos.UpRight,     x + 1, y - 1);
            LinkCell(cell, LinkPos.Right,       x + 1, y + 0);
            LinkCell(cell, LinkPos.BottomRight, x + 1, y + 1);
            LinkCell(cell, LinkPos.Bottom,      x + 0, y + 1);
            LinkCell(cell, LinkPos.BottomLeft,  x - 1, y + 1);
            LinkCell(cell, LinkPos.Left,        x - 1, y + 0);
            LinkCell(cell, LinkPos.UpLeft,      x - 1, y - 1);

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
            mActiveChesses.Add(chess); // 丟進工作中的池內
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

    void LinkCell(CellUnit cell, LinkPos pos, int x, int y)
    {
        cell.Neighbors[pos] = GetCellByPos(x, y);
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

    IHintLayout CreateOrGetHint()
    {
        return (mIdleHint.Count < 1) ? Instantiate(m_HintPrefab, m_IdleChessRoot) : mIdleHint.Dequeue();
    }


    void SwitchGameStatus(GameStatus s, object args = null)
    {
        if (mCurrentStatus == s)
            throw new System.Exception($"不應該有一樣的遊戲狀態: {s}");

        mCurrentStatus = s;

        switch (mCurrentStatus)
        {
            case GameStatus.None:
                ClearHints();
                break;
            case GameStatus.BlackPickUp:
                {
                    ClearHints();
                    ClearFunctionalCellData();

                    // show hint
                    ShowHintAt(mCells.Get(0), HintType.Select); // 角落A
                    ShowHintAt(mCells.Get(Options.CellCount - 1), HintType.Select); // 角落B
                    var halfBoardSize = Options.BoardSize / 2;
                    ShowHintAt(mCells.GetByXy(halfBoardSize - 1, halfBoardSize - 1), HintType.Select); // 中間C
                    ShowHintAt(mCells.GetByXy(halfBoardSize, halfBoardSize), HintType.Select); // 中間D
                }
                break;
            case GameStatus.BlackPickUpConfirm:
                {
                    var id = (int)args;
                    ClearHints();
                    ClearFunctionalCellData();

                    // show hint
                    ShowHintAt(mCells.Get(id), HintType.Confirm);
                }
                break;
            case GameStatus.WhitePickUp:
                {
                    ClearHints();
                    ClearFunctionalCellData();

                    {
                        var blackPicked = mCells.Get(mBleckPickedIndex);

                        void ShowHintIfNeighborExist(LinkPos pos)
                        {
                            if (blackPicked.Neighbors[pos] != null)
                                ShowHintAt(blackPicked.Neighbors[pos], HintType.Confirm);
                        }

                        ShowHintIfNeighborExist(LinkPos.Bottom);
                        ShowHintIfNeighborExist(LinkPos.Left);
                        ShowHintIfNeighborExist(LinkPos.Up);
                        ShowHintIfNeighborExist(LinkPos.Right);
                    }
                }
                break;
            case GameStatus.WhitePickUpConfirm:
                {
                    var id = (int)args;
                    ClearHints();
                    ClearFunctionalCellData();

                    // show hint
                    ShowHintAt(mCells.Get(id), HintType.Confirm);
                }
                break;
            case GameStatus.BlackAttackFrom:
                break;
            case GameStatus.BlackAttackTo:
                break;
            case GameStatus.WhiteAttackFrom:
                break;
            case GameStatus.WhiteAttackTo:
                break;
            case GameStatus.End:
                break;
            default:
                break;
        }
    }

    void ClearHints()
    {
        if (mActiveHint.Count > 0)
        {
            var hints = mActiveHint.ToArray();
            foreach (var hint in hints)
            {
                hint.AppendTo(m_IdleChessRoot); // 移動至閒置區
                mIdleHint.Enqueue(hint); // 放進回收桶
            }
        }
    }
    void ClearFunctionalCellData()
    {
        mFunctionalCells.Clear();
    }

    void KillChess(int index)
    {
        IChessUnit unit = null;
        foreach (var chess in mActiveChesses)
        {
            if (chess.Index == index)
                unit = chess;
        }

        mActiveChesses.Remove(unit);
        mIdleChesses.Enqueue(unit);
        unit.Layout.AppendTo(m_IdleChessRoot);
    }
    
    IHintLayout ShowHintAt(CellUnit cell, HintType type)
    {
        // add to functional set
        mFunctionalCells.Add(cell.Index);

        var hint = CreateOrGetHint();
        hint.HintType = type;
        hint.AppendTo(cell.Layout.Transform);

        mActiveHint.Add(hint);

        return hint;
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