using UnityEngine;
using UnityEngine.AI;

namespace DaftAppleGames.Darskerry.Core.Extensions
{
    public static class TerrainExtensions
    {
        public static void AlignObject(this Terrain terrain, GameObject targetGameObject, bool alignPosition, bool alignRotation,
            bool alignX, bool alignY, bool alignZ)
        {
            if (alignPosition)
            {
                float terrainHeight = GetTerrainHeightAtTransform(terrain, targetGameObject.transform);
                Vector3 newPosition = targetGameObject.transform.position;
                newPosition.y = terrainHeight;
                targetGameObject.transform.position = newPosition;
            }

            if (alignRotation)
            {
                Quaternion newRotation = GetTerrainSlopeAtTransform(terrain, targetGameObject.transform);
                if (!alignX)
                {
                    newRotation.x = targetGameObject.transform.rotation.x;
                }

                if (!alignY)
                {
                    newRotation.y = targetGameObject.transform.rotation.y;
                }

                if (!alignZ)
                {
                    newRotation.z = targetGameObject.transform.rotation.z;
                }
                targetGameObject.transform.rotation = newRotation;
            }

        }

        private static Quaternion GetTerrainSlopeAtTransform(Terrain terrain, Transform targetTransform)
        {
            RaycastHit[] hits = Physics.RaycastAll( targetTransform.position + (Vector3.up * 2.0f) , Vector3.down , maxDistance:10f );
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform != targetTransform)
                {
                    return Quaternion.LookRotation( Vector3.ProjectOnPlane(targetTransform.forward,hit.normal).normalized , hit.normal );
                }
            }

            return targetTransform.rotation;
        }

        private static float GetTerrainHeightAtTransform(Terrain terrain, Transform targetTransform)
        {
            return terrain.SampleHeight(targetTransform.position);
        }

        public static bool GetRandomLocation(this Terrain terrain, Vector3 center, float minDistance, float maxDistance, out Vector3 location)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minDistance, maxDistance);
            Vector3 offset = new Vector3(randomDirection.x, 0, randomDirection.y) * distance;
            Vector3 randomPosition = center + offset;

            if (Terrain.activeTerrain)
            {
                float terrainHeight = terrain.SampleHeight(randomPosition);
                randomPosition.y = terrainHeight;
            }

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit myNavHit, 100, -1))
            {
                location = myNavHit.position;
                return true;
            }

            location = randomPosition;
            return false;
        }
    }
}