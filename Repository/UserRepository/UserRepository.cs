using UserAuthenticationJWT.Repository;
using UserAuthenticationJWT.Model;

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

            using(StreamReader connection = new StreamReader(_connectionDB))
            {
                await connection.ReadLineAsync();
                while(!connection.EndOfStream)
                {
                    userData = connection.ReadLine().Split(';');
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

    public async Task<User> GetById(int userId)
    {
        try
        {
            string[] userData;

            using(StreamReader connection = new StreamReader(_connectionDB))
            {
                await connection.ReadLineAsync();
                while(!connection.EndOfStream)
                {
                    userData = connection.ReadLine().Split(';');
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

    public async Task Create(User newUser)
    {
        try
        {
            string newUserData;
            using(StreamWriter connection = new StreamWriter(_connectionDB, true))
            {
                if(string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.FirstName) || string.IsNullOrEmpty(newUser.LastName))
                {
                    throw new Exception("There empty data");
                }
                newUserData = $"{newUser.Email};{new Random().Next(int.MaxValue)};{newUser.FirstName};{newUser.LastName}";
                await connection.WriteLineAsync(newUserData);
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
            using (FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(connection))
            using (StreamWriter writer = new StreamWriter(tempDB, true))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var userToUpdateData = line.Split(";");
                    
                    if (userToUpdateData.Length > 1 && 
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
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the user.", ex);
        }
        finally
        {
            if (File.Exists(tempDB))
            {
                File.Delete(tempDB);
            }
        }
    }


    public async Task Delete(int userId)
    {
        string tempDB = $"{_connectionDB}.tmp";

        try
        {
            using (FileStream connection = new FileStream(_connectionDB, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(connection))
            using (StreamWriter writer = new StreamWriter(tempDB, true))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var userToUpdateData = line.Split(";");
                    
                    if (userToUpdateData.Length > 1 && 
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
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Not exists file '{_connectionDB}' ");
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the user.", ex);
        }
        finally
        {
            if (File.Exists(tempDB))
            {
                File.Delete(tempDB);
            }
        }
    }
}