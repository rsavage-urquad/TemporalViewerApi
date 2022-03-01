namespace TemporalViewerApi.Models
{
    /// <summary>
    /// PrimaryKeyColumnInput - Class for Primary Key Column Input items
    /// </summary>
    public class PrimaryKeyColumnInput
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string InputValue { get; set; }

        /// <summary>
        /// PrimaryKeyColumnInput() - Default constructor
        /// </summary>
        public PrimaryKeyColumnInput()
        {
            ColumnName = "";
            ColumnType = "";
            InputValue = "";
        }
    }
}
