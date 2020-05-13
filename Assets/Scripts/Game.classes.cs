using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

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

    public IChessUnit New()
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

    public IChessUnit Get(int index)
    {
        return ActiveChesses.FirstOrDefault(c => c.Index == index);
    }

    public void MoveToIdle(IChessUnit chess)
    {
        if (!ActiveChesses.Contains(chess))
            return;

        chess.Layout.AppendTo(mIdleRoot);

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

        ActiveHints.Remove(hint); // 移除活耀區
        IdleHints.Enqueue(hint); // 放進回收桶
    }
}