using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MonsterDerby.Presentation.Animation.Race;
using UnityEditor;
using UnityEngine;

namespace MonsterDerby.Presentation.Animation.Race.Editor
{
    public static class SpriteMotionSetGenerator
    {
        [MenuItem("Assets/MonsterDerby/Animation/Create Sprite Motion Set From Sprite Library", true)]
        private static bool ValidateCreateFromSpriteLibrary()
        {
            // 実行時に詳細チェックするため、メニューは常に表示する。
            return true;
        }

        [MenuItem("Assets/MonsterDerby/Animation/Create Sprite Motion Set From Sprite Library")]
        private static void CreateFromSpriteLibrary()
        {
            if (!TryGetSpriteLibraryAssetType(out var spriteLibraryAssetType))
            {
                EditorUtility.DisplayDialog(
                    "SpriteMotionSet Generator",
                    "SpriteLibraryAsset 型が見つかりません。2D Animation パッケージを導入してください。",
                    "OK");
                return;
            }

            var source = Selection.activeObject;
            if (source == null || !spriteLibraryAssetType.IsInstanceOfType(source))
            {
                EditorUtility.DisplayDialog(
                    "SpriteMotionSet Generator",
                    "Sprite Library Asset を1つ選択してください。",
                    "OK");
                return;
            }

            if (!TryGetCategoryNames(source, out var categoryNames, out var error))
            {
                EditorUtility.DisplayDialog("SpriteMotionSet Generator", error, "OK");
                return;
            }

            var motionSet = ScriptableObject.CreateInstance<SpriteMotionSet>();
            string sourcePath = AssetDatabase.GetAssetPath(source);
            string folderPath = Path.GetDirectoryName(sourcePath) ?? "Assets";
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folderPath, source.name + "_SpriteMotionSet.asset").Replace("\\", "/"));

            AssetDatabase.CreateAsset(motionSet, assetPath);

            var so = new SerializedObject(motionSet);

            ApplyClip(so, source, categoryNames, "_idleClip", FindCategory(categoryNames, "idle"), 12f, true);
            ApplyClip(so, source, categoryNames, "_readyClip", FindCategory(categoryNames, "ready", "idle"), 12f, true);
            ApplyClip(so, source, categoryNames, "_runClip", FindCategory(categoryNames, "run"), 12f, true);
            ApplyClip(so, source, categoryNames, "_attackClip", FindCategory(categoryNames, "attack", "atk"), 10f, false);
            ApplyClip(so, source, categoryNames, "_deathClip", FindCategory(categoryNames, "death", "dead"), 8f, false);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(motionSet);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = motionSet;
            EditorGUIUtility.PingObject(motionSet);
        }

        private static void ApplyClip(
            SerializedObject so,
            UnityEngine.Object source,
            IReadOnlyList<string> categoryNames,
            string clipPropertyName,
            string category,
            float fps,
            bool loop)
        {
            var clipProperty = so.FindProperty(clipPropertyName);
            if (clipProperty == null)
            {
                return;
            }

            var categoryProp = clipProperty.FindPropertyRelative("_category");
            var labelsProp = clipProperty.FindPropertyRelative("_labels");
            var fpsProp = clipProperty.FindPropertyRelative("_fps");
            var loopProp = clipProperty.FindPropertyRelative("_loop");

            if (string.IsNullOrEmpty(category) || !categoryNames.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                categoryProp.stringValue = string.Empty;
                labelsProp.arraySize = 0;
                fpsProp.floatValue = fps;
                loopProp.boolValue = loop;
                return;
            }

            if (!TryGetCategoryLabels(source, category, out var labels, out _))
            {
                labels = new List<string>();
            }

            categoryProp.stringValue = category;
            labelsProp.arraySize = labels.Count;
            for (int i = 0; i < labels.Count; i++)
            {
                labelsProp.GetArrayElementAtIndex(i).stringValue = labels[i];
            }
            fpsProp.floatValue = fps;
            loopProp.boolValue = loop;
        }

        private static string FindCategory(IReadOnlyList<string> categories, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                var exact = categories.FirstOrDefault(c => string.Equals(c, candidate, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(exact))
                {
                    return exact;
                }

                var contains = categories.FirstOrDefault(c => c.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrEmpty(contains))
                {
                    return contains;
                }
            }

            return string.Empty;
        }

        private static bool TryGetCategoryNames(UnityEngine.Object source, out List<string> categories, out string error)
        {
            categories = new List<string>();
            error = string.Empty;

            var method = source.GetType().GetMethod("GetCategoryNames", BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                error = "SpriteLibraryAsset.GetCategoryNames() が見つかりません。";
                return false;
            }

            var result = method.Invoke(source, null);
            categories = ToStringList(source, result);
            if (categories.Count == 0)
            {
                error = "カテゴリが見つかりません。Sprite Library Asset の内容を確認してください。";
                return false;
            }

            return true;
        }

        private static bool TryGetCategoryLabels(UnityEngine.Object source, string category, out List<string> labels, out string error)
        {
            labels = new List<string>();
            error = string.Empty;

            var method = source.GetType().GetMethod(
                "GetCategoryLabelNames",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(string) },
                modifiers: null);

            if (method == null)
            {
                // フォールバック: 古いAPI差分向け
                method = source.GetType().GetMethod("GetCategoryLabelNames", BindingFlags.Public | BindingFlags.Instance);
            }

            if (method == null)
            {
                error = "SpriteLibraryAsset.GetCategoryLabelNames(string) が見つかりません。";
                return false;
            }

            var result = method.Invoke(source, new object[] { category });
            labels = ToStringList(source, result);
            return true;
        }

        private static List<string> ToStringList(UnityEngine.Object source, object value)
        {
            var result = new List<string>();
            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var text = ConvertItemToString(source, item);
                    if (!string.IsNullOrEmpty(text))
                    {
                        result.Add(text);
                    }
                }
            }

            return result;
        }

        private static string ConvertItemToString(UnityEngine.Object source, object item)
        {
            if (item is string s)
            {
                return s;
            }

            if (item is int hash && TryResolveHashToName(source, hash, out var resolved))
            {
                return resolved;
            }

            if (item is int)
            {
                // ハッシュ解決できない数値はラベルとして不正なので捨てる。
                return string.Empty;
            }

            var itemType = item.GetType();
            var stringPropertyNames = new[] { "name", "Name", "label", "Label" };
            for (int i = 0; i < stringPropertyNames.Length; i++)
            {
                var prop = itemType.GetProperty(stringPropertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || prop.PropertyType != typeof(string))
                {
                    continue;
                }

                var value = prop.GetValue(item) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            var fallback = item.ToString();
            if (IsOnlyDigits(fallback))
            {
                return string.Empty;
            }

            return fallback;
        }

        private static bool TryResolveHashToName(UnityEngine.Object source, int hash, out string name)
        {
            name = null;
            var type = source.GetType();

            var preferredNames = new[]
            {
                "GetStringFromHash",
                "GetCategoryNameFromHash",
                "GetLabelNameFromHash",
                "GetNameFromHash",
            };

            for (int i = 0; i < preferredNames.Length; i++)
            {
                var m = type.GetMethod(
                    preferredNames[i],
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(int) },
                    modifiers: null);

                if (m == null || m.ReturnType != typeof(string))
                {
                    continue;
                }

                var target = m.IsStatic ? null : source;
                var value = m.Invoke(target, new object[] { hash }) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    name = value;
                    return true;
                }
            }

            // 2D Animation の SpriteLibrary static API を使える場合はそちらでも解決を試みる。
            var spriteLibraryType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType("UnityEngine.U2D.Animation.SpriteLibrary", throwOnError: false))
                .FirstOrDefault(t => t != null);

            if (spriteLibraryType != null)
            {
                var staticMethod = spriteLibraryType.GetMethod(
                    "GetStringFromHash",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(int) },
                    modifiers: null);

                if (staticMethod != null && staticMethod.ReturnType == typeof(string))
                {
                    var value = staticMethod.Invoke(null, new object[] { hash }) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        name = value;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsOnlyDigits(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetSpriteLibraryAssetType(out Type spriteLibraryAssetType)
        {
            spriteLibraryAssetType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType("UnityEngine.U2D.Animation.SpriteLibraryAsset", throwOnError: false))
                .FirstOrDefault(t => t != null);

            return spriteLibraryAssetType != null;
        }
    }
}
