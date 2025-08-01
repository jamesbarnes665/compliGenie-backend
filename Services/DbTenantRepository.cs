using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompliGenie.Models;
using CompliGenie.Services.Interfaces;
using CompliGenie.Data;
using Microsoft.EntityFrameworkCore;

namespace CompliGenie.Services
{
    public class DbTenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _context;

        public DbTenantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant> CreateAsync(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task<Tenant?> GetByIdAsync(Guid id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
        {
            return await _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == subdomain);
        }

        public async Task<bool> ExistsAsync(string companyName)
        {
            return await _context.Tenants.AnyAsync(t => t.Name == companyName);
        }

        public async Task<List<Tenant>> GetAllAsync()
        {
            return await _context.Tenants.ToListAsync();
        }
    }
}
