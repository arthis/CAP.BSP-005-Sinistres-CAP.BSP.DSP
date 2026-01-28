using MediatR;

namespace CAP.BSP.DSP.Application.Commands;

/// <summary>
/// Marker interface for commands in the CQRS pattern.
/// Commands represent write operations that change system state.
/// Uses MediatR for command handling with Unit return type (void equivalent).
/// </summary>
public interface ICommand : IRequest<CommandResult>
{
}
