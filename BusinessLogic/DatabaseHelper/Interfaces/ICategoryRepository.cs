using Reconova.BusinessLogic.Exceptions;
using Reconova.Data.Models;

namespace Reconova.BusinessLogic.DatabaseHelper.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Result<List<Category>>> GetAllCategories();

        Task<Result<Category>> GetCategoryById(int id);

        Task<Result<bool>> AddCategory(Category category);

        Task<Result<bool>> UpdateCategory(Category category);

        Task<Result<bool>> DeleteCategory(int id);
    }
}
