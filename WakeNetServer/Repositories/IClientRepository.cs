using WakeNetServer.Domain;
using WakeNetServer.Networking;

namespace WakeNetServer.Repositories;

public interface IClientRepository
{
    Task<IReadOnlyList<ClientRecord>> GetAllAsync(CancellationToken ct = default);
    Task UpsertAsync(ClientSession session, CancellationToken ct = default);
    Task DeleteAsync(string clientId, CancellationToken ct = default);
}

