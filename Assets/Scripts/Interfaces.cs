using UnityEngine;

public interface IChessUnit
{
    ChessType ChessType { get; set; }
    IChessLayout Layout { get; set; }
    int Index { get; set; }
}

public interface IChessLayout
{
    ChessType ChessType { get; set; }

    void AppendTo(Transform t);
}

public interface IHintUnit
{
    IHintLayout Layout { get; set; }
}
public interface IHintLayout
{
    HintType HintType { get; set; }
    void AppendTo(Transform t);
}


public interface ICellLayout
{
    string Info { get; set; }
    ICellLayout Init(IInfoCenter ic, int id);
    Transform Transform { get; }
}

public interface IInfoCenter
{
    void InvokeEvent(string msg, object args = null);
}