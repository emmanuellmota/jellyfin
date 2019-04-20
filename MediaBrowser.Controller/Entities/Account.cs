using MediaBrowser.Model.Serialization;
using System;

namespace MediaBrowser.Controller.Entities
{
    /// <summary>
    /// Class User
    /// </summary>
    public class Account
    {
        [IgnoreDataMember]
        public int Id { get; set; }

        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the account is enable.
        /// </summary>
        /// <value>Is enable.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [IgnoreDataMember]
        public string Password { get; set; }
        public string Salt { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public bool IsTrial { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>The expiration date.</value>
        public DateTime? ExpDate { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>The group id.</value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the plain id.
        /// </summary>
        /// <value>The plain id.</value>
        public int? PlainId { get; set; }

        /// <summary>
        /// Gets or sets the credit.
        /// </summary>
        /// <value>The credit.</value>
        public int Credit { get; set; }

        /// <summary>
        /// Gets or sets the credit.
        /// </summary>
        /// <value>The credit.</value>
        [IgnoreDataMember]
        public int CreateById { get; set; }

        /// <summary>
        /// Gets or sets the created data.
        /// </summary>
        /// <value>The created date.</value>
        public DateTime DateCreated { get; set; }

    }
}
