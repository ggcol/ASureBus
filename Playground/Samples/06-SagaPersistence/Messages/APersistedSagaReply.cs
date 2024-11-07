﻿using ASureBus.Abstractions;

namespace Playground.Samples._06_SagaPersistence.Messages;

public class APersistedSagaReply(string something) : IAmACommand
{
    public string Something { get; } = something;
}