using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Move.Client;

namespace Move.Core
{
    //  マッチングクラス
    [DisallowMultipleComponent]
    public sealed class MatchmakingController : MonoBehaviour
    {
        [SerializeField] NetworkClient _networkClient;

        CancellationTokenSource _cts;
        bool _isMatchSuccessed;


        void Start()
        {
            _cts = new CancellationTokenSource();
            if (_networkClient != null)
            {
                //  マッチ成功時に関数を駆動させるために代入
                NetworkClient.OnMatchmakingSuccess += OnMatchSuccess;
            }

            CheckMatchStatusLoop(_cts.Token).Forget();
        }

        //  マッチング成功時に呼ばれる
        void OnMatchSuccess()
        {
            _isMatchSuccessed = true;
        }

        async UniTaskVoid CheckMatchStatusLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_isMatchSuccessed)
                    {
                        StartGame();
                        return;
                    }

                    if (_networkClient != null && _networkClient.IsConnected)
                    {
                        //  マッチングトライ
                        _networkClient.SendMatchingsSignal();
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("マッチング確認ループが安全に破棄されました。");
            }
        }

        void OnDestroy()
        {
                NetworkClient.OnMatchmakingSuccess -= OnMatchSuccess;

            _cts?.Cancel();
            _cts?.Dispose();
        }

        void StartGame()
        {
            Debug.Log("ゲームスタート");
        }
    }
}