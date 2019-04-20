using System;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Net
{
    public class AuthorizationInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid UserId => User == null ? Guid.Empty : User.Id;

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid AccountId => Account == null ? Guid.Empty : Account.Guid;

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        /// <value>The device.</value>
        public string Device { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        public string Client { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the account token.
        /// </summary>
        /// <value>The account token.</value>
        public string AccountToken { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        public string Token { get; set; }

        public Account Account { get; set; }

        public User User { get; set; }
    }
}
