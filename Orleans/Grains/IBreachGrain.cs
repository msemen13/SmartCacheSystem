using Orleans;
using System.Threading.Tasks;

public interface IBreachGrain : IGrainWithStringKey
{
    Task<bool> IsBreached();
    Task SetBreachedStatus(bool status);
}