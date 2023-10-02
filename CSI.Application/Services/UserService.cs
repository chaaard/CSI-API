using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Domain.Entities;
using CSI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Diagnostics.CodeAnalysis;

namespace CSI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDBContext _dbContext;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;
        private readonly int saltiness = 70;
        private readonly int nIterations = 10101;

        public UserService(AppDBContext dbContext, IPasswordHashService passwordHashService, IJwtService jwtService)
        {
            _dbContext = dbContext;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;
        }

        public async Task<UserDto> AuthenticateAsync(string username, string password)
        {
            int salt = Convert.ToInt32(saltiness);
            int iterations = Convert.ToInt32(nIterations);
            string Token = "";

            if (username != null && password != null)
            {
                var result = await _dbContext.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return new UserDto();
                }
                else
                {
                    if (!result.IsLogin)
                    {
                        var HashedPassword = _passwordHashService.HashPassword(password, result.Salt, iterations, salt);

                        if (result.Hash == HashedPassword)
                        {
                            result.IsLogin = true;
                            _ = await _dbContext.SaveChangesAsync();

                            Token = _jwtService.GenerateToken(result);
                            return new UserDto
                            {
                                Id = result.Id,
                                EmployeeNumber = result.EmployeeNumber,
                                FirstName = result.FirstName,
                                LastName = result.LastName,
                                Username = result.Username,
                                IsLogin = result.IsLogin,
                                Token = Token,
                                Message = "Login Successful"
                            };
                        }

                        return new UserDto();
                    }

                    return new UserDto
                    {
                        Id = result.Id,
                        EmployeeNumber = result.EmployeeNumber,
                        FirstName = result.FirstName,
                        LastName = result.LastName,
                        Username = result.Username,
                        IsLogin = result.IsLogin,
                        Token = Token,
                        Message = "User is already logged in."
                    };
                }
            }
            else
            {
                return new UserDto();
            }
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public async Task<UserDto> AuthenticateADAsync()
        {
            string Token = "";
            string domainName = "snrshopping";

            using (var context = new PrincipalContext(ContextType.Domain, domainName))
            {
                string username = Environment.UserName;

                using (var searcher = new DirectorySearcher())
                {
                    searcher.Filter = $"(&(objectClass=user)(sAMAccountName={username}))";

                    SearchResult result = searcher.FindOne()!;

                    if (result != null)
                    {
                        var userInDb = await _dbContext.Users
                            .Where(u => u.Username == username)
                            .FirstOrDefaultAsync();

                        if (userInDb == null)
                        {
                            return new UserDto();
                        }
                        else
                        {
                            if (!userInDb.IsLogin)
                            {
                                userInDb.IsLogin = true;
                                _ = await _dbContext.SaveChangesAsync();

                                Token = _jwtService.GenerateToken(userInDb);
                                return new UserDto
                                {
                                    Id = userInDb.Id,
                                    EmployeeNumber = userInDb.EmployeeNumber,
                                    FirstName = userInDb.FirstName,
                                    LastName = userInDb.LastName,
                                    Username = userInDb.Username,
                                    IsLogin = userInDb.IsLogin,
                                    Token = Token,
                                    Message = "Login Successful"
                                };
                            }
                            else
                            {
                                return new UserDto
                                {
                                    Id = userInDb.Id,
                                    EmployeeNumber = userInDb.EmployeeNumber,
                                    FirstName = userInDb.FirstName,
                                    LastName = userInDb.LastName,
                                    Username = userInDb.Username,
                                    IsLogin = userInDb.IsLogin,
                                    Token = Token,
                                    Message = "User is already logged in."
                                };
                            }
                        }
                    }
                }

                return new UserDto();
            }
        }

        public async Task<UserDto> Logout(string username)
        {
            if (username != null)
            {
                var result = await _dbContext.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return new UserDto();
                }
                else
                {
                    if (result.IsLogin)
                    {
                        result.IsLogin = false;
                        _ = await _dbContext.SaveChangesAsync();

                        return new UserDto
                        {
                            Id = result.Id,
                            EmployeeNumber = result.EmployeeNumber,
                            FirstName = result.FirstName,
                            LastName = result.LastName,
                            Username = result.Username,
                            IsLogin = result.IsLogin,
                            Token = "",
                            Message = "Logout Successful"
                        };
                    }
                    return new UserDto();
                }
            }
            else
            {
                return new UserDto();
            }
        }
    }
}
