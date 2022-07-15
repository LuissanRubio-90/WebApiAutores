using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Policy;
using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios
{
    public class GeneradorEnlaces
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccesor;
        private readonly IActionContextAccessor actionContextAccesor;

        public GeneradorEnlaces(IAuthorizationService authorizationService,
                                IHttpContextAccessor httpContextAccesor,
                                IActionContextAccessor actionContextAccesor)
        {
            this.authorizationService = authorizationService;
            this.httpContextAccesor = httpContextAccesor;
            this.actionContextAccesor = actionContextAccesor;
        }

        private IUrlHelper ConstruirURLHelper()
        {
            var factoria = httpContextAccesor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();

            return factoria.GetUrlHelper(actionContextAccesor.ActionContext);
        }

        private async Task<bool> EsAdmin()
        {
            var httpContext = httpContextAccesor.HttpContext;
            var resultado = await authorizationService.AuthorizeAsync(httpContext.User, "esAdmin");
            return resultado.Succeeded;
        }
        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var esAdmin = await EsAdmin();
            var Url = ConstruirURLHelper(); 
            autorDTO.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                                                 descripcion: "self",
                                                 metodo: "GET"));
            if (esAdmin)
            {
                autorDTO.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
                         descripcion: "autor-actualizar",
                         metodo: "PUT"));
                autorDTO.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
                                         descripcion: "self",
                                         metodo: "DELETE"));
            }

        }
    }
}
