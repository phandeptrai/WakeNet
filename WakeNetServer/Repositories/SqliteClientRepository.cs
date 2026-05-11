using WakeNetServer.Data;
using WakeNetServer.Domain;
using WakeNetServer.Networking;

namespace WakeNetServer.Repositories;

public sealed class SqliteClientRepository : IClientRepository
{
    private readonly AppDb _db;

    public SqliteClientRepository(AppDb db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClientRecord>> GetAllAsync(CancellationToken ct = default)
    {
        var list = new List<ClientRecord>();
        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            SELECT ClientId, Hostname, Ip, Mac, Os, FirstSeenUtc, LastSeenUtc
            FROM Clients
            ORDER BY LastSeenUtc DESC;
            """;

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            list.Add(new ClientRecord(
                ClientId: reader.GetString(0),
                Hostname: reader.GetString(1),
                Ip: reader.GetString(2),
                Mac: reader.GetString(3),
                Os: reader.GetString(4),
                FirstSeenUtc: DateTime.Parse(reader.GetString(5), null, System.Globalization.DateTimeStyles.RoundtripKind),
                LastSeenUtc: DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind)
            ));
        }

        return list;
    }

    public async Task UpsertAsync(ClientSession session, CancellationToken ct = default)
    {
        var now = session.LastSeenUtc;

        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO Clients (ClientId, Hostname, Ip, Mac, Os, FirstSeenUtc, LastSeenUtc)
            VALUES ($clientId, $hostname, $ip, $mac, $os, $firstSeenUtc, $lastSeenUtc)
            ON CONFLICT(ClientId) DO UPDATE SET
                Hostname = excluded.Hostname,
                Ip = excluded.Ip,
                Mac = excluded.Mac,
                Os = excluded.Os,
                LastSeenUtc = excluded.LastSeenUtc;
            """;

        cmd.Parameters.AddWithValue("$clientId", session.ClientId);
        cmd.Parameters.AddWithValue("$hostname", session.Hostname ?? "");
        cmd.Parameters.AddWithValue("$ip", session.Ip ?? "");
        cmd.Parameters.AddWithValue("$mac", session.Mac ?? "");
        cmd.Parameters.AddWithValue("$os", session.Os ?? "");
        cmd.Parameters.AddWithValue("$firstSeenUtc", now.ToString("O"));
        cmd.Parameters.AddWithValue("$lastSeenUtc", now.ToString("O"));

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string clientId, CancellationToken ct = default)
    {
        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Clients WHERE ClientId = $clientId;";
        cmd.Parameters.AddWithValue("$clientId", clientId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}

