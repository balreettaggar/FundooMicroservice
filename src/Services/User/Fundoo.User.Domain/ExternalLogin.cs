namespace Fundoo.User.Domain;

public class ExternalLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderSubject { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
}
