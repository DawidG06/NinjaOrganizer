using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
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
    /// <summary>
    /// This class is responsible for users manipulating.
    /// Authorized access excluding user registration or authentication.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly INinjaOrganizerRepository _ninjaOrganizerRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UserController> _logger;

        public UserController(INinjaOrganizerRepository ninjaOrganizerRepository, IMapper mapper, IUserService userService, IOptions<AppSettings> appSettings, ILogger<UserController> logger)
        {
            _ninjaOrganizerRepository = ninjaOrganizerRepository ??
               throw new ArgumentNullException(nameof(ninjaOrganizerRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _appSettings = appSettings.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Register new user.
        /// </summary>
        /// <param name="userForRegisterDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserForRegisterDto userForRegisterDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_ninjaOrganizerRepository.UserExist(userForRegisterDto.Username))
                return BadRequest("Username exists");

            if (userForRegisterDto.Username == userForRegisterDto.Password)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided username should be different from the password.");
            }

            // map model to entity
            var user = _mapper.Map<User>(userForRegisterDto);
            var userToReturn = _mapper.Map<UserDto>(user);

            try
            {
                _userService.Create(user, userForRegisterDto.Password);
                // redirecting to Get user
                return CreatedAtRoute("GetUser",
                    new { id = userToReturn.Id }, userToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exeption while user register.",ex);
                return BadRequest(new { message = ex.Message });
            }

        }

        /// <summary>
        /// User authenticate.
        /// </summary>
        /// <param name="userForAuth"></param>
        /// <returns>Return basic user info and authentication token.</returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserForAuthenticateDto userForAuth)
        {
            var user = _userService.Authenticate(userForAuth.Username, userForAuth.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            // generate token for authorization
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var expiresDate = token.ValidTo;

            // map model to entity
            var userToReturn = _mapper.Map<UserDto>(user);

            // return basic user info and authentication token
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

        
        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            foreach(var singleUser in users)
            {
                singleUser.Taskboards = _ninjaOrganizerRepository.GetTaskboardsForUser(singleUser.Id).ToList();
            }

            // map model to entity
            var userDto = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDto);
        }

        /// <summary>
        /// Get specific user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            user.Taskboards = _ninjaOrganizerRepository.GetTaskboardsForUser(user.Id).ToList();

            // map model to entity
            var userToReturn = _mapper.Map<UserDto>(user);
            return Ok(userToReturn);
        }

        /// <summary>
        /// Update specific user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userForUpdate"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UserForUpdateDto userForUpdate)
        {
            // map model to entity
            var user = _mapper.Map<User>(userForUpdate);
            user.Id = id;

            try
            {
                _userService.Update(user, userForUpdate.Password);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exception while user update.", ex);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete specific user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return NoContent();
        }

        // for google authentication
        //[AllowAnonymous]
        //public IActionResult GoogleLogin()
        //{
          
        //}

        //[AllowAnonymous]
        //public Task<IActionResult> GoogleResponse()
        //{
           
        //}

    }

}

