namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

    public Task<bool> UserConnected(string? username, string connectionId)
    {
        var isOnline = false;
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username!))
            {
                OnlineUsers[username!].Add(connectionId);
            }
            else
            {
                if (username != null) OnlineUsers.Add(username, new List<string> { connectionId });
                isOnline = true;
            }
        }
        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string? username, string connectionId)
    {
        var isOffline = false;
        lock (OnlineUsers)
        {
            if (username != null && !OnlineUsers.ContainsKey(username))
                return Task.FromResult(isOffline);

            if (username != null)
            {
                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count != 0)
                    return Task.FromResult(isOffline);

                OnlineUsers.Remove(username);
            }

            isOffline = true;
        }
        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray()!;
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionsIds;

        lock (OnlineUsers)
        {
            connectionsIds = OnlineUsers.GetValueOrDefault(username)!;
        }
        
        return Task.FromResult(connectionsIds);
    }
}