# モンスター育成仕様書

## 概要
- モンスターごとに能力値・スキル・成長度・状態を保持
- トレーニングや経験値で成長
- 交配で新モンスター生成

## パラメータ
- 種族ID（speciesId）
- ニックネーム
- 経験値
- 成長インクリメント（topSpeed, accel, stamina の配列）
- スキルリスト（skillId, unlockLevel）
- 親情報（parentMonsterIds）

## 育成フロー
1. トレーニング選択→成長インクリメント増加
2. 経験値獲得→レベルUP
3. スキル習得（ショップ/イベント）
4. 交配→新モンスター生成（親情報記録）

## データ構造
- MonsterSaveData: monsterId, speciesId, nickname, experience, growthIncrements（topSpeed, accel, stamina[]）, monsterSkills（skillId, unlockLevel[]）, parentMonsterIds
## アセット要件
- 種族定義: SpeciesDefinitionSO（ScriptableObject, Assets/MasterData/Species/）
- モンスター見た目: MonsterVisualDefinitionSO（SpriteLibrary, MotionSet, Assets/Art/Characters/）
- スキル定義: SkillDefinitionSO
- 保存形式: JSON（MonsterSaveData, Application.persistentDataPath/save.json）

---

# モンスター図鑑仕様（未実装）

- ゲーム内で全モンスターの情報を一覧・詳細表示できる図鑑機能
- 各モンスターの種族・能力値・スキルを表示
- UI/UX・データ構造・保存形式は要検討

## モンスター図鑑 詳細仕様（未実装）

- 画面レイアウト
  - 左：モンスター一覧リスト（名前・アイコンのみ、IDは表示しない）
  - 右：選択中モンスターの詳細表示
    - 名前、アイコン、説明
    - 能力値：各能力（例：スピード、スタミナ、加速など）を5段階評価で表示（数値は非表示）
    - 成長タイプ：早熟／晩成／普通／気まぐれ など
    - 習得可能スキル一覧（スキル名のみ、詳細はスキル図鑑参照）
    - 入手方法やフレーバーテキスト
- ステータス表示
  - 5段階評価（★やバーなどで視覚的に表現）
  - レベルマックス時の数値は表示しない
- UI
  - ポケモン図鑑風レイアウト
  - 検索・絞り込み（名前・成長タイプなど）
- 未確認（未戦闘・未育成）は???で表示
  - 戦闘のみの場合は解禁される情報が少ない
  - 解禁状態はセーブデータに保存

---
