    using Microsoft.EntityFrameworkCore;
    using SATCFDITEST.Models.Entity;

namespace SATCFDITEST.Data
{
    public class MyAppDbContext : DbContext
    {

        public DbSet<Complemento> Complemento { get; set; }
        public DbSet<SolicitudArhivo> SolicitudArhivo { get; set; }

        public DbSet<Verificacion> Verificacion { get; set; }

        public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options)
        {
         
        }

    }
}
