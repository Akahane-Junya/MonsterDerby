# レース仕様書

## 概要
- 複数モンスターが参加し、順位を競う
- 能力値＋ランダム要素で順位決定

## レースパラメータ
- 距離、天候、参加費、報酬
- 参加モンスター数上限
- レベル制限（参加可能な最低/最高レベル）

## 進行フロー
1. レース選択
2. 参加モンスター選択（レベル制限を満たすモンスターのみ選択可）
3. 参加費支払い
4. レース実行（順位決定）
5. 報酬・経験値・実績付与
6. レース履歴記録

## データ構造
- RaceHistoryData: raceId, date, entryMonsterId, rank, reward, weather, distance
- RaceDefinition: raceId, name, distance, weather, entryFee, reward, minLevel, maxLevel, maxParticipants

---
