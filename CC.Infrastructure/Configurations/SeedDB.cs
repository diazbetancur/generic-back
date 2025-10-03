namespace CC.Infrastructure.Configurations;

public class SeedDB
{
  private readonly DBContext _context;
  public SeedDB(DBContext context)
  {
    _context = context;

  }

  public async Task SeedAsync()
  {
    await _context.Database.EnsureCreatedAsync();

  }
}