# konane
training project


## 真實遊戲畫面
![](https://i.imgur.com/ve917Nk.png)

## intro

核心邏輯放在 [Game.cs](Assets/Scripts/Game.cs),
主要透過控制 `GameStatus` 來控制輸入反應&畫面呈現.  
起始場景為 [SampleScene](Assets/Scenes/SampleScene.unity).

## 操作
- 透過 `GameLogic` 物件上的 `Game` 組件上的 `Board Size` 可以調整棋盤大小  
    ![](https://i.imgur.com/ZLhXO9Z.png)  
    雖然目前介面排版只保證 `6x6` & `8x8`, 但理論上可以無限擴充大小
- `Restart` 可以隨時重開棋局
- 遊戲過程全透過滑鼠點擊操作
- 每個階段都會提示可操作的格子(cell), 在特定場合下, 如果點擊這些以外的格子, 視同取消目前的操作
