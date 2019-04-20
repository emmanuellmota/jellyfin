using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Cryptography;

namespace Emby.Server.Implementations.Library
{
    public class DefaultAuthenticationProvider : IAuthenticationProvider, IRequiresResolvedUser
    {
        private readonly ICryptoProvider _cryptographyProvider;
        public DefaultAuthenticationProvider(ICryptoProvider crypto)
        {
            _cryptographyProvider = crypto;
        }

        public string Name => "Default";

        public bool IsEnabled => true;
        
        // This is dumb and an artifact of the backwards way auth providers were designed.
        // This version of authenticate was never meant to be called, but needs to be here for interface compat
        // Only the providers that don't provide local user support use this
        public Task<ProviderAuthenticationResult> Authenticate(string username, string password)
        {
            throw new NotImplementedException();
        }
        
        // This is the verson that we need to use for local users. Because reasons.
        public Task<ProviderAuthenticationResult> Authenticate(string username, string password, User resolvedUser)
        {
            bool success = false;
            if (resolvedUser == null)
            {
                throw new Exception("Invalid username or password");
            }

            // As long as jellyfin supports passwordless users, we need this little block here to accomodate
            if (IsPasswordEmpty(resolvedUser, password))
            {
                return Task.FromResult(new ProviderAuthenticationResult
                {
                    Username = username
                });
            }

            ConvertPasswordFormat(resolvedUser);
            byte[] passwordbytes = Encoding.UTF8.GetBytes(password);

            PasswordHash readyHash = new PasswordHash(resolvedUser.Password);
            byte[] calculatedHash;
            string calculatedHashString;
            if (_cryptographyProvider.GetSupportedHashMethods().Contains(readyHash.Id) || _cryptographyProvider.DefaultHashMethod == readyHash.Id)
            {
                if (string.IsNullOrEmpty(readyHash.Salt))
                {
                    calculatedHash = _cryptographyProvider.ComputeHash(readyHash.Id, passwordbytes);
                    calculatedHashString = BitConverter.ToString(calculatedHash).Replace("-", string.Empty);
                }
                else
                {
                    calculatedHash = _cryptographyProvider.ComputeHash(readyHash.Id, passwordbytes, readyHash.SaltBytes);
                    calculatedHashString = BitConverter.ToString(calculatedHash).Replace("-", string.Empty);
                }

                if (calculatedHashString == readyHash.Hash)
                {
                    success = true;
                    // throw new Exception("Invalid username or password");
                }
            }
            else
            {
                throw new Exception(string.Format($"Requested crypto method not available in provider: {readyHash.Id}"));
            }

            // var success = string.Equals(GetPasswordHash(resolvedUser), GetHashedString(resolvedUser, password), StringComparison.OrdinalIgnoreCase);

            if (!success)
            {
                throw new Exception("Invalid username or password");
            }

            return Task.FromResult(new ProviderAuthenticationResult
            {
                Username = username
            });
        }

        // This allows us to move passwords forward to the newformat without breaking. They are still insecure, unsalted, and dumb before a password change
        // but at least they are in the new format.
        private void ConvertPasswordFormat(User user)
        {
            if (string.IsNullOrEmpty(user.Password))
            {
                return;
            }

            if (!user.Password.Contains("$"))
            {
                string hash = user.Password;
                user.Password = string.Format("$SHA1${0}", hash);
            }
            
            if (user.EasyPassword != null && !user.EasyPassword.Contains("$"))
            {
                string hash = user.EasyPassword;
                user.EasyPassword = string.Format("$SHA1${0}", hash);
            }
        }

        public Task<bool> HasPassword(User user)
        {
            var hasConfiguredPassword = !IsPasswordEmpty(user, GetPasswordHash(user));
            return Task.FromResult(hasConfiguredPassword);
        }

        private bool IsPasswordEmpty(User user, string password)
        {
            return (string.IsNullOrEmpty(user.Password) && string.IsNullOrEmpty(password));
        }

        public Task ChangePassword(User user, string newPassword)
        {
            ConvertPasswordFormat(user);
            // This is needed to support changing a no password user to a password user
            if (string.IsNullOrEmpty(user.Password))
            {
                PasswordHash newPasswordHash = new PasswordHash(_cryptographyProvider);
                newPasswordHash.SaltBytes = _cryptographyProvider.GenerateSalt();
                newPasswordHash.Salt = PasswordHash.ConvertToByteString(newPasswordHash.SaltBytes);
                newPasswordHash.Id = _cryptographyProvider.DefaultHashMethod;
                newPasswordHash.Hash = GetHashedStringChangeAuth(newPassword, newPasswordHash);
                user.Password = newPasswordHash.ToString();
                return Task.CompletedTask;
            }

            PasswordHash passwordHash = new PasswordHash(user.Password);
            if (passwordHash.Id == "SHA1" && string.IsNullOrEmpty(passwordHash.Salt))
            {
                passwordHash.SaltBytes = _cryptographyProvider.GenerateSalt();
                passwordHash.Salt = PasswordHash.ConvertToByteString(passwordHash.SaltBytes);
                passwordHash.Id = _cryptographyProvider.DefaultHashMethod;
                passwordHash.Hash = GetHashedStringChangeAuth(newPassword, passwordHash);
            }
            else if (newPassword != null)
            {
                passwordHash.Hash = GetHashedString(user, newPassword);
            }

            if (string.IsNullOrWhiteSpace(passwordHash.Hash))
            {
                throw new ArgumentNullException(nameof(passwordHash.Hash));
            }

            user.Password = passwordHash.ToString();

            return Task.CompletedTask;
        }

        public string GetPasswordHash(User user)
        {
            return user.Password;
        }

        public string GetHashedStringChangeAuth(string newPassword, PasswordHash passwordHash)
        {
            passwordHash.HashBytes = Encoding.UTF8.GetBytes(newPassword);
            return PasswordHash.ConvertToByteString(_cryptographyProvider.ComputeHash(passwordHash));
        }

        /// <summary>
        /// Gets the hashed string.
        /// </summary>
        public string GetHashedString(User user, string str)
        {
            PasswordHash passwordHash;
            if (string.IsNullOrEmpty(user.Password))
            {
                passwordHash = new PasswordHash(_cryptographyProvider);
            }
            else
            {
                ConvertPasswordFormat(user);
                passwordHash = new PasswordHash(user.Password);
            }

            if (passwordHash.SaltBytes != null)
            {
                // the password is modern format with PBKDF and we should take advantage of that
                passwordHash.HashBytes = Encoding.UTF8.GetBytes(str);
                return PasswordHash.ConvertToByteString(_cryptographyProvider.ComputeHash(passwordHash));
            }
            else
            {
                // the password has no salt and should be called with the older method for safety
                return PasswordHash.ConvertToByteString(_cryptographyProvider.ComputeHash(passwordHash.Id, Encoding.UTF8.GetBytes(str)));
            }
        }

        /// <summary>
        /// Gets the hashed string.
        /// </summary>
        public string GetHashedString(string str)
        {
            return BitConverter.ToString(_cryptographyProvider.ComputeSHA1(Encoding.UTF8.GetBytes(str))).Replace("-", string.Empty);
        }
    }
}
