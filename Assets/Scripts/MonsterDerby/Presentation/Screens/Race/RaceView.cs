using UnityEngine;
using UnityEngine.UIElements;
using MonsterDerby.Presentation.Navigation;
using MonsterDerby.Presentation.Screens.Race.UI;
using MonsterDerby.Presentation.Screens.Race.World;

namespace MonsterDerby.Presentation.Screens.Race
{
    /// <summary>
    /// Race画面のUIRoot (MonoBehaviour)
    /// Presenterから制御される
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class RaceView : MonoBehaviour, IScreenView
    {
        [Header("World")]
        [SerializeField] private RaceWorldRoot _worldRoot;

        private UIDocument _uiDocument;
        private VisualElement _root;

        // UI各部
        private CourseSelectUI _courseSelectUI;
        private RacingUI _racingUI;
        private ResultUI _resultUI;

        // プロパティ
        public RaceWorldRoot WorldRoot => _worldRoot;
        public CourseSelectUI CourseSelectUI => _courseSelectUI;
        public RacingUI RacingUI => _racingUI;
        public ResultUI ResultUI => _resultUI;

        private void Awake()
        {
            if (_worldRoot == null)
                throw new System.InvalidOperationException("RaceWorldRoot が Inspector に設定されていません。");

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
                throw new System.InvalidOperationException("UIDocument コンポーネントが見つかりません。");
        }

        private void OnEnable()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            _root = _uiDocument.rootVisualElement;

            // UI各部を初期化
            _courseSelectUI = new CourseSelectUI(_root.Q("CourseSelectRoot"));
            _racingUI = new RacingUI(_root.Q("RacingRoot"));
            _resultUI = new ResultUI(_root.Q("ResultRoot"));

            // 初期状態はすべて非表示
            _courseSelectUI.SetActive(false);
            _racingUI.SetActive(false);
            _resultUI.SetActive(false);
        }
    }
}