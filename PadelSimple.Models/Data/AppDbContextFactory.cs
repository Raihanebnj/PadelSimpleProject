using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PadelSimple.Models.Data;

namespace PadelSimple.Models;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=padelsimple.web.db")
            .Options;

        return new AppDbContext(options);
    }
}
