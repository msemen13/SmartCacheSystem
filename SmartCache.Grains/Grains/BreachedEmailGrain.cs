using SmartCache.Grains.Abstractions;
using SmartCache.Grains.State;

namespace SmartCache.Grains.Grains
{
    public class BreachedEmailGrain : Grain, IBreachedEmailGrain
    {
        private bool _isBreached;
        private readonly IPersistentState<CheckingEmailState> _checkingEmailState;

        public BreachedEmailGrain(
            [PersistentState("checkemail", "blobStorage")] IPersistentState<CheckingEmailState> checkingEmailState)
        {
            _checkingEmailState = checkingEmailState;
        }

        public override async Task OnActivateAsync(CancellationToken token)
        {
            await _checkingEmailState.ReadStateAsync();
            _isBreached = _checkingEmailState.State.IsBreached;
        }
        
        public Task<bool> IsBreached() => Task.FromResult(_isBreached);

        public async Task<bool> AddBreachedEmail()
        {
            if (_isBreached)
            {
                return false;
            }

            _checkingEmailState.State.Email = this.GetPrimaryKeyString();
            _checkingEmailState.State.IsBreached = true;

            await _checkingEmailState.WriteStateAsync();

            _isBreached = true;

            return true;
        }

        public async Task Remove()
        {
            await _checkingEmailState.ClearStateAsync();
            _isBreached = false;
        }
    }
}
