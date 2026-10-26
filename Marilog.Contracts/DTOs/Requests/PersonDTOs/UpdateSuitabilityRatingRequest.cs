using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.PersonDTOs;

public sealed record UpdateSuitabilityRatingRequest(CrewSuitabilityRating Rating);