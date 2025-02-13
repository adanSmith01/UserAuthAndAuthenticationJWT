using UserAuthAndAuthenticationJWT.Repository.UserRepository;
using UserAuthAndAuthenticationJWT.Models;

public class UserRepository: IUserRepository
{
    private readonly string _connectionDB;

    public UserRepository(string connectionDB) => _connectionDB = connectionDB;

    public async Task<List<User>> GetAll()
    {
        try
        {
            var users = new List<User>();
            string[] userData;

            using(FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using(StreamReader reader = new StreamReader(connection))
            {
                await reader.ReadLineAsync();
                string lineOfData;

                while(!reader.EndOfStream)
                {
                    lineOfData = await reader.ReadLineAsync();
                    userData = lineOfData.Split(';');
                    users.Add(new User{
                        Email = userData[0],
                        Id = int.Parse(userData[1]),
                        FirstName = userData[2],
                        LastName = userData[3],
                    });
                }
            }

            return users;
        }
        catch(FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
    }

    public async Task<User> GetBy(int? userId)
    {
        try
        {
            string[] userData;

            if(userId == null) throw new ArgumentException("ID cannot be null");

            using(FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using(StreamReader reader = new StreamReader(connection))
            {
                await reader.ReadLineAsync();
                string lineOfData;

                while(!reader.EndOfStream)
                {
                    lineOfData = await reader.ReadLineAsync();
                    userData = lineOfData.Split(';');
                    if(Convert.ToInt32(userData[1]) == userId)
                    {
                        var user = new User{
                            Email = userData[0],
                            Id = int.Parse(userData[1]),
                            FirstName = userData[2],
                            LastName = userData[3]
                        };

                        return user;
                    }
                }
            }

            return null;
        }
        catch(FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
    }

    public async Task<User> GetBy(string userEmail)
    {
        try
        {
            string[] userData;
            if(string.IsNullOrEmpty(userEmail)) return null;
            
            using(FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using(StreamReader reader = new StreamReader(_connectionDB))
            {
                await reader.ReadLineAsync();
                string lineOfData;

                while(!reader.EndOfStream)
                {
                    lineOfData = await reader.ReadLineAsync();
                    userData = lineOfData.Split(';');
                    if(userData[0] == userEmail)
                    {
                        var user = new User{
                            Email = userData[0],
                            Id = int.Parse(userData[1]),
                            FirstName = userData[2],
                            LastName = userData[3]
                        };

                        return user;
                    }
                }
            }

            return null;
        }
        catch(FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
    }

    public async Task Create(User newUser)
    {
        try
        {
            string newUserData;
            using(FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using(StreamWriter writer = new StreamWriter(connection))
            {
                if(string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.FirstName) || string.IsNullOrEmpty(newUser.LastName))
                {
                    throw new Exception("There empty data");
                }
                newUserData = $"{newUser.Email};{new Random().Next(int.MaxValue)};{newUser.FirstName};{newUser.LastName}";
                await writer.WriteLineAsync(newUserData);
            }
        }
        catch(FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
    }

    public async Task Update(User userToUpdate)
    {
        string tempDB = $"{_connectionDB}.tmp";

        try
        {
            if(string.IsNullOrEmpty(userToUpdate.Email) || string.IsNullOrEmpty(userToUpdate.FirstName) || string.IsNullOrEmpty(userToUpdate.LastName))
            {
                throw new Exception("There empty data");
            }
            using(FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using(StreamReader reader = new StreamReader(connection))
            using(StreamWriter writer = new StreamWriter(tempDB, true))
            {
                string line;
                while((line = await reader.ReadLineAsync()) != null)
                {
                    var userToUpdateData = line.Split(";");
                    
                    if(userToUpdateData.Length > 1 && 
                        int.TryParse(userToUpdateData[1], out int id) &&
                        id == userToUpdate.Id)
                    {
                        userToUpdateData[0] = userToUpdate.Email;
                        userToUpdateData[2] = userToUpdate.FirstName;
                        userToUpdateData[3] = userToUpdate.LastName;
                    }
                    await writer.WriteLineAsync(string.Join(";", userToUpdateData));
                }
            }

            File.Delete(_connectionDB);
            File.Move(tempDB, _connectionDB);
        }
        catch(FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
        finally
        {
            if(File.Exists(tempDB))
            {
                File.Delete(tempDB);
            }
        }
    }


    public async Task Delete(int? userId)
    {
        string tempDB = $"{_connectionDB}.tmp";

        try
        {
            if(userId == null) throw new ArgumentException("ID cannot be null");

            using(FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using(StreamReader reader = new StreamReader(connection))
            using(StreamWriter writer = new StreamWriter(tempDB, true))
            {
                string line;
                while((line = await reader.ReadLineAsync()) != null)
                {
                    var userToUpdateData = line.Split(";");
                    
                    if(userToUpdateData.Length > 1 && 
                        int.TryParse(userToUpdateData[1], out int id) &&
                        id == userId)
                    {
                        await writer.WriteLineAsync(string.Join(";", userToUpdateData));
                    }
                }
            }

            File.Delete(_connectionDB);
            File.Move(tempDB, _connectionDB);
        }
        catch(FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
        finally
        {
            if(File.Exists(tempDB))
            {
                File.Delete(tempDB);
            }
        }
    }
}