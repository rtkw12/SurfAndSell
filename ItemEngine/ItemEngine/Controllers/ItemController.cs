using Common.Util;
using ItemEngine.Models;
using Microsoft.AspNetCore.Mvc;

namespace ItemEngine.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetItems(CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var session = await _itemService.GetSessionAsync();

            var items = await _itemService.GetItems(session, cancellationToken);

            return Ok(new PaginatedViewResult<Item>(items.Cursor.Current.Select(x => new Item(x)), items.Pagination));
        }

        [HttpGet]
        [Route("{storeId}")]
        public async Task<IActionResult> GetItemsByStore(string storeId, CancellationToken cancellationToken, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var session = await _itemService.GetSessionAsync();

            var items = await _itemService.GetItemsByStore(session, storeId, cancellationToken);

            return Ok(new PaginatedViewResult<Item>(items.Cursor.Current.Select(x => new Item(x)), items.Pagination));
        }

        [HttpGet]
        [Route("{storeId}/{id}")]
        public async Task<IActionResult> GetItemById(string storeId, string id, CancellationToken cancellationToken)
        {
            var session = await _itemService.GetSessionAsync();

            var result = await _itemService.TryGetItem(session, storeId, id, cancellationToken);

            return result.Success
                ? Ok(new Item(result.Value))
                : NotFound(result.ErrorMessage);
        }

        [HttpPost]
        [Route("{storeId}/add")]
        public async Task<IActionResult> AddItem(string storeId, ItemInput input, CancellationToken cancellationToken)
        {
            var session = await _itemService.GetSessionAsync();

            var result = await _itemService.TryAddItem(session, input, cancellationToken);

            return result.Success
                ? Ok(new Item(result.Value))
                : NotFound(result.ErrorMessage);
        }

        [HttpPut]
        [Route("{storeId}/{id}/quantity")]
        public async Task<IActionResult> UpdateItemQuantity(string storeId, string id, ItemQuantityUpdate update, CancellationToken cancellationToken)
        {
            var session = await _itemService.GetSessionAsync();

            var result = await _itemService.TryAddQuantity(session, storeId, id, update.Quantity, cancellationToken);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Route("{storeId}/{id}/update")]
        public async Task<IActionResult> UpdateItem(string storeId, string id, ItemUpdate update, CancellationToken cancellationToken)
        {
            var session = await _itemService.GetSessionAsync();

            var result = await _itemService.TryUpdateItem(session, storeId, id, cancellationToken, update.Name, update.Description);

            return result.Success
                ? Ok()
                : BadRequest(result.ErrorMessage);
        }

        [HttpDelete]
        [Route("{storeId}/{id}/delete")]
        public async Task<IActionResult> DeleteItem(string storeId, string id, CancellationToken cancellationToken)
        {
            var session = await _itemService.GetSessionAsync();

            var result = await _itemService.TryRemoveItem(session, storeId, id, cancellationToken);

            return result.Success
                ? Ok()
                : NotFound(result.ErrorMessage);
        }
    }
}
