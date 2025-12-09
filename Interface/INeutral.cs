using icone_backend.Dtos.Additives.Requests;
using icone_backend.Dtos.Neutral.Requests;
using icone_backend.Dtos.Neutral.Responses;

namespace icone_backend.Interface
{
    public interface INeutral
    {
        Task<IReadOnlyList<NeutralResponse>> GetAllAsync(Guid? companyId, CancellationToken ct);
        Task<NeutralResponse?> GetByIdAsync(int id, CancellationToken ct);
        Task<NeutralResponse> PreviewAsync(CreateNeutralRequest request, CancellationToken ct);
        Task<NeutralResponse> CreateAsync(CreateNeutralRequest request, Guid? companyId, CancellationToken ct);
        Task<NeutralResponse?> UpdateAsync(int id, CreateNeutralRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);

        Task<AdditiveScoresDto> AnalyzeDraftAsync(CreateNeutralRequest request, CancellationToken ct);
    }
}
