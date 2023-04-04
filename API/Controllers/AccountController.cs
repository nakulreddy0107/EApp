using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.entities;
using API.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;


namespace API.Controllers
{
    public class AccountController: BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService=tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName=registerDto.Username.ToLower(),  
                passwordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                passwordSalt=hmac.Key
            };
            _context.MyProperty.Add(user);
            await _context.SaveChangesAsync();

            var mydto= new UserDto
            {
                Username=user.UserName,
                Token = _tokenService.CreateToken(user)
            };
            return mydto;
        }

        

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.MyProperty.SingleOrDefaultAsync(x =>
            x.UserName==loginDto.Username);

            if(user==null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.passwordSalt);

            var Computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0; i<Computedhash.Length;i++)
            {
                if(Computedhash[i] != user.passwordHash[i]) return Unauthorized("Invalid password");
            }

            var mydto= new UserDto
            {
                Username=user.UserName,
                Token = _tokenService.CreateToken(user)
            };
            return mydto;
        }

       

        private async Task<bool> UserExists(string username)
        {
            return await _context.MyProperty.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}