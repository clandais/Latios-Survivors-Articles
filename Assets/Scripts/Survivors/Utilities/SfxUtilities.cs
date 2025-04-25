using Latios;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Utilities
{
    [BurstCompile]
    public static class SfxUtilities
    {
        public static Entity GetSfx(in OneShotSfxSpawnerRef oneShotSfxSpawner,
            in ComponentLookup<OneShotSfxSpawner> spawnerLookup,
            in BufferLookup<OneShotSfxElement> sfxLookup,
            ref SystemRng rng)
        {
            var spawner = spawnerLookup[oneShotSfxSpawner.SfxPrefab];
            var buffer = sfxLookup[oneShotSfxSpawner.SfxPrefab];
            if (buffer.Length == 0) return Entity.Null;

            var prefabIndex = rng.NextInt(0, spawner.SfxCount);
            return buffer[prefabIndex].Prefab;
        }
    }
}