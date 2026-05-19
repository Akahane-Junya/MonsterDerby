using System;
using UnityEngine;

namespace MonsterDerby.Domain.Catalog
{
    /// <summary>
    /// 図鑑アンロック状態（1体分）
    /// </summary>
    [Serializable]
    public class CatalogUnlockState
    {
        [SerializeField] public string Id;
        [SerializeField] public CatalogUnlockStage Stage;
    }
}
