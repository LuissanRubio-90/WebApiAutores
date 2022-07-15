using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuracion;
        private readonly SignInManager<IdentityUser> signinManager;
        private readonly HashServices hashService;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager,
                                 IConfiguration configuracion,
                                 SignInManager<IdentityUser> signinManager,
                                 IDataProtectionProvider dataProtectionProvider,
                                 HashServices hashService)
        {
            this.userManager = userManager;
            this.configuracion = configuracion;
            this.signinManager = signinManager;
            this.hashService = hashService;
            dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_quizas_secreto");
        }

        [HttpGet("hash/{textoPlano}")]
        public ActionResult RealizarHash(string textoPlano)
        {
            var resultado1 = hashService.Hash(textoPlano);
            var resultado2 = hashService.Hash(textoPlano);

            return Ok(new
            {
                textoPlano,
                Hash1 = resultado1,
                Hash2 = resultado2
            });
        }


        [HttpGet("encriptar")]
        public ActionResult Encriptar()
        {
            var textoPlano = "Luis Rubio";
            var textoCifrado = dataProtector.Protect(textoPlano);
            var textoDesencriptado = dataProtector.Unprotect(textoCifrado);
            return Ok(new
            {
                textoPlano,
                textoCifrado,
                textoDesencriptado
            });
        }


        [HttpGet("encriptarPorTiempo")]
        public ActionResult EncriptarPorTiempo()
        {
            var protectorLimitadoPorTiempo = dataProtector.ToTimeLimitedDataProtector();
            var textoPlano = "Luis Rubio";
            var textoCifrado = protectorLimitadoPorTiempo.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(6000);
            var textoDesencriptado = dataProtector.Unprotect(textoCifrado);
            return Ok(new
            {
                textoPlano,
                textoCifrado,
                textoDesencriptado
            });
        }

        //Endpoint para registro
        [HttpPost("registrar", Name = "registrarUsuario")] //api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(Credenciales credencialesUsuario)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        //Endpoint para login
        [HttpPost("login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(Credenciales credencialesUsuario)
        {
            var resultado = await signinManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("login incorrecto");
            }
        }

        //Endpoint para renovar token
        [HttpGet("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new Credenciales()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }

        //Construccion de token
        private async Task<RespuestaAutenticacion> ConstruirToken(Credenciales credencialesUsuario)
        {
            var claims = new List<Claim>() //creando variable de tipo List<Claim>
            {
                new Claim("email", credencialesUsuario.Email), //Valor del DTO Credenciales
                new Claim("Lo que yo quiera", "cualquier otro valor")
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracion["llavejwt"])); //Variable para obtener el token
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256); //Variable de credenciales
            var expiracion = DateTime.UtcNow.AddMinutes(30); //Variable de fecha de expiracion
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds); //Variable en el que se obtiene el token de seguridad

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }


        [HttpPost("hacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        [HttpPost("removerAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }
    }
}
