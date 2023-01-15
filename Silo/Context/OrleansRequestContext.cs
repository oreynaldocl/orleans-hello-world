using Orleans.Runtime;

namespace Silo.Context
{
    public class OrleansRequestContext : IOrleansRequestContext
    {
        public Guid TraceId
        {
            get
            {
                object traceId = RequestContext.Get("traceId");
                return traceId == null ? Guid.Empty : (Guid)traceId;
            }
        }
    }
}
