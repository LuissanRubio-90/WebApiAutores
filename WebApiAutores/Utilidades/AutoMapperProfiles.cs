using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //Creando mapeo automatico (<Desde, Hasta>)
            CreateMap<AutorCreacionDTO, Autor>(); // Desde la aplicacion a la base de datos
            CreateMap<Autor, AutorDTO>(); // Desde la base de datos a la aplicacion
            CreateMap<Autor, AutorDTOConLibros>()
                .ForMember(AutorDTO => AutorDTO.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros)); 

            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));
            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOConAutores>()
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
            
        }

        private List<AutoresLibros> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutoresLibros>();
            if (libroCreacionDTO.AutoresId == null)
            {
                return resultado;
            }

            foreach(var autorId in libroCreacionDTO.AutoresId)
            {
                resultado.Add(new AutoresLibros() { AutorId = autorId });
            }

            return resultado;
        }

        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();

            if (libro.AutoresLibros == null)
            {
                return resultado;
            }

            foreach (var autorlibro in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = autorlibro.AutorId,
                    Nombre = autorlibro.oAutor.Nombre
                });
            }

            return resultado;
        }

        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            if (autor.AutoresLibros == null)
            {
                return resultado;
            }

            foreach (var autorLibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO() 
                { 
                    Id=autorLibro.LibroId,
                    Titulo=autorLibro.oLibro.Titulo
                    
                });
            }

            return resultado;
        }


    }
}
