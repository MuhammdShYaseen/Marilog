using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.Commands.Currency
{
    public record CreateCurrencyCommand(
     string Code,
     string Name,
     decimal ExchangeRate,
     string? Symbol = null
 );
}
