using icone_backend.Dtos.Ingredient.Requests;
using icone_backend.Dtos.Ingredient.Responses;

namespace icone_backend.Interfaces
{
    public interface IIngredientInterface
    {
        Task<IEnumerable<IngredientResponse>> GetAllAsync(Guid? companyId);
        Task<IngredientResponse?> GetByIdAsync(int id);
        Task<IngredientResponse> CreateAsync(CreateIngredientRequest request, Guid? companyId);
        Task<IngredientResponse?> UpdateAsync(int id, UpdateIngredientRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
