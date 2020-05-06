using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.IdentityModel.Tokens;
using NinjaOrganizer.API.Entities;
using NinjaOrganizer.API.Models;
using NinjaOrganizer.API.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NinjaOrganizer.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly INinjaOrganizerRepository _ninjaOrganizerRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private string secret = ""; //TODO moze bd trzeba przeniesc do klasy

        public UserController(INinjaOrganizerRepository ninjaOrganizerRepository, IMapper mapper, IUserService userService)
        {
            _ninjaOrganizerRepository = ninjaOrganizerRepository ??
               throw new ArgumentNullException(nameof(ninjaOrganizerRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserForAuthenticateDto userForAuth)
        {
            var user = _userService.Authenticate(userForAuth.Email, userForAuth.Password);

            if (user == null)
                return BadRequest(new { message = "Email or password is incorrect." });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email,userForAuth.Email)
                }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Email = userForAuth.Email,
                Token = tokenString
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserForRegisterDto userForRegisterDto)
        {
            var user = _mapper.Map<User>(userForRegisterDto);

            try
            {
                _userService.Create(user, userForRegisterDto.Password);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var userDto = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDto);
        }

        [HttpGet("{email}")]
        public IActionResult GetById(string email)
        {
            var user = _userService.GetByEmail(email);
            var model = _mapper.Map<UserDto>(user);
            return Ok(model);
        }

        [HttpPut("{email}")]
        public IActionResult Update(string email, [FromBody]UserForUpdateDto userForUpdate)
        {
            // map model to entity and set id
            var user = _mapper.Map<User>(userForUpdate);
            user.Email = email;

            try
            {
                // update user 
                _userService.Update(user, userForUpdate.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{email}")]
        public IActionResult Delete(string email)
        {
            _userService.Delete(email);
            return Ok();
        }

    }
}
