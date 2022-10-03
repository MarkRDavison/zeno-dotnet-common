namespace mark.davison.common.server.sample.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController : BaseController<Author>
{
    public AuthorController(ILogger<AuthorController> logger, IRepository repository, IServiceScopeFactory serviceScopeFactory, ICurrentUserContext currentUserContext) : base(logger, repository, serviceScopeFactory, currentUserContext)
    {
    }

    protected override void PatchUpdate(Author persisted, Author patched)
    {
        throw new NotImplementedException();
    }
}
