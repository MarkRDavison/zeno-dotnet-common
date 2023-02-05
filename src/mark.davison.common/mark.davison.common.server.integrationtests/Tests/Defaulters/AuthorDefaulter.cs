namespace mark.davison.common.server.integrationtests.Tests.Defaulters;

public class AuthorDefaulter : IEntityDefaulter<Author>
{
    public const string LAST_NAME = "DEFAULTED";
    public Task DefaultAsync(Author entity, User user)
    {
        entity.LastName = LAST_NAME;

        return Task.CompletedTask;
    }
}
