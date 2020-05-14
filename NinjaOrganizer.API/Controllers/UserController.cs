using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NinjaOrganizer.API.Entities;
using NinjaOrganizer.API.Helpers;
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
   // [Route("api/users")]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly INinjaOrganizerRepository _ninjaOrganizerRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;

        public UserController(INinjaOrganizerRepository ninjaOrganizerRepository, IMapper mapper, IUserService userService, IOptions<AppSettings> appSettings)
        {
            _ninjaOrganizerRepository = ninjaOrganizerRepository ??
               throw new ArgumentNullException(nameof(ninjaOrganizerRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserForAuthenticateDto userForAuth)
        {
            var user = _userService.Authenticate(userForAuth.Username, userForAuth.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7), //TODO sprawdzic czy dziala i zwalidowac
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var expiresDate = token.ValidTo;

            // return basic user info and authentication token
            var userToReturn = _mapper.Map<UserDto>(user);

            return Ok(new
            {
                Id = userToReturn.Id,
                FirstName = userToReturn.FirstName,
                LastName = userToReturn.LastName,
                UserName = userToReturn.Username,
                NumberOfTaskboards = userToReturn.NumberOfTaskboards,
                Taskboards = userToReturn.Taskboards,
                Token = tokenString,
                Expires = expiresDate
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserForRegisterDto userForRegisterDto)
        {
            var user = _mapper.Map<User>(userForRegisterDto);

            var userToReturn = _mapper.Map<UserDto>(user);

            try
            {
                _userService.Create(user, userForRegisterDto.Password);
                return CreatedAtRoute("GetUser",
                    new { id = userToReturn.Id }, userToReturn);
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
            foreach(var singleUser in users)
                singleUser.Taskboards = _ninjaOrganizerRepository.GetTaskboardsForUser(singleUser.Id).ToList();

            var userDto = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDto);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            user.Taskboards = _ninjaOrganizerRepository.GetTaskboardsForUser(user.Id).ToList();
            var model = _mapper.Map<UserDto>(user);
            return Ok(model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UserForUpdateDto userForUpdate)
        {
            // map model to entity and set id
            var user = _mapper.Map<User>(userForUpdate);
            user.Id = id;

            try
            {
                // update user 
                _userService.Update(user, userForUpdate.Password);
                return NoContent();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return NoContent();
        }

    }
}
