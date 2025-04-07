#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace DaftAppleGames.Pooling
{
    public enum PrefabPoolType
    {
        FootstepMarks,
        FootstepParticles,
        AiCharacter
    }

    public class PrefabPool : MonoBehaviour
    {
        #region Class Variables

        [BoxGroup("Prefab Settings")] [SerializeField] private PrefabPoolType prefabPoolType;
        [BoxGroup("Prefab Settings")] [SerializeField] private bool destroyAfterDelay = true;
        [ShowIf("destroyAfterDelay")] [BoxGroup("Prefab Settings")] [SerializeField] private float lifeTimeInSeconds;
        [BoxGroup("Prefab Settings")] [SerializeField] private GameObject poolPrefab;
        [BoxGroup("Prefab Settings")] [SerializeField] private Transform instanceContainer;
        [BoxGroup("Pool Settings")] [SerializeField] internal int poolMaxSize = 5;

        [BoxGroup("Pool Settings")] [SerializeField] internal int poolInitialSize = 10;

        /*
        [BoxGroup("DEBUG")][SerializeField] private int poolActiveCountDebug;
        [BoxGroup("DEBUG")][SerializeField] private int poolInactiveCountDebug;
        */
        public PrefabPoolType PrefabPoolType => prefabPoolType;
        private ObjectPool<GameObject> _prefabInstancePool;

        internal int ActivePoolSize => _prefabInstancePool.CountActive;

        #endregion

        #region Startup

        /// <summary>
        /// Configure the component on awake
        /// </summary>   
        protected virtual void Awake()
        {
            _prefabInstancePool = new ObjectPool<GameObject>(CreatePrefabInstancePoolItem, PrefabInstanceOnTakeFromPool,
                PrefabInstanceOnReturnToPool, PrefabInstanceOnDestroyPoolObject, false,
                poolInitialSize, poolMaxSize);
        }

        #endregion

        #region Update

        /*
        private void Update()
        {
            poolActiveCountDebug = _prefabInstancePool.CountActive;
            poolInactiveCountDebug = _prefabInstancePool.CountInactive;
        }

        */

        #endregion

        #region Class methods

        public GameObject SpawnInstance(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject prefabInstance = _prefabInstancePool.Get();
            prefabInstance.transform.position = spawnPosition;
            prefabInstance.transform.rotation = spawnRotation;
            if (destroyAfterDelay)
            {
                StartCoroutine(ReturnToPoolAfterDelay(prefabInstance));
            }

            return prefabInstance;
        }

        public void DespawnInstance(GameObject prefabInstance)
        {
            prefabInstance.transform.position = Vector3.zero;
            prefabInstance.transform.rotation = Quaternion.identity;
            prefabInstance.SetActive(false);
            _prefabInstancePool.Release(prefabInstance);
        }

        private IEnumerator ReturnToPoolAfterDelay(GameObject prefabInstance)
        {
            yield return new WaitForSeconds(lifeTimeInSeconds);
            _prefabInstancePool.Release(prefabInstance);
        }

        private GameObject CreatePrefabInstancePoolItem()
        {
            GameObject prefabInstance = Instantiate(poolPrefab, instanceContainer, false);
            prefabInstance.name = $"{poolPrefab.name}-New";
            return prefabInstance;
        }

        private void PrefabInstanceOnTakeFromPool(GameObject prefabInstance)
        {
            prefabInstance.name = $"{poolPrefab.name}-Taken";
            prefabInstance.SetActive(true);
        }

        private void PrefabInstanceOnReturnToPool(GameObject prefabInstance)
        {
            prefabInstance.name = $"{poolPrefab.name}-Returned";
            prefabInstance.transform.position = Vector3.zero;
            prefabInstance.transform.rotation = Quaternion.identity;
            prefabInstance.SetActive(false);
        }

        private static void PrefabInstanceOnDestroyPoolObject(GameObject prefabInstance)
        {
            Destroy(prefabInstance);
        }

        #endregion
    }
}