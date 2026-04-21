using Marilog.Kernel.Enums;


namespace Marilog.Application.DTOs.Commands.Rank
{
    public record CreateRankCommand(
    string RankCode,
    string RankName,
    Department Department
);
}
