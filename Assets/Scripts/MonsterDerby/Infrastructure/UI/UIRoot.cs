using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Infrastructure.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        public VisualElement Root => uiDocument.rootVisualElement;

        private void Awake()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();

        }
    }
}