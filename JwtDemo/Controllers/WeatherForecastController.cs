using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtDemo.Controllers
{
        [ApiController]
        [Route("[controller]")]
        public class WeatherForecastController : ControllerBase
        {
                private static readonly string[] Summaries = new[]
                {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

                private readonly ILogger<WeatherForecastController> _logger;
                private readonly IConfiguration _configuration;

                public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
                {
                        _logger = logger;
                        this._configuration = configuration;
                }

                [Authorize]
                [HttpGet(Name = "GetWeatherForecast")]
                public IEnumerable<WeatherForecast> Get()
                {
                        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                        {
                                Date = DateTime.Now.AddDays(index),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                        })
                        .ToArray();
                }

                [HttpPost("login")]
                public async Task<ActionResult<bool>> Login([FromBody] LoginModel loginModel)
                {
                        if (string.IsNullOrEmpty(loginModel.Username) || string.IsNullOrEmpty(loginModel.Password))
                        {
                                return BadRequest();
                        }
                        if (!loginModel.Username.Equals("admin") && !loginModel.Password.Equals("admin"))
                        {
                                return Unauthorized("Invalid username/password");
                        }
                        string token = CreateToken(loginModel.Username);
                        return Ok(token);
                }

                private string CreateToken(string userName)
                {
                        List<Claim> claims = new List<Claim>
                        {
                                new Claim(ClaimTypes.Name, userName),
                                new Claim(ClaimTypes.Role, "Admin")
                        };

                        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SecretKey").Value));

                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                        var token = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now.AddDays(1),
                            signingCredentials: creds);

                        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                        return jwt;
                }
        }

        public class LoginModel
        {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
