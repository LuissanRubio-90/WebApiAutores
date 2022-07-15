using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validations;

namespace WebApiAutores.DTOs
{
    public class AutorCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe de tener mas de {1} caracteres")]
        [PrimeraLetraMayus]
        public string Nombre { get; set; }
    }
}
