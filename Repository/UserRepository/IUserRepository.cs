using UserAuthAndAuthenticationJWT.Models;
namespace UserAuthAndAuthenticationJWT.Repository.UserRepository;

public interface IUserRepository
{
    Task<List<User>> GetAll();
    Task<User> GetBy(int? userId);
    Task<User> GetBy(string userEmail);
    Task Create(User newUser);
    Task Update(User userModified);
    Task Delete(int? userId);
}