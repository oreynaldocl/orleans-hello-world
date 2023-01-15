namespace Silo.Context
{
    public interface IOrleansRequestContext
    {
        Guid TraceId { get; }
    }
}