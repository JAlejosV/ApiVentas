namespace ApiVentas.Services
{
    public class MariaDbSettings
    {
        public SshConnectionSettings SSH { get; set; } = new();
        public MariaDbConnectionSettings Database { get; set; } = new();
    }

    public class SshConnectionSettings
    {
        public string Host { get; set; }
        public int Port { get; set; } = 22;
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class MariaDbConnectionSettings
    {
        public string Host { get; set; }
        public int Port { get; set; } = 3306;
        public string User { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
