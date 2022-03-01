# DataLayerGen

This C# Web API project can be used to retrieve Temporal information from a Microsoft SQL Server Database.

API Methds include
* /Schema/TemporalTables - Retrieve list of Temporal Tables (Base and History) in the Database
* /Schema/TemporalTableByName - Get Temporal Table info (Base and History) for a give table (Schema and Table Name)
* /Schema/PrimaryKeys - Get list of Primary Key field info for requested table (Schema and Table Name)
* /Schema/TableColumnsById - Get list of Column info for a requested Table Object Id
* /Schema/TableColumnsByName - Get list of Column info for a requested Table (Schema and Table Name).
* /TemporalInfo/GetData - Get Temporal information for a given Primary Key from the requested Temporal base and history tables
* /TemporalInfo/GetDelta - Gets changed column name and old/new values for a given Primary Key from the requested Temporal base and history tables
* /TemporalInfo/GetColumnHistory - Gets old/new values for a specific column for a given Primary Key in Temporal base and history tables