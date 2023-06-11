using Microsoft.AspNetCore.Mvc;
using UserEngine.Models;

namespace UserEngine.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterUser(UserInput input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _userService.GetSessionAsync();
            var result = await _userService.TryAddUser(session, input, cancellationToken);

            return result.Success
                ? Ok(new User(result.Value))
                : BadRequest(result.ErrorMessage);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUser(string id, CancellationToken cancellationToken)
        {
            using var session = await _userService.GetSessionAsync();
            var result = await _userService.TryGetUser(session, id, cancellationToken);

            return result.Success
                ? Ok(new User(result.Value))
                : BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Route("{id}/update")]
        public async Task<IActionResult> UpdateUser(string id, UserUpdate input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _userService.GetSessionAsync();
            var result = await _userService.TryUpdateUser(session, cancellationToken, id, input.Email, input.Password, input.Password);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Route("{id}/update/owner")]
        public async Task<IActionResult> UpdateUserStore(string id, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _userService.GetSessionAsync();
            var result = await _userService.TryUpdateUser(session, cancellationToken, id, type: UserType.STORE_OWNER);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Route("{id}/update/admin")]
        public async Task<IActionResult> UpdateUserAdmin(string id, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _userService.GetSessionAsync();
            var result = await _userService.TryUpdateUser(session, cancellationToken, id, type: UserType.ADMIN);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginInput input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _userService.GetSessionAsync();
            var result = await _userService.Login(session, input.Email, input.Password, cancellationToken);

            return result.Success
                ? Ok(new User(result.Value))
                : BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Route("{id}/logout")]
        public async Task<IActionResult> Logout(string id, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _userService.GetSessionAsync();
            var result = await _userService.Logout(session, id, cancellationToken);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }
    }
}
