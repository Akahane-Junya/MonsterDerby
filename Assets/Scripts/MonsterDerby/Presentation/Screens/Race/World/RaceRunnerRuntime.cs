using System;
using MonsterDerby.Domain.SharedKernel;
using UnityEngine;

namespace MonsterDerby.Presentation.Screens.Race.World
{
    /// <summary>
    /// ランナー1体分の表示責務を保持するランタイムオブジェクト。
    /// </summary>
    public sealed class RaceRunnerRuntime
    {
        public RaceRunnerRuntime(MonsterId monsterId, GameObject gameObject)
        {
            MonsterId = monsterId;
            GameObject = gameObject ?? throw new ArgumentNullException(nameof(gameObject));
            Transform = gameObject.transform;
        }

        public MonsterId MonsterId { get; }
        public GameObject GameObject { get; }
        public Transform Transform { get; }

        public void SetPosition(float xPx, float yPx)
        {
            Transform.position = new Vector3(xPx, yPx, 0f);
        }

        public void Destroy()
        {
            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
            }
        }
    }
}