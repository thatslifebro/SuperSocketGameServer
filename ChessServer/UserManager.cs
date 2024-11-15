namespace GameServer;

public class UserManager
{
    Dictionary<string, User> _userMap = new();

    UInt32 _userSequence = 0;

    public ErrorCode AddUser(string sessionId, string userId)
    {
        if(_userMap.ContainsKey(sessionId))
        {
            Console.WriteLine($"Already exist user: {sessionId}");
            return ErrorCode.AlreadyExistUser;
        }

        _userSequence++;

        User user = new User(sessionId, userId, _userSequence);
        _userMap.Add(sessionId, user);

        return ErrorCode.None;
    }

    public void RemoveUser(string sessionId)
    {
        _userMap.Remove(sessionId);
    }

    public User? GetUser(string sessionId)
    {
        if(_userMap.TryGetValue(sessionId, out User? user))
        {
            return user;
        }

        return null;
    }

    public List<User> GetAllUser()
    {
        return _userMap.Values.ToList();
    }
}
