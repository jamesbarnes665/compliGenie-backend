namespace CompliGenie.Context
{
    public static class CurrentTenant
    {
        private static readonly AsyncLocal<Guid?> _tenantId = new();

        public static Guid? Id
        {
            get => _tenantId.Value;
            set => _tenantId.Value = value;
        }
    }
}
