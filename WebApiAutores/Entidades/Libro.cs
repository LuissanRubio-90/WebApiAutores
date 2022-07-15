using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validations;

namespace WebApiAutores.Entidades
{
    public class Libro
    {
        public int Id { get; set; }

        [Required]
        [PrimeraLetraMayus]
        [StringLength(maximumLength: 250)]
        public string Titulo { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public List<Comentario> Comentarios { get; set; }
        public List<AutoresLibros> AutoresLibros { get; set; }

    }
}
