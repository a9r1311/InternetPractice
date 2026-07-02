using UnityEngine;
using Move.Client;

namespace Move.Player
{
    [DisallowMultipleComponent]
    //  プレイヤー操作受付クラス
    public sealed class PlayerInputController : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] float _moveSpeed = 5f;
        [SerializeField] NetworkClient _networkClient; // あなたの作ったClient窓口

        // ビットフラグの定義（前:1, 後:2, 左:4, 右:8）
        const byte INPUT_UP    = 1 << 0;    //  00000001 (1)
        const byte INPUT_DOWN  = 1 << 1;    //  00000010 (2)
        const byte INPUT_LEFT  = 1 << 2;    //  00000100 (4)
        const byte INPUT_RIGHT = 1 << 3;    //  00001000 (8)

        Transform _transform;

        void Awake()
        {
            _transform = transform;
        }

        void Update()
        {
            byte inputFlags = 0;

            if (Input.GetKey(KeyCode.W)) inputFlags |= INPUT_UP;
            if (Input.GetKey(KeyCode.S)) inputFlags |= INPUT_DOWN;
            if (Input.GetKey(KeyCode.A)) inputFlags |= INPUT_LEFT;
            if (Input.GetKey(KeyCode.D)) inputFlags |= INPUT_RIGHT;
            
            if (inputFlags == 0) return;

            Vector3 moveDir = Vector3.zero;
            if ((inputFlags & INPUT_UP)    != 0) moveDir.z += 1f;
            if ((inputFlags & INPUT_DOWN)  != 0) moveDir.z -= 1f;
            if ((inputFlags & INPUT_LEFT)  != 0) moveDir.x -= 1f;
            if ((inputFlags & INPUT_RIGHT) != 0) moveDir.x += 1f;

            if (moveDir != Vector3.zero)
            {
                moveDir.Normalize();
                
                _transform.position += moveDir * (_moveSpeed * Time.deltaTime);
            }

            _networkClient.SendPosition(_transform.position);
        }
    }
}

