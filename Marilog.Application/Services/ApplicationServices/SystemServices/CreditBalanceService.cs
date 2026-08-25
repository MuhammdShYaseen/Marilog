using Marilog.Application.Interfaces.Services.CreditBalance;
using Marilog.Contracts.DTOs.Requests.CreditBalance;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
#if false
    public class CreditBalanceService : ICreditBalanceService
    {
        private readonly IRepository<CreditBalance> _creditBalanceRepository;
        private readonly IRepository<Currency> _currencyRepository;

        public CreditBalanceService(
            IRepository<CreditBalance> creditBalanceRepository,
            IRepository<Currency> currencyRepository)
        {
            _creditBalanceRepository = creditBalanceRepository;
            _currencyRepository = currencyRepository;
        }

        public async Task<CreditBalanceResponse> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _creditBalanceRepository.Query()
                .Include(x => x.Currency)
                .Include(x => x.SenderCompany)
                .Include(x => x.ReceiverCompany)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException($"CreditBalance '{id}' not found.");

            return Map(entity);
        }

        public async Task<IReadOnlyList<CreditBalanceResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await _creditBalanceRepository.Query()
                .Include(x => x.Currency)
                .Include(x => x.SenderCompany)
                .Include(x => x.ReceiverCompany)
                .AsNoTracking()
                .ToListAsync(ct);

            return entities.Select(Map).ToList();
        }

        public async Task<CreditBalanceResponse> CreateAsync(CreateCreditBalanceRequest request, CancellationToken ct = default)
        {
            var currencyExists = await _currencyRepository.Query()
                .AnyAsync(x => x.Id == request.CurrencyId, ct);
            if (!currencyExists)
                throw new InvalidOperationException($"Currency '{request.CurrencyId}' not found.");

            var entity = CreditBalance.Create(
                request.PaymentId,
                request.CurrencyId,
                request.Amount,
                request.SenderCompanyId,
                request.ReceiverCompanyId);

            await _creditBalanceRepository.AddAsync(entity, ct);
            await _creditBalanceRepository.SaveChangesAsync(ct);

            return await GetByIdAsync(entity.Id, ct);
        }

        public async Task<CreditBalanceResponse> UpdateAsync(int id, UpdateCreditBalanceRequest request, CancellationToken ct = default)
        {
            var entity = await _creditBalanceRepository.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException($"CreditBalance '{id}' not found.");

            var currencyExists = await _currencyRepository.Query()
                .AnyAsync(x => x.Id == request.CurrencyId, ct);
            if (!currencyExists)
                throw new InvalidOperationException($"Currency '{request.CurrencyId}' not found.");

            entity.Update(
                request.CurrencyId,
                request.Amount,
                request.SenderCompanyId,
                request.ReceiverCompanyId);

            _creditBalanceRepository.Update(entity);
            await _creditBalanceRepository.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _creditBalanceRepository.Query()
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException($"CreditBalance '{id}' not found.");

            if (entity.Payments.Any())
                throw new InvalidOperationException(
                    "Cannot delete a CreditBalance that has allocated payments.");

            _creditBalanceRepository.HardDelete(entity);
            await _creditBalanceRepository.SaveChangesAsync(ct);
        }

        private static CreditBalanceResponse Map(CreditBalance entity) => new()
        {
            Id = entity.Id,
            PaymentId = entity.PaymentId,
            CurrencyId = entity.CurrencyId,
            CurrencyCode = entity.Currency?.CurrencyCode ?? string.Empty,
            Amount = entity.Amount,
            AllocatedAmount = entity.AllocatedAmount,
            UnallocatedAmount = entity.UnallocatedAmount,
            IsFullyAllocated = entity.IsFullyAllocated,
            SenderCompanyId = entity.SenderCompanyId,
            SenderCompanyName = entity.SenderCompany?.CompanyName,
            ReceiverCompanyId = entity.ReceiverCompanyId,
            ReceiverCompanyName = entity.ReceiverCompany?.CompanyName
        };
    }
#endif
}
