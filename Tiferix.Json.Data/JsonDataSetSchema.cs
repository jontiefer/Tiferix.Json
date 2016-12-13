/* Copyright © 2016 Jonathan Tiefer - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the GNU Lesser General Public License (LGPL)
 *
 * You should have received a copy of the LGPL license with
 * this file.
 *
 * /

/*  This file is part of the Tiferix.Json library.
*
*   Tiferix.Json is free software: you can redistribute it and/or modify
*   it under the terms of the GNU Lesser General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*   Tiferix.Json is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU Lesser General Public License for more details.
*
*  You should have received a copy of the GNU Lesser General Public License
*   along with the Tiferix.Json library.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Tiferix.Json.Tools;
using Tiferix.Global;

namespace Tiferix.Json.Data
{
    /// <summary>
    /// Contains all schema information associated with a Tiferix.JsonDataSet data file which will be used for serializing and deserializing Json data to and from 
    /// ADO.Net DataSet objects.  All JsonDataSetSchema information will be stored in a JsonDataSetSchema file which will serialized and deserialized 
    /// using the data contained int he JsonDataSetSchema class.   The JsonDataSetSchema class will contain a set of JsonTableSchema objects 
    /// containing the schema for each DataTable contained in the DataSet.
    /// </summary>
    public class JsonDataSetSchema
    {
        #region Member Variables
        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDataSetSchema()
        {
            try
            {
                Tables = new List<JsonTableSchema>();                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor function of JsonDataSetSchema class.");
            }
        }

        #endregion

        #region Dataset Schema Information Properties

        /// <summary>
        /// The name of the DataSet associated with the JsonDataSetSchema class.
        /// </summary>
        public string DataSetName { get; set; }

        #endregion

        #region Table Schema Properties, Functions

        /// <summary>
        /// A list of JsonTableSchema objects that containing the schema information for each DataTable contained within the DataSet associated with 
        /// the JsonDataSetSchema class.
        /// </summary>
        public List<JsonTableSchema> Tables { get; protected set; }               

        #endregion
    }

    #region Json Table/Column Schema Classes

    /// <summary>
    /// Contains all schema information specific to the table contained within a DataSet associated with a Tiferix.JsonDataSet data file.  The schema 
    /// information will be used for serializing and deserializing Json data to and from ADO.Net DataTable objects.  JsonTableSchema information will 
    /// be stored within a JsonDataSetSchema object and stored in a JsonDataSetSchema file or directly stored in a JsonTableSchema file if it is not 
    /// part of a dataset.  The JsonTableSchema class will contain a set of JsonColumnSchema objects containing the schema for each DataColumn 
    /// contained in the DataTable.
    /// </summary>
    public class JsonTableSchema
    {
        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonTableSchema()
        {
            try
            {
                Columns = new List<JsonColumnSchema>();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor function of JsonTableSchema class.");
            }
        }

        #endregion

        #region Table Schema Information Properties

        /// <summary>
        /// The name of the DataTable associated with the JsonTableSchema class.
        /// </summary>
        public string TableName { get; set; }

        #endregion

        #region Column Schema Properties, Functions

        /// <summary>
        /// A list of JsonColumnSchema objects that containing the schema information for each DataColumn contained within the DataTable associated with 
        /// the JsonTableSchema class.
        /// </summary>
        public List<JsonColumnSchema> Columns { get; protected set; }

        #endregion
    }

    /// <summary>
    /// Contains all schema information specific to the column contained within a DataTable associated with a Tiferix.JsonDataSet data file.  The 
    /// JsonColumnSchema object contains all field related information associated with an ADO.Net DataColumn, including field name, ADO.Net data 
    /// type, MaxLength, Null Allowance, etc.  Each column's JsonColumnSchema information will be stored within a JsonTableSchema object and stored 
    /// in the JsonDataSetSchema or JsonTableSchema file.  The JsonColumnSchema class will be used to serialize and deserialize Json data to and 
    /// from ADO.Net DataColumn objects of each table of the DataSet.
    /// </summary>
    public class JsonColumnSchema
    {
        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonColumnSchema()
        {
            try
            {                  
                ColumnName = "";
                DataType = typeof(string);
                PrimaryKey = false;
                Unique = false;
                MaxLength = 0;
                AllowDBNull = true;
                AutoIncrement = false;
                AutoIncrementSeed = 0;
                AutoIncrementStep = 1;
                Caption = "";
                DateTimeMode = DataSetDateTime.Local;
                DefaultValue = null;
                ReadOnly = false;
                Expression = "";          
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor function of JsonColumnSchema class.");
            }
        }

        #endregion

        #region Column Schema Propeties

        /// <summary>
        /// A column with the same name already exists in the collection. The name comparison is not case sensitive.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the type of data stored in the column.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Indicates if the field is the PrimaryKey column of the table.
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the values in each row of the column must be unique.
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of a text column.
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        ///  Gets or sets a value that indicates whether null values are allowed in this column for rows that belong to the table.        
        /// </summary>
        public bool AllowDBNull { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the column automatically increments the value of the column for new rows added to the table.
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        /// Gets or sets the starting value for a column that has its AutoIncrement property set to true. The default is 0.
        /// </summary>
        public long AutoIncrementSeed { get; set; }

        /// <summary>
        /// Gets or sets the increment used by a column with its System.Data.DataColumn.AutoIncrement property set to true.
        /// </summary>
        public long AutoIncrementStep { get; set; }


        /// <summary>
        /// Gets or sets the caption for the column.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the DateTimeMode for the column.
        /// </summary>
        public DataSetDateTime DateTimeMode { get; set; }

        /// <summary>
        /// Gets or sets the default value for the column when you are creating new rows.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the expression used to filter rows, calculate the values in a column, or create an aggregate column.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the column allows for changes as soon as a row has been added to the table.
        /// </summary>
        public bool ReadOnly { get; set; }        

        #endregion
    }

    #endregion
}
