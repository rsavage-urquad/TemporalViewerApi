namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TemporalTable - Class to represent Temporal Table information.
    /// </summary>
    public class TemporalTable
    {
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public int BaseTableObjectId { get; set; }
        public string HistorySchemaName { get; set; }
        public string HistoryTableName { get; set; }
        public int HistoryTableObjectId { get; set; }

        /// <summary>
        /// TemporalTable() - Default constructor
        /// </summary>
        public TemporalTable()
        {
            BaseSchemaName = "";
            BaseTableName = "";
            BaseTableObjectId = 0;
            HistorySchemaName = "";
            HistoryTableName = "";
            HistoryTableObjectId = 0;
        }
    }
}
