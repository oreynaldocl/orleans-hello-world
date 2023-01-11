using Orleans;

namespace Interfaces
{
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
        Task<string> GetContent();

    }
}