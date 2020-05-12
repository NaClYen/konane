using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 純資料
/// </summary>
public class ChessUnit : IChessUnit
{
    public ChessType ChessType { get; set; }
    public IChessLayout Layout { get; set; }
}
