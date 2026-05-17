using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonsterDerby.Presentation.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class GlobalButtonFeedback : MonoBehaviour
    {
        private readonly Dictionary<Button, ButtonState> _states = new Dictionary<Button, ButtonState>();

        private UIDocument _uiDocument;
        private VisualElement _root;
        private IVisualElementScheduledItem _refreshSchedule;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                return;
            }

            _root = _uiDocument.rootVisualElement;
            if (_root == null)
            {
                return;
            }

            RefreshBindings();
            _refreshSchedule = _root.schedule.Execute(RefreshBindings).Every(300);
        }

        private void OnDisable()
        {
            if (_refreshSchedule != null)
            {
                _refreshSchedule.Pause();
                _refreshSchedule = null;
            }

            foreach (var pair in _states)
            {
                Unregister(pair.Key);
            }

            _states.Clear();
            _root = null;
        }

        private void RefreshBindings()
        {
            if (_root == null)
            {
                return;
            }

            var buttons = _root.Query<Button>().ToList();

            for (var i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (button == null || _states.ContainsKey(button))
                {
                    continue;
                }

                Register(button);
            }

            var removed = ListPool<Button>.Get();
            foreach (var pair in _states)
            {
                if (pair.Key.panel == null)
                {
                    removed.Add(pair.Key);
                }
            }

            for (var i = 0; i < removed.Count; i++)
            {
                var dead = removed[i];
                Unregister(dead);
                _states.Remove(dead);
            }

            ListPool<Button>.Release(removed);
        }

        private void Register(Button button)
        {
            var state = new ButtonState(button);
            _states.Add(button, state);

            button.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            button.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            button.RegisterCallback<PointerDownEvent>(OnPointerDown);
            button.RegisterCallback<PointerUpEvent>(OnPointerUp);
            button.RegisterCallback<FocusInEvent>(OnFocusIn);
            button.RegisterCallback<FocusOutEvent>(OnFocusOut);

            Apply(button);
        }

        private void Unregister(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            button.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            button.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            button.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            button.UnregisterCallback<FocusInEvent>(OnFocusIn);
            button.UnregisterCallback<FocusOutEvent>(OnFocusOut);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            var button = evt.currentTarget as Button;
            if (button == null || !_states.TryGetValue(button, out var state))
            {
                return;
            }

            state.Hovered = true;
            Apply(button);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            var button = evt.currentTarget as Button;
            if (button == null || !_states.TryGetValue(button, out var state))
            {
                return;
            }

            state.Hovered = false;
            state.Pressed = false;
            Apply(button);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            var button = evt.currentTarget as Button;
            if (button == null || !_states.TryGetValue(button, out var state))
            {
                return;
            }

            state.Pressed = true;
            Apply(button);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            var button = evt.currentTarget as Button;
            if (button == null || !_states.TryGetValue(button, out var state))
            {
                return;
            }

            state.Pressed = false;
            Apply(button);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            var button = evt.currentTarget as Button;
            if (button == null || !_states.TryGetValue(button, out var state))
            {
                return;
            }

            state.Focused = true;
            Apply(button);
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            var button = evt.currentTarget as Button;
            if (button == null || !_states.TryGetValue(button, out var state))
            {
                return;
            }

            state.Focused = false;
            Apply(button);
        }

        private void Apply(Button button)
        {
            if (!_states.TryGetValue(button, out var state))
            {
                return;
            }

            button.style.borderLeftWidth = 0f;
            button.style.borderRightWidth = 0f;
            button.style.borderTopWidth = 0f;
            button.style.borderBottomWidth = 0f;
            button.style.borderLeftColor = new StyleColor(Color.clear);
            button.style.borderRightColor = new StyleColor(Color.clear);
            button.style.borderTopColor = new StyleColor(Color.clear);
            button.style.borderBottomColor = new StyleColor(Color.clear);

            if (!button.enabledInHierarchy)
            {
                button.style.opacity = 0.55f;
                button.style.scale = new StyleScale(new Scale(new Vector3(1f, 1f, 1f)));
                if (state.HasBaseBackground)
                {
                    button.style.backgroundColor = new StyleColor(Tint(state.BaseBackgroundColor, -0.08f));
                }
                return;
            }

            button.style.opacity = 1f;

            var targetScale = 1f;
            if (state.Hovered)
            {
                targetScale = 1.03f;
            }

            if (state.Pressed)
            {
                targetScale = 0.97f;
            }

            button.style.scale = new StyleScale(new Scale(new Vector3(targetScale, targetScale, 1f)));

            if (state.HasBaseBackground)
            {
                var bg = state.BaseBackgroundColor;
                if (state.Hovered)
                {
                    bg = Tint(bg, 0.12f);
                }

                if (state.Pressed)
                {
                    bg = Tint(bg, -0.1f);
                }

                button.style.backgroundColor = new StyleColor(bg);
            }

        }

        private static Color Tint(Color color, float amount)
        {
            var r = Mathf.Clamp01(color.r + amount);
            var g = Mathf.Clamp01(color.g + amount);
            var b = Mathf.Clamp01(color.b + amount);
            return new Color(r, g, b, color.a);
        }

        private sealed class ButtonState
        {
            public ButtonState(Button button)
            {
                var resolved = button.resolvedStyle;
                BaseBackgroundColor = resolved.backgroundColor;
                HasBaseBackground = BaseBackgroundColor.a > 0.001f;

                BaseBorderLeftColor = resolved.borderLeftColor;
                BaseBorderRightColor = resolved.borderRightColor;
                BaseBorderTopColor = resolved.borderTopColor;
                BaseBorderBottomColor = resolved.borderBottomColor;

                BaseBorderLeftWidth = resolved.borderLeftWidth;
                BaseBorderRightWidth = resolved.borderRightWidth;
                BaseBorderTopWidth = resolved.borderTopWidth;
                BaseBorderBottomWidth = resolved.borderBottomWidth;
            }

            public bool Hovered;
            public bool Pressed;
            public bool Focused;
            public readonly bool HasBaseBackground;

            public readonly Color BaseBackgroundColor;
            public readonly Color BaseBorderLeftColor;
            public readonly Color BaseBorderRightColor;
            public readonly Color BaseBorderTopColor;
            public readonly Color BaseBorderBottomColor;
            public readonly float BaseBorderLeftWidth;
            public readonly float BaseBorderRightWidth;
            public readonly float BaseBorderTopWidth;
            public readonly float BaseBorderBottomWidth;
        }

        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

            public static List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new List<T>();
            }

            public static void Release(List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}