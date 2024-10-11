using UnityEngine;
using static ToolkitEngine.MaterialFX.MaterialEffectManagerConfig;

namespace ToolkitEngine.MaterialFX
{
    public class MaterialEffectManager : ConfigurableSubsystem<MaterialEffectManager, MaterialEffectManagerConfig>
    {
        #region Methods

        public static void Spawn(MaterialEffectType fxMaterialType, GameObject target, Vector3 point, Vector3 normal)
        {
            var other = target.GetComponent<MaterialEffect>();
            if (other == null)
                return;

            if (!CastInstance.Config.TryGetReaction(fxMaterialType, other.materialEffectType, out Reaction reaction))
                return;

            reaction.Instantiate(point, normal);
            other.onReaction?.Invoke();
        }

        public static void Spawn(MaterialEffect fxMaterialType, Collision collision)
        {
            var other = collision.collider.GetComponentInParent<MaterialEffect>();
            if (other == null)
                return;

            // Only want to spawn one reaction per collision
            if (fxMaterialType.GetHashCode() < other.GetHashCode())
                return;

            if (!CastInstance.Config.TryGetReaction(fxMaterialType.materialEffectType, other.materialEffectType, out Reaction reaction)
                || collision.impulse.sqrMagnitude < (reaction.minImpulseThreshold * reaction.minImpulseThreshold)
                || !reaction.isDefined)
            {
                return;
            }

            var contact = collision.GetContact(0);
            reaction.Instantiate(contact.point, contact.normal);

            fxMaterialType.onReaction?.Invoke();
            other.onReaction?.Invoke();
        }

        #endregion
    }
}