using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Common
{
    public static class ResultExtensions
    {
        public static Contracts.Common.Result ToContract(this Domain.Common.Result result)
        {
            if (result.IsSuccess)
                return Contracts.Common.Result.Ok();

            return Contracts.Common.Result.Fail(result.Error ?? "");
        }
    }
}
