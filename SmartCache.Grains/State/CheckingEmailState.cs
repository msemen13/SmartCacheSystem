namespace SmartCache.Grains.State
{
    [GenerateSerializer]
    public record CheckingEmailState
    {
        [Id(0)]
        public string Email { get; set; } 

        [Id(1)]
        public bool IsBreached { get; set; }
    }
}
