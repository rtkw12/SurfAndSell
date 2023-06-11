using Common.Util;
using Microsoft.AspNetCore.Mvc;
using UserEngine.Models;

namespace UserEngine.Controllers
{
    [ApiController]
    [Route("stores")]
    public class StoreController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IUserService _userService;

        public StoreController(IStoreService storeService, IUserService userService)
        {
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetStores(CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            using var session = await _storeService.GetSessionAsync();
            var result = await _storeService.GetStores(session, cancellationToken);

            if (result.Cursor.Current == null)
            {
                return NotFound();
            }

            return Ok(
                new PaginatedViewResult<Store>(result.Cursor.Current.Select(x => new Store(x)), result.Pagination));
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterStore(StoreInput input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _storeService.GetSessionAsync();
            var result = await _storeService.TryAddStore(session, input, cancellationToken);

            return result.Success
                ? Ok(new Store(result.Value))
                : BadRequest(result.ErrorMessage);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetStore(string id, CancellationToken cancellationToken)
        {
            using var session = await _storeService.GetSessionAsync();
            var result = await _storeService.TryGetStoreById(session, id, cancellationToken);

            return result.Success
                ? Ok(new Store(result.Value))
                : BadRequest(result.ErrorMessage);
        }

        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetStoreByUser(string id, CancellationToken cancellationToken)
        {
            using var session = await _storeService.GetSessionAsync();
            var result = await _storeService.TryGetStoreById(session, id, cancellationToken);

            return result.Success
                ? Ok(new Store(result.Value))
                : BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Route("user/{userId}/update/{id}")]
        public async Task<IActionResult> UpdateStoreByUser(string userId, string id, StoreUpdate input, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) { return BadRequest("Model given was not valid"); }

            using var session = await _storeService.GetSessionAsync();
            var result = await _storeService.TryUpdateStoreByUser(session, cancellationToken, id, userId, input.Name, input.Description, input.Status ?? StoreStatus.OPEN);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }
    }
}
