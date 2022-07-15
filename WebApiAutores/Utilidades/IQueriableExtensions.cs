using WebApiAutores.DTOs;

namespace WebApiAutores.Utilidades
{
    public static class IQueriableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
        {
            return queryable.Skip((paginacionDTO.pagina - 1) * paginacionDTO.recordsPorPagina).Take(paginacionDTO.recordsPorPagina);
        }
    }
}
