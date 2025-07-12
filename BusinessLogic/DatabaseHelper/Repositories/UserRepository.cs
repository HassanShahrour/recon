using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {

        public UserRepository(ReconovaDbContext context, IMapper mapper) : base(context, mapper)
        {
        }


        public async Task<Result<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.ScanResults)
                    .Include(u => u.AIResults)
                    .Where(u => u.IsDeleted == 0)
                    .ToListAsync();


                if (users is null || !users.Any())
                {
                    return Result<List<User>>.Failure("No users found.");
                }

                return Result<List<User>>.Success(users);
            }
            catch (InvalidDataException ex)
            {
                return Result<List<User>>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching users: " + ex.Message);
                return Result<List<User>>.Failure("An error occurred while fetching users.");
            }
        }

        public async Task<Result<User>> GetUserById(string id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ScanResults)
                    .Include(u => u.AIResults)
                    .Where(u => u.IsDeleted == 0)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return Result<User>.Failure("User not found");
                }

                return Result<User>.Success(user);
            }
            catch (InvalidDataException ex)
            {
                return Result<User>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user: " + ex.Message);
                return Result<User>.Failure("An error occurred while fetching user.");
            }
        }

        public async Task<Result<bool>> UpdateUser(User updatedUser)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == updatedUser.Id && u.IsDeleted == 0);

                if (existingUser == null)
                {
                    return Result<bool>.Failure("User not found.");
                }

                existingUser.FirstName = updatedUser.FirstName;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Email = updatedUser.Email;
                existingUser.NormalizedEmail = updatedUser.Email?.ToUpper();
                existingUser.NormalizedUserName = updatedUser.Email?.ToUpper();
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.BirthDate = updatedUser.BirthDate;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (InvalidDataException ex)
            {
                return Result<bool>.Failure($"Invalid data: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating user: " + ex.Message);
                return Result<bool>.Failure("An error occurred while updating user.");
            }
        }


        public async Task<Result<bool>> DeleteUser(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted == 0);

                if (user == null)
                    return Result<bool>.Failure("User not found.");


                user.IsDeleted = 1;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the user: {ex.Message}");
            }
        }


    }
}
