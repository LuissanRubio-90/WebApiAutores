using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validations;

namespace WebApiAutores.DTOs
{
    public class LibroCreacionDTO
    {
        [PrimeraLetraMayus]
        [StringLength(maximumLength: 250)]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public List<int> AutoresId { get; set; }
    }
}
