using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Auth.Commands.Register
{
    public class RegisterCommandHandler(IAppDbContext context, IPasswordHasher<Users> passwordHasher) :IRequestHandler<RegisterCommand,int>
    {
        public async Task<int>Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email?.Trim().ToLower();
            var normalizedUsername = request.UserName?.Trim();

            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
            if (userRole == null)
                throw new Exception("Role 'User' not found in database. Seed it first.");
            
            if (await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
                throw new Exception("Email already exists.");

            if (await context.Users.AnyAsync(u => u.UserName == normalizedUsername, cancellationToken))
                throw new Exception("Username already exists.");

            var user = new Users
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Adress = request.Address,
                City = request.City,
                Email = normalizedEmail,
                UserName = normalizedUsername,
                PhoneNumber = request.PhoneNumber,
                Password=request.Password,
                RoleId=2,
                IsEnabled = true,
                TokenVersion = 0
            };

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
