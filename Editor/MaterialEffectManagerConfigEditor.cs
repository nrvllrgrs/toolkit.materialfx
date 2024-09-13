using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ToolkitEngine.MaterialFX;

namespace ToolkitEditor.MaterialFX
{
    [CustomEditor(typeof(MaterialEffectManagerConfig))]
    public class ReactionTypeEditor : Editor
    {
        #region Fields

        protected MaterialEffectManagerConfig m_reactionType;

        protected SerializedProperty m_materialEffects;
        private ReorderableList m_materialEffectList;

        protected SerializedProperty m_reactions;

        private GUIStyle m_matrixTextStyle;
        private GUIStyle m_matrixCellStyle;

        private IEnumerable<MaterialEffectType> m_cachedMaterialEffects = null;
        private Texture2D m_unselectedIcon, m_selectedIcon, m_validUnselectedIcon, m_validSelectedIcon;

        private Vector2Int m_selected;
        private SerializedProperty m_selectedReaction;

        #endregion

        #region Properties

        public Texture2D UnselectedIcon => GetIcon(ref m_unselectedIcon, "079705ecaf509cf4eaba6506d4a1157c");
        public Texture2D SelectedIcon => GetIcon(ref m_selectedIcon, "75718c73a8f0d1e4f986220a6a8afd44");
        public Texture2D ValidUnselectedIcon => GetIcon(ref m_validUnselectedIcon, "e0f8f0d7bf016d34cb7ddb785c40aff0");
        public Texture2D ValidSelectedIcon => GetIcon(ref m_validSelectedIcon, "919bbaf1586df3f4f8202c2fa241e0c1");

        #endregion

        #region Methods

        private void OnEnable()
        {
            if (target == null)
                return;

            m_cachedMaterialEffects = AssetDatabase.FindAssets("t:MaterialEffectType")
                .Select(x => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(x), typeof(MaterialEffectType)))
                .Cast<MaterialEffectType>();

            m_reactionType = (MaterialEffectManagerConfig)target;
            m_materialEffects = serializedObject.FindProperty(nameof(m_materialEffects));

            m_reactions = serializedObject.FindProperty(nameof(m_reactions));
        }

        private void OnDisable()
        {
            Save();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_materialEffectList == null)
            {
                m_materialEffectList = new ReorderableList(m_reactionType.fxMaterials.ToArray(), typeof(MaterialEffectType), false, true, true, true);
                m_materialEffectList.drawHeaderCallback += (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Material Effects");
                };
                m_materialEffectList.drawElementCallback += OnDrawElementCallback;
                m_materialEffectList.onCanAddCallback += OnCanAddCallback;
                m_materialEffectList.onAddDropdownCallback += OnAddDropdownCallback;
                //m_substancesList.onReorderCallback += OnReorderCallback;
                m_materialEffectList.onCanRemoveCallback += OnCanRemoveCallback;
                m_materialEffectList.onRemoveCallback += OnRemoveCallback;
            }

            // Draw substance list
            m_materialEffectList.DoLayoutList();

            if (m_reactionType.fxMaterials.Any())
            {
                EditorGUILayout.LabelField("Reactions Matrix", EditorStyles.boldLabel);

                var validSubstances = m_reactionType.fxMaterials.Where(x => x != null).ToArray();
                GUILayout.Space(EditorGUIUtility.labelWidth + EditorGUIUtility.singleLineHeight * validSubstances.Length);

                var lastRect = GUILayoutUtility.GetLastRect();

                if (m_matrixTextStyle == null)
                {
                    m_matrixTextStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleRight,
                        fixedHeight = EditorGUIUtility.singleLineHeight,
                    };
                }

                if (m_matrixCellStyle == null)
                {
                    m_matrixCellStyle = new GUIStyle(GUIStyle.none);
                }

                MaterialEffectManagerConfig.Reaction reaction;

                int i = 0;
                foreach (var substanceType in m_reactionType.fxMaterials)
                {
                    if (substanceType == null)
                        continue;

                    // Draw column header
                    var x = lastRect.x + EditorGUIUtility.labelWidth + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (validSubstances.Length - i);
                    var pivot = new Vector2(x, lastRect.y);

                    GUIUtility.RotateAroundPivot(90f, pivot);
                    EditorGUI.LabelField(new Rect(x, lastRect.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                        substanceType.name,
                        m_matrixTextStyle);
                    GUIUtility.RotateAroundPivot(-90f, pivot);

                    // Draw row header
                    var y = lastRect.y + EditorGUIUtility.labelWidth + EditorGUIUtility.singleLineHeight * i;
                    EditorGUI.LabelField(
                        new Rect(lastRect.x, y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                        substanceType.name,
                        m_matrixTextStyle);

                    // Draw row buttons
                    int lastIndex = validSubstances.Length - i - 1;
                    for (int j = 0; j < validSubstances.Length - i; ++j)
                    {
                        var jIndex = validSubstances.Length - (j + 1);
                        if (!m_reactionType.TryGetReaction(validSubstances[i], validSubstances[jIndex], out reaction))
                            continue;

                        x = lastRect.x + EditorGUIUtility.labelWidth + EditorGUIUtility.singleLineHeight * j + EditorGUIUtility.standardVerticalSpacing * (j + 1);
                        var cellRect = new Rect(x, y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);

                        Texture2D icon;
                        
                        // If element is selected...
                        if (m_selected.x == i && m_selected.y == jIndex)
                        {
                            icon = reaction.isDefined
                                ? ValidSelectedIcon
                                : SelectedIcon;
                        }
                        else
                        {
                            icon = reaction.isDefined
                                ? ValidUnselectedIcon
                                : UnselectedIcon;
                        }
                        
                        if (GUI.Button(cellRect, new GUIContent(icon), m_matrixCellStyle))
                        {
                            m_selected.x = i;
                            m_selected.y = jIndex;
                            m_selectedReaction = null;
                        }
                    }

                    ++i;
                }

                EditorGUILayout.Separator();

                if (!m_selected.x.Between(0, m_reactionType.fxMaterials.Count - 1) || !m_selected.y.Between(0, m_reactionType.fxMaterials.Count - 1))
                {
                    m_selected = Vector2Int.zero;
                }

                var pair = new MaterialEffectManagerConfig.FXMaterialPair(m_reactionType.fxMaterials[m_selected.x], m_reactionType.fxMaterials[m_selected.y]);
                if (m_reactionType.TryGetReaction(pair, out reaction))
                {
                    EditorGUILayout.LabelField(string.Format("Reaction - {0} / {1}", m_reactionType.fxMaterials[m_selected.x].name, m_reactionType.fxMaterials[m_selected.y].name), EditorStyles.boldLabel);

                    if (m_selectedReaction == null)
                    {
                        int index = (m_selected.y * (m_selected.y + 1) / 2) + m_selected.x;
                        m_selectedReaction = m_reactions.FindPropertyRelative("values")
                            .GetArrayElementAtIndex(index);
                    }

                    if (m_selectedReaction != null)
                    {
						EditorGUILayout.PropertyField(m_selectedReaction.FindPropertyRelative("m_minImpulseThreshold"));
						EditorGUILayout.PropertyField(m_selectedReaction.FindPropertyRelative("m_spawner"));
                        //EditorGUILayout.PropertyField(m_selectedReaction.FindPropertyRelative("m_capacity"));
                        //EditorGUILayout.PropertyField(m_selectedReaction.FindPropertyRelative("m_lifetime"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Helper Methods

        private IEnumerable<Object> GetAvailableSubstances()
        {
            return m_cachedMaterialEffects.Except(m_reactionType.fxMaterials);
        }

        private Texture2D GetIcon(ref Texture2D texture, string guid)
        {
            if (texture == null)
            {
                texture = EditorGUIUtility.Load(AssetDatabase.GUIDToAssetPath(guid)) as Texture2D;
            }
            return texture;
        }

        private void Save()
        {
            if (m_reactionType == null || EditorApplication.isPlaying)
                return;

            EditorUtility.SetDirty(m_reactionType);
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region ReorderableList Methods

        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.PropertyField(rect, m_materialEffects.GetArrayElementAtIndex(index), GUIContent.none);
        }

        private bool OnCanAddCallback(ReorderableList list)
        {
            return GetAvailableSubstances().Any();
        }

        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            var substances = GetAvailableSubstances();

            var addMenu = new GenericMenu();
            foreach (var substance in substances.OrderBy(x => x.name))
            {
                addMenu.AddItem(new GUIContent(substance.name), false, OnAddFXMaterial, substance);
            }

            addMenu.ShowAsContext();
        }

        private void OnAddFXMaterial(object parameter)
        {
            if (parameter == null)
                return;

            var substance = parameter as MaterialEffectType;

            m_reactionType.fxMaterials.Add(substance);
            m_materialEffectList.list = m_reactionType.fxMaterials.ToArray();

            // Add reaction with other substances
            foreach (var other in m_reactionType.fxMaterials)
            {
                m_reactionType.SetReaction(
                    substance,
                    other,
                    new MaterialEffectManagerConfig.Reaction());
            }

            m_selectedReaction = null;
            Save();
        }

        private void OnReorderCallback(ReorderableList list)
        {
            m_reactionType.fxMaterials = list.list as IList<MaterialEffectType>;
            m_selectedReaction = null;
            Save();
        }

        private bool OnCanRemoveCallback(ReorderableList list)
        {
            return m_reactionType.fxMaterials != null && m_reactionType.fxMaterials.Count > 0;
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            // Substance to be removed
            var substance = m_reactionType.fxMaterials[list.index];

            // Remove reactions with other substances
            foreach (var other in m_reactionType.fxMaterials)
            {
                m_reactionType.RemoveReaction(substance, other);
            }

            // Remove substance from list
            m_reactionType.fxMaterials.RemoveAt(list.index);
            list.list = m_reactionType.fxMaterials.ToArray();

            m_selectedReaction = null;
            Save();
        }

        #endregion

        #region Structures

        public struct SubstanceMenuEventArgs
        {
            public MaterialEffectType faction1;
            public MaterialEffectType faction2;
            public MaterialEffectManagerConfig.Reaction reaction;
        }

        #endregion
    }
}