using System;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace CompliGenie.Context
{
    // Interface for tenant context
    public interface ICurrentTenant
    {
        Guid Id { get; set; }
        string? Name { get; set; }
        string? StripeAccountId { get; set; }
        bool IsSet { get; }
        void Clear();
    }

    // Scoped service implementation using HttpContext
    public class CurrentTenant : ICurrentTenant
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        // Thread-safe storage for background jobs
        private static readonly AsyncLocal<TenantInfo> _asyncLocalTenant = new AsyncLocal<TenantInfo>();

        public CurrentTenant(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid Id
        {
            get
            {
                // Try HttpContext first (web requests)
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.Items.ContainsKey("TenantId") == true && httpContext.Items["TenantId"] is Guid tenantId)
                {
                    return tenantId;
                }
                
                // Fall back to AsyncLocal (background jobs)
                return _asyncLocalTenant.Value?.Id ?? Guid.Empty;
            }
            set
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["TenantId"] = value;
                }
                
                // Also set in AsyncLocal for background jobs
                if (_asyncLocalTenant.Value == null)
                {
                    _asyncLocalTenant.Value = new TenantInfo();
                }
                _asyncLocalTenant.Value.Id = value;
            }
        }

        public string? Name
        {
            get
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.Items.ContainsKey("TenantName") == true)
                {
                    return httpContext.Items["TenantName"]?.ToString();
                }
                
                return _asyncLocalTenant.Value?.Name;
            }
            set
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["TenantName"] = value;
                }
                
                if (_asyncLocalTenant.Value == null)
                {
                    _asyncLocalTenant.Value = new TenantInfo();
                }
                _asyncLocalTenant.Value.Name = value;
            }
        }

        public string? StripeAccountId
        {
            get
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.Items.ContainsKey("StripeAccountId") == true)
                {
                    return httpContext.Items["StripeAccountId"]?.ToString();
                }
                
                return _asyncLocalTenant.Value?.StripeAccountId;
            }
            set
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["StripeAccountId"] = value;
                }
                
                if (_asyncLocalTenant.Value == null)
                {
                    _asyncLocalTenant.Value = new TenantInfo();
                }
                _asyncLocalTenant.Value.StripeAccountId = value;
            }
        }

        public bool IsSet => Id != Guid.Empty;

        public void Clear()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items.Remove("TenantId");
                httpContext.Items.Remove("TenantName");
                httpContext.Items.Remove("StripeAccountId");
            }
            
            _asyncLocalTenant.Value = null!;
        }

        // Helper class for AsyncLocal storage
        private class TenantInfo
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public string? StripeAccountId { get; set; }
        }
    }
}

