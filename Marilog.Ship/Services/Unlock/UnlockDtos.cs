namespace Marilog.Ship.Services.Unlock
{
    public record UnlockRequestResponse(int Id, string RequestCode, string Reason, DateTime CreatedAt);
}