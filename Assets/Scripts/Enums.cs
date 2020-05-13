public enum LinkDirection
{
    Up = 0,
    UpRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
    UpLeft
}

public enum ChessType
{
    Black,
    White
}

public enum HintType
{
    Select,  // 可以進攻
    Confirm,    // 可到達
}

public enum GameStatus
{
    None,               // 初始狀態
    BlackPickUp,        // 黑方選擇拿掉的目標
    BlackPickUpConfirm, // 黑方確認拿掉的目標
    WhitePickUp,        // 白方選擇拿掉的目標
    WhitePickUpConfirm, // 白方確認拿掉的目標
    BlackAttackFrom,    // 黑方選擇進攻用棋子
    BlackAttackTo,      // 黑方選擇進攻地點
    WhiteAttackFrom,    // 白方選擇進攻用棋子
    WhiteAttackTo,      // 白方選擇進攻地點
    End
}