﻿namespace mark.davison.common.client.abstractions.CQRS;

public interface IActionDispatcher
{
    Task Dispatch<TAction>(TAction action, CancellationToken cancellation)
        where TAction : class, IAction<TAction>;
    Task Dispatch<TAction>(CancellationToken cancellation)
        where TAction : class, IAction<TAction>, new();
}