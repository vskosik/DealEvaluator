using Deal_Evaluator.Data;
using Deal_Evaluator.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DealEvaluator.Tests;

public class PropertyCrudTests
{
    private DbContextOptions<DealEvaluatorContext> GetInMemoryDbContextOptions()
    {
        return new DbContextOptionsBuilder<DealEvaluatorContext>()
            .UseInMemoryDatabase(databaseName: "DealEvaluatorTestDb")
            .Options;
    }
    
    // Test Create Operation
    [Fact]
    public async Task CreateProperty_ShouldAddProperty()
    {
        var options = GetInMemoryDbContextOptions();

        await using (var context = new DealEvaluatorContext(options))
        {
            var property = new Property
            {
                UserId = "1",
                Address = "123 Main Test Street",
                City = "London",
                State = "TX",
                ZipCode = "12345",
                PropertyType = PropertyTypes.Condo,
                PropertyConditions = PropertyConditions.Bad
            };
            
            context.Properties.Add(property);
            await context.SaveChangesAsync();
        }

        await using (var context = new DealEvaluatorContext(options))
        {
            var savedProperty = context.Properties.SingleOrDefault(p => p.Address == "123 Main Test Street");
            Assert.NotNull(savedProperty);
            Assert.Equal("123 Main Test Street", savedProperty.Address);
            context.Properties.Remove(savedProperty);
            await context.SaveChangesAsync();
        }
    }
    
    // Test Read operation
    [Fact]
    public async Task ReadProperty_ShouldReturnProperty()
    {
        var options = GetInMemoryDbContextOptions();
        await using (var context = new DealEvaluatorContext(options))
        {
            var property = new Property
            {
                UserId = "1",
                Address = "123 Main Test Street",
                City = "London",
                State = "TX",
                ZipCode = "12345",
                PropertyType = PropertyTypes.Condo,
                PropertyConditions = PropertyConditions.Bad
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
        }

        await using (var context = new DealEvaluatorContext(options))
        {
            var property = await context.Properties.FirstOrDefaultAsync(p => p.Address == "123 Main Test Street");

            Assert.NotNull(property);
            Assert.Equal("123 Main Test Street", property.Address);
            
            context.Properties.Remove(property);
            await context.SaveChangesAsync();
        }
    }

    // Test Update operation
    [Fact]
    public async Task UpdateProperty_ShouldModifyProperty()
    {
        var options = GetInMemoryDbContextOptions();
        await using (var context = new DealEvaluatorContext(options))
        {
            var property = new Property
            {
                UserId = "1",
                Address = "123 Main Test Street",
                City = "London",
                State = "TX",
                ZipCode = "12345",
                PropertyType = PropertyTypes.Condo,
                PropertyConditions = PropertyConditions.Bad
            };
            
            context.Properties.Add(property);
            await context.SaveChangesAsync();
        }

        await using (var context = new DealEvaluatorContext(options))
        {
            var property = await context.Properties.FirstOrDefaultAsync(p => p.Address == "123 Main Test Street");
            property.Sqft = 1900;
            await context.SaveChangesAsync();
        }

        await using (var context = new DealEvaluatorContext(options))
        {
            var updatedProperty = await context.Properties.FirstOrDefaultAsync(p => p.Address == "123 Main Test Street");
            Assert.NotNull(updatedProperty);
            Assert.Equal(1900, updatedProperty.Sqft);
            
            context.Properties.Remove(updatedProperty);
            await context.SaveChangesAsync();
        }
    }

    // Test Delete operation
    [Fact]
    public async Task DeleteProperty_ShouldRemoveProperty()
    {
        var options = GetInMemoryDbContextOptions();
        await using (var context = new DealEvaluatorContext(options))
        {
            var property = new Property
            {
                UserId = "1",
                Address = "123 Main Test Street",
                City = "London",
                State = "TX",
                ZipCode = "12345",
                PropertyType = PropertyTypes.Condo,
                PropertyConditions = PropertyConditions.Bad
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();
        }

        await using (var context = new DealEvaluatorContext(options))
        {
            var property = await context.Properties.FirstOrDefaultAsync(p => p.Address == "123 Main Test Street");
            context.Properties.Remove(property);
            await context.SaveChangesAsync();
        }

        await using (var context = new DealEvaluatorContext(options))
        {
            var deletedProperty = await context.Properties.FirstOrDefaultAsync(p => p.Address == "123 Main Test Street");
            Assert.Null(deletedProperty); 
        }
    }
}