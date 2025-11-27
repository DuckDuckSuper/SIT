
namespace MapProvider.Utils
{
    public class DBConnection
    {
        private readonly string _connectionString;

       public DBConnection()
        {
            // Read individual environment variables
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var pass = Environment.GetEnvironmentVariable("DB_PASS") ?? "123";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var sslMode = Environment.GetEnvironmentVariable("DB_SSLMODE") ?? "Require";
            var dbType = Environment.GetEnvironmentVariable("DB_TYPE") ?? "LOCAL";

            if(dbType.ToUpper()=="LOCAL")
            {
                _connectionString = $"Host={host};Port={port};Username={user};Password={pass};Database={dbName};";
            }
            else
            {
                _connectionString = $"Host={host};Port={port};User Id={user};Password={pass};Database={dbName};";
            }



        
Console.WriteLine("Connection String1: " + _connectionString);
    _connectionString =
    "Host=aws-0-ap-southeast-1.pooler.supabase.com;" +
    "Port=5432;" +
    "User Id=postgres.toxvcyasdfabdopusdjq;" +
    "Password='Testing#$';" +
    "Database=postgres;";


Console.WriteLine("Connection String2: " + _connectionString);



        }

        public DBConnection(string conn)  
        {
            _connectionString = conn;
        }


        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
