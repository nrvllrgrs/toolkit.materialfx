using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ToolkitEngine.MaterialFX.MaterialEffectManagerConfig;

namespace ToolkitEngine.MaterialFX
{
    [CreateAssetMenu(menuName = "Toolkit/Config/MaterialEffectManager Config")]
    public class MaterialEffectManagerConfig : ScriptableObject
    {
        #region Fields

        [SerializeField]
        private List<MaterialEffectType> m_materialEffects = new();

        [SerializeField]
        private SerializableFXMaterialReaction m_reactions = new();

        #endregion

        #region Properties

        public IList<MaterialEffectType> fxMaterials { get => m_materialEffects; set => m_materialEffects = value.ToList(); }
        public IEnumerable<FXMaterialPair> keys => m_reactions.Keys;

        #endregion

        #region Methods

        public bool TryGetReaction(FXMaterialPair p, out Reaction reaction)
        {
            if (p == null || ReferenceEquals(p.fxMaterial1, null) || ReferenceEquals(p.fxMaterial2, null))
            {
                reaction = null;
                return false;
            }

            if (!m_reactions.ContainsKey(p))
            {
				Debug.LogFormat("{0}-{1}: HashCode = {2}", p.fxMaterial1.name, p.fxMaterial2.name, p.GetHashCode());
				foreach (FXMaterialPair k in m_reactions.Keys)
				{
                    if (Equals(p, k))
                    {
                        try
                        {
                            reaction = m_reactions[new FXMaterialPair(p.fxMaterial2, p.fxMaterial1)];
							return true;
						}
                        catch
                        {
                            Debug.LogFormat("Reaction {0}-{1} not found!", k.fxMaterial1.name, k.fxMaterial2.name);
                        }
                    }

                    //Debug.LogFormat("Check: {0}-{1}: HashCode = {2}; Equals = {3}", k.fxMaterial1.name, k.fxMaterial2.name, k.GetHashCode(), Equals(p, k));
				}
			}

            return m_reactions.TryGetValue(p, out reaction);
        }

        public bool TryGetReaction(MaterialEffectType a, MaterialEffectType b, out Reaction reaction)
        {
			return TryGetReaction(new FXMaterialPair(a, b), out reaction);
        }

        public void SetReaction(MaterialEffectType a, MaterialEffectType b, Reaction reaction)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return;

            var pair = new FXMaterialPair(a, b);
            if (m_reactions.ContainsKey(pair))
            {
                m_reactions[pair] = reaction;
            }
            else
            {
                m_reactions.Add(pair, reaction);
            }
        }

        public bool RemoveReaction(MaterialEffectType a, MaterialEffectType b)
        {
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

            var pair = new FXMaterialPair(a, b);
            if (m_reactions.ContainsKey(pair))
            {
                m_reactions.Remove(pair);
                return true;
            }
            return false;
        }

        #endregion

        #region Structures

        [Serializable]
        public class FXMaterialPair : IEquatable<FXMaterialPair>
        {
            #region Fields

            [SerializeField]
            private MaterialEffectType m_fxMaterial1, m_fxMaterial2;

            private int? m_hashCode = null;

            #endregion

            #region Properties

            public MaterialEffectType fxMaterial1 => m_fxMaterial1;
            public MaterialEffectType fxMaterial2 => m_fxMaterial2;

			#endregion

			#region Constructors

			public FXMaterialPair(MaterialEffectType fxMaterial1, MaterialEffectType fxMaterial2)
            {
                m_fxMaterial1 = fxMaterial1;
                m_fxMaterial2 = fxMaterial2;
			}

            #endregion

            #region Methods

            public bool Equals(FXMaterialPair other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return Equals(GetHashCode(), other.GetHashCode());
            }

			public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                if (obj.GetType() != GetType())
                    return false;

                return Equals((FXMaterialPair)obj);
            }

            public override int GetHashCode()
            {
				if (!m_hashCode.HasValue)
				{
					int a = m_fxMaterial1.GetHashCode();
					int b = m_fxMaterial2.GetHashCode();

					m_hashCode = a > b
						? HashCode.Combine(m_fxMaterial1, m_fxMaterial2)
						: HashCode.Combine(m_fxMaterial2, m_fxMaterial1);
				}
				return m_hashCode.Value;
			}

            #endregion
        }

        [Serializable]
        public class Reaction
        {
            #region Fields

            [SerializeField]
            private Spawner m_spawner;

            [SerializeField, Min(0f), Tooltip("Minimum impact magnitude for reaction to occur.")]
            private float m_minImpulseThreshold;

            #endregion

            #region Properties

            public float minImpulseThreshold => m_minImpulseThreshold;
            public bool isDefined => m_spawner.isDefined;

			#endregion

			#region Methods

			internal void Instantiate(Vector3 point, Vector3 normal)
			{
				Instantiate(point, Quaternion.LookRotation(normal));
			}

			internal void Instantiate(Vector3 position, Quaternion rotation)
			{
				m_spawner.Instantiate(position, rotation);
			}

			#endregion
		}

		#endregion
	}

	[Serializable]
    public class SerializableFXMaterialReaction : SerializableDictionary<FXMaterialPair, Reaction>
    { }
}