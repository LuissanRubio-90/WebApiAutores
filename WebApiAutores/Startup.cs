using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenXmlPowerTools;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using WebApiAutores.Filtros;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

[assembly: ApiConventionType(typeof(DefaultApiConventions))] //Documentacion de responses
namespace WebApiAutores
{
    public class Startup
    {
        
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //Limpiando el mapeo de los tipos de los claim
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //Creando metodo para los servicios
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltrosDeExcepcion)); // Registrando filtro de excepcion
                opciones.Conventions.Add(new SwaggerAgrupaPorVersion()); //Agrupando versiones
            }).AddJsonOptions(x => //Resolviendo problema de loop infinito entre libros y autores
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

            //Obteniendo el connection string de appsettings
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddResponseCaching();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters //Validacion de los tokens para inicio de sesion
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "WebAPIAutores", 
                    Version = "v1",
                    Description = "Este es un web api para trabajar con autores y libros",
                    Contact = new OpenApiContact
                    {
                        Email = "luisrubio.dev90@gmail.com",
                        Name = "Luis Enrique Rubio Ortiz"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    }
                });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });
                c.OperationFilter<AgregarParametroXVersion>();

                c.OperationFilter<AgregarParametroHATEOAS>();

                //Configuracion para obtener el token de autorizacion
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);

                c.IncludeXmlComments(rutaXML);
            });

            services.AddAutoMapper(typeof(Startup));

            //Servicios para Identity
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Publicando autorizacion basado en claims
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("esAdmin", politica => politica.RequireClaim("esAdmin"));
                opciones.AddPolicy("esVendedor", politica => politica.RequireClaim("esVendedor"));
            });

            //Publicando servicio de proteccion de datos
            services.AddDataProtection();

            //Registrando servicios de encriptacion
            services.AddTransient<HashServices>();

            //Publicando servicio CORS
            services.AddCors(opciones => 
            {
               opciones.AddDefaultPolicy(builder =>
               {
                   builder.WithOrigins("http://apirequest.io").AllowAnyMethod().AllowAnyHeader().
                   WithExposedHeaders(new string[] {"cantidadTotalRegistros"});
               });
            });

            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //Publicacion de Application Insights para azure
            // services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:ConnectionString"]);

            /*
              NOTA: Error en la publicacion debido a Application Insight en appsettings.json:
            "ApplicationInsights": {
               "ConnectionString": null,
               "InstrumentationKey=9ff06179-eee0-4267-95ba-5047a5500a15;IngestionEndpoint=https": //eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/
  }
             */
        }

        //Creando metodo para Middlewares
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {     
            if (env.IsDevelopment())
            {
               
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "webApiAutores v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "webApiAutores v2");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
