using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Core.Utilities;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {

        private readonly UserUtility _userUtility;

        public UserRepository(ReconovaDbContext context, IMapper mapper, UserUtility userUtility) : base(context, mapper)
        {
            _userUtility = userUtility;
        }


        public async Task<Result<List<User>>> GetAllUsers()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();
                var userResult = await GetUserById(userId.ToString());

                if (!userResult.IsSuccess || userResult.Value == null)
                    return Result<List<User>>.Failure("User not found.");

                var user = userResult.Value;

                var users = new List<User>();

                if (user.Role == "Admin")
                {
                    users = await _context.Users
                    .Include(u => u.ScanResults)
                    .Include(u => u.AIResults)
                    .Include(u => u.Plan)
                    .ToListAsync();
                }
                else
                {
                    users = await _context.Users
                        .Include(u => u.ScanResults)
                        .Include(u => u.AIResults)
                        .Include(u => u.Plan)
                        .Where(u => u.IsDeleted == 0)
                        .ToListAsync();
                }


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

        public async Task<Result<List<User>>> GetAllUsersExceptLoggedIn()
        {
            try
            {
                var loggedInUserId = await _userUtility.GetLoggedInUserId();

                var users = await _context.Users
                    .Include(u => u.ScanResults)
                    .Include(u => u.AIResults)
                    .Include(u => u.Plan)
                    .Where(u => u.Id != loggedInUserId.ToString() && u.IsDeleted == 0)
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
                    .Include(u => u.Skills)
                    .Include(u => u.Plan)
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
                    .Include(u => u.Skills)
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
                existingUser.Education = updatedUser.Education;
                existingUser.Header = updatedUser.Header;
                existingUser.Bio = updatedUser.Bio;
                existingUser.Country = updatedUser.Country;


                if (updatedUser?.ProfilePhoto != null && updatedUser.ProfilePhoto.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updatedUser.ProfilePhoto.FileName);
                    var uploadPath = Path.Combine("wwwroot", "users", "images");
                    var filePath = Path.Combine(uploadPath, fileName);

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await updatedUser.ProfilePhoto.CopyToAsync(stream);
                    }

                    existingUser.ProfilePhotoPath = $"/users/images/{fileName}";
                }

                if (updatedUser?.CoverPhoto != null && updatedUser.CoverPhoto.Length > 0)
                {
                    var coverName = Guid.NewGuid().ToString() + Path.GetExtension(updatedUser.CoverPhoto.FileName);
                    var coverPath = Path.Combine("wwwroot", "users", "images", coverName);

                    using (var stream = new FileStream(coverPath, FileMode.Create))
                    {
                        await updatedUser.CoverPhoto.CopyToAsync(stream);
                    }

                    existingUser.CoverPhotoPath = $"/users/images/{coverName}";
                }


                var updatedSkillNames = updatedUser?.Skills?
                     .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                     .Select(s => s.Name!.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .ToList() ?? new List<string>();


                var existingSkillNames = existingUser?.Skills?.Select(s => s.Name).ToList();


                var skillsToRemove = existingUser?.Skills?
                    .Where(s => !updatedSkillNames.Contains(s.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (skillsToRemove != null && skillsToRemove.Any())
                {
                    _context.Skill.RemoveRange(skillsToRemove);
                }


                var skillsToAddNames = updatedSkillNames
                    .Where(name => !existingSkillNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                var skillsToAdd = skillsToAddNames.Select(name => new Skill
                {
                    Name = name,
                    UserId = existingUser?.Id
                }).ToList();

                if (skillsToAdd.Any())
                {
                    await _context.Skill.AddRangeAsync(skillsToAdd);
                }

                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return Result<bool>.Failure("User not found.");


                user.IsDeleted = (sbyte)(user.IsDeleted == 0 ? 1 : 0);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the user: {ex.Message}");
            }
        }

        public async Task<string> GetLoggedInUserPhoto()
        {
            try
            {
                var userId = await _userUtility.GetLoggedInUserId();

                var user = await GetUserById(userId.ToString());

                return user.Value.ProfilePhotoPath ?? "~/images/account-bg.jpg";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user photo: " + ex.Message);
                return "~/images/account-bg.jpg";
            }
        }

        public async Task<Result<List<Post>>> GetUserPosts(string userId)
        {
            try
            {
                var posts = await _context.Post
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Media)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return Result<List<Post>>.Success(posts);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user posts: " + ex.Message);
                return Result<List<Post>>.Failure("An error occurred while fetching user posts.");
            }
        }

    }
}
