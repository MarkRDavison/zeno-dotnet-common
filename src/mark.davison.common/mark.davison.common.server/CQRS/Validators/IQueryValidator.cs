﻿namespace mark.davison.common.server.CQRS.Validators;

public interface IQueryValidator<TRequest, TResponse>
    where TRequest : class, IQuery<TRequest, TResponse>
    where TResponse : Response, new()
{
    public Task<TResponse> ValidateAsync(TRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken);
}
