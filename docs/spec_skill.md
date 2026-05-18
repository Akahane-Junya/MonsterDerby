# スキル仕様

## 1. データ構造・定義

- SkillDefinitionSO（ScriptableObject）
  - Unityエディタ上でスキルを定義・管理
  - 主なフィールド:
    - skillId（ID）、skillName（表示名）、description（説明）、icon（アイコン）
    - category（SkillCategory: PassiveTerrain/PassiveCondition/ActiveAttack）
    - cooldownSeconds, hitChance01, targetingMode, maximumTargets（攻撃系）
    - terrainTag（地形系）
    - effects（EffectEntry[]: 効果種別・値・持続・対象）
- SkillDefinition（Domain）
  - ゲーム内ロジックで利用
  - サブクラスで用途を分離
    - PassiveTerrainSkillDefinition
    - PassiveConditionSkillDefinition
    - ActiveAttackSkillDefinition
- SkillPresetFactory
  - 一部スキルはプリセットとしてコードで定義
- SkillDatabaseSO
  - SkillDefinitionSOの配列を一括管理
- MasterDataCatalog
  - 全スキルScriptableObjectを一元管理

## 2. 主な仕様

- スキルはID・名前・説明・カテゴリ・効果・アイコン等を持つ
- 効果は複数持てる（例：速度倍率、加算、スタミナ消費など）
- 攻撃系スキルはクールダウン・命中率・対象数・ターゲット方式を持つ
- 地形・条件発動型スキルも存在
- スキルはScriptableObjectで管理し、ゲーム起動時に全件を読み込む

## 3. 保存・管理

- スキル定義はScriptableObject（SkillDefinitionSO）でAssets/MasterData/Skill/配下に保存
- ゲーム内ではSkillIdで一意に管理
- SkillRepository経由で取得・参照

## スキル図鑑 詳細仕様（未実装）

- 画面レイアウト
  - 左：スキル一覧リスト（名前・アイコンのみ、IDは表示しない）
  - 右：選択中スキルの詳細表示
    - 名前、アイコン、説明
    - カテゴリ（攻撃／パッシブなど）
    - 効果内容（テキストでわかりやすく記載）
    - クールダウン・命中率など（攻撃系のみ）
    - フレーバーテキスト
- UI
  - ポケモン図鑑風レイアウト
  - 検索・絞り込み（名前・カテゴリ・効果など）
- 未確認（未取得・未使用）は???で表示
  - 一部情報のみ解禁、詳細は条件達成で解禁
  - 解禁状態はセーブデータに保存

---
