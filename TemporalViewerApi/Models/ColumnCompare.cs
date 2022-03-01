namespace TemporalViewerApi.Models
{
    /// <summary>
    /// ColumnCompare Class - Used to hold Column Compare information.
    /// </summary>
    public class ColumnCompare
    {
        public dynamic NewValue { get; set; }
        public dynamic OldValue { get; set; }

        /// <summary>
        /// ColumnCompare() - Default Constructor
        /// </summary>
        public ColumnCompare()
        {
            NewValue = null;
            OldValue = null;
        }
    }
}
