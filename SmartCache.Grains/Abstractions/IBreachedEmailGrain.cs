namespace SmartCache.Grains.Abstractions
{
  public  interface IBreachedEmailGrain : IGrainWithStringKey
    {
        Task<bool> IsBreached();
        Task<bool> AddBreachedEmail();
        Task Remove();
    }
}