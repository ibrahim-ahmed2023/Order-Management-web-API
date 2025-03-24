using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.ServiceContracts;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemsGetterService _orderItemsGetterService;
        private readonly IOrderItemsAdderService _orderItemsAdderService;
        private readonly IOrderItemsUpdaterService _orderItemsUpdaterService;
        private readonly IOrderItemsDeleterService _orderItemsDeleterService;
        private readonly ILogger<OrderItemsController> _logger;

        public OrderItemsController(
            IOrderItemsGetterService orderItemsGetterService,
            IOrderItemsAdderService orderItemsAdderService,
            IOrderItemsUpdaterService orderItemsUpdaterService,
            IOrderItemsDeleterService orderItemsDeleterService,
            ILogger<OrderItemsController> logger)
        {
            _orderItemsGetterService = orderItemsGetterService;
            _orderItemsAdderService = orderItemsAdderService;
            _orderItemsUpdaterService = orderItemsUpdaterService;
            _orderItemsDeleterService = orderItemsDeleterService;
            _logger = logger;
        }

        private void LogInformation(string message, Guid? id = null)
        {
            _logger.LogInformation(id.HasValue ? $"{message} ID: {id}" : message);
        }

        /// <summary>
        /// Retrieves all order items for a specific order.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<OrderItemResponse>>> GetOrderItemsByOrderId(Guid orderId)
        {
            LogInformation("Retrieving order items for Order", orderId);
            var orderItems = await _orderItemsGetterService.GetOrderItemsByOrderId(orderId);
            LogInformation("Order items retrieved successfully for Order", orderId);
            return Ok(orderItems);
        }

        /// <summary>
        /// Retrieves an order item by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemResponse?>> GetOrderItemById(Guid id)
        {
            LogInformation("Retrieving order item by Order Item", id);
            var orderItem = await _orderItemsGetterService.GetOrderItemByOrderItemId(id);
            if (orderItem is null)
            {
                _logger.LogWarning($"Order item not found for Order Item ID: {id}.");
                return NotFound();
            }
            LogInformation("Order item retrieved successfully. Order Item", id);
            return Ok(orderItem);
        }

        /// <summary>
        /// Adds a new order item.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderItemResponse>> AddOrderItem(Guid orderId, OrderItemAddRequest orderItemRequest)
        {
            LogInformation("Adding order item for Order", orderId);
            var addedOrderItem = await _orderItemsAdderService.AddOrderItem(orderItemRequest);
            LogInformation("Order item added successfully. Order Item", addedOrderItem.OrderItemId);
            return CreatedAtAction(nameof(GetOrderItemById), new { id = addedOrderItem.OrderItemId }, addedOrderItem);
        }

        /// <summary>
        /// Updates an existing order item.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderItemResponse>> UpdateOrderItem(Guid id, OrderItemUpdateRequest orderItemRequest)
        {
            if (id != orderItemRequest.OrderItemId)
            {
                _logger.LogWarning($"Invalid Order Item ID in the request: {orderItemRequest.OrderItemId}.");
                return BadRequest();
            }
            LogInformation("Updating order item. Order Item", id);
            var updatedOrderItem = await _orderItemsUpdaterService.UpdateOrderItem(orderItemRequest);
            LogInformation("Order item updated successfully. Order Item", updatedOrderItem.OrderItemId);
            return Ok(updatedOrderItem);
        }

        /// <summary>
        /// Deletes an order item.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderItem(Guid orderId, Guid id)
        {
            LogInformation("Deleting order item. Order Item", id);
            var isDeleted = await _orderItemsDeleterService.DeleteOrderItemByOrderItemId(id);
            if (!isDeleted)
            {
                _logger.LogWarning($"Order item not found for deletion. Order Item ID: {id}.");
                return NotFound();
            }
            LogInformation("Order item deleted successfully. Order Item", id);
            return NoContent();
        }
    }
}
