using Orleans;

namespace Grains.Interfaces
{
    public interface IHello : IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
    }
}
