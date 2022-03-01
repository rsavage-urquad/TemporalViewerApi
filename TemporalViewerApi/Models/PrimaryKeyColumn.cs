namespace TemporalViewerApi.Models
{
    /// <summary>
    /// PrimaryKeyColumn - Class for Primary Key Column items
    /// </summary>
    public class PrimaryKeyColumn
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }

        /// <summary>
        /// PrimaryKeyColumn() - Default constructor
        /// </summary>
        public PrimaryKeyColumn()
        {
            ColumnName = "";
            ColumnType = "";
        }
    }
}
