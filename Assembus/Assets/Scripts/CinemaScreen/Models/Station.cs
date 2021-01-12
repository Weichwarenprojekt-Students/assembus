namespace CinemaScreen.Models
{
    public class Station
    {
        /// <summary>
        ///     The child count of the station
        /// </summary>
        public int ChildCount;
        /// <summary>
        ///     The amount of items that came before this station
        /// </summary>
        public int PreviousItems;
        /// <summary>
        ///     The name of the station
        /// </summary>
        public string Name;
    }
}