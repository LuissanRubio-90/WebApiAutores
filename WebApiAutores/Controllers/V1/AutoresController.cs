using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    //[Route("api/v1/[controller]")] // api/autores
    [Route("api/[controller]")]
    [CabeceraEstaPresente("x-version", "1")]
    //[ApiConventionType(typeof(DefaultApiConventions))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "esAdmin")]
    public class AutoresController : ControllerBase
    {
        //Injeccion de dependencias
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "obtenerAutoresv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //Otorgando autenticacion en el endpoint
        public async Task<ActionResult<List<AutorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();

            //Paginacion
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable.OrderBy(autor => autor.Nombre).Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);


            //if (incluirHATEOAS)
            //{
            //    var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");
            //    //dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded));
            //    var resultado = new ColeccionDeRecursos<AutorDTO> { Valores = dtos };
            //    resultado.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutores", new { }),
            //                                          descripcion: "self",
            //                                          metodo: "GET"));

            //    if (esAdmin.Succeeded)
            //    {
            //        resultado.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("crearAutor", new { }),
            //                             descripcion: "crear-autor",
            //                             metodo: "POST"));
            //    }

            //    return Ok(resultado);
            //}


            //return Ok(dtos);
        }

        [HttpGet("{id:int}", Name = "obtenerAutorv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        //[ProducesResponseType(404)]//Documentando respuestas
        //[ProducesResponseType(200)]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id /*[FromHeader] string incluirHATEOAS*/)
        {
            var autor = await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.oLibro)
                .FirstOrDefaultAsync(x => x.Id == id);


            if (autor == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<AutorDTOConLibros>(autor);
            return dto;
        }

        //Generando enlaces


        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev1")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorv1")] //Insertar
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);
            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }
            var autor = mapper.Map<Autor>(autorCreacionDTO);

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutorv1", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv1")] // api/autores/id - Editar
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Borrar un autor
        /// </summary>
        /// <param name="id">Id del autor a borrar</param>
        /// <returns></returns>

        [HttpDelete("{id:int}", Name = "borrarAutorv1")] // api/autores/id - Eliminar
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
