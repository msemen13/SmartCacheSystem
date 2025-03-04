namespace SmartCache.Grains.Abstractions
{
  public  interface IBreachedEmailGrain : IGrainWithStringKey
    {
        Task<bool> IsBreached();
        Task SetBreachedStatus(bool status);
        Task<bool> AddBreachedEmail();
        Task Remove();
    }
}