using System;
using System.Collections.Generic;
using System.Linq;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TemporalViewerColumnHistoryResults Class - Results from Temporal Viewer Api "GetColumnHistory" method
    /// </summary>
    public class TemporalViewerColumnHistoryResults
    {
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public string HistorySchemaName { get; set; }
        public string HistoryTableName { get; set; }
        public TableColumn ColumnInfo { get; set; }
        public List<DeltaInfo> ColumnHistory { get; set; }
        public List<string> Messages { get; set; }
        public bool isValid { get { return (Messages.Count == 0); } }

        public TemporalViewerColumnHistoryResults()
        {
            BaseSchemaName = "";
            BaseTableName = "";
            HistorySchemaName = "";
            HistoryTableName = "";
            ColumnInfo = new TableColumn();
            ColumnHistory = new List<DeltaInfo>();
            Messages = new List<string>();
        }

        public TemporalViewerColumnHistoryResults(TemporalViewerResults results, string columnName)
        {
            BaseSchemaName = results.BaseSchemaName;
            BaseTableName = results.BaseTableName;
            HistorySchemaName = results.HistorySchemaName;
            HistoryTableName = results.HistoryTableName;
            ColumnInfo = results.TableColumns.FirstOrDefault(c => c.ColumnName == columnName);
            ColumnHistory = PopulateColumnHistory(results, columnName);
            Messages = results.Messages;
        }

        /// <summary>
        /// PopulateDelta() - Populated the Column History information Items
        /// </summary>
        /// <param name="results">Results to act upon</param>
        /// <param name="columnName">Column Name to extract History</param>
        /// <returns>List of Delta Info items</returns>
        protected List<DeltaInfo> PopulateColumnHistory(TemporalViewerResults results, string columnName)
        {
            List<DeltaInfo> colHistoryInfo = new List<DeltaInfo>();
            dynamic newInfo;
            dynamic oldInfo;
            CompareDictionary dict = new CompareDictionary();

            // Iterate over the History Rows to compare the row to the prior state (as rows will be in time descending order)
            for (int i = 0; i < (results.HistoryInfo.Count - 1); i++)
            {
                newInfo = results.HistoryInfo[i];
                oldInfo = results.HistoryInfo[i + 1];

                dict.PopulateCompareDictionary(newInfo, true);
                dict.PopulateCompareDictionary(oldInfo, false);
                DeltaInfo rowDelta = new DeltaInfo();

                // Iterate over the columns to identify changes.
                foreach (var col in results.TableColumns)
                {
                    switch (col.GeneratedType)
                    {
                        case 0:
                            // Only interested in specified column
                            if (col.ColumnName == columnName)
                            {
                                if (dict.CompareDict[col.ColumnName].NewValue != dict.CompareDict[col.ColumnName].OldValue)
                                {
                                    DeltaColumn deltaCol = new DeltaColumn(col.ColumnName, dict.CompareDict[col.ColumnName].OldValue, dict.CompareDict[col.ColumnName].NewValue);
                                    rowDelta.ChangedColumns.Add(deltaCol);
                                }
                            }
                            break;
                        case 1:
                            rowDelta.StartDate = dict.CompareDict[col.ColumnName].NewValue;
                            break;
                        case 2:
                            rowDelta.EndDate = dict.CompareDict[col.ColumnName].NewValue;
                            break;
                    }
                }

                // If a change was identified, add the row to the results
                if (rowDelta.ChangedColumns.Count > 0)
                {
                    colHistoryInfo.Add(rowDelta);
                }
            }

            // TODO: (Question) Do we want to include initial state?

            // Clean up End Timestamp (i.e. - History row was present, but column of interest was not changed.
            DateTime lastStartDate = colHistoryInfo[0].EndDate;
            foreach (var item in colHistoryInfo)
            {
                item.EndDate = (item.EndDate == lastStartDate) ? item.EndDate : lastStartDate;
                lastStartDate = item.StartDate;
            }

            return colHistoryInfo;
        }
    }
}
