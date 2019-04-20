using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Persistence
{
    /// <summary>
    /// Provides an interface to implement a User repository
    /// </summary>
    public interface IUserRepository : IRepository
    {
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        void DeleteUser(User user);

        // TODO: Delete Account

        /// <summary>
        /// Retrieves all accounts.
        /// </summary>
        /// <returns>IEnumerable{Account}.</returns>
        List<Account> RetrieveAllAccounts();

        void CreateAccount(Account account);
        void UpdateAccount(Account account);
        Account GetAccount(Guid guid, bool openLock);

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>IEnumerable{User}.</returns>
        List<User> RetrieveAllUsers();

        void CreateUser(User user);
        void UpdateUser(User user);
    }
}
