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

namespace ASureBus.Abstractions;

public interface IHandleMessage<in TMessage>
    where TMessage : IAmAMessage
{
    public Task Handle(TMessage message, IMessagingContext context,
        CancellationToken cancellationToken);

    public Task HandleError(Exception ex, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        throw ex;
    }
}