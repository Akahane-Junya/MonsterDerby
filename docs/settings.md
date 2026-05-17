# 設定項目・仕様（現状実装ベース）

## 設定項目一覧
- BGM音量: 0-100, サウンド全体に即時反映
- SE音量: 0-100, 効果音全体に即時反映
- フルスクリーン: bool, 画面表示切替
- 解像度: string, 例: "1920x1080", "1280x720"
- ウィンドウモード: string, "フルスクリーン"/"ウィンドウ"/"ボーダレス"
- 言語: string, "日本語"/"English"
- キーコンフィグ: 各操作に対するキー割当
- データリセット: 全データ初期化

## 実装・反映範囲
- SettingsViewで全項目を編集・即時反映
- SettingsService経由でWorldState.settingsを更新
- サウンド・画面・言語は即時反映

## データ構造
- SettingsData: bgmVolume, seVolume, fullscreen, resolution, windowMode, language, keyConfig

---
