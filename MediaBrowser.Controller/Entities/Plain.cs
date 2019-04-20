using System.Runtime.Serialization;

namespace MediaBrowser.Controller.Entities
{
    public class Plain
    {
        [IgnoreDataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>Name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the maximun simultaneous screens.
        /// </summary>
        /// <value>Amount.</value>
        public int MaxSimultaneousScreens { get; set; }
    }
}
