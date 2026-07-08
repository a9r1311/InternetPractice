using System.Net;
using System.Net.Sockets;

namespace Move.Matchmaker
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            var app = builder.Build();

            app.UseCors();

            app.MapGet("/api/match/request", () => _cachedResponse);

            app.Run("http://0.0.0.0:5000");
        }

        //  InGameServevrのipAddressとportを返すためのrecord
        public record MatchResponse(string ipAddress, int port);

        //  ipAdressとPortをキャッシュ
        static readonly MatchResponse _cachedResponse = new(
            ServerConfig.GetLocalIpAddress(),
            8000
        );

        public static class ServerConfig
        {
            //  ローカルIPアドレス取得
            public static string GetLocalIpAddress()
            {
                try
                {
                    using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                    socket.Connect("8.8.8.8", 65530);
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                    {
                        return endPoint.Address.ToString();
                    }
                }
                catch
                {

                }
                return "127.0.0.1";
            }
        }
    }
}