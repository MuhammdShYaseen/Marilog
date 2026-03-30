namespace Marilog.Application.DTOs.LogDTOs
{
    public sealed record LogStats(int Errors, int Warnings, int Info, int Debug);
}