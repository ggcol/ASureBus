using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.Entities.NotNullPatternReturns;

internal record SagaAlreadyCompleted(SagaType SagaType, Guid CorrelationId)
{
}