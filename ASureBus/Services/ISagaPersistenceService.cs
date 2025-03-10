﻿using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Services;

internal interface ISagaPersistenceService
{
    internal Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default);

    internal Task Save<TItem>(TItem item, SagaType sagaType,
        Guid correlationId, CancellationToken cancellationToken = default);

    internal Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default);
}