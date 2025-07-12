using AutoMapper;
using Reconova.BusinessLogic.Exceptions;
using Reconova.Data;

namespace Reconova.BusinessLogic.DatabaseHelper.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly ReconovaDbContext _context;
        protected readonly IMapper _mapper;

        protected BaseRepository(ReconovaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Executes a database operation and returns a result with error handling.
        /// </summary>
        protected async Task<Result<TResult>> ExecuteWithHandlingAsync<TResult>(Func<Task<TResult>> operation)
        {
            try
            {
                var result = await operation();
                return Result<TResult>.Success(result);
            }
            catch (InvalidDataException ex)
            {
                return Result<TResult>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during operation: " + ex.Message);
                return Result<TResult>.Failure("An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Executes a database operation without returning a result, with error handling.
        /// </summary>
        protected async Task<Result<bool>> ExecuteWithHandlingAsync(Func<Task> operation)
        {
            try
            {
                await operation();
                return Result<bool>.Success(true);
            }
            catch (InvalidDataException ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during operation: " + ex.Message);
                return Result<bool>.Failure("An unexpected error occurred.");
            }
        }
    }
}
