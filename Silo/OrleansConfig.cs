namespace Silo
{
    public class OrleansConfig
    {
        public string Invariant { get; set; }
        public string ConnectionString { get; set; }

        public int SiloPort { get; set; }
        public int GatewayPort { get; set; }
        public bool UseDashboard { get; set; }
    }
}
