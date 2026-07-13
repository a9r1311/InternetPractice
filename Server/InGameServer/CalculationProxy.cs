using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using InGameServer.User;

namespace InGameServer.Core
{
    // 計算専用のメモリ空間（ここで何が起きてもマスターは無傷）
    public unsafe class CalculationProxy
    {
        private readonly UserData* _masterData; // 信頼するマスター領域のポインタ

        public CalculationProxy(UserData* masterData)
        {
            _masterData = masterData;
        }

        // 不正なデータが混じっていても、計算結果を汚染させないための「検証付き計算」
        public bool TryMatch(out int partnerId)
        {
            partnerId = -1;

            // 1. スナップショットの取得（ローカル変数にコピーすることで、
            //    途中でメモリが書き換えられても計算ロジックは安定する）
            UserData localCopy = *_masterData;

            // 2. 事後バリデーション（ここで不正な値を論理的に遮断）
            if (localCopy.UserId < 0 || localCopy.UserId > 1000000)
                return false; // 不正なデータは計算せずに棄却

            // 3. 安全な計算処理
            //if (localCopy.Status)
            //{
            //    partnerId = localCopy.TargetId;
            //    return true;
            //}

            return false;
        }
    }
}