﻿using Rsb.Accessories.Heavy;

namespace Rsb.Core.Enablers.Entities;

internal interface IRsbMessage
{
    internal Guid MessageId { get; set; }
    internal Guid CorrelationId { get; init; }
    internal string MessageName { get; init; }
    public bool IsCommand { get; }
    public IReadOnlyList<HeavyRef>? Heavies { get; init; }
}