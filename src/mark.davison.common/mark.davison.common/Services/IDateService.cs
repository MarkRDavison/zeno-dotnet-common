namespace mark.davison.common.Services;

public interface IDateService
{
    public DateTime Now { get; }
    public DateOnly Today { get; }
}
