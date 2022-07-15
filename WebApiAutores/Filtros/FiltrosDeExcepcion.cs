using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filtros
{
    public class FiltrosDeExcepcion: ExceptionFilterAttribute
    {
        private readonly ILogger<FiltrosDeExcepcion> logger;

        public FiltrosDeExcepcion(ILogger<FiltrosDeExcepcion> logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);
            base.OnException(context);
        }
    }
}
