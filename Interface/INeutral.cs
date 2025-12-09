using icone_backend.Dtos.Neutral.Requests;
using icone_backend.Dtos.Neutral.Responses;

namespace icone_backend.Interface
{
    public interface INeutral
    {
        Task<NeutralResponse> PreviewAsync(CreateNeutralRequest request, CancellationToken ct);
        Task<NeutralResponse> CreateAsync(CreateNeutralRequest request, Guid? companyId, CancellationToken ct);
        Task<IReadOnlyList<NeutralResponse>> GetAllAsync(Guid? companyId, CancellationToken ct);
        Task<NeutralResponse?> GetByIdAsync(int id, CancellationToken ct);
    }
}
