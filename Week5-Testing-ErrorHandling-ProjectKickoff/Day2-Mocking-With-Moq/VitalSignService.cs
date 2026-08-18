using CardiacMonitoring.Api.DTOs.VitalSigns;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;

namespace CardiacMonitoring.Api.Services;

public class VitalSignService : IVitalSignService
{
    private readonly IRepository<VitalSign> _repository;
    private readonly IRiskEvaluator _riskEvaluator;

    public VitalSignService(IRepository<VitalSign> repository, IRiskEvaluator riskEvaluator)
    {
        _repository = repository;
        _riskEvaluator = riskEvaluator;
    }

    public async Task<VitalSignResponse> RecordReadingAsync(CreateVitalSignRequest request)
    {
        var vitalSign = new VitalSign
        {
            PatientId = request.PatientId,
            HeartRateBpm = request.HeartRateBpm,
            SystolicBp = request.SystolicBp,
            DiastolicBp = request.DiastolicBp,
            OxygenSaturationPercent = request.OxygenSaturationPercent,
            RecordedAtUtc = DateTime.UtcNow
        };

        // Scored before saving — every persisted reading always carries
        // the risk level that was true at the moment it was recorded.
        vitalSign.RiskLevel = _riskEvaluator.Evaluate(vitalSign);

        await _repository.AddAsync(vitalSign);
        await _repository.SaveChangesAsync();

        return new VitalSignResponse(
            vitalSign.Id, vitalSign.PatientId, vitalSign.HeartRateBpm,
            vitalSign.SystolicBp, vitalSign.DiastolicBp,
            vitalSign.OxygenSaturationPercent, vitalSign.RecordedAtUtc,
            vitalSign.RiskLevel);
    }
}
