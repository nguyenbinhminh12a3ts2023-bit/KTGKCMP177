using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace aspnetboilerplate.com.EntityFrameworkCore;

public static class comDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<comDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString)
               .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public static void Configure(DbContextOptionsBuilder<comDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection)
               .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }
}
