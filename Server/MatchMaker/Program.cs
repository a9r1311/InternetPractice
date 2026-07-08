namespace Move.Matchmaker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options => {
                options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            var app = builder.Build();

            app.UseCors();

            app.MapGet("/api/match/request", () => new
            {
                ipAddress = "127.0.0.1",
                port = 8000
            });

            app.Run("http://127.0.0.1:5000");
        }
    }
}