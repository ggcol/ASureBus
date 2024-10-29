﻿using Asb.Core.TypesHandling.Entities;

namespace Asb.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal interface ISagaIO
{
    internal Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default);

    internal Task Unload(object? implSaga, Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default);

    internal Task Delete(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default);
}