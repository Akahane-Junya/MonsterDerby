# ショップ仕様書

## 概要
- ショップでは「経験値アイテム（小/中/大）」と「スキル習得（ランダム/確定）」のみを販売
- 所持金で購入、購入後は即時でモンスターに反映

## 商品一覧
- 経験値アップ小（+30EXP, 100G）
- 経験値アップ中（+100EXP, 300G）
- 経験値アップ大（+250EXP, 700G）
- スキル習得ランダム（未習得スキルからランダム, 500G）
- スキル習得確定（確定スキル, 2000G）

## 購入フロー
1. 商品ボタン押下（UI上で各商品ごとにボタン）
2. 所持金チェック
3. 経験値/スキルを即時付与（スキルは4つ所持時は忘却UI表示）
4. 所持金減算
5. レベル等を更新

## UI要素
- moneyLabel: 所持金表示
- expSmallButton/expMediumButton/expLargeButton: 経験値アイテム購入
- skillRandomButton/skillGuaranteedButton: スキル習得
- 忘却UI: スキル4つ所持時のみ表示

## データ構造
- 購入処理はWorldStateのmoney・CurrentMonsterのexp/skillsを直接更新

---
