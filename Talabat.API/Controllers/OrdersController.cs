﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Talabat.API.Dtos;
using Talabat.API.Errors;
using Talabat.BLL.Interfaces;
using Talabat.DAL.Entities.Order_Aggregate;

namespace Talabat.API.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService , IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto )
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email); 

            var orderAddress = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);

            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, orderAddress);

            if (order == null)  
                return BadRequest(new ApiResponse(400, "An Error Occured During Creating The Order "));
            return Ok(order); 
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser( )
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _orderService.GetOrdersForUserAsync(buyerEmail);

            return Ok( _mapper.Map<IReadOnlyList<Order> , IReadOnlyList< OrderToReturnDto>>(orders)); 
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderForUser(int orderId )
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var order = await _orderService.GetOrderByIdForUserAsync(orderId, buyerEmail);

            return Ok(_mapper.Map<Order, OrderToReturnDto> (order));
        }

        [HttpGet("deliveryMethods")] 
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            return Ok(await _orderService.GetDeliveryMethodsAsync());
        }

    }
}
