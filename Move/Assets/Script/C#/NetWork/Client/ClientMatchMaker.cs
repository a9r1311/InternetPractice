using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Move.Client
{
    public static class ClientMatchmaker
    {
        private const string MatchmakerUrl = "http://10.219.32.59:5000/api/match/request";
        public static async Task<ServerInfo> RequestMatchServerAsync()
        {
            ServerInfo serverInfo = default;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(MatchmakerUrl))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield(); 
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[マッチメーカーエラー] サーバー情報の取得に失敗: {webRequest.error}");
                    return serverInfo;
                }

                try
                {
                    string jsonResult = webRequest.downloadHandler.text;
                    serverInfo = JsonUtility.FromJson<ServerInfo>(jsonResult);
                    
                    Debug.Log($"[マッチメーカー成功] 試合サーバーを確保しました。 接続先 -> {serverInfo.ipAddress}:{serverInfo.port}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[JSON解析エラー] データのパースに失敗しました: {ex.Message}");
                }
            }

            return serverInfo;
        }
    }
}