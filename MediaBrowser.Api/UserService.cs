using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Security;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Services;
using MediaBrowser.Model.Users;

namespace MediaBrowser.Api
{
    /// <summary>
    /// Class PostUsers
    /// </summary>
    [Route("/Users", "POST", Summary = "Gets a list of users")]
    public class PostUsers : IReturn<UserDto[]>
    {
        [ApiMember(Name = "AccessToken", Description = "Account Token", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string AccessToken { get; set; }
    }

    /// <summary>
    /// Class GetUsers
    /// </summary>
    [Route("/Users", "GET", Summary = "Gets a list of users")]
    [Authenticated(Roles = "Admin")]
    public class GetUsers : IReturn<UserDto[]>
    {
        [ApiMember(Name = "IsHidden", Description = "Optional filter by IsHidden=true or false", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "GET")]
        public bool? IsHidden { get; set; }

        [ApiMember(Name = "IsDisabled", Description = "Optional filter by IsDisabled=true or false", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "GET")]
        public bool? IsDisabled { get; set; }

        [ApiMember(Name = "IsGuest", Description = "Optional filter by IsGuest=true or false", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "GET")]
        public bool? IsGuest { get; set; }
    }

    [Route("/Users/Public", "GET", Summary = "Gets a list of publicly visible users for display on a login screen.")]
    public class GetPublicUsers : IReturn<UserDto[]>
    {
    }

    /// <summary>
    /// Class GetUser
    /// </summary>
    [Route("/Users/{Id}", "GET", Summary = "Gets a user by Id")]
    [Authenticated(EscapeParentalControl = true)]
    public class GetUser : IReturn<UserDto>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Class DeleteUser
    /// </summary>
    [Route("/Users/{Id}", "DELETE", Summary = "Deletes a user")]
    [Authenticated(Roles = "Admin")]
    public class DeleteUser : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "DELETE")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Class AuthenticateUser
    /// </summary>
    [Route("/Users/{Id}/Authenticate", "POST", Summary = "Authenticates a user")]
    public class AuthenticateUser : IReturn<AuthenticationResult>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public Guid Id { get; set; }

        [ApiMember(Name = "Pw", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Pw { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [ApiMember(Name = "Password", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Class AuthenticateUser
    /// </summary>
    [Route("/Users/AuthenticateByName", "POST", Summary = "Authenticates a user")]
    public class AuthenticateUserByName : IReturn<AuthenticationResult>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Username", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [ApiMember(Name = "Password", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Password { get; set; }

        [ApiMember(Name = "Pw", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Pw { get; set; }
    }

    /// <summary>
    /// Class AuthenticateProfile
    /// </summary>
    [Route("/Users/AuthenticateAccount", "POST", Summary = "Authenticates a profile")]
    public class AuthenticateAccount : IReturn<AuthenticationResult>
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        [ApiMember(Name = "Email", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [ApiMember(Name = "Password", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Password { get; set; }
    }


    /// <summary>
    /// Class AuthenticateUser
    /// </summary>
    [Route("/Users/AuthenticateByToken", "POST", Summary = "Authenticates a user")]
    public class AuthenticateAccountByToken : IReturn<AuthenticationResult>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Guid", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        [ApiMember(Name = "AccessToken", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string AccessToken { get; set; }
    }

    /// <summary>
    /// Class UpdateUserPassword
    /// </summary>
    [Route("/Users/{Id}/Password", "POST", Summary = "Updates a user's password")]
    [Authenticated]
    public class UpdateUserPassword : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string CurrentPassword { get; set; }

        public string CurrentPw { get; set; }

        public string NewPw { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reset password].
        /// </summary>
        /// <value><c>true</c> if [reset password]; otherwise, <c>false</c>.</value>
        public bool ResetPassword { get; set; }
    }

    /// <summary>
    /// Class UpdateUserEasyPassword
    /// </summary>
    [Route("/Users/{Id}/EasyPassword", "POST", Summary = "Updates a user's easy password")]
    [Authenticated]
    public class UpdateUserEasyPassword : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        /// <value>The new password.</value>
        public string NewPassword { get; set; }

        public string NewPw { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reset password].
        /// </summary>
        /// <value><c>true</c> if [reset password]; otherwise, <c>false</c>.</value>
        public bool ResetPassword { get; set; }
    }

    /// <summary>
    /// Class UpdateUser
    /// </summary>
    [Route("/Users/{Id}", "POST", Summary = "Updates a user")]
    [Authenticated]
    public class UpdateUser : UserDto, IReturnVoid
    {
    }

    /// <summary>
    /// Class UpdateUser 
    /// </summary>
    [Route("/Users/{Id}/Policy/MaxParentalRating", "POST", Summary = "Updates a user max parental rating policy")]
    [Authenticated]
    public class UpdateUserMaxParentalRatingPolicy : IReturnVoid
    {
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public Guid Id { get; set; }

        [ApiMember(Name = "MaxParentalRating", IsRequired = false, DataType = "int", ParameterType = "body", Verb = "POST")]
        public int? MaxParentalRating { get; set; }
    }

    /// <summary>
    /// Class UpdateUser
    /// </summary>
    [Route("/Users/{Id}/Policy", "POST", Summary = "Updates a user policy")]
    [Authenticated(Roles = "admin")]
    public class UpdateUserPolicy : UserPolicy, IReturnVoid
    {
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Class UpdateUser
    /// </summary>
    [Route("/Users/{Id}/Configuration", "POST", Summary = "Updates a user configuration")]
    [Authenticated]
    public class UpdateUserConfiguration : UserConfiguration, IReturnVoid
    {
        [ApiMember(Name = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Class CreateUser
    /// </summary>
    [Route("/Users/New", "POST", Summary = "Creates a user")]
    public class CreateUserByName : IReturn<UserDto>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [ApiMember(Name = "Name", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        [ApiMember(Name = "AccessToken", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string AccessToken { get; set; }
    }

    /// <summary>
    /// Class CreateAccount
    /// </summary>
    [Route("/Users/NewAccount", "POST", Summary = "Creates a account")]
    [Authenticated(Roles = "Admin")]
    public class CreateAccount : IReturn<UserDto>
    {
        [ApiMember(Name = "Email", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Email { get; set; }

        [ApiMember(Name = "Enabled", IsRequired = false, DataType = "bool", ParameterType = "body", Verb = "POST")]
        public bool Enabled { get; set; }

        [ApiMember(Name = "Password", IsRequired = true, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Password { get; set; }

        [ApiMember(Name = "IsTrial", IsRequired = false, DataType = "string", ParameterType = "body", Verb = "POST")]
        public bool IsTrial { get; set; }

        [ApiMember(Name = "ExpDate", IsRequired = false, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string ExpDate { get; set; }

        [ApiMember(Name = "Notes", IsRequired = false, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Notes { get; set; }

        [ApiMember(Name = "GroupId", IsRequired = false, DataType = "int", ParameterType = "body", Verb = "POST")]
        public int GroupId { get; set; }

        [ApiMember(Name = "PlainId", IsRequired = false, DataType = "int", ParameterType = "body", Verb = "POST")]
        public int PlainId { get; set; }

        [ApiMember(Name = "Credit", IsRequired = false, DataType = "int", ParameterType = "body", Verb = "POST")]
        public int Credit { get; set; }

        [ApiMember(Name = "CreatedById", IsRequired = false, DataType = "int", ParameterType = "body", Verb = "POST")]
        public int CreatedById { get; set; }

        [ApiMember(Name = "Profiles", IsRequired = false, DataType = "Array<string>", ParameterType = "body", Verb = "POST")]
        public List<string> Profiles { get; set; }
    }

    [Route("/Users/ForgotPassword", "POST", Summary = "Initiates the forgot password process for a local user")]
    public class ForgotPassword : IReturn<ForgotPasswordResult>
    {
        [ApiMember(Name = "EnteredUsername", IsRequired = false, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string EnteredUsername { get; set; }
    }

    [Route("/Users/ForgotPassword/Pin", "POST", Summary = "Redeems a forgot password pin")]
    public class ForgotPasswordPin : IReturn<PinRedeemResult>
    {
        [ApiMember(Name = "Pin", IsRequired = false, DataType = "string", ParameterType = "body", Verb = "POST")]
        public string Pin { get; set; }
    }

    /// <summary>
    /// Class UsersService
    /// </summary>
    public class UserService : BaseApiService
    {
        /// <summary>
        /// The _user manager
        /// </summary>
        private readonly IUserManager _userManager;
        private readonly ISessionManager _sessionMananger;
        private readonly IAuthenticationRepository _authRepo;
        private readonly IServerConfigurationManager _config;
        private readonly INetworkManager _networkManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IAuthorizationContext _authContext;

        public UserService(IUserManager userManager, ISessionManager sessionMananger, IAuthenticationRepository authRepo, IServerConfigurationManager config, INetworkManager networkManager, IDeviceManager deviceManager, IAuthorizationContext authContext)
        {
            _userManager = userManager;
            _sessionMananger = sessionMananger;
            _authRepo = authRepo;
            _config = config;
            _networkManager = networkManager;
            _deviceManager = deviceManager;
            _authContext = authContext;
        }



        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Post(PostUsers request)
        {
            var users = _userManager.Users;

            var session = _authRepo.Get(new AuthenticationInfoQuery
            {
                AccessToken = request.AccessToken

            }).Items.FirstOrDefault();

            if (session == null)
            {
                throw new ArgumentException("Invalid access token.");
            }

            var account = _userManager.Accounts.FirstOrDefault(i => i.Guid == session.UserId);

            if (account == null)
            {
                throw new ArgumentException("Invalid account.");
            }

            users = users.Where(i => i.AccountId == account.Id);

            var result = users
                .OrderBy(u => u.Name)
                .Select(i => {
                    var dto = _userManager.GetUserDto(i, Request.RemoteIp);
                    dto.Session = _sessionMananger.Sessions.FirstOrDefault(u => u.UserId == i.Id);
                    return dto;
                })
                .ToArray();

            return ToOptimizedResult(result);
        }

        public object Get(GetPublicUsers request)
        {
            // If the startup wizard hasn't been completed then just return all users
            if (!_config.Configuration.IsStartupWizardCompleted)
            {
                return Get(new GetUsers
                {
                    IsDisabled = false
                });
            }

            return Get(new GetUsers
            {
                IsHidden = false,
                IsDisabled = false

            }, true, true);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetUsers request)
        {
            return Get(request, false, false);
        }

        private object Get(GetUsers request, bool filterByDevice, bool filterByNetwork)
        {
            var users = _userManager.Users;

            if (request.IsDisabled.HasValue)
            {
                users = users.Where(i => i.Policy.IsDisabled == request.IsDisabled.Value);
            }

            if (request.IsHidden.HasValue)
            {
                users = users.Where(i => i.Policy.IsHidden == request.IsHidden.Value);
            }

            if (filterByDevice)
            {
                var deviceId = _authContext.GetAuthorizationInfo(Request).DeviceId;

                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    users = users.Where(i => _deviceManager.CanAccessDevice(i, deviceId));
                }
            }

            if (filterByNetwork)
            {
                if (!_networkManager.IsInLocalNetwork(Request.RemoteIp))
                {
                    users = users.Where(i => i.Policy.EnableRemoteAccess);
                }
            }

            var result = users
                    .OrderBy(u => u.Name)
                    .Select(i => {
                        i.AccountEmail = _userManager.Accounts.First(a => a.Id == i.AccountId)?.Email;
                        return _userManager.GetUserDto(i, Request.RemoteIp);
                    })
                    .ToArray();

            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetUser request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            var result = _userManager.GetUserDto(user, Request.RemoteIp);

            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public Task Delete(DeleteUser request)
        {
            return DeleteAsync(request);
        }

        public Task DeleteAsync(DeleteUser request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            _sessionMananger.RevokeUserTokens(user.Id, null);

            return _userManager.DeleteUser(user);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task<object> Post(AuthenticateAccount request)
        {
            var auth = _authContext.GetAuthorizationInfo(Request);

            var result = _sessionMananger.AuthenticateNewSessionAccount(new AuthenticationRequest
            {
                App = auth.Client,
                AppVersion = auth.Version,
                DeviceId = auth.DeviceId,
                DeviceName = auth.Device,
                Password = request.Password,
                RemoteEndPoint = Request.RemoteIp,
                Email = request.Email

            });

            return ToOptimizedResult(result);
        }

        public async Task<object> Post(AuthenticateAccountByToken request)
        {
            var session = _authRepo.Get(new AuthenticationInfoQuery
            {
                AccessToken = request.AccessToken

            }).Items.FirstOrDefault();

            if (session == null)
            {
                throw new ArgumentException("Invalid access token.");
            }

            var account = _userManager.Accounts.FirstOrDefault(i => i.Guid == session.UserId);

            if (account == null)
            {
                throw new ArgumentException("Invalid account.");
            }

            var user = _userManager.Users.FirstOrDefault(i => i.AccountId == account.Id && i.Id == Guid.Parse(request.Guid));

            if (user == null)
            {
                throw new ArgumentException("Invalid account.");
            }

            var auth = _authContext.GetAuthorizationInfo(Request);

            var result = await _sessionMananger.AuthenticateNewSession(new AuthenticationRequest
            {
                App = auth.Client,
                AppVersion = auth.Version,
                DeviceId = auth.DeviceId,
                DeviceName = auth.Device,
                PasswordSha1 = user.Password,
                RemoteEndPoint = Request.RemoteIp,
                Username = user.Name

            }).ConfigureAwait(false);

            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public object Post(AuthenticateUser request)
        {
            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            return Post(new AuthenticateUserByName
            {
                Username = user.Name,
                Password = request.Password,
                Pw = request.Pw
            });
        }

        public async Task<object> Post(AuthenticateUserByName request)
        {
            var auth = _authContext.GetAuthorizationInfo(Request);

            var result = await _sessionMananger.AuthenticateNewSession(new AuthenticationRequest
            {
                App = auth.Client,
                AppVersion = auth.Version,
                DeviceId = auth.DeviceId,
                DeviceName = auth.Device,
                Password = request.Pw,
                PasswordSha1 = request.Password,
                RemoteEndPoint = Request.RemoteIp,
                Username = request.Username

            }).ConfigureAwait(false);

            return ToOptimizedResult(result);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public Task Post(UpdateUserPassword request)
        {
            return PostAsync(request);
        }

        public async Task PostAsync(UpdateUserPassword request)
        {
            AssertCanUpdateUser(_authContext, _userManager, request.Id, true);

            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            if (request.ResetPassword)
            {
                await _userManager.ResetPassword(user).ConfigureAwait(false);
            }
            else
            {
                var success = await _userManager.AuthenticateUser(user.Name, request.CurrentPw, request.CurrentPassword, Request.RemoteIp, false).ConfigureAwait(false);

                if (success == null)
                {
                    throw new ArgumentException("Invalid user or password entered.");
                }

                await _userManager.ChangePassword(user, request.NewPw).ConfigureAwait(false);

                var currentToken = _authContext.GetAuthorizationInfo(Request).Token;

                _sessionMananger.RevokeUserTokens(user.Id, currentToken);
            }
        }

        public void Post(UpdateUserEasyPassword request)
        {
            AssertCanUpdateUser(_authContext, _userManager, request.Id, true);

            var user = _userManager.GetUserById(request.Id);

            if (user == null)
            {
                throw new ResourceNotFoundException("User not found");
            }

            if (request.ResetPassword)
            {
                _userManager.ResetEasyPassword(user);
            }
            else
            {
                _userManager.ChangeEasyPassword(user, request.NewPw, request.NewPassword);
            }
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public async Task Post(UpdateUser request)
        {
            var id = GetPathValue(1);

            AssertCanUpdateUser(_authContext, _userManager, new Guid(id), false);

            var dtoUser = request;

            var user = _userManager.GetUserById(id);

            if (string.Equals(user.Name, dtoUser.Name, StringComparison.Ordinal))
            {
                _userManager.UpdateUser(user);
                _userManager.UpdateConfiguration(user, dtoUser.Configuration);
            }
            else
            {
                await _userManager.RenameUser(user, dtoUser.Name).ConfigureAwait(false);

                _userManager.UpdateConfiguration(dtoUser.Id, dtoUser.Configuration);
            }
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public async Task<object> Post(CreateAccount request)
        {
            request.ExpDate.Equals(null);

            Account newAccount = await _userManager.CreateAccount(new Account
            {
                CreateById = request.CreatedById,
                Credit = request.Credit,
                DateCreated = DateTime.UtcNow,
                Email = request.Email,
                Enabled = request.Enabled,
                ExpDate = request.ExpDate.Equals(null) ? request.IsTrial ? DateTime.Today.AddDays(10) : (DateTime?) null : DateTime.ParseExact(request.ExpDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                GroupId = request.GroupId,
                Guid = Guid.NewGuid(),
                IsTrial = request.IsTrial,
                Notes = request.Notes,
                Password = request.Password,
                PlainId = request.PlainId,
            }).ConfigureAwait(false);

            request.Profiles.ForEach(async name => {
                User user = await _userManager.CreateUser(String.Format("[{0}]{1}", newAccount.Guid.ToString("N"), name), newAccount).ConfigureAwait(false);
            });

            return null;

            // var result = _userManager.GetUserDto(newUser, Request.RemoteIp);
            // return ToOptimizedResult(result);
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public async Task<object> Post(CreateUserByName request)
        {
            var dtoUser = request;

            var users = _userManager.Users;

            var session = _authRepo.Get(new AuthenticationInfoQuery
            {
                AccessToken = request.AccessToken

            }).Items.FirstOrDefault();

            if (session == null)
            {
                throw new ArgumentException("Invalid access token.");
            }

            var account = _userManager.Accounts.FirstOrDefault(i => i.Guid == session.UserId);

            if (account == null)
            {
                throw new ArgumentException("Invalid account.");
            }

            var profiles = users.Where(i => i.AccountId == account.Id).ToList();

            if (profiles.Count < 5)
            {
                var newUser = await _userManager.CreateUser(String.Format("[{0}]{1}", account.Guid.ToString("N"), dtoUser.Name), account).ConfigureAwait(false);

                var result = _userManager.GetUserDto(newUser, Request.RemoteIp);

                return ToOptimizedResult(result);

            }

            throw new ArgumentException("The maximum number of profiles has been reached.");
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public async Task<object> Post(ForgotPassword request)
        {
            var isLocal = Request.IsLocal || _networkManager.IsInLocalNetwork(Request.RemoteIp);

            var result = await _userManager.StartForgotPasswordProcess(request.EnteredUsername, isLocal).ConfigureAwait(false);

            return result;
        }

        public async Task<object> Post(ForgotPasswordPin request)
        {
            var result = await _userManager.RedeemPasswordResetPin(request.Pin).ConfigureAwait(false);

            return result;
        }

        public void Post(UpdateUserConfiguration request)
        {
            AssertCanUpdateUser(_authContext, _userManager, request.Id, false);

            _userManager.UpdateConfiguration(request.Id, request);

        }

        public void Post(UpdateUserMaxParentalRatingPolicy request)
        {
            var user = _userManager.GetUserById(request.Id);

            user.Policy.MaxParentalRating = request.MaxParentalRating;

            _userManager.UpdateUserPolicy(request.Id, user.Policy);
        }

        public void Post(UpdateUserPolicy request)
        {
            var user = _userManager.GetUserById(request.Id);

            // If removing admin access
            if (!request.IsAdministrator && user.Policy.IsAdministrator)
            {
                if (_userManager.Users.Count(i => i.Policy.IsAdministrator) == 1)
                {
                    throw new ArgumentException("There must be at least one user in the system with administrative access.");
                }
            }

            // If disabling
            if (request.IsDisabled && user.Policy.IsAdministrator)
            {
                throw new ArgumentException("Administrators cannot be disabled.");
            }

            // If disabling
            if (request.IsDisabled && !user.Policy.IsDisabled)
            {
                if (_userManager.Users.Count(i => !i.Policy.IsDisabled) == 1)
                {
                    throw new ArgumentException("There must be at least one enabled user in the system.");
                }

                var currentToken = _authContext.GetAuthorizationInfo(Request).Token;
                _sessionMananger.RevokeUserTokens(user.Id, currentToken);
            }

            _userManager.UpdateUserPolicy(request.Id, request);
        }
    }
}
