﻿using Rsb.Core.Enablers.Entities;

namespace Rsb.Core.Messaging;

internal interface ICollectMessage
{
    internal Queue<IRsbMessage> Messages { get; }
}