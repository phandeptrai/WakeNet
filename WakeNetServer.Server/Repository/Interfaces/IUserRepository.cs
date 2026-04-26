// Lưu/đọc User admin của hệ thống.
using WakeNetServer.Server.Models;

namespace WakeNetServer.Server.Repository.Interfaces;

public interface IUserRepository
{
    User? GetById(string id);
    User? GetByUsername(string username);
    List<User> GetAll();

    void Upsert(User user);
}

