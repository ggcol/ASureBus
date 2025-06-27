using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Core.Entities.NotNullPatternReturns;

internal class SagaAlreadyCompleted(SagaType sagaType, Guid correlationId)
{
}