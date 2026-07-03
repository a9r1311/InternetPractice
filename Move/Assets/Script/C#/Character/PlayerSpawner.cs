using System.Collections.Generic;
using UnityEngine;

namespace Move.Player
{
    //  プレイヤー生成クラス(シングルトン)
    [DisallowMultipleComponent]
    public class PlayerSpawner : MonoBehaviour
    {
        public static PlayerSpawner Instance { get; private set; }

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private int poolSize = 10;

        readonly Queue<GameObject> _pool = new Queue<GameObject>();

        void Awake()
        {
            Instance = this;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(playerPrefab, this.transform);
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        //  生成API
        public GameObject Spawn(Vector3 position)
        {
            if (_pool.Count > 0)
            {
                GameObject obj = _pool.Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = Quaternion.identity;
                obj.SetActive(true);
                return obj;
            }
            else
            {
                return Instantiate(playerPrefab, position, Quaternion.identity);
            }
        }

        //  削除API
        public void Despawn(GameObject obj)
        {
            if (!obj.activeSelf) return;

            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}