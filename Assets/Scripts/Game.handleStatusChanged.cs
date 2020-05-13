using System;
using System.Linq;

public partial class Game
{
    void SwitchGameStatus(GameStatus s, object args = null)
    {
        if (mCurrentStatus == s)
            throw new Exception($"不應該有一樣的遊戲狀態: {s}");

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

                    // 標示所有活著的黑子並且有攻擊機會的格子
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
                    ShowJumpableCell(cell, LinkDirection.Bottom);
                    ShowJumpableCell(cell, LinkDirection.Up);
                    ShowJumpableCell(cell, LinkDirection.Right);
                    ShowJumpableCell(cell, LinkDirection.Left);
                }
                break;
            case GameStatus.WhiteAttackFrom:
                {
                    CleanAll();

                    // 標示所有活著的白子並且有攻擊機會的格子
                    foreach (var cell in from chessUnit in mChessPool.ActiveChesses
                                         where chessUnit.ChessType == ChessType.White && HasAttackChance(chessUnit)
                                         select mCells.Get(chessUnit.Index))
                    {
                        ShowHintAt(cell, HintType.Confirm);
                    }
                }
                break;
            case GameStatus.WhiteAttackTo:
                {
                    CleanAll();

                    // 標示可進攻的地方
                    var cell = mCells.Get(mAttackerSelection);
                    ShowJumpableCell(cell, LinkDirection.Bottom);
                    ShowJumpableCell(cell, LinkDirection.Up);
                    ShowJumpableCell(cell, LinkDirection.Right);
                    ShowJumpableCell(cell, LinkDirection.Left);
                }
                break;
            case GameStatus.End:
                break;
            default:
                throw new Exception($"沒有處理的 GameStatus:{mCurrentStatus}");
        }
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

    void ShowJumpableCell(CellUnit cell, LinkDirection direction)
    {
        var jumpableCell = GetJumpableCell(cell, direction);

        if (jumpableCell != null)
        {
            jumpableCell.Args = direction; // 將來源cell & 方向存進 cell 中
            ShowHintAt(jumpableCell, HintType.Confirm); // 傳入來源格
            ShowJumpableCell(jumpableCell, direction); // 遞迴下去
        }
    }
}