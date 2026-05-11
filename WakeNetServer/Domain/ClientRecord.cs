namespace WakeNetServer.Domain;

public sealed record ClientRecord(
    string ClientId,
    string Hostname,
    string Ip,
    string Mac,
    string Os,
    DateTime FirstSeenUtc,
    DateTime LastSeenUtc);

