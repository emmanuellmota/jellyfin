using System.Runtime.Serialization;

namespace MediaBrowser.Controller.Entities
{
    public class Group
    {
        [IgnoreDataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>Name.</value>
        public string Name { get; set; }
    }
}
