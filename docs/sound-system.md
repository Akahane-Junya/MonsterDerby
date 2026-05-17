# サウンドシステム仕様（現状実装ベース）

## サウンド種別
- BGM: 画面・進行状況ごとに切替、ループ再生
- SE: ボタン・レース・購入・スキル習得等の効果音

## 管理・再生
- SoundDatabase（ScriptableObject）でBGM/SEアセットを一元管理
- SoundServiceで再生・停止・音量制御
- SettingsServiceと連携し、音量・ミュートを即時反映
- UI（SettingsView）から音量調整

## 実装方針
- 依存性注入（DI）でサービスを管理
- シングルトン/ServiceLocatorは不使用
- サウンド再生はイベント駆動

## アセット要件
- SoundDatabase: Assets/MasterData/Sound/
- BGM: Assets/Art/Sound/BGM/
- SE: Assets/Art/Sound/SE/

---
