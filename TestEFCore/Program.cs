#define SqlServer 
//#define PostgreSQL 
//#define InMemory 

#if SqlServer && PostgreSQL || SqlServer && InMemory || PostgreSQL && InMemory
#error "Only one database provider can be defined"
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

var dbContext = new MyDbContext();

var query = dbContext.Persons
    .Include(x => x.Addresses);
Console.WriteLine(query.ToQueryString());

class MyDbContext : DbContext
{
    public MyDbContext() { }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if SqlServer && !PostgreSQL && !InMemory
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=test;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true");
#endif
#if PostgreSQL && !SqlServer && !InMemory
        optionsBuilder.UseNpgsql("User ID=test;Password=passw0rd%01;Host=localhost;Port=5434;Database=rps2.db.billing");
#endif
#if InMemory && !SqlServer && !PostgreSQL
        optionsBuilder.UseInMemoryDatabase("Test");
#endif
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AddressEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Address> Adresses { get; set; } = null!;

}

class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
}

class Address
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;
}

class PersonEntityConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");
        builder.HasKey(x=> x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Addresses)
            .WithOne(x => x.Person)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
            ;
    }
}
class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}