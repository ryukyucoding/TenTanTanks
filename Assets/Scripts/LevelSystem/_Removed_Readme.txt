這個資料夾原本有一套比較複雜的 LevelManager / WaveManager 關卡系統。
目前專案已改為只使用 `SimpleLevelController + GameManager` 來控制關卡與多波敵人，
所以相關舊系統腳本（LevelManager、WaveManager、各種 Debugger / Setup / AutoConfig 等）已被移除，
避免造成混淆與編譯依賴。

如果未來真的需要重新啟用那套系統，建議從版本控制或備份中復原原始腳本，
而不是依賴這個專案目前的精簡版本。


