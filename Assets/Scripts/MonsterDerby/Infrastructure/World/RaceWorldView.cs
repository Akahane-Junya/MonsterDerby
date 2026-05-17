using UnityEngine;

namespace MonsterDerby.Infrastructure.World
{
    /// <summary>
    /// RaceWorldRoot の内部表示（選択/観戦/結果）を切り替える。
    /// ScreenIdはRace固定のまま、サブ状態で子GOをON/OFFする。
    /// </summary>
    public sealed class RaceWorldView : MonoBehaviour
    {
        [Header("Children under RaceWorldRoot")]
        [SerializeField] private GameObject selectRoot;
        [SerializeField] private GameObject viewRoot;
        [SerializeField] private GameObject resultRoot;

        private void Awake()
        {
            // 起動時は選択表示にする（好みで変えてOK）
            ShowSelect();
        }

        public void ShowSelect() => Set(select: true, view: false, result: false);
        public void ShowView()   => Set(select: false, view: true, result: false);
        public void ShowResult() => Set(select: false, view: false, result: true);

        private void Set(bool select, bool view, bool result)
        {
            if (selectRoot != null) selectRoot.SetActive(select);
            if (viewRoot != null)   viewRoot.SetActive(view);
            if (resultRoot != null) resultRoot.SetActive(result);
        }
    }
}
