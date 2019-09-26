using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Syy.Tools
{
    public class UnityObjectRecord : EditorWindow
    {
        [MenuItem("Window/UnityObjectRecord")]
        public static void Open()
        {
            GetWindow<UnityObjectRecord>(typeof(UnityObjectRecord).Name);
        }

        List<string> _paths;
        UnityEngine.Object _addObject;
        Vector2 _scrollPos;

        string Key => typeof(UnityObjectRecord).Name + "Key";

        void OnEnable()
        {
            Init();
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        void Init()
        {
            string value = EditorPrefs.GetString(Key, "");
            _paths = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Add to Record");
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _addObject = EditorGUILayout.ObjectField(_addObject, typeof(UnityEngine.Object), true);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_addObject == null))
                    {
                        if (GUILayout.Button("Add"))
                        {
                            string path = "";
                            if (AssetDatabase.IsMainAsset(_addObject))
                            {
                                path = AssetDatabase.GetAssetPath(_addObject);
                            }
                            else
                            {
                                path = GetHierarchyPath(_addObject as GameObject);
                            }

                            if (!string.IsNullOrEmpty(path))
                            {
                                string value = EditorPrefs.GetString(Key, "");
                                value = string.IsNullOrEmpty(value) ? value : value + ",";
                                EditorPrefs.SetString(Key, value + path);
                                Init();
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                _scrollPos = scroll.scrollPosition;

                EditorGUILayout.LabelField("Record List");
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    string deletePath = null;
                    foreach (var path in _paths)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("×", GUILayout.Width(20)))
                            {
                                deletePath = path;
                            }

                            if (path.StartsWith("Assets"))
                            {
                                var obj = AssetDatabase.LoadMainAssetAtPath(path);
                                if (obj == null)
                                {
                                    EditorGUILayout.LabelField("Missing " + path);
                                }
                                else
                                {
                                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                                }
                            }
                            else
                            {
                                var obj = GameObject.Find(path);
                                if (obj == null)
                                {
                                    EditorGUILayout.LabelField("Missing " + path);
                                }
                                else
                                {
                                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                                }
                            }
                        }
                    }

                    if (_paths.Count == 0)
                    {
                        EditorGUILayout.LabelField("Empty");
                    }

                    if (!string.IsNullOrEmpty(deletePath))
                    {
                        _paths.Remove(deletePath);
                        EditorPrefs.SetString(Key, string.Join(",", _paths));
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                if (GUILayout.Button("All Reset"))
                {
                    if (EditorUtility.DisplayDialog("UnityObjectRecord", "Are you sure you want to reset record?", "Reset", "Cancel"))
                    {
                        EditorPrefs.SetString(Key, "");
                        Init();
                    }
                }
            }
        }

        void Update()
        {
            Repaint();
        }

        string GetHierarchyPath(GameObject obj, StringBuilder sb = null)
        {
            sb = sb ?? new StringBuilder();
            sb.Insert(0, "/" + obj.name);
            if (obj.transform.parent == null)
            {
                return sb.ToString();
            }
            return GetHierarchyPath(obj.transform.parent.gameObject, sb);
        }
    }
}
