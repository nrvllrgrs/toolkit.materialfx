using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.MaterialFX
{
    [AddComponentMenu("Toolkit/Material FX/Material Effect")]
    public class MaterialEffect : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private MaterialEffectType m_materialEffectType;

        #endregion

        #region Events

        [SerializeField]
        private UnityEvent m_onReaction;

		#endregion

		#region Properties

		public MaterialEffectType materialEffectType => m_materialEffectType;
        public UnityEvent onReaction => m_onReaction;

        #endregion

        #region Methods

        private void OnCollisionEnter(Collision collision)
        {
            ReactionManager.Spawn(this, collision);
        }

        #endregion
    }
}