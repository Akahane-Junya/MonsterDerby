# セーブデータ・データ仕様

## WorldState構造
- money: 所持金
- remainingDays: 残り日数
- currentMonster: MonsterSaveData
- awards: RaceAwardSaveData[]
- trophyOwnerships: TrophyOwnershipSaveData[]
- courseRecords: CourseRecordSaveData[]
- settings: SettingsData
- version: データバージョン

## MonsterSaveData
- monsterId, speciesId, nickname, experience
- growthIncrements（topSpeed, accel, stamina[]）
- monsterSkills（skillId, unlockLevel[]）
- parentMonsterIds

## 保存形式
- JsonUtilityによるJSON保存（Application.persistentDataPath/save.json）
- バージョン管理あり（SaveData.version）

---
