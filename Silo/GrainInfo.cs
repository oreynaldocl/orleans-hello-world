namespace Silo
{
    public class GrainInfo
    {
        public List<string> Methods { get; set; }
        public GrainInfo()
        {
            Methods = new List<string>();
        }
    }
}
