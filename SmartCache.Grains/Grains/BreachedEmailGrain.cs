using Microsoft.Extensions.Logging;
using SmartCache.Grains.Abstractions;
using SmartCache.Grains.State;

namespace SmartCache.Grains.Grains
{
    public class BreachedEmailGrain : Grain, IBreachedEmailGrain
    {
        private bool _isBreached;
        private readonly IPersistentState<CheckingEmailState> _checkingEmailState;
        private readonly ILogger<BreachedEmailGrain> _logger;

        public BreachedEmailGrain(
            [PersistentState("checkemail", "blobStorage")] IPersistentState<CheckingEmailState> checkingEmailState,
            ILogger<BreachedEmailGrain> logger)
        {
            _checkingEmailState = checkingEmailState;
            _logger = logger;
        }

        public override async Task OnActivateAsync(CancellationToken token)
        {
            try
            {
                _logger.LogInformation("Activating grain: {Email}", this.GetPrimaryKeyString());
                await _checkingEmailState.ReadStateAsync();
                _isBreached = _checkingEmailState.State.IsBreached;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(OnActivateAsync)} - Error: {ex.Message}");
            }
        }

        public Task<bool> IsBreached() => Task.FromResult(_isBreached);

        public async Task<bool> AddBreachedEmail()
        {
            if (_isBreached)
            {
                _logger.LogInformation("{Email} is already added to breached", this.GetPrimaryKeyString());
                return false;
            }

            _logger.LogInformation("{Email} adding to breached", this.GetPrimaryKeyString());
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
