﻿using Asb.Configurations;

namespace Playground.Settings;

public class ServiceBusSettings : IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
}