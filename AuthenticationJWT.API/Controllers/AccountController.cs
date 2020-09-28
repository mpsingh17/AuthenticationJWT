using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationJWT.Core.DTOs;
using AuthenticationJWT.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationJWT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public AccountController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto input)
        {
            // TODO: validate input object coming from client and send appropriate response.

            var user = new User
            {
                FirstName = input.FirstName, // Custom property
                LastName = input.LastName, // Custom property
                UserName = input.UserName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            // TODO: validate role exists. If not then add user to default role.

            await _userManager.AddToRolesAsync(user, input.Roles);

            return StatusCode(201);
        }
    }
}