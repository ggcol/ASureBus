﻿using ASureBus.Core.Entities;

namespace ASureBus.Core.Messaging;

internal interface ICollectMessage
{
    internal Queue<IAsbMessage> Messages { get; }
}