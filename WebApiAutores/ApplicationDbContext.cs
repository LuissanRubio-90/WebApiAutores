using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        //Creando llaves compuestas para AutorID y LibroID
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //Importante: si no esta base.OnModelCreting, IdentityDbContext no va a funcionar
            modelBuilder.Entity<AutoresLibros>().HasKey(al => new {al.AutorId, al.LibroId});
        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutoresLibros> AutoresLibros { get; set; }
    }
}
