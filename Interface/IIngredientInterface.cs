using icone_backend.Dtos.Ingredient;
using icone_backend.Dtos.Ingridient;

namespace icone_backend.Interfaces
{
    public interface IIngredientInterface
    {
        Task<IEnumerable<IngredientResponse>> GetAllAsync(Guid companyId);
        Task<IngredientResponse?> GetByIdAsync(int id);
        Task<IngredientResponse> CreateAsync(CreateIngredientRequest request);
        Task<IngredientResponse?> UpdateAsync(int id, UpdateIngredientRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
