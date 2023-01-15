using Orleans;

namespace Interfaces
{
    public interface IGreetingsGrain: IGrainWithIntegerKey
    {
        Task<string> SendGreetings(string greetings);
    }
}
