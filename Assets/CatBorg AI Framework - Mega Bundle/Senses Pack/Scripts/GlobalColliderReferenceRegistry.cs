using System.Runtime.CompilerServices;
using UnityEngine;

namespace Senses2026
{
    public static class GlobalColliderReferenceRegistry
    {
        private static readonly ConditionalWeakTable<Collider, ColliderReference> Cache
            = new ConditionalWeakTable<Collider, ColliderReference>();


        public static ColliderReference Get(Collider col)
        {
            // FAST PATH: already in global cache
            if (Cache.TryGetValue(col, out var cref))
                return cref;

            // SLOW PATH: collider has no cached entry — do TryGetComponent()
            if (!col.TryGetComponent(out cref))
            {
                // AUTO-FIX: add ColliderReference if none exists
                cref = col.gameObject.AddComponent<ColliderReference>();
            }

            // Add to global cache
            Cache.Add(col, cref);

            return cref;
        }
    }
}
