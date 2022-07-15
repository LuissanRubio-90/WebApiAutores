using WebApiAutores;

var builder = WebApplication.CreateBuilder(args);

//Creando instancia de la clase Startup para construir los metodos ConfigureServices y Configure
var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);


var app = builder.Build();
var servicioLogger = (ILogger<Startup>)app.Services.GetService(typeof(ILogger<Startup>));   
startup.Configure(app, app.Environment, servicioLogger);


app.Run();
