using System;
using System.Threading;

namespace CompliGenie.Services
{
    public static class CurrentTenant
    {
        private static AsyncLocal<Guid?> _tenantId = new AsyncLocal<Guid?>();
        
        public static Guid? Id
        {
            get => _tenantId.Value;
            set => _tenantId.Value = value;
        }
        
        public static void Clear()
        {
            _tenantId.Value = null;
        }
    }
}
