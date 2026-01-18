using FoodEcommerceWebAPI.Data;
using FoodEcommerceWebAPI.Models.DTOs;
using FoodEcommerceWebAPI.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodEcommerceWebAPI.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public UsersController(ApplicationDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }
       
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await dbContext.Users
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found");
            }

            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await dbContext.Users
                .Where(u => u.UserId == id)
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO newUser)
        {
            if (string.IsNullOrEmpty(newUser.UserName) || string.IsNullOrEmpty(newUser.PhoneNumber) ||  string.IsNullOrEmpty(newUser.PasswordHash))
            {
                return BadRequest("Invalid user data: Username, Phone, and Password are required.");
            }

            var userEntity = new UserEntity
            {
                UserName = newUser.UserName,
                PhoneNumber = newUser.PhoneNumber,
                Email = newUser.Email,
                PasswordHash = newUser.PasswordHash
            };

            dbContext.Users.Add(userEntity);
            await dbContext.SaveChangesAsync();

            return Ok(newUser);

        }
    }
}
