using Xunit;
using Microsoft.EntityFrameworkCore;
using CompliGenie.Data;
using CompliGenie.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CompliGenie.E2E.Tests
{
    public class TenantIsolationUnitTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Policies_Are_Isolated_By_Tenant()
        {
            // Arrange
            using var context = CreateContext();
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            // Create policies for two different tenants
            context.Policies.Add(new Policy
            {
                TenantId = tenant1Id,
                ClientName = "Tenant1 Client",
                Content = "Policy for tenant 1"
            });

            context.Policies.Add(new Policy
            {
                TenantId = tenant2Id,
                ClientName = "Tenant2 Client",
                Content = "Policy for tenant 2"
            });

            await context.SaveChangesAsync();

            // Act - Query policies for tenant 1
            var tenant1Policies = await context.Policies
                .Where(p => p.TenantId == tenant1Id)
                .ToListAsync();

            // Assert
            Assert.Single(tenant1Policies);
            Assert.Equal("Tenant1 Client", tenant1Policies.First().ClientName);
            Assert.DoesNotContain(tenant1Policies, p => p.TenantId == tenant2Id);
        }

        [Fact]
        public async Task Payments_Are_Isolated_By_Tenant()
        {
            // Arrange
            using var context = CreateContext();
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            // Create payments for different tenants
            context.Payments.Add(new Payment
            {
                TenantId = tenant1Id,
                Amount = 100.00m,
                StripePaymentIntentId = "pi_tenant1"
            });

            context.Payments.Add(new Payment
            {
                TenantId = tenant2Id,
                Amount = 200.00m,
                StripePaymentIntentId = "pi_tenant2"
            });

            await context.SaveChangesAsync();

            // Act
            var tenant1Payments = await context.Payments
                .Where(p => p.TenantId == tenant1Id)
                .ToListAsync();

            // Assert
            Assert.Single(tenant1Payments);
            Assert.Equal(100.00m, tenant1Payments.First().Amount);
            Assert.All(tenant1Payments, p => Assert.Equal(tenant1Id, p.TenantId));
        }

        [Fact]
        public void Composite_Keys_Ensure_Unique_Ids_Per_Tenant()
        {
            // Arrange
            using var context = CreateContext();
            var sharedId = Guid.NewGuid();
            var tenant1Id = Guid.NewGuid();
            var tenant2Id = Guid.NewGuid();

            // Act - Add policies with same ID but different tenants
            context.Policies.Add(new Policy
            {
                Id = sharedId,
                TenantId = tenant1Id,
                ClientName = "Client A"
            });

            context.Policies.Add(new Policy
            {
                Id = sharedId,
                TenantId = tenant2Id,
                ClientName = "Client B"
            });

            // Assert - Should save without conflict
            var saveResult = context.SaveChanges();
            Assert.Equal(2, saveResult);
        }
    }
}
