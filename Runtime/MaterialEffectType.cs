using System;
using UnityEngine;
using NaughtyAttributes;

namespace ToolkitEngine.MaterialFX
{
    [CreateAssetMenu(menuName = "Toolkit/Material FX/Material Effect")]
    public class MaterialEffectType : ScriptableObject, IEquatable<MaterialEffectType>
    {
		#region Fields

		[SerializeField, ReadOnly]
		private string m_id = Guid.NewGuid().ToString();

		private int? m_hashCode = null;

		#endregion

		#region Properties

		public string id => m_id;

		#endregion

		#region Methods

		public bool Equals(MaterialEffectType other)
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

			return Equals((MaterialEffectType)obj);
		}

		public override int GetHashCode()
		{
			if (!m_hashCode.HasValue)
			{
				m_hashCode = m_id.GetHashCode();
			}
			return m_hashCode.Value;
		}

		#endregion
	}
}