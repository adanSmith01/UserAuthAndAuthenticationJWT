using UserAuthenticationJWT.Model;
namespace UserAuthenticationJWT.Repository;

public interface IUserRepository
{
    Task<List<User>> GetAll();
    Task<User> GetById(int userId);
    Task Create(User newUser);
    Task Update(User userModified);
    Task Delete(int userId);
}