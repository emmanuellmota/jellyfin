using System;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using SQLitePCL.pretty;

namespace Emby.Server.Implementations.Data
{
    /// <summary>
    /// Class SQLiteUserRepository
    /// </summary>
    public class SqliteUserRepository : BaseSqliteRepository, IUserRepository
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SqliteUserRepository(
            ILoggerFactory loggerFactory,
            IServerApplicationPaths appPaths,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory.CreateLogger(nameof(SqliteUserRepository)))
        {
            _jsonSerializer = jsonSerializer;

            DbFilePath = Path.Combine(appPaths.DataPath, "users.db");
        }

        /// <summary>
        /// Gets the name of the repository
        /// </summary>
        /// <value>The name.</value>
        public string Name => "SQLite";

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <returns>Task.</returns>
        public void Initialize()
        {
            using (var connection = CreateConnection())
            {
                RunDefaultInitialization(connection);

                var localUsersTableExists = TableExists(connection, "LocalUsersv2");

                connection.RunQueries(new[] {
                    "CREATE TABLE IF NOT EXISTS Groups (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL)",
                    "CREATE TABLE IF NOT EXISTS Plains (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, MaxSimultaneousScreens INTEGER)",
                    "CREATE TABLE IF NOT EXISTS Accounts (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Guid GUID, Enabled BIT, Password TEXT NOT NULL, Email TEXT NOT NULL, IsTrial BIT NOT NULL ON CONFLICT REPLACE DEFAULT 0, ExpDate DATETIME, Notes TEXT, GroupId INTEGER, PlainId INTEGER, Credit INTEGER NOT NULL DEFAULT 0, CreatedById INTEGER, CONSTRAINT \"Group\" FOREIGN KEY (GroupId) REFERENCES Groups (Id) ON DELETE SET NULL ON UPDATE CASCADE, CONSTRAINT \"Plain\" FOREIGN KEY (PlainId) REFERENCES Plains (Id) ON DELETE SET NULL ON UPDATE CASCADE, CONSTRAINT \"CreatedById\" FOREIGN KEY (CreatedById) REFERENCES Accounts (Id) ON DELETE SET NULL ON UPDATE CASCADE)",
                    "CREATE TABLE IF NOT EXISTS LocalUsersv2 (Id INTEGER PRIMARY KEY, guid GUID NOT NULL, data BLOB NOT NULL, AccountId INTEGER NOT NULL, CONSTRAINT Account FOREIGN KEY (AccountId) REFERENCES Accounts (Id) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "DROP INDEX IF EXISTS idx_users"
                });

                if (!localUsersTableExists && TableExists(connection, "Users"))
                {
                    TryMigrateToLocalUsersTable(connection);
                }

                if (!localUsersTableExists)
                {
                    SetInitialDatabseValues(connection);

                }

                RemoveEmptyPasswordHashes();
            }
        }

        private void TryMigrateToLocalUsersTable(ManagedConnection connection)
        {
            try
            {
                connection.RunQueries(new[]
                {
                    "INSERT INTO LocalUsersv2 (guid, data) SELECT guid,data from users"
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error migrating users database");
            }
        }

        private void SetInitialDatabseValues(ManagedConnection connection)
        {
            try
            {
                connection.RunQueries(new[]
                {
                    "INSERT INTO Groups VALUES (NULL, 'Cliente')",
                    "INSERT INTO Groups VALUES (NULL, 'Administrador')",
                    "INSERT INTO Groups VALUES (NULL, 'Revendedor Master')",
                    "INSERT INTO Groups VALUES (NULL, 'Revendedor')",
                    "INSERT INTO Plains VALUES (NULL, 'Básico', 1)",
                    "INSERT INTO Plains VALUES (NULL, 'Padrão', 2)",
                    "INSERT INTO Plains VALUES (NULL, 'Premium', 4)"
            });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error inserting initial data to database");
            }
        }

        #region Account

        /// <summary>
        /// Save a account in the repo
        /// </summary>
        public void CreateAccount(Account account)
        {
            CreateAccount(account, null);
        }

        /// <summary>
        /// Save a account in the repo
        /// </summary>
        public void CreateAccount(Account account, Account by)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("INSERT INTO Accounts (Guid,Enabled,Password,Email,IsTrial,ExpDate,Notes,GroupId,PlainId,Credit,CreatedById) values (@Guid,@Enabled,@Password,@Email,@IsTrial,@ExpDate,@Notes,@GroupId,@PlainId,@Credit,@CreateById)"))
                        {
                            statement.TryBind("@Guid", account.Guid.ToGuidBlob());
                            statement.TryBind("@Enabled", account.Enabled);
                            statement.TryBind("@Password", account.Password);
                            statement.TryBind("@Email", account.Email);
                            statement.TryBind("@IsTrial", account.IsTrial);
                            statement.TryBind("@ExpDate", account.ExpDate?.ToDateTimeParamValue());
                            statement.TryBind("@Notes", account.Notes);
                            statement.TryBind("@GroupId", account.GroupId);
                            statement.TryBind("@PlainId", account.PlainId);
                            statement.TryBind("@Credit", account.Credit);
                            statement.TryBind("@CreateById", account.CreateById);

                            statement.MoveNext();
                        }

                        var createdUser = GetAccount(account.Guid, false);

                        if (createdUser == null)
                        {
                            throw new ApplicationException("created account should never be null");
                        }

                    }, TransactionMode);
                }
            }
        }

        public void UpdateAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("UPDATE Accounts SET Enabled=@Enabled,Password=@Password,Email=@Email,IsTrial=@IsTrial,ExpDate=@ExpDate,Notes=@Notes,GroupId=@GroupId,PlainId=@PlainId,Credit=@Credit WHERE Id=@Id"))
                        {
                            statement.TryBind("@Id", account.Id);
                            statement.TryBind("@Enabled", account.Enabled);
                            statement.TryBind("@Password", account.Password);
                            statement.TryBind("@Email", account.Email);
                            statement.TryBind("@IsTrial", account.IsTrial);
                            statement.TryBind("@ExpDate", account.ExpDate?.ToDateTimeParamValue());
                            statement.TryBind("@Notes", account.Notes);
                            statement.TryBind("@GroupId", account.GroupId);
                            statement.TryBind("@PlainId", account.PlainId);
                            statement.TryBind("@Credit", account.Credit);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }
        }

        public Account GetAccount(Guid guid, bool openLock)
        {
            using (openLock ? WriteLock.Read() : null)
            {
                using (var connection = CreateConnection(true))
                {
                    using (var statement = connection.PrepareStatement("select Id,Guid,Enabled,Password,Email,IsTrial,ExpDate,Notes,GroupId,PlainId,Credit,CreatedById from Accounts where Guid=@Guid"))
                    {
                        statement.TryBind("@Guid", guid);

                        foreach (var row in statement.ExecuteQuery())
                        {
                            return GetAccount(row);
                        }
                    }
                }
            }

            return null;
        }

        private Account GetAccount(IReadOnlyList<IResultSetValue> row)
        {
            return new Account()
            {
                Id = row[0].ToInt(),
                Guid = row.IsDBNull(1) ? Guid.NewGuid() : row[1].ReadGuidFromBlob(),
                Enabled = row[2].ToBool(),
                Password = row[3].ToString(),
                Email = row[4].ToString(),
                IsTrial = row[5].ToBool(),
                ExpDate = row.IsDBNull(6) ? (DateTime?) null : row[6].ReadDateTime(),
                Notes = row[7].ToString(),
                GroupId = row[8].ToInt(),
                PlainId = row.IsDBNull(9) ? (int?) null : row[9].ToInt(),
                Credit = row[10].ToInt(),
                CreateById = row[11].ToInt(),
            };
        }

        /// <summary>
        /// Retrieve all accounts from the database
        /// </summary>
        /// <returns>IEnumerable{Account}.</returns>
        public List<Account> RetrieveAllAccounts()
        {
            var list = new List<Account>();
            var withoutGuid = new List<int>();

            using (WriteLock.Read())
            {
                using (var connection = CreateConnection(true))
                {
                    foreach (var row in connection.Query("SELECT Id,Guid,Enabled,Password,Email,IsTrial,ExpDate,Notes,GroupId,PlainId,Credit,CreatedById FROM Accounts"))
                    {
                        list.Add(GetAccount(row));

                        if (row.IsDBNull(1))
                        {
                            withoutGuid.Add(row.GetInt32(0));
                        }
                    }
                }
            }

            withoutGuid.ForEach(id => {
                using (WriteLock.Write())
                {
                    using (var connection = CreateConnection())
                    {
                        connection.RunInTransaction(db =>
                        {
                            using (var statement = db.PrepareStatement("UPDATE Accounts SET Guid=@Guid WHERE Id=@Id"))
                            {
                                statement.TryBind("@Id", id);
                                statement.TryBind("@Guid", list.Find(r => r.Id == id).Guid.ToGuidBlob());
                                statement.MoveNext();
                            }

                        }, TransactionMode);
                    }
                }
            });

            return list;
        }

        /// <summary>
        /// Deletes the account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">account</exception>
        public void DeleteAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("delete from Accounts where Id=@id"))
                        {
                            statement.TryBind("@id", account.Id);
                            statement.MoveNext();
                        }
                    }, TransactionMode);
                }
            }
        }

        #endregion

        private void RemoveEmptyPasswordHashes()
        {
            foreach (var user in RetrieveAllUsers())
            {
                // If the user password is the sha1 hash of the empty string, remove it
                if (!string.Equals(user.Password, "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", StringComparison.Ordinal)
                    || !string.Equals(user.Password, "$SHA1$DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", StringComparison.Ordinal))
                {
                    continue;
                }

                user.Password = null;
                var serialized = _jsonSerializer.SerializeToBytes(user);

                using (WriteLock.Write())
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("update LocalUsersv2 set data=@data where Id=@InternalId"))
                        {
                            statement.TryBind("@InternalId", user.InternalId);
                            statement.TryBind("@data", serialized);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }

        }

        #region User

        /// <summary>
        /// Save a user in the repo
        /// </summary>
        public void CreateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var serialized = _jsonSerializer.SerializeToBytes(user);

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("insert into LocalUsersv2 (guid, data) values (@guid, @data)"))
                        {
                            statement.TryBind("@guid", user.Id.ToGuidBlob());
                            statement.TryBind("@data", serialized);

                            statement.MoveNext();
                        }

                        var createdUser = GetUser(user.Id, false);

                        if (createdUser == null)
                        {
                            throw new ApplicationException("created user should never be null");
                        }

                        user.InternalId = createdUser.InternalId;

                    }, TransactionMode);
                }
            }
        }

        public void UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var serialized = _jsonSerializer.SerializeToBytes(user);

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("update LocalUsersv2 set data=@data, accountId=@accountId where Id=@InternalId"))
                        {
                            statement.TryBind("@InternalId", user.InternalId);
                            statement.TryBind("@data", serialized);
                            statement.TryBind("@accountId", user.AccountId);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }
        }

        private User GetUser(Guid guid, bool openLock)
        {
            using (openLock ? WriteLock.Read() : null)
            {
                using (var connection = CreateConnection(true))
                {
                    using (var statement = connection.PrepareStatement("select id,guid,data,accountId from LocalUsersv2 where guid=@guid"))
                    {
                        statement.TryBind("@guid", guid);

                        foreach (var row in statement.ExecuteQuery())
                        {
                            return GetUser(row);
                        }
                    }
                }
            }

            return null;
        }

        private User GetUser(IReadOnlyList<IResultSetValue> row)
        {
            var id = row[0].ToInt64();
            var guid = row[1].ReadGuidFromBlob();
            var accountId = row[3].ToInt();

            using (var stream = new MemoryStream(row[2].ToBlob()))
            {
                stream.Position = 0;
                var user = _jsonSerializer.DeserializeFromStream<User>(stream);
                user.InternalId = id;
                user.Id = guid;
                user.AccountId = accountId;
                return user;
            }
        }

        /// <summary>
        /// Retrieve all users from the database
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        public List<User> RetrieveAllUsers()
        {
            var list = new List<User>();

            using (WriteLock.Read())
            {
                using (var connection = CreateConnection(true))
                {
                    foreach (var row in connection.Query("select id,guid,data,accountID from LocalUsersv2"))
                    {
                        list.Add(GetUser(row));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentNullException">user</exception>
        public void DeleteUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("delete from LocalUsersv2 where Id=@id"))
                        {
                            statement.TryBind("@id", user.InternalId);
                            statement.MoveNext();
                        }
                    }, TransactionMode);
                }
            }
        }

        #endregion

        #region Groups

        /// <summary>
        /// Save a group in the repo
        /// </summary>
        public void CreateGroup(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("INSERT INTO Groups (Name) values (@Name)"))
                        {
                            statement.TryBind("@Name", group.Name);
                            statement.MoveNext();
                        }

                        var createdGroup = GetGroup(group.Id, false);

                        if (createdGroup == null)
                        {
                            throw new ApplicationException("created group should never be null");
                        }

                    }, TransactionMode);
                }
            }
        }

        public void UpdateGroup(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("UPDATE Groups SET Name=@Name WHERE Id=@Id"))
                        {
                            statement.TryBind("@Id", group.Id);
                            statement.TryBind("@Name", group.Name);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }
        }

        public Group GetGroup(int id, bool openLock)
        {
            using (openLock ? WriteLock.Read() : null)
            {
                using (var connection = CreateConnection(true))
                {
                    using (var statement = connection.PrepareStatement("select Id,Name from Groups where Id=@Id"))
                    {
                        statement.TryBind("@Id", id);

                        foreach (var row in statement.ExecuteQuery())
                        {
                            return GetGroup(row);
                        }
                    }
                }
            }

            return null;
        }

        private Group GetGroup(IReadOnlyList<IResultSetValue> row)
        {
            return new Group()
            {
                Id = row[0].ToInt(),
                Name = row[1].ToString(),
            };
        }

        /// <summary>
        /// Retrieve all groups from the database
        /// </summary>
        /// <returns>IEnumerable{Group}.</returns>
        public List<Group> RetrieveAllGroups()
        {
            var list = new List<Group>();

            using (WriteLock.Read())
            {
                using (var connection = CreateConnection(true))
                {
                    foreach (var row in connection.Query("SELECT Id,Name FROM Groups"))
                    {
                        list.Add(GetGroup(row));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">group</exception>
        public void DeleteGroup(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("delete from Groups where Id=@Id"))
                        {
                            statement.TryBind("@Id", group.Id);
                            statement.MoveNext();
                        }
                    }, TransactionMode);
                }
            }
        }

        #endregion

        #region Plains

        /// <summary>
        /// Save a plain in the repo
        /// </summary>
        public void CreatePlains(Plain plain)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("INSERT INTO Plains (Name, MaxSimultaneousScreens) values (@Name, @MaxSimultaneousScreens)"))
                        {
                            statement.TryBind("@Name", plain.Name);
                            statement.TryBind("@MaxSimultaneousScreens", plain.MaxSimultaneousScreens);
                            statement.MoveNext();
                        }

                        var createdPlain = GetPlain(plain.Id, false);

                        if (createdPlain == null)
                        {
                            throw new ApplicationException("created plain should never be null");
                        }

                    }, TransactionMode);
                }
            }
        }

        public void UpdatePlain(Plain plain)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("UPDATE Plains SET Name=@Name,MaxSimultaneousScreens=@MaxSimultaneousScreens WHERE Id=@Id"))
                        {
                            statement.TryBind("@Id", plain.Id);
                            statement.TryBind("@Name", plain.Name);
                            statement.TryBind("@Name", plain.MaxSimultaneousScreens);
                            statement.MoveNext();
                        }

                    }, TransactionMode);
                }
            }
        }

        public Plain GetPlain(int id, bool openLock)
        {
            using (openLock ? WriteLock.Read() : null)
            {
                using (var connection = CreateConnection(true))
                {
                    using (var statement = connection.PrepareStatement("select Id,Name,MaxSimultaneousScreens from Groups where Id=@Id"))
                    {
                        statement.TryBind("@Id", id);

                        foreach (var row in statement.ExecuteQuery())
                        {
                            return GetPlain(row);
                        }
                    }
                }
            }

            return null;
        }

        private Plain GetPlain(IReadOnlyList<IResultSetValue> row)
        {
            return new Plain()
            {
                Id = row[0].ToInt(),
                Name = row[1].ToString(),
                MaxSimultaneousScreens = row[2].ToInt(),
            };
        }

        /// <summary>
        /// Retrieve all plains from the database
        /// </summary>
        /// <returns>IEnumerable{Group}.</returns>
        public List<Plain> RetrieveAllPlains()
        {
            var list = new List<Plain>();

            using (WriteLock.Read())
            {
                using (var connection = CreateConnection(true))
                {
                    foreach (var row in connection.Query("SELECT Id,Name,MaxSimultaneousScreens FROM Plains"))
                    {
                        list.Add(GetPlain(row));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Deletes the plain.
        /// </summary>
        /// <param name="plain">The plain.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">plain</exception>
        public void DeletePlain(Plain plain)
        {
            if (plain == null)
            {
                throw new ArgumentNullException("plain");
            }

            using (WriteLock.Write())
            {
                using (var connection = CreateConnection())
                {
                    connection.RunInTransaction(db =>
                    {
                        using (var statement = db.PrepareStatement("delete from Plains where Id=@Id"))
                        {
                            statement.TryBind("@Id", plain.Id);
                            statement.MoveNext();
                        }
                    }, TransactionMode);
                }
            }
        }

        #endregion

    }
}
