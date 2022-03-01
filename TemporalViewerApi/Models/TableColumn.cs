namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TableColumn - Class to represent Table column information.
    /// </summary>
    public class TableColumn
    {
        public string ColumnName { get; set; }
        public int ColumnId { get; set; }
        public string ColumnTypeName { get; set; }
        public int MaxLen { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public int GeneratedType { get; set; }

        /// <summary>
        /// TableColumn() - Default constructor
        /// </summary>
        public TableColumn()
        {
            ColumnName = "";
            ColumnId = 0;
            ColumnTypeName = "";
            MaxLen = 0;
            Precision = 0;
            Scale = 0;
            IsNullable = false;
            IsIdentity = false;
            GeneratedType = 0;
        }

    }
}
