/// <summary>
/// 純資料
/// </summary>
public class ChessUnit : IChessUnit
{
    public ChessType ChessType { get; set; }
    public IChessLayout Layout { get; set; }
    public int Index { get; set; }
}
