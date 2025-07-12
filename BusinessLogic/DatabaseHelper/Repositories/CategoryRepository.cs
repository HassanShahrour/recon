using Microsoft.EntityFrameworkCore;
using Reconova.BusinessLogic.DatabaseHelper.Interfaces;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ReconovaDbContext _context;

        public CategoryRepository(ReconovaDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Category>>> GetAllCategories()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();

                if (categories == null || !categories.Any())
                {
                    return Result<List<Category>>.Failure("No categories found.");
                }

                return Result<List<Category>>.Success(categories);
            }
            catch (Exception ex)
            {
                return Result<List<Category>>.Failure($"An error occurred while retrieving categories: {ex.Message}");
            }
        }

        public async Task<Result<Category>> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return Result<Category>.Failure("Category not found.");
                }

                return Result<Category>.Success(category);
            }
            catch (Exception ex)
            {
                return Result<Category>.Failure($"An error occurred while retrieving the category: {ex.Message}");
            }
        }

        public async Task<Result<bool>> AddCategory(Category category)
        {
            try
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while adding the category: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateCategory(Category category)
        {
            try
            {
                var existing = await _context.Categories.FindAsync(category.Id);
                if (existing == null)
                    return Result<bool>.Failure("Category not found.");

                existing.Name = category.Name;
                _context.Categories.Update(existing);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while updating the category: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return Result<bool>.Failure("Category not found.");

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while deleting the category: {ex.Message}");
            }
        }
    }
}
