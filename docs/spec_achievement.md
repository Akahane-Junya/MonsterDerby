# 実績・トロフィー仕様書

## 概要
- レース勝利・育成達成・特定条件で実績/トロフィー付与

## パラメータ
- 実績ID、名前、条件、報酬

## 付与フロー
1. 条件達成時に自動付与
2. 実績リスト・トロフィーリストに記録

## データ構造
- AwardsData: trophyList, achievementList
- TrophyData/AchievementData: id, name, condition, reward

---
