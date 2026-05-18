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

- ゲーム内で全モンスターの情報を一覧・詳細表示できる図鑑機能を追加する
- 各モンスターの種族・能力値・スキル・入手方法などを表示
- UI/UX・データ構造・保存形式は要検討
