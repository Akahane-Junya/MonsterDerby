using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MonsterDerby.Domain.SharedKernel;

namespace MonsterDerby.Presentation.Screens.Race.UI
{
    /// <summary>
    /// コース選択フェーズのUI制御
    /// </summary>
    public sealed class CourseSelectUI
    {
        private readonly VisualElement _root;

        public readonly struct CourseOption
        {
            public CourseOption(string raceId, CourseId courseId, string label)
            {
                RaceId = raceId;
                CourseId = courseId;
                Label = label;
            }

            public string RaceId { get; }
            public CourseId CourseId { get; }
            public string Label { get; }
        }

        public event Action<CourseOption> OnCourseSelected;
        public event Action OnBackClicked;

        public CourseSelectUI(VisualElement root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public void SetOptions(IReadOnlyList<CourseOption> options)
        {
            _root.Clear();

            _root.style.flexDirection = FlexDirection.Column;
            _root.style.justifyContent = Justify.FlexStart;
            _root.style.alignItems = Align.Stretch;

            var topRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var backButton = new Button(() => OnBackClicked?.Invoke())
            {
                text = "戻る"
            };
            backButton.style.width = 88;
            backButton.style.height = 28;
            backButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            backButton.style.fontSize = 11;
            backButton.style.color = new StyleColor(new UnityEngine.Color(0.94f, 0.8f, 0.5f, 1f));
            backButton.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.14f, 0.08f, 0.03f, 0.92f));
            backButton.style.borderTopLeftRadius = 14;
            backButton.style.borderTopRightRadius = 14;
            backButton.style.borderBottomLeftRadius = 14;
            backButton.style.borderBottomRightRadius = 14;
            backButton.style.borderLeftWidth = 1;
            backButton.style.borderRightWidth = 1;
            backButton.style.borderTopWidth = 1;
            backButton.style.borderBottomWidth = 1;
            backButton.style.borderLeftColor = new StyleColor(new UnityEngine.Color(0.7f, 0.56f, 0.22f, 1f));
            backButton.style.borderRightColor = new StyleColor(new UnityEngine.Color(0.7f, 0.56f, 0.22f, 1f));
            backButton.style.borderTopColor = new StyleColor(new UnityEngine.Color(0.7f, 0.56f, 0.22f, 1f));
            backButton.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.7f, 0.56f, 0.22f, 1f));

            topRow.Add(backButton);
            _root.Add(topRow);

            var title = new Label("レースを選択")
            {
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new StyleColor(new UnityEngine.Color(1f, 0.87f, 0.42f, 1f)),
                    unityTextOutlineWidth = 1,
                    unityTextOutlineColor = new StyleColor(UnityEngine.Color.black),
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 6
                }
            };
            _root.Add(title);

            var subtitle = new Label("開催レース")
            {
                style =
                {
                    fontSize = 10,
                    color = new StyleColor(new UnityEngine.Color(0.82f, 0.82f, 0.74f, 1f)),
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 6
                }
            };
            _root.Add(subtitle);

            var list = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch
                }
            };
            list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            list.verticalScrollerVisibility = ScrollerVisibility.Auto;
            list.style.paddingRight = 2;
            _root.Add(list);

            if (options == null || options.Count == 0)
            {
                var empty = new Button { text = "レースがありません" };
                empty.SetEnabled(false);
                empty.style.height = 34;
                empty.style.marginBottom = 4;
                empty.style.paddingLeft = 8;
                empty.style.paddingRight = 8;
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.unityFontStyleAndWeight = FontStyle.Bold;
                empty.style.fontSize = 11;
                empty.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.13f, 0.12f, 0.19f, 0.72f));
                empty.style.borderTopLeftRadius = 6;
                empty.style.borderTopRightRadius = 6;
                empty.style.borderBottomLeftRadius = 6;
                empty.style.borderBottomRightRadius = 6;
                empty.style.borderLeftWidth = 2;
                empty.style.borderRightWidth = 2;
                empty.style.borderTopWidth = 2;
                empty.style.borderBottomWidth = 2;
                empty.style.borderLeftColor = new StyleColor(new UnityEngine.Color(0.36f, 0.36f, 0.46f, 0.9f));
                empty.style.borderRightColor = new StyleColor(new UnityEngine.Color(0.36f, 0.36f, 0.46f, 0.9f));
                empty.style.borderTopColor = new StyleColor(new UnityEngine.Color(0.36f, 0.36f, 0.46f, 0.9f));
                empty.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.36f, 0.36f, 0.46f, 0.9f));
                empty.style.color = new StyleColor(new UnityEngine.Color(0.67f, 0.67f, 0.74f, 1f));
                list.Add(empty);
                return;
            }

            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var cardButton = new Button(() => OnCourseSelected?.Invoke(option))
                {
                    text = option.Label
                };

                cardButton.style.height = 34;
                cardButton.style.width = new Length(100, LengthUnit.Percent);
                cardButton.style.marginBottom = 4;
                cardButton.style.paddingLeft = 8;
                cardButton.style.paddingRight = 8;
                cardButton.style.unityTextAlign = TextAnchor.MiddleCenter;
                cardButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                cardButton.style.fontSize = 11;
                cardButton.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.11f, 0.27f, 0.13f, 0.9f));
                cardButton.style.borderTopLeftRadius = 6;
                cardButton.style.borderTopRightRadius = 6;
                cardButton.style.borderBottomLeftRadius = 6;
                cardButton.style.borderBottomRightRadius = 6;
                cardButton.style.borderLeftWidth = 2;
                cardButton.style.borderRightWidth = 2;
                cardButton.style.borderTopWidth = 2;
                cardButton.style.borderBottomWidth = 2;
                cardButton.style.borderLeftColor = new StyleColor(new UnityEngine.Color(0.34f, 0.75f, 0.42f, 1f));
                cardButton.style.borderRightColor = new StyleColor(new UnityEngine.Color(0.34f, 0.75f, 0.42f, 1f));
                cardButton.style.borderTopColor = new StyleColor(new UnityEngine.Color(0.34f, 0.75f, 0.42f, 1f));
                cardButton.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.34f, 0.75f, 0.42f, 1f));
                cardButton.style.color = new StyleColor(new UnityEngine.Color(0.87f, 1f, 0.88f, 1f));

                list.Add(cardButton);
            }
        }

        public void SetActive(bool active)
        {
            _root.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}