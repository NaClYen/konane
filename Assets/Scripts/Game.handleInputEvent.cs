
using UnityEngine;

public partial class Game
{
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
                        var attackerChess = mChessPool.Get(mAttackerSelection);
                        AppendChessToCell(attackerChess, jumpedCell);

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
                {
                    if (isTouchFunctionCell)
                    {
                        mAttackerSelection = id;
                        SwitchGameStatus(GameStatus.WhiteAttackTo); // next
                    }
                }
                break;
            case GameStatus.WhiteAttackTo:
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
                        var attackerChess = mChessPool.Get(mAttackerSelection);
                        AppendChessToCell(attackerChess, jumpedCell);

                        // reset data
                        mAttackerSelection = -1;

                        SwitchGameStatus(GameStatus.BlackAttackFrom); // change
                    }
                    else
                    {
                        SwitchGameStatus(GameStatus.WhiteAttackFrom); // back
                    }
                }
                break;
            case GameStatus.End:
                break;
        }
    }

    void KillBetween(CellUnit startCell, CellUnit endCell, LinkDirection direction)
    {
        var nextCell = startCell.Neighbors[direction];
        KillChess(nextCell.Index);
        var nextNextCell = nextCell.Neighbors[direction];
        if (nextNextCell != endCell)
            KillBetween(nextNextCell, endCell, direction);
    }

    void KillChess(int index)
    {
        var chess = mChessPool.Get(index);
        if (chess == null)
            return;

        mChessPool.MoveToIdle(chess);
        Debug.Log($"KillChess at {index}");
    }
}
