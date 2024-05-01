namespace mark.davison.common.server.Authentication;

public class AuthTokens
{
    public string? access_token { get; set; }
    public string? refresh_token { get; set; }

    [MemberNotNullWhen(returnValue: true, nameof(AuthTokens.access_token))]
    [MemberNotNullWhen(returnValue: true, nameof(AuthTokens.refresh_token))]
    public bool Valid => !string.IsNullOrEmpty(access_token) && !string.IsNullOrEmpty(refresh_token);
}