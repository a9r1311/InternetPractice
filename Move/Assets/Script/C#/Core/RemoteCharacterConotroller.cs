using UnityEngine;

namespace Move.Character
{
    //  対戦相手を動かすスクリプト
    public sealed class RemoteCharacterController : MonoBehaviour
    {
        private Vector3 _targetPosition;    //  対象の座標
        [SerializeField] private float _moveSpeed = 15f;

        void Start()
        {
            _targetPosition = transform.position;
        }

        public void SetTargetPosition(Vector3 pos)
        {
            _targetPosition = pos;
        }

        void Update()
        {
            transform.position = Vector3.Lerp(
                transform.position, _targetPosition, Time.deltaTime * _moveSpeed
                );
        }
    }
}