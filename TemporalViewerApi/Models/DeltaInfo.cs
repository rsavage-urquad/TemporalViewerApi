using System;
using System.Collections.Generic;
using System.Data.Common;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// DeltaInfo - Class to contain the collection of changed information
    /// </summary>
    public class DeltaInfo
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DeltaColumn> ChangedColumns { get; set; }

        /// <summary>
        /// DeltaInfo() - Default constructor
        /// </summary>
        public DeltaInfo()
        {
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
            ChangedColumns = new List<DeltaColumn>();
        }
    }

    /// <summary>
    /// DeltaColumn - Class to contain change info for a column
    /// </summary>
    public class DeltaColumn
    {
        public string ColumnName { get; set; }
        public dynamic OldValue { get; set; }
        public dynamic NewValue { get; set; }

        /// <summary>
        /// DeltaColumn() - Default constructor
        /// </summary>
        public DeltaColumn()
        {
            ColumnName = "";
            OldValue = null;
            NewValue = null;
        }

        /// <summary>
        /// DeltaColumn() - Fully populated constructor.
        /// </summary>
        /// <param name="columnName">Column Name</param>
        /// <param name="oldValue">Old Value</param>
        /// <param name="newValue">New Value</param>
        public DeltaColumn(string columnName, dynamic oldValue, dynamic newValue )
        {
            ColumnName = columnName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
