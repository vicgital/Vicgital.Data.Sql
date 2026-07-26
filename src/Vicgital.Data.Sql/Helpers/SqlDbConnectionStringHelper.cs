namespace Vicgital.Data.Sql.Helpers
{
    public static class SqlDbConnectionStringHelper
    {
        public static string GetSqlDbConnectionString(string server, Enums.Databases database, string port, string username, string password)
        {
            #region Validate
            ArgumentException.ThrowIfNullOrEmpty(server, nameof(server));
            ArgumentException.ThrowIfNullOrEmpty(username, nameof(username));
            ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
            #endregion

            string connectionString;
            if (!string.IsNullOrEmpty(port))
                connectionString = $"Server={server},{port};Initial Catalog={database};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;packet size=12000";
            else
                connectionString = $"Server={server};Initial Catalog={database};Persist Security Info=False;User ID={username};Password={password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;packet size=12000";

            return connectionString;

        }

    }
}

