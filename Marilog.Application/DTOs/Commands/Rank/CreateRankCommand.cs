using Marilog.Domain.Entities;

namespace Marilog.Application.DTOs.Commands.Rank
{
    public record CreateRankCommand(
    string RankCode,
    string RankName,
    Department Department
);
}
