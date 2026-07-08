using Move.Server;

//  Moveのサーバースクリプト
class Program
{
    static void Main(string[] args)
    {
        NetworkManager server = new NetworkManager();
        server.StartServer(8000); // 9050番ポートで待ち受け
        server.UpdateLoop();      // 無限ループで通信を処理し続ける
    }
}