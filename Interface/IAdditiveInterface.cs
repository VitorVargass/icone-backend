using icone_backend.Dtos.Additives.Requests;
using icone_backend.Dtos.Additives.Responses;

namespace icone_backend.Interface
{
    public interface IAdditiveInterface
    {
        Task<IEnumerable<AdditiveResponse>> GetAllAsync(Guid? companyId);
        Task<AdditiveResponse?> GetByIdAsync(int id);
        Task<AdditiveResponse> CreateAsync(AdditiveRequest request, Guid? companyId);
        Task<AdditiveResponse?> UpdateAsync(int id, AdditiveRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
