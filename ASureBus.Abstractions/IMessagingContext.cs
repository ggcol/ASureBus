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

using ASureBus.Abstractions.Options.Messaging;

namespace ASureBus.Abstractions;

public interface IMessagingContext
{
    public Guid CorrelationId { get; }

    public Task Send<TCommand>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public IMessagingContext Bind(Guid correlationId);

    public Task Send<TCommand>(TCommand message, SendOptions options,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task SendAfter<TCommand>(TCommand message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task SendScheduled<TCommand>(TCommand message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;

    public Task Publish<TEvent>(TEvent message, PublishOptions options,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;

    public Task PublishAfter<TEvent>(TEvent message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;

    public Task PublishScheduled<TEvent>(TEvent message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;
}