using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;

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
    RectTransform m_IdleRoot = null;
    

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
                {
                    if (isTouchFunctionCell)
                    {
                        mAttackerSelection = id;
                        SwitchGameStatus(GameStatus.BlackAttackTo); // next
                    }
                }
                break;
            case GameStatus.BlackAttackTo:
                {
                    if (isTouchFunctionCell)
                    {
                        // kill chess(es)
                        Debug.Log($"jump to cell:{id}");

                        var attackStartCell = mCells.Get(mAttackerSelection);
                        var jumpedCell = mCells.Get(id);
                        var direction = (LinkDirection)jumpedCell.Args;
                        KillBetween(attackStartCell, jumpedCell, direction);

                        // move attacker
                        var attackerChess = mChessPool.GetInActive(mAttackerSelection);
                        attackerChess.Layout.AppendTo(jumpedCell.Layout.Transform);

                        // reset data
                        mAttackerSelection = -1;

                        SwitchGameStatus(GameStatus.WhiteAttackFrom); // change
                    }
                    else
                    {
                        SwitchGameStatus(GameStatus.BlackAttackFrom); // back
                    }
                }
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
            LinkCell(cell, LinkDirection.Up,          x + 0, y - 1);
            LinkCell(cell, LinkDirection.UpRight,     x + 1, y - 1);
            LinkCell(cell, LinkDirection.Right,       x + 1, y + 0);
            LinkCell(cell, LinkDirection.BottomRight, x + 1, y + 1);
            LinkCell(cell, LinkDirection.Bottom,      x + 0, y + 1);
            LinkCell(cell, LinkDirection.BottomLeft,  x - 1, y + 1);
            LinkCell(cell, LinkDirection.Left,        x - 1, y + 0);
            LinkCell(cell, LinkDirection.UpLeft,      x - 1, y - 1);

            // debug
            cell.Layout.Info = i.ToString();
        }
    }

    void InitChess()
    {
        mChessPool.Init(m_ChessPrefab, m_IdleRoot);

        for (var i = 0; i < Options.CellCount; i++)
        {
            var chess = mChessPool.GetOrNew();
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

    void SwitchGameStatus(GameStatus s, object args = null)
    {
        if (mCurrentStatus == s)
            throw new System.Exception($"不應該有一樣的遊戲狀態: {s}");

        mCurrentStatus = s;

        switch (mCurrentStatus)
        {
            case GameStatus.None:
                CleanAll();
                break;
            case GameStatus.BlackPickUp:
                {
                    CleanAll();

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
                    CleanAll();

                    // show hint
                    ShowHintAt(mCells.Get(id), HintType.Confirm);
                }
                break;
            case GameStatus.WhitePickUp:
                {
                    CleanAll();

                    {
                        var blackPicked = mCells.Get(mBleckPickedIndex);

                        void ShowHintIfNeighborExist(LinkDirection pos)
                        {
                            if (blackPicked.Neighbors[pos] != null)
                                ShowHintAt(blackPicked.Neighbors[pos], HintType.Confirm);
                        }

                        ShowHintIfNeighborExist(LinkDirection.Bottom);
                        ShowHintIfNeighborExist(LinkDirection.Left);
                        ShowHintIfNeighborExist(LinkDirection.Up);
                        ShowHintIfNeighborExist(LinkDirection.Right);
                    }
                }
                break;
            case GameStatus.WhitePickUpConfirm:
                {
                    var id = (int)args;
                    CleanAll();

                    // show hint
                    ShowHintAt(mCells.Get(id), HintType.Confirm);
                }
                break;
            case GameStatus.BlackAttackFrom:
                {
                    CleanAll();

                    // 標示所有活著的黑棋並且有攻擊機會的格子
                    foreach (var cell in from chessUnit in mChessPool.ActiveChesses
                                         where chessUnit.ChessType == ChessType.Black && HasAttackChance(chessUnit)
                                         select mCells.Get(chessUnit.Index))
                    {
                        ShowHintAt(cell, HintType.Confirm);
                    }
                }
                break;
            case GameStatus.BlackAttackTo:
                {
                    CleanAll();

                    // 標示可進攻的地方
                    var cell = mCells.Get(mAttackerSelection);
                    ShowJumpableCell(cell, LinkDirection.Bottom, cell);
                    ShowJumpableCell(cell, LinkDirection.Up, cell);
                    ShowJumpableCell(cell, LinkDirection.Right, cell);
                    ShowJumpableCell(cell, LinkDirection.Left, cell);
                }
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

    void ShowJumpableCell(CellUnit cell, LinkDirection direction, CellUnit sourCellUnit)
    {
        var jumpableCell = GetJumpableCell(cell, direction);

        if (jumpableCell != null)
        {
            jumpableCell.Args = direction; // 將來源cell & 方向存進 cell 中
            ShowHintAt(jumpableCell, HintType.Confirm); // 傳入來源格
            ShowJumpableCell(jumpableCell, direction, cell); // 遞迴下去
        }
    }

    CellUnit GetJumpableCell(CellUnit cell, LinkDirection direction)
    {
        // 第一步要有格子且有棋子
        var cell_step_1 = cell.Neighbors[direction];
        if (cell_step_1 != null && mChessPool.GetInActive(cell_step_1.Index) != null)
        {
            // 第二步要有格子但不能有棋子
            var cell_step_2 = cell_step_1.Neighbors[direction];
            if (cell_step_2 != null && mChessPool.GetInActive(cell_step_2.Index) == null)
            {
                return cell_step_2;
            }
        }

        // 除此之外都NG
        return null;
    }

    bool HasAttackChance(IChessUnit chess)
    {
        var startCell = mCells.Get(chess.Index);

        if (GetJumpableCell(startCell, LinkDirection.Up) != null)
            return true;
        if (GetJumpableCell(startCell, LinkDirection.Bottom) != null)
            return true;
        if (GetJumpableCell(startCell, LinkDirection.Right) != null)
            return true;
        if (GetJumpableCell(startCell, LinkDirection.Left) != null)
            return true;

        return false;
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

    void KillChess(int index)
    {
        var chess = mChessPool.GetInActive(index);
        if (chess == null)
            return;

        mChessPool.MoveToIdle(chess);
        chess.Layout.AppendTo(m_IdleRoot);
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

    void KillBetween(CellUnit startCell, CellUnit endCell, LinkDirection direction)
    {
        var nextCell = startCell.Neighbors[direction];
        KillChess(nextCell.Index);
        var nextNextCell = nextCell.Neighbors[direction];
        if (nextNextCell != endCell)
            KillBetween(nextNextCell, endCell, direction);
    }

    #region test

    public int Test_CellX = 0;
    public int Test_CellY = 0;

    [ContextMenu("test - Neighbors")]
    void Test_ShowIndexInfo()
    {
        var cell = GetCellByPos(Test_CellX, Test_CellY);

        SetNeighborInfo(cell, LinkDirection.Up, "U");
        SetNeighborInfo(cell, LinkDirection.UpRight, "UR");
        SetNeighborInfo(cell, LinkDirection.Right, "R");
        SetNeighborInfo(cell, LinkDirection.BottomRight, "BR");
        SetNeighborInfo(cell, LinkDirection.Bottom, "B");
        SetNeighborInfo(cell, LinkDirection.BottomLeft, "BL");
        SetNeighborInfo(cell, LinkDirection.Left, "L");
        SetNeighborInfo(cell, LinkDirection.UpLeft, "UL");
    }

    void SetNeighborInfo(CellUnit cell, LinkDirection direction, string info)
    {
        var n = cell.Neighbors[direction];
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

    public void ClearAllArgs()
    {
        foreach (var cell in mCells)
            cell.Args = null;
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

class ChessPool
{
    public Queue<IChessUnit> IdleChesses = new Queue<IChessUnit>();
    public HashSet<IChessUnit> ActiveChesses = new HashSet<IChessUnit>();

    ChessLayout mPrefab;
    Transform mIdleRoot;

    public void Init(ChessLayout prefab, Transform idleRoot)
    {
        mPrefab = prefab;
        mIdleRoot = idleRoot;
    }

    public IChessUnit GetOrNew()
    {
        IChessUnit chessUnit;
        if (IdleChesses.Count > 1)
            chessUnit = IdleChesses.Dequeue();
        else
        {
            chessUnit = new ChessUnit();
            chessUnit.Layout = Object.Instantiate(mPrefab, mIdleRoot);
        }

        // 丟進工作中的池內
        ActiveChesses.Add(chessUnit);

        return chessUnit;
    }

    public IChessUnit GetInActive(int index)
    {
        return ActiveChesses.FirstOrDefault(c => c.Index == index);
    }

    public void MoveToIdle(IChessUnit chess)
    {
        if (!ActiveChesses.Contains(chess))
            return;

        ActiveChesses.Remove(chess);
        IdleChesses.Enqueue(chess);
    }
}

class HintPool
{
    public Queue<HintUnit> IdleHints = new Queue<HintUnit>();
    public HashSet<HintUnit> ActiveHints = new HashSet<HintUnit>();

    HintLayout mPrefab;
    Transform mIdleRoot;

    public void Init(HintLayout prefab, Transform idleRoot)
    {
        mPrefab = prefab;
        mIdleRoot = idleRoot;
    }

    public HintUnit CreateOrGetHint()
    {
        HintUnit hint;
        if(IdleHints.Count > 1)
            hint = IdleHints.Dequeue();
        else
        {
            hint = new HintUnit();
            hint.Layout = Object.Instantiate(mPrefab, mIdleRoot);
        }

        ActiveHints.Add(hint);
        return hint;
    }

    public void Kill(HintUnit hint)
    {
        hint.Layout.AppendTo(mIdleRoot); // 移動至閒置區
        IdleHints.Enqueue(hint); // 放進回收桶
    }
}