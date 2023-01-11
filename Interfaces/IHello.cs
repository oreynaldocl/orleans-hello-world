using Orleans;

namespace Interfaces
{
    public interface IHello : IGrainWithGuidKey
    {
        Task<string> SayHello(string greeting);
        Task<string> GetContent();

    }
}