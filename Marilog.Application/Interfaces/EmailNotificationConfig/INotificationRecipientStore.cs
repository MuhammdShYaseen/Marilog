namespace Marilog.Application.Interfaces.EmailNotificationConfig
{
    public interface INotificationRecipientStore
    {
        Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default);

        Task AddAsync(string email, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default);

        Task UpdateAsync(string currentEmail, string newEmail, CancellationToken cancellationToken = default);

        Task RemoveAsync(string email, CancellationToken cancellationToken = default);

        Task RemoveRangeAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default);
    }
}
