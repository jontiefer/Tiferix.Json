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
using System.Runtime.Serialization.Json;
using Tiferix.Json.Data;
using Tiferix.Json.Tools;
using Tiferix.Global;

namespace Tiferix.Json.Serialization
{
    /// <summary>
    /// The JsonDataSetSchema class will be used to serialize and deserialize the schema information used to initialize DataSet objects and which are 
    /// also associated with Tiferix.JsonDataSet data files to and from Tiferix.JsonDataSetSchema files.  This class will utilize the JsonDataSetSchemaParser
    /// class to read and extract all schema data from the JsonDataSetSchema data files stored in a format compatible with the Tiferix.Json library.  
    /// In the case of serialization, the JsonDataSetSchemaSerializer class will utilize the JsonDataWriter class to read the schema information from 
    /// a DataSet object and extract and serialize the data to the appropriate format in a JsonDataSetSchema file.    
    /// </summary>
    public class JsonDataSetSchemaSerializer : IDisposable
    {
        #region Member Variables

        private bool m_blDisposed = false;

        #endregion

        #region Member Object Variables
        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDataSetSchemaSerializer()
        {
            try
            {
            }
            catch(Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor function of JsonDataSetSchemaSerializer class.");
            }
        }

        #endregion        

        #region Destruction/Cleanup

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_blDisposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //            
            m_blDisposed = true;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~JsonDataSetSchemaSerializer()
        {
            Dispose(false);
        }

        #endregion

        #region DataSet Json Schema Deserialization/Generation Properties, Functions

        /// <summary>
        /// Parses and deserializes the JsonDataSetSchema data stream into a DataSet object with the schema information stored in the data stream.
        /// The JsonDataSetSchemaParser class will be used to extract all the schema data in the stream into a JsonDataSetSchema object which then
        /// will be deserialized and used to initialize the schema of the DataSet object.
        /// </summary>
        /// <param name="streamSchema">The stream that will access the JsonDataSetSchema file data.</param>
        /// <param name="dsData">A reference to a DataSet that will be initialized with the schema data contained in the JsonDataSetSchema data stream.</param>
        /// <returns></returns>
        public virtual bool DeserializeJsonDataSetSchema(Stream streamSchema, ref DataSet dsData)
        {
            JsonDataSetSchemaParser jsParser = null;

            try
            {                
                jsParser = new JsonDataSetSchemaParser(streamSchema);
                JsonDataSetSchema dataSchema = jsParser.ParseJsonSchema();

                if (dataSchema == null)
                    throw new InvalidDataException("Json Data Schema file invalid.  Failed to parse Json Schema file.");

                dsData = new DataSet();                
                dsData.DataSetName = dataSchema.DataSetName;

                foreach (JsonTableSchema tblSchema in dataSchema.Tables)
                {
                    DataTable dtTable = DeserializeJsonTableSchema(tblSchema);
                    dsData.Tables.Add(dtTable);
                }//next tblSchema                

                return true;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonDataSetSchema Overload 1 function of JsonDataSetSchemaSerializer class.");
                return false;
            }
            finally
            {
                if (jsParser != null)
                    jsParser.Dispose();
            }
        }

        /// <summary>
        /// Parses and deserializes the JsonDataSetSchema data stream into a DataSet object with schema information stored in the data stream.
        /// The JsonDataSetSchemaParser class will be used to extract all the schema data in the stream into a JsonDataSetSchema object which then
        /// will be deserialized and used to initialize the schema of the DataSet object.
        /// </summary>
        /// <param name="streamSchema">The stream that will access the JsonDataSetSchema file data.</param>
        /// <returns></returns>
        public virtual DataSet DeserializeJsonDataSetSchema(Stream streamSchema)
        {            
            try
            {
                DataSet dsData = new DataSet();
                DeserializeJsonDataSetSchema(streamSchema, ref dsData);
                
                return dsData;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonDataSetSchema Overload 2 function of JsonDataSetSchemaSerializer class.");
                return null;
            }            
        }

        /// <summary>
        /// Parses and deserializes the JsonDataSetSchema file into a DataSet object with schema information stored in the schema file. 
        /// The JsonDataSetSchemaParser class will be used to extract all the schema data in the file into a JsonDataSetSchema object which then 
        /// will be deserialized and used to initialize the schema of the DataSet object.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonDataSetSchema file to be deserialized.</param>
        /// <param name="dsData">A reference to a DataSet that will be initialized with the schema data contained in the JsonDataSetSchema data file.</param>
        /// <returns></returns>
        public virtual bool DeserializeJsonDataSetSchema(string strFileName, ref DataSet dsData)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                return DeserializeJsonDataSetSchema(fs, ref dsData);                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonDataSetSchema Overload 3 function of JsonDataSetSchemaSerializer class.");
                return false;
            }
        }

        /// <summary>
        /// Parses and deserializes the JsonDataSetSchema file into a DataSet object with schema information stored in the schema file. 
        /// The JsonDataSetSchemaParser class will be used to extract all the schema data in the file into a JsonDataSetSchema object which then 
        /// will be deserialized and used to initialize the schema of the DataSet object.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonDataSetSchema file to be deserialized.</param>
        /// <returns></returns>
        public virtual DataSet DeserializeJsonDataSetSchema(string strFileName)
        {
            try
            {
                DataSet dsData = new DataSet();
                DeserializeJsonDataSetSchema(strFileName, ref dsData);

                return dsData;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonDataSetSchema Overload 4 function of JsonDataSetSchemaSerializer class.");
                return null;
            }
        }

        #endregion

        #region DataTable Json Schema Deserialization/Generation Properties, Functions

        /// <summary>
        /// An internal function that will deserialize the schema information contained in the JsonTableSchema passed to the function into an ADO.Net 
        /// DataTable object which will then be returned by the function.  The JsonTableSchema object should have been previously loaded from the 
        /// JsonDataSetSchema file before calling this function.  The function will properly convert, map and load all the appropriate columns and 
        /// table settings of the DataTable with the schema information stored in the JsonTableSchema object.
        /// </summary>
        /// <param name="jsonTblSchema">JsonTableSchema object containing the schema information to deserialize to a DataTable.</param>
        /// <returns></returns>
        protected virtual DataTable DeserializeJsonTableSchema(JsonTableSchema jsonTblSchema)
        {
            try
            {
                DataTable dtTable = new DataTable(jsonTblSchema.TableName);
                List<DataColumn> lstPrimaryKeyCols = new List<DataColumn>();

                foreach (JsonColumnSchema jsonColSchema in jsonTblSchema.Columns)
                {
                    DataColumn column = new DataColumn();
                    column.ColumnName = jsonColSchema.ColumnName;
                    column.DataType = jsonColSchema.DataType;
                    column.Unique = jsonColSchema.Unique;

                    if(column.DataType == typeof(string) && jsonColSchema.MaxLength > 0)
                        column.MaxLength = jsonColSchema.MaxLength;

                    column.AllowDBNull = jsonColSchema.AllowDBNull;
                    column.AutoIncrement = jsonColSchema.AutoIncrement;
                    column.AutoIncrementSeed = jsonColSchema.AutoIncrementSeed;

                    if(jsonColSchema.AutoIncrementStep > 0)
                        column.AutoIncrementStep = jsonColSchema.AutoIncrementStep;

                    if(jsonColSchema.Caption != "")
                        column.Caption = jsonColSchema.Caption;

                    if(jsonColSchema.DataType == typeof(DateTime))
                        column.DateTimeMode = jsonColSchema.DateTimeMode;

                    if (jsonColSchema.DefaultValue != null)
                        column.DefaultValue = jsonColSchema.DefaultValue;

                    column.Expression = jsonColSchema.Expression;
                    column.ReadOnly = jsonColSchema.ReadOnly;

                    if (jsonColSchema.PrimaryKey)
                        lstPrimaryKeyCols.Add(column);

                    dtTable.Columns.Add(column);
                }//next jsonColSchema

                if (lstPrimaryKeyCols.Count > 0)
                    dtTable.PrimaryKey = lstPrimaryKeyCols.ToArray();

                return dtTable;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonTableSchema Overload 1 function of JsonDataSetSchemaSerializer class.");
                return null;
            }            
        }

        /// <summary>
        /// Parses and deserializes the JsonTableSchema data stream into a DataTable object with the schema information stored in the data stream.
        /// The JsonDataSetSchemaParser class will be used to extract all the schema data in the stream into a JsonTableSchema object which then
        /// will be deserialized and used to initialize the schema of the DataTable object.
        /// </summary>
        /// <param name="streamSchema">The stream that will access the JsonTableSchema file data.</param>
        /// <returns></returns>
        public virtual DataTable DeserializeJsonTableSchema(Stream streamSchema)
        {
            JsonDataSetSchemaParser jsParser = null;

            try
            {
                jsParser = new JsonDataSetSchemaParser(streamSchema);
                JsonTableSchema tblSchema = jsParser.ParseJsonTableSchema();

                if(tblSchema == null)
                    throw new InvalidDataException("Json Data Schema file invalid.  Failed to parse Json Schema file.");

                DataTable dtTable = DeserializeJsonTableSchema(tblSchema);

                return dtTable;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonTableSchema Overload 2 function of JsonDataSetSchemaSerializer class.");
                return null;
            }
            finally
            {
                if (jsParser != null)
                    jsParser.Dispose();
            }
        }

        /// <summary>
        /// Parses and deserializes the JsonTableSchema file into a DataTable object with the schema information stored in the schema file.
        /// The JsonDataSetSchemaParser class will be used to extract all the schema data in the stream into a JsonTableSchema object which then
        /// will be deserialized and used to initialize the schema of the DataTable object.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonTableSchema file to deserialize.</param>
        /// <returns></returns>
        public virtual DataTable DeserializeJsonTableSchema(string strFileName)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                DataTable dtTable = DeserializeJsonTableSchema(fs);

                return dtTable;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonTableSchema Overload 3 function of JsonDataSetSchemaSerializer class.");
                return null;
            }
        }

        #endregion

        #region DataSet Json Schema Serialization/Generation Properties, Functions

        /// <summary>
        /// Serializes the schema of a DataSet into to a Tiferix JsonDataSetSchema object which can then be stored in a JsonDataSetSchema file.
        /// This version of the function will also be used internally to serialize a DataSet into a JsonDataSetSchema object before writing the schema 
        /// information to a data stream or file on disk.
        /// </summary>
        /// <param name="dsSchema">DataSet with schema to be serialized to JsonDataSetSchema object.</param>
        /// <returns></returns>
        public virtual JsonDataSetSchema SerializeDataSetSchema(DataSet dsSchema)
        {
            try
            {
                JsonDataSetSchema dataSchema = new Data.JsonDataSetSchema();
                dataSchema.DataSetName = dsSchema.DataSetName;

                foreach (DataTable dtSchema in dsSchema.Tables)
                {
                    JsonTableSchema tblSchema = SerializeTableSchema(dtSchema);
                    dataSchema.Tables.Add(tblSchema);
                }//next dtSchema

                return dataSchema;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataSetSchema Overload 1 function of JsonDataSetSchemaSerializer class.");
                return null;
            }
        }

        /// <summary>
        /// Serializes the schema of a DataSet to a Tiferix JsonDataSetSchema stream.  The function will utilize the JsonDataWriter object to extract all 
        /// data from the DataSet and write it the appropriate format into the Tiferix.JsonDataSetSchema data stream.   All .Net DataSet objects will first be 
        /// extracted and serialized into JsonDataSetSchema objects which will then be written to the JsonDataSchema data stream.
        /// </summary>
        /// <param name="streamSchema">JsonDataSetSchema data stream that will be used to serialize the schema information from the DataSet.</param>
        /// <param name="dsSchema">DataSet with schema to be serialized to JsonDataSetSchema data stream.</param>
        public virtual void SerializeDataSetSchema(Stream streamSchema, DataSet dsSchema)
        {
            try
            {
                JsonDataSetSchema dataSchema = SerializeDataSetSchema(dsSchema);

                JsonDataWriter jwrtSchema = new JsonDataWriter(streamSchema);

                //Writes opening '{' object bracket of Json file.
                jwrtSchema.WriteBeginObject();

                //"DataSet": "DataSetName",
                jwrtSchema.WritePropDataValue("DataSet", dataSchema.DataSetName, true);

                //"Tables": [
                jwrtSchema.WritePropertyName("Tables");
                jwrtSchema.WriteBeginArray(true);
                                
                //Loops through each table schema and writes it to the stream.
                for(int iTableIndex = 0; iTableIndex < dataSchema.Tables.Count; iTableIndex++)
                {
                    SerializeTableSchema(jwrtSchema, dataSchema.Tables[iTableIndex]);

                    if (iTableIndex < dataSchema.Tables.Count - 1)
                        //Writes comma field delimiter after the table end object to separate each table record.
                        jwrtSchema.WriteFieldDelimiter();
                    else
                        //The final table record will have a closing object bracket without a comma.
                        jwrtSchema.WriteNewLine();
                }//next 

                //Writes closing ']' of tables array.
                jwrtSchema.WriteEndArray();

                //Writes closing '}' object bracket of Json file.
                jwrtSchema.WriteEndObject();                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataSetSchema Overload 2 function of JsonDataSetSchemaSerializer class.");                
            }
        }

        /// <summary>
        /// Serializes the schema of a DataSet to a Tiferix JsonDataSetSchema file.  The function will utilize the JsonDataWriter object to extract all 
        /// data from the DataSet and write it the appropriate format into the Tiferix.JsonDataSet file.   All .Net DataSet objects will first be 
        /// extracted and serialized into JsonDataSetSchema objects which will then be written to JsonDataSchema file.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonDataSetSchema file to be serialized with the schema information of the
        /// DataSet.</param>
        /// <param name="dsSchema">DataSet with schema to be serialized to JsonDataSetSchema file.</param>
        public virtual void SerializeDataSetSchema(string strFileName, DataSet dsSchema)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                SerializeDataSetSchema(fs, dsSchema);

                fs.Close();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataSetSchema Overload 3 function of JsonDataSetSchemaSerializer class.");                
            }
        }

        #endregion

        #region DataTable Json Schema Serialization/Generation Properties, Functions

        /// <summary>
        /// Serializes the schema of a DataTable into to a Tiferix JsonTableSchema object which can then be stored in a JsonDataSetSchema file.
        /// This version of the function will also be used internally to serialize a DataSet or DataTable into a JsonDataSetSchema/JsonTableSchema
        /// object before writing the schema information to a data stream or file on disk.
        /// </summary>
        /// <param name="dtSchema">DataTable with schema to be serialized to JsonTableSchema object.</param>
        /// <returns></returns>
        public virtual JsonTableSchema SerializeTableSchema(DataTable dtSchema)
        {
            try
            {
                JsonTableSchema tblSchema = new Data.JsonTableSchema();

                foreach (DataColumn column in dtSchema.Columns)
                {
                    JsonColumnSchema colSchema = new JsonColumnSchema();
                    colSchema.ColumnName = column.ColumnName;
                    colSchema.DataType = column.DataType;
                    colSchema.Unique = column.Unique;
                    colSchema.MaxLength = column.MaxLength;
                    colSchema.AllowDBNull = column.AllowDBNull;
                    colSchema.AutoIncrement = column.AutoIncrement;
                    colSchema.AutoIncrementSeed = column.AutoIncrementSeed;
                    colSchema.AutoIncrementStep = column.AutoIncrementStep;
                    colSchema.Caption = column.Caption;
                    colSchema.DateTimeMode = column.DateTimeMode;                    
                    colSchema.DefaultValue = column.DefaultValue;
                    colSchema.Expression = column.Expression;
                    colSchema.ReadOnly = column.ReadOnly;

                    if (dtSchema.PrimaryKey != null)
                    {
                        if (dtSchema.PrimaryKey.Contains(column))
                            colSchema.PrimaryKey = true;
                    }//end if

                    tblSchema.Columns.Add(colSchema);
                }//next colSchema

                return tblSchema;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeTableSchema Overload 1 function of JsonDataSetSchemaSerializer class.");
                return null;
            }
        }

        /// <summary>
        /// An internal function that will use a JsonDataWriter object to serialize the data stored in a JsonTableSchema object to a JsonDataSetSchema 
        /// data stream or file.  The JsonTableSchema object will have been previously loaded before the function is called and the JsonDataWriter
        /// will be positioned at the next table schema record in the data stream.
        /// </summary>
        /// <param name="jwrtSchema">JsonDataWriter object used for serializing the schema information of the DataTable to the JsonDataSetSchema
        /// data stream.</param>
        /// <param name="tblSchema">The JsonTableSchema object containing all the schema information of a DataTable to serialize to the JsonDataSetSchema 
        /// data stream.</param>
        protected virtual void SerializeTableSchema(JsonDataWriter jwrtSchema, JsonTableSchema tblSchema)
        {
            try
            {
                //Writes opening '{' object bracket of either Tables schema array (of DataSet Schema file) or of the entire table schema file.
                jwrtSchema.WriteBeginObject();

                //"Table": "Table Name",
                jwrtSchema.WritePropDataValue("Table", tblSchema.TableName, true);

                //"Columns": [
                jwrtSchema.WritePropertyName("Columns");
                jwrtSchema.WriteBeginArray(true);
                
                //Loops through each column of the table schema and serializes the data to the stream.
                for(int iColIndex = 0; iColIndex < tblSchema.Columns.Count; iColIndex++)
                {
                    JsonColumnSchema colSchema = tblSchema.Columns[iColIndex];

                    //Write opening '{' object bracket for the column schema.
                    jwrtSchema.WriteBeginObject();

                    jwrtSchema.WritePropDataValue("ColumnName", colSchema.ColumnName, true);
                    jwrtSchema.WritePropDataValue("DataType", JsonDataUtils.ConvertToJsonDataType(colSchema.DataType), true);
                    jwrtSchema.WritePropDataValue("PrimaryKey", colSchema.PrimaryKey, true);
                    jwrtSchema.WritePropDataValue("Unique", colSchema.Unique, true);
                    jwrtSchema.WritePropDataValue("MaxLength", colSchema.MaxLength, true);
                    jwrtSchema.WritePropDataValue("AllowDBNull", colSchema.AllowDBNull, true);
                    jwrtSchema.WritePropDataValue("AutoIncrement", colSchema.AutoIncrement, true);
                    jwrtSchema.WritePropDataValue("AutoIncrementSeed", colSchema.AutoIncrementSeed, true);
                    jwrtSchema.WritePropDataValue("AutoIncrementStep", colSchema.AutoIncrementStep, true);
                    jwrtSchema.WritePropDataValue("Caption", colSchema.Caption, true);
                    jwrtSchema.WritePropDataValue("DateTimeMode", colSchema.DateTimeMode.ToString(), true);

                    if (colSchema.DefaultValue != null)
                        jwrtSchema.WritePropDataValue("DefaultValue", colSchema.DefaultValue.ToString(), true);
                    else
                        jwrtSchema.WritePropDataNullValue("DefaultValue", true);

                    jwrtSchema.WritePropDataValue("ReadOnly", colSchema.ReadOnly);

                    //Write closing '}' object bracket for the column schema.
                    jwrtSchema.WriteEndObject();

                    if (iColIndex < tblSchema.Columns.Count - 1)
                        //Writes comma field delimiter after the column end object to separate each column record.
                        jwrtSchema.WriteFieldDelimiter();
                    else
                        //The final column record will have a closing object bracket without a comma.
                        jwrtSchema.WriteNewLine();
                }//next iColIndex

                //Writes closing ']' of columns array.
                jwrtSchema.WriteEndArray();

                //Writes closing '}' object bracket of either Tables schema array (of DataSet Schema file) or of the entire table schema file.
                jwrtSchema.WriteEndObject();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeTableSchema Overload 2 function of JsonDataSetSchemaSerializer class.");                
            }
        }

        /// <summary>
        /// Serializes the schema of a DataTable to a Tiferix JsonTableSchema stream.  The function will utilize the JsonDataWriter object to extract all 
        /// data from the DataTable and write it the appropriate format into the Tiferix.JsonTableSchema data stream.   All .Net DataTable objects will
        /// first be extracted and serialized into JsonTableSchema objects which will then be written to the JsonTableSchema data stream.
        /// </summary>
        /// <param name="streamSchema">JsonTableSchema data stream that will be used to serialize the schema information from the DataTable.</param>
        /// <param name="dtSchema">DataTable with schema to be serialized to JsonTableSchema data stream.</param>
        public virtual void SerializeTableSchema(Stream streamSchema, DataTable dtSchema)
        {
            try
            {
                JsonTableSchema tblSchema = SerializeTableSchema(dtSchema);
                JsonDataWriter jwrtSchema = new JsonDataWriter(streamSchema);

                SerializeTableSchema(jwrtSchema, tblSchema);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeTableSchema Overload 2 function of JsonDataSetSchemaSerializer class.");                
            }
        }

        /// <summary>
        /// Serializes the schema of a DataTable to a Tiferix JsonTableSchema file.  The function will utilize the JsonDataWriter object to extract all 
        /// data from the DataTable and write it the appropriate format into the Tiferix.JsonTableSchema file.   All .Net DataTable objects will
        /// first be extracted and serialized into JsonTableSchema objects which will then be written to the JsonTableSchema file.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonTableSchema file to be serialized with the schema information of the
        /// DataTable.</param>
        /// <param name="dtSchema">DataTable with schema to be serialized to JsonTableSchema file.</param>
        public virtual void SerializeTableSchema(string strFileName, DataTable dtSchema)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                SerializeTableSchema(fs, dtSchema);

                fs.Close();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeTableSchema Overload 3 function of JsonDataSetSchemaSerializer class.");                
            }
        }

        #endregion
    }  
}
