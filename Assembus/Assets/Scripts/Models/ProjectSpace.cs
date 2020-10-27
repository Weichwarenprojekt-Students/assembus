namespace Models
{
    public class ProjectSpace
    {
        /// <summary>
        ///     The project's name
        /// </summary>
        public string Name;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The project's name</param>
        public ProjectSpace(string name)
        {
            this.Name = name;
        }
    }
}