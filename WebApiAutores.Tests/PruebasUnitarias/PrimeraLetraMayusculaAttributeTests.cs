
using System.ComponentModel.DataAnnotations;

using WebApiAutores.Validations;

namespace WebApiAutores.Tests.PruebasUnitarias
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributeTests
    {
        [TestMethod]
        public void PrimeraLetraMinuscula_DevuelveError()
        {
            //Preparacion
            var primeraLetraMayuscula = new PrimeraLetraMayusAttribute();
            var valor = "luis";
            var valContext = new ValidationContext(new {Nombre = valor});

            //Ejecucion
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);

            //Validacion
            Assert.AreEqual("La primera letra debe ser mayuscula", resultado.ErrorMessage);
        }

        [TestMethod]
        public void ValorNulo_NoDevuelveError()
        {
            //Preparacion
            var primeraLetraMayuscula = new PrimeraLetraMayusAttribute();
            string valor = null;
            var valContext = new ValidationContext(new { Nombre = valor });

            //Ejecucion
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);

            //Validacion
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public void PrimeraLetraMinuscula_NoDevuelveError()
        {
            //Preparacion
            var primeraLetraMayuscula = new PrimeraLetraMayusAttribute();
            string valor = "Luis";
            var valContext = new ValidationContext(new { Nombre = valor });

            //Ejecucion
            var resultado = primeraLetraMayuscula.GetValidationResult(valor, valContext);

            //Validacion
            Assert.IsNull(resultado);
        }
    }
}
