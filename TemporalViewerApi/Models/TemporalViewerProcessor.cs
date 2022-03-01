using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using TemporalViewerApi.Repository;
using Dapper;
using System.Collections;
using System.Text;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TemporalViewerProcessor Class - Processing class for the Temporal Viewer functionality.
    /// </summary>
    public class TemporalViewerProcessor : ITemporalViewerProcessor
    {
        #region Properties

        private readonly ISchemaRepository _repo;
        private readonly IConfiguration _configuration;

        public List<string> Messages { get; set; }
        protected TemporalTable TemporalTableInfo;
        protected List<LookupParam> LookupValues;

        #endregion Properties

        #region Constructor(s)

        /// <summary>
        /// TemporalViewerProcessor() Default Constructor
        /// </summary>
        /// <param name="repo">Schema Repository (injected)</param>
        public TemporalViewerProcessor(ISchemaRepository repo, IConfiguration configuration)
        {
            Messages = new List<string>();
            TemporalTableInfo = new TemporalTable();
            LookupValues = new List<LookupParam>();
            _repo = repo;
            _configuration = configuration;
        }

        #endregion Constructor(s)

        #region Public

        public TemporalViewerResults Process(TemporalViewerRequest request)
        {
            TemporalViewerResults results = new TemporalViewerResults();

            // Load Temporal Table Info (Base and History Names)
            TemporalTableInfo = _repo.GetTemporalTableByName(request.SchemaName, request.TableName);
            if (TemporalTableInfo is null)
            {
                results.Messages.Add("Temporal Table Infon not found, please verify Schema and Table names.");
                return results;
            }

            // Load Columns
            results.TableColumns = _repo.GetTableColumns(request.SchemaName, request.TableName).TableColumns;
            if (results.TableColumns.Count == 0)
            {
                results.Messages.Add("Table Column not found, please verify Schema and Table names.");
                return results;
            }

            // Validate Request Details
            if (!ValidLookupInfo(request, results.TableColumns))
            {
                results.Messages.AddRange(Messages);
                return results;
            }

            // Load History Data 
            if (!PopulateHistory(results))
            {
                results.Messages.AddRange(Messages);
                return results;
            }

            // Populate Diff Indicators
            PopulateDiffInfo(results);

            return results;
        }

        #endregion Public

        #region Protected

        /// <summary>
        /// ValidLookupInfo() - Make sure all Primary Key Columns have been provided in the 
        /// input request.  Also validate the input data types are valid.
        /// </summary>
        /// <param name="request">Input Request object</param>
        /// <param name="tableColumns">List of Columns in the table. </param>
        /// <returns>True if all present, otherwise false</returns>
        protected bool ValidLookupInfo(TemporalViewerRequest request, List<TableColumn> tableColumns)
        {
            bool gotAllCols = true;
            bool dataTypesValid = true;
            TableColumn tableCol;
            ValidatorResult valResult;

            List<PrimaryKeyColumn> pkCols = _repo.GetPrimaryKeys(request.SchemaName, request.TableName).PrimaryKeys;

            // Validate all PK Columns Present in input
            foreach (PrimaryKeyColumn pkCol in pkCols)
            {
                var temp = request.LookupInfo.FirstOrDefault(c => c.ColumnName == pkCol.ColumnName);
                if (temp is null)
                {
                    gotAllCols = false;
                    Messages.Add($"PK Column {pkCol.ColumnName} not in Lookup Input collection.");
                }
            }

            // Validate PK Column Data Type
            foreach (var lCol in request.LookupInfo)
            {
                // Validate expected type matches input type
                tableCol = tableColumns.FirstOrDefault(tc => tc.ColumnName == lCol.ColumnName);
                if (tableCol is null)
                {
                    dataTypesValid = false;
                    Messages.Add($"Input Lookup column \"{lCol.ColumnName}\" not in Table.");
                    continue;
                }
                // Validate column datatypes match
                if (tableCol.ColumnTypeName != lCol.ColumnType)
                {
                    dataTypesValid = false;
                    Messages.Add($"Input Lookup column \"{lCol.ColumnName}\" data type different from Table column's datatype.");
                    continue;
                }

                // Validate input value's datatype and get the value (if valid)
                valResult = ValidateInputType(lCol);
                dataTypesValid &= valResult.IsValid;
                if (valResult.IsValid)
                {
                    LookupValues.Add(new LookupParam { ColumnName = lCol.ColumnName, ColumnType = lCol.ColumnType, InputValue = valResult.InputValue });
                }
            }

            return gotAllCols && dataTypesValid;
        }

        /// <summary>
        /// ValidateInputType() - Validate the input column's Type and retrieve the value (as a dynamic)
        /// </summary>
        /// <param name="col">Column to be parsed</param>
        /// <returns>ValidatorResult object containing the IsValid boolean and the parsed InputValue as a dynamic.</returns>
        protected ValidatorResult ValidateInputType(PrimaryKeyColumnInput col)
        {
            ValidatorResult result = new ValidatorResult();

            switch (col.ColumnType.ToLower())
            {
                case "varchar":
                case "narchar":
                case "text":
                case "ntext":
                case "uniqueidentifier":
                    result.IsValid = true;
                    result.InputValue = col.InputValue;
                    break;
                case "char":
                case "nchar":
                    if (col.InputValue.Length <= 1)
                    {
                        result.IsValid = true;
                        result.InputValue = col.InputValue;
                    }
                    break;
                case "int":
                case "smallint":
                case "tinyint":
                    int intTemp;
                    if (int.TryParse(col.InputValue, out intTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = intTemp;
                    }
                    break;
                case "bigint":
                    long longTemp;
                    if (long.TryParse(col.InputValue, out longTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = longTemp;
                    }
                    break;
                case "numeric":
                case "decimal":
                case "smallmoney":
                case "money":
                    decimal decimalTemp;
                    if (decimal.TryParse(col.InputValue, out decimalTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = decimalTemp;
                    }
                    break;
                case "bit":
                    List<string> goodValues = new List<string> { "true", "false", "0", "1" };
                    string bitTemp = col.InputValue.ToLower();
                    bool boolTemp = false;
                    if (goodValues.Contains(bitTemp))
                    {
                        result.IsValid = true;
                        boolTemp = (bitTemp == "true") ? true : boolTemp;
                        boolTemp = (bitTemp == "1") ? true : boolTemp;
                        boolTemp = (bitTemp == "false") ? false: boolTemp;
                        boolTemp = (bitTemp == "0") ? false : boolTemp;
                        result.InputValue = boolTemp;
                    }
                    break;
                case "float":
                    double doubleTemp;
                    if (double.TryParse(col.InputValue, out doubleTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = doubleTemp;
                    }
                    break;
                case "real":
                    float floatTemp;
                    if (float.TryParse(col.InputValue, out floatTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = floatTemp;
                    }
                    break;
                case "date":
                case "datetime2":
                case "smalldatetime":
                case "datetime":
                    DateTime datetimeTemp;
                    if (DateTime.TryParse(col.InputValue, out datetimeTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = datetimeTemp;
                    }
                    break;
                case "datetimeoffset":
                    DateTimeOffset datetimeoffsetTemp;
                    if (DateTimeOffset.TryParse(col.InputValue, out datetimeoffsetTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = datetimeoffsetTemp;
                    }
                    break;
                case "time":
                    TimeSpan timespanTemp;
                    if (TimeSpan.TryParse(col.InputValue, out timespanTemp))
                    {
                        result.IsValid = true;
                        result.InputValue = timespanTemp;
                    }
                    break;
                default:
                    Messages.Add($"Unexpected Column Type {col.ColumnType} for column {col.ColumnType}.");
                    break;
            }

            // Bad value, add message
            if (!result.IsValid)
            {
                Messages.Add($"Could not parse value for column \"{col.ColumnName}\".  Expected type \"{col.ColumnType}\", got \"{col.InputValue}\".");
            }

            return result;
        }

        protected bool PopulateHistory(TemporalViewerResults results)
        {
            string sql = FormatSqlComand(results);
            if (sql == "")
            {
                return false;
            }

            // Add Query Parameters
            var parameters = new DynamicParameters();
            LookupValues.ForEach(lv => parameters.Add(lv.ColumnName, lv.InputValue));

            try
            {
                using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DbConnStr")))
                {
                    //results.HistoryInfo = db.Query<dynamic>("SELECT * FROM Employee").ToList();
                    results.HistoryInfo = db.Query<dynamic>(sql, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                Messages.Add($"Caught Exception: {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// FormatSqlComand() - Format the SQL Command to retrieve the Current and History rows.
        /// </summary>
        /// <param name="results">Results object which contains list of columns.</param>
        /// <returns>SQL Select String</returns>
        protected string FormatSqlComand(TemporalViewerResults results)
        {
            // Columns Text
            string columns = results.TableColumns.Aggregate<TableColumn, string>("", (str, col) => str + col.ColumnName + ", ");
            columns = columns.Remove(columns.Length - 2, 2);    // Remove final ", "

            // WHERE/AND Clause
            string filter = LookupValues.Select( c => c.ColumnName).Aggregate<string, string>(
                "", 
                (str, colName) => str + ((str == "") ? "WHERE " : "AND ") + colName + " = @" + colName + " ");

            // Identify Sort Column (Begin Temporal Datestamp)
            string orderColumnName = results.TableColumns.FirstOrDefault(c => c.GeneratedType == 1).ColumnName;

            // Build SQL Command
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT {columns} ");
            sql.Append($"FROM {TemporalTableInfo.BaseSchemaName}.{TemporalTableInfo.BaseTableName} with(nolock) ");
            sql.Append(filter);
            sql.Append(" UNION ");
            sql.Append($"SELECT {columns} ");
            sql.Append($"FROM {TemporalTableInfo.HistorySchemaName}.{TemporalTableInfo.HistoryTableName} with(nolock) ");
            sql.Append(filter);
            sql.Append($"ORDER BY {orderColumnName} DESC");

            return sql.ToString();
        }

        /// <summary>
        /// PopulateDiffInfo() - Populate the difference indicators for each history row
        /// </summary>
        /// <param name="results">object containing the results, which includes the history rows</param>
        protected void PopulateDiffInfo(TemporalViewerResults results)
        {
            dynamic newInfo;
            dynamic oldInfo;
            List<bool> diffRow;
            CompareDictionary dict = new CompareDictionary();

            // Iterate over the History Rows to compare the row to the prior state (as rows will be in time descending order)
            for (int i = 0; i < (results.HistoryInfo.Count - 1); i++)
            {
                newInfo = results.HistoryInfo[i];
                oldInfo = results.HistoryInfo[i + 1];
                diffRow = new List<bool>();

                dict.PopulateCompareDictionary(newInfo, true);
                dict.PopulateCompareDictionary(oldInfo, false);

                // Iterate over the columns to identify changes.
                foreach (var col in results.TableColumns)
                {
                    // Ignore Generated Columns (Temporal Timestamps)
                    if (col.GeneratedType == 0)
                    {
                        diffRow.Add(dict.CompareDict[col.ColumnName].NewValue != dict.CompareDict[col.ColumnName].OldValue);
                    }
                    else
                    {
                        diffRow.Add(false);
                    }
                }
                results.DiffInds.Add(diffRow);
            }

            // Create final Diff row (all false as this is the start)
            diffRow = new List<bool>();
            diffRow.AddRange(Enumerable.Repeat(false, results.TableColumns.Count));
            results.DiffInds.Add(diffRow);
        }

        /// <summary>
        /// PopulateCompareDictionary() - Populates the Compare Dictionary for the Old or New Column info
        /// </summary>
        /// <param name="compareDict">Compare Dictionary to be populated</param>
        /// <param name="rowIinfo">Row Info (dynamic Object)</param>
        /// <param name="isNew">New/Old indicator</param>
        protected static void PopulateCompareDictionary(Dictionary<string, ColumnCompare> compareDict, dynamic rowIinfo, bool isNew)
        {
            foreach (var col in rowIinfo)
            {
                var x = col.Key;
                var y = col.Value;
                if (!compareDict.ContainsKey(col.Key))
                {
                    compareDict.Add(col.Key, new ColumnCompare());
                }

                if (isNew)
                {
                    compareDict[col.Key].NewValue = col.Value;
                }
                else
                {
                    compareDict[col.Key].OldValue = col.Value;
                }
            }
        }

        #endregion Protected

        #region ValidatorResult Class

        /// <summary>
        /// ValidatorResult Class - Used to pass Validation information.
        /// </summary>
        protected class ValidatorResult
        {
            public bool IsValid { get; set; }
            public dynamic InputValue { get; set; }

            public ValidatorResult()
            {
                IsValid = false;
                InputValue = null;
            }
        }

        #endregion ValidatorResult

        #region LookupParam Class

        /// <summary>
        /// LookupParam Class - Used to hold Lookup Parameter information.
        /// </summary>
        protected class LookupParam
        {
            public string ColumnName { get; set; }
            public string ColumnType { get; set; }
            public dynamic InputValue { get; set; }

            public LookupParam()
            {
                ColumnName = "";
                ColumnType = "";
                InputValue = null;
            }
        }

        #endregion LookupParam

    }
}
