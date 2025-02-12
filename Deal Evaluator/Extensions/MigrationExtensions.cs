using Deal_Evaluator.Data;
using Microsoft.EntityFrameworkCore;

namespace Deal_Evaluator.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        
        using var context = scope.ServiceProvider.GetRequiredService<DealEvaluatorContext>();
        
        context.Database.Migrate();
    }
}