﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Talabat.API.Dtos;
using Talabat.API.Errors;
using Talabat.API.Extensions;
using Talabat.BLL.Interfaces;
using Talabat.DAL.Entities.Identity;

namespace Talabat.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager ,SignInManager<AppUser> signInManager , 
            ITokenService tokenService , IMapper mapper )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }


        [HttpPost("login")] 
        public async Task<ActionResult<UserDto>> Login(LoginDto  loginDto )
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user == null)
                return Unauthorized(new ApiResponse(401));
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password , false); 
            if (!result.Succeeded) 
                return Unauthorized(new ApiResponse(401)); 
            return Ok( new UserDto()  // manual map 
            {
                DisplayName = user.DisplayName ,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user ,_userManager)
            });
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto  registerDto)
        {

            if ( CheckEmailExists(registerDto.Email).Result.Value) // Result as block next code till check
                return BadRequest(new ApiValidationErrorResponse() { Errors = new[] { "This Email Is Already In use" } });

            var user = new AppUser() 
            {
                DisplayName = registerDto.DisplayName,
                UserName = registerDto.Email.Split("@")[0],
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = new Address()
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Country = registerDto.Country,
                    City = registerDto.City,
                    Street = registerDto.Street,
                },
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded)
                return BadRequest(new ApiResponse(400));
            return Ok(new UserDto()  // manual map 
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user ,_userManager)
              
            });
        }
        [Authorize] 
        [HttpGet]    
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user  =await _userManager.FindByEmailAsync(email); 

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user, _userManager) 
            });
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            
            var user = await _userManager.FindUserWithAddressByEmailAsync(User);

            return Ok(_mapper.Map<Address, AddressDto>(user.Address)); // map using AutoMapper  
        }
        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto newAddress)
        {
            var user = await _userManager.FindUserWithAddressByEmailAsync(User);
            user.Address = _mapper.Map<AddressDto, Address>(newAddress);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiValidationErrorResponse() { Errors= new[] { "an Error Occured With updating User Address" } });
            return Ok(newAddress);
        }
        [HttpGet("emailexists")] 
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery]string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }


    }
}
