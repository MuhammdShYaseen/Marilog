using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.ValueObjects.Laytime;
using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Services.Laytime
{
    public interface ILaytimeEngine
    {
        LaytimeSegmentBuild BuildSegments(
                        LaytimeCalculation calculation,
                        CargoOperationTerms operationTerms,
                        LaytimeRuleOptions ruleOptions,
                        bool onceOnDemurrage,
                        decimal? allowedDaysOverride = null);

        LaytimeResult Calculate(LaytimeCalculation calculation, LaytimeSegmentBuild build,
                                decimal demurrageRateUsdPerDay, decimal despatchRateUsdPerDay,
                                DespatchBasis despatchBasis);
    }
}
