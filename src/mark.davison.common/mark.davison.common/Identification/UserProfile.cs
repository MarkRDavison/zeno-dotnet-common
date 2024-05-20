namespace mark.davison.common.Identification;

public class UserProfile
{
    public Guid sub { get; set; }
    public string? name { get; set; }
    public string? preferred_username { get; set; }
    public string? given_name { get; set; }
    public string? family_name { get; set; }
    public string? email { get; set; }
}
