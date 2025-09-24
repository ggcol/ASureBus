// Copyright (c) 2025 Gianluca Colombo (red.Co)
//
// This file is part of ASureBus (https://github.com/ggcol/ASureBus).
//
// ASureBus is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of
// the License, or (at your option) any later version.
//
// ASureBus is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with ASureBus. If not, see <https://www.gnu.org/licenses/>.

namespace ASureBus.Abstractions.Configurations;

public interface IConfigureAzureServiceBus
{
    public string ConnectionString { get; set; }
    /// <summary>
    /// May be "AmqpTcp" or "AmqpWebSockets", default is "AmqpWebSocket".
    /// Maps to Azure.Messaging.ServiceBus.ServiceBusTransportType.
    /// </summary>
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    /// <summary>
    /// May be "fixed" or "exponential", default is "fixed".
    /// Maps to Azure.Messaging.ServiceBus.ServiceBusRetryMode.
    /// </summary>
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; } 
    public bool? EnableMessageLockAutoRenewal { get; set; }
    public int? MessageLockRenewalPreemptiveThresholdInSeconds { get; set; }
    public TimeSpan? MaxAutoLockRenewalDuration { get; set; }
}