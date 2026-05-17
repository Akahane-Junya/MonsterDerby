# MonsterDerby

本プロジェクトはUnity製のモンスター育成・レースゲームです。

## 概要
- サウンド・設定・セーブデータは全て独自実装
- UIはUI Toolkitで統一
- DI/サービスパターン採用
- JSONによるセーブ/ロード

## 開発環境
- Unity 2022.x 以降
- .NET Standard 2.1

## ディレクトリ構成
- `Assets/` ... ゲーム本体・アセット
- `docs/` ... ドキュメント
- `ProjectSettings/` ... Unityプロジェクト設定

## ビルド・実行方法
1. Unity Hubでプロジェクトを開く
2. `Assets/Scenes/` から任意のシーンを選択し再生

## 主要ドキュメント
- docs/architecture.md ... アーキテクチャ設計
- docs/ui-guideline.md ... UI設計指針
- docs/save-data.md ... セーブデータ仕様
- docs/settings.md ... 設定項目仕様
- docs/sound-system.md ... サウンドシステム
- docs/contributing.md ... コントリビュート手順
