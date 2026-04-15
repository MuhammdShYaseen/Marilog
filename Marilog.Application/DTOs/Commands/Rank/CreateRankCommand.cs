using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Application.DTOs.Commands.Rank
{
    public record CreateRankCommand(
    string RankCode,
    string RankName,
    Department Department
);
}
