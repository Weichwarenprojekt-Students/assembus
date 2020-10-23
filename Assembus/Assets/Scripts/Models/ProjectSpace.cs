using System;

namespace Models
{
    public class ProjectSpace
    {
        /// <summary>
        /// The project's name
        /// </summary>
        public string name;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The project's name</param>
        public ProjectSpace(string name)
        {
            this.name = name;
        }
    }
}