using UnityEditor;
using ToolkitEngine.MaterialFX;

namespace ToolkitEditor.MaterialFX
{
	[CustomEditor(typeof(MaterialEffect))]
    public class MaterialEffectEditor : BaseToolkitEditor
    {
		#region Fields

		protected SerializedProperty m_materialEffectType;
		protected SerializedProperty m_onReaction;

		#endregion

		#region Methods

		protected virtual void OnEnable()
		{
			m_materialEffectType = serializedObject.FindProperty(nameof(m_materialEffectType));
			m_onReaction = serializedObject.FindProperty(nameof(m_onReaction));
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.PropertyField(m_materialEffectType);
		}

		protected override void DrawEvents()
		{
			if (EditorGUILayoutUtility.Foldout(m_onReaction, "Events"))
			{
				EditorGUILayout.PropertyField(m_onReaction);
			}
		}

		#endregion
	}
}