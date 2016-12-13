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
using System.Globalization;
using Tiferix.Json.Data;
using Tiferix.Json.Tools;
using Tiferix.Global;

namespace Tiferix.Json.Serialization
{
    /// <summary>
    /// The JsonDataSetSerializer class will be used to serialize and deserialize the data contained in DataSet objects to and from Tiferix.JsonDataSet 
    /// data files.   This class will utilize the JsonDataSetParser class to read and extract all data from the JsonDataSet data files stored in a 
    /// format compatible with the Tiferix.Json library.   In the case of serialization, the JsonDataSetSerializer class will utilize the JsonDataWriter class 
    /// to read all the data from a DataSet object and extract and serialize the data to the appropriate format in a JsonDataSet data file.
    /// NOTE: The JsonDataSetSerializer class only handles serialization and deserialization of data to and from ADO.Net DataSets.  In future versions 
    /// a Serializer library will be constructed to handle serializing and deserializing data to and from .Net classes and various types of objects.
    /// </summary>
    public class JsonDataSetSerializer : IDisposable
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
        public JsonDataSetSerializer()
        {
            try
            {
                InitializeDataSetSerializer();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor function of JsonDataSetSerializer class.");
            }
        }

        /// <summary>
        /// Initializes the JsonDataSetSerializer class with the appropriate default serialization and deserialization settings.
        /// </summary>
        protected virtual void InitializeDataSetSerializer()
        {
            try
            {
                FieldDelimiter = ',';
                this.Encoding = Encoding.UTF8;
                DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss");
                DateTimeZone = DateTimeKind.Unspecified;
                Culture = CultureInfo.InvariantCulture;
                AutoIdent = true;
                IdentChar = ' ';
                IdentSpacing = 2;                                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in InitializeDataWriter function of JsonDataSetSerializer class.");
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
        ~JsonDataSetSerializer()
        {
            Dispose(false);
        }

        #endregion

        #region Json General Formatting and Encoding Properties, Functions

        /// <summary>
        /// The character used for separating invididual fields of a table in the Json DataSet data files.  The default fielf delimiter for separating table fields is a 
        /// comma character.
        /// </summary>
        public char FieldDelimiter { get; set; }

        /// <summary>
        /// The character encoding to be used for the Json DataSet data files, which are always stored in string format.  This will be the character encoding 
        /// used for the entire Json data file.  The default character encoding is UTF8.
        /// </summary>
        public Encoding Encoding { get; protected set; }

        /// <summary>
        /// The format to use for storing Date/Time values in the Json DataSet data file.
        /// </summary>
        public DateTimeFormat DateTimeFormat { get; set; }

        /// <summary>
        /// Indicates which time zone will be associated with the Date/Time values stored in the Json DataSet data file.
        /// </summary>
        public DateTimeKind DateTimeZone { get; set; }

        /// <summary>
        /// The specific culture or locale  associated with the data stored in the Json DataSet data file.
        /// </summary>
        public CultureInfo Culture { get; set; }

        #endregion

        #region Json Identation Formatting Properties, Functions

        /// <summary>
        /// Indicates if the JsonDataWriter used for serializing data in the JsonDataSetSerialize class will automatically handling the identation of the 
        /// Json data as it is written to the Json DataSet data file.  If AutoIdent is set to true, then the JsonDataWriter class will keep track of the identation 
        /// of the various nested blocks of data and handling the identation of the various nested blocks will not be neccessary.   
        /// NOTE: Certain format restrictions apply, when AutoIdent mode is set to true.
        [DefaultValue(true)]
        public bool AutoIdent { get; set; }

        /// <summary>
        /// The number of identation characters (usually a space) to use to ident each line of data in the Json DataSet data file.  Each time an identation string 
        /// is written, it will use the number of identation spaces specified in this property.  The default number of indentation spaces is 2.
        /// </summary>
        [DefaultValue(2)]
        public int IdentSpacing { get; set; }

        /// <summary>
        /// The character used which will represent an identation space in the Json DataSet data file.  Each time an identation string is written, this character 
        /// will be written to the Json DataSet data file, using the number of identation spaces specified in the class.  The default identation character is a space.
        /// </summary>
        [DefaultValue(' ')]
        public char IdentChar { get; set; }

        #endregion

        #region DataSet Json Data Deserialization Properties, Functions

        /// <summary>
        /// Parses and deserializes a JsonDataSet data stream into a DataSet object that has been initialized with schema that matches the JsonDataSet 
        /// data stream.  The JsonDataSetParser class will be initialized with the appropriate settings and used to read and extract all the appropriate 
        /// data stored in the JsonDataSet data stream into the matching tables and records of the DataSet object passed to the function.
        /// </summary>
        /// <param name="streamJson">The stream that will access the JsonDataSet data file data.</param>
        /// <param name="dsDataSet">A reference to a schema filled DataSet that will be loaded with the data from the JsonDataSet data file.</param>
        /// <param name="encoding">The character encoding used for the Json DataSet data stream being deserialized.  The default character 
        /// encoding is UTF8.</param>
        /// <param name="blClearDataSet">Clears the DataSet to be loaded of all previous data it contains before proceeding with the deserialization 
        /// operation.</param>
        /// <returns></returns>
        public virtual bool DeserializeJsonDataSet(Stream streamJson, ref DataSet dsDataSet, Encoding encoding = null, bool blClearDataSet = true)
        {
            JsonDataSetParser jdsParser = null;

            try
            {
                if(blClearDataSet)
                    dsDataSet.Clear();

                jdsParser = new JsonDataSetParser(dsDataSet);
                jdsParser.DateTimeFormat = DateTimeFormat;

                bool blRtnVal = jdsParser.ParseJsonData(streamJson, encoding);

                return blRtnVal;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonDataSet Overload 1 function of JsonDataSetSerializer class.");
                return false;
            }
            finally
            {
                if (jdsParser != null)
                    jdsParser.Dispose();
            }
        }

        /// <summary>
        /// Parses and deserializes a JsonDataSet data file into a DataSet object that has been initialized with schema that matches the JsonDataSet 
        /// data file.  The JsonDataSetParser class will be initialized with the appropriate settings and used to read and extract all the appropriate 
        /// data stored in the JsonDataSet data file into the matching tables and records of the DataSet object passed to the function.
        /// </summary>
        /// <param name="strFileName">The full name and path to the JsonDataSet data file to be deserialized.</param>
        /// <param name="dsDataSet">A reference to a schema filled DataSet that will be loaded with the data from the JsonDataSet data file.</param>
        /// <param name="encoding">The character encoding used for the Json DataSet data stream being deserialized.  The default character 
        /// encoding is UTF8.</param>
        /// <param name="blClearDataSet">Clears the DataSet to be loaded of all previous data it contains before proceeding with the deserialization 
        /// operation.</param>
        /// <returns></returns>
        public virtual bool DeserializeJsonDataSet(string strFileName, ref DataSet dsDataSet, Encoding encoding = null, bool blClearDataSet = true)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                return DeserializeJsonDataSet(fs, ref dsDataSet, encoding, blClearDataSet);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonDataSet Overload 2 function of JsonDataSetSerializer class.");
                return false;
            }
        }

        #endregion

        #region DataTable Json Data Deserialization Properties, Functions

        /// <summary>
        /// Parses and deserializes a JsonTable data stream into a DataTable object that has been initialized with schema that matches the JsonDataTable 
        /// data stream.  The JsonDataSetParser class will be initialized with the appropriate settings and used to read and extract all the appropriate 
        /// data stored in the JsonTable data stream into the matching records of the DataTable object passed to the function.
        /// </summary>
        /// <param name="streamJson">The stream that will access the JsonTable data file data.</param>
        /// <param name="dtTable">A reference to a schema filled DataTable that will be loaded with the data from the JsonTable data file.</param>
        /// <param name="encoding">The character encoding used for the Json Table data stream being deserialized.  The default character 
        /// encoding is UTF8.</param>
        /// <param name="blClearTable">Clears the DataTable to be loaded of all previous data it contains before proceeding with the deserialization 
        /// operation.</param>
        public virtual bool DeserializeJsonTable(Stream streamJson, ref DataTable dtTable, Encoding encoding = null, bool blClearTable = true)
        {
            JsonDataSetParser jdsParser = null;

            try
            {
                if (blClearTable)
                    dtTable.Clear();

                jdsParser = new Data.JsonDataSetParser(dtTable);
                jdsParser.DateTimeFormat = DateTimeFormat;

                return jdsParser.ParseJsonTable(streamJson, encoding);                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonTable Overload 1 function of JsonDataSetSchemaSerializer class.");
                return false;
            }
            finally
            {
                if (jdsParser != null)
                    jdsParser.Dispose();
            }
        }

        /// <summary>
        /// Parses and deserializes a JsonTable data file into a DataTable object that has been initialized with schema that matches the JsonTable 
        /// data file.  The JsonDataSetParser class will be initialized with the appropriate settings and used to read and extract all the appropriate 
        /// data stored in the JsonTable data file into the matching records of the DataTable object passed to the function.
        /// </summary>
        /// <param name="strFileName">The full name and path to the JsonTable data file to be deserialized.</param>
        /// <param name="dtTable">A reference to a schema filled DataTable that will be loaded with the data from the JsonTable data file.</param>
        /// <param name="encoding">The character encoding used for the Json Table data stream being deserialized.  The default character 
        /// encoding is UTF8.</param>
        /// <param name="blClearTable">Clears the DataTable to be loaded of all previous data it contains before proceeding with the deserialization 
        /// operation.</param>
        /// <returns></returns>
        public virtual bool DeserializeJsonTable(string strFileName, ref DataTable dtTable, Encoding encoding = null, bool blClearTable = true)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                return DeserializeJsonTable(fs, ref dtTable, encoding, blClearTable);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in DeserializeJsonTable Overload 2 function of JsonDataSetSerializer class.");
                return false;
            }
        }

        #endregion

        #region DataSet Json Data Serialization Properties, Functions

        /// <summary>
        /// Serializes the data in a DataSet to a Tiferix JsonDataSet data stream.  The function will utilize the JsonDataWriter object to extract all data from the
        /// DataSet and write it the appropriate format into the Tiferix.JsonDataSet data stream.  Before serializing the DataSet, all formatting settings must be set 
        /// in the JsonDataSetSerializer class beforehand and it will be required that the same formatting settings bet set when deserializing the
        /// JsonDataSet data file.
        /// </summary>
        /// <param name="stream">JsonDataSet data stream that will be used to serialize data from the DataSet.</param>
        /// <param name="dsDataSet">The DataSet object to be serialized to JsonDataSet data stream.</param>
        public virtual void SerializeDataSet(Stream stream, DataSet dsDataSet)
        {
            try
            {                                
                JsonDataWriter jwrtData = new JsonDataWriter(stream, Encoding);
                InitJsonDataWriter(jwrtData);

                //Writes opening '{' object bracket of the Json file.
                jwrtData.WriteBeginObject();

                //"DataSetName": [
                jwrtData.WritePropertyName(dsDataSet.DataSetName);
                jwrtData.WriteBeginArray();
                
                int iTableIndex = 0;
                int iTableCount = dsDataSet.Tables.Count;

                //Loops through each data table and writes it to the stream.
                foreach (DataTable dtTable in dsDataSet.Tables)
                {
                    SerializeDataTable(jwrtData, dtTable);

                    if (iTableIndex < iTableCount - 1)
                        jwrtData.WriteFieldDelimiter(false);

                    jwrtData.WriteNewLine();

                    iTableIndex++;
                }//next dtTable

                //Writes closing ']' of tables array of the dataset.
                jwrtData.WriteEndArray();

                //Write closing '}' object bracket of the Json file.
                jwrtData.WriteEndObject();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataSet Overload 1 function of JsonDataSetSerializer class.");
            }
        }

        /// <summary>
        /// Serializes the data in a DataSet to a Tiferix JsonDataSet data file.  The function will utilize the JsonDataWriter object to extract all data from the
        /// DataSet and write it the appropriate format into the Tiferix.JsonDataSet data file.  Before serializing the DataSet, all formatting settings must be set 
        /// in the JsonDataSetSerializer class beforehand and it will be required that the same formatting settings bet set when deserializing the
        /// JsonDataSet data file.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonDataSet data file that will be serialized with the data in the DataSet.</param>
        /// <param name="dsDataSet">The DataSet object to be serialized to JsonDataSet data file.</param>
        public virtual void SerializeDataSet(string strFileName, DataSet dsDataSet)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Create);
                SerializeDataSet(fs, dsDataSet);

                fs.Flush();
                fs.Close();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataSet Overload 2 function of JsonDataSetSerializer class.");
            }
        }

        #endregion

        #region DataTable Json Data Serialization Properties, Functions

        /// <summary>
        /// An internal function that will serialize the data of each DataTable contained in the DataSet being serialized into the appropriate Table record 
        /// in the JsonDataSet data stream.  The JsonDataWriter will contain the active stream of data that should be set at the position where the 
        /// table record and its data will be writen in the JsonDataSet data stream.   
        /// </summary>
        /// <param name="jwrtData">JsonDataWriter object used for serializing DataSet data to JsonDataSet data stream.</param>
        /// <param name="dtTable">The DataTable to be serialized to the JsonDataSet data stream.</param>
        protected virtual void SerializeDataTable(JsonDataWriter jwrtData, DataTable dtTable)
        {
            try
            {
                //Writes opening '{' object bracket of either Tables schema array (of DataSet Schema file) or of the entire table schema file.
                jwrtData.WriteBeginObject();

                //"Table Name": [
                jwrtData.WritePropertyName(dtTable.TableName);
                jwrtData.WriteBeginArray();

                int iRowIndex = 0;
                int iRowCount = dtTable.Rows.Count;

                foreach (DataRow rowData in dtTable.Rows)
                {
                    SerializeDataRowValues(jwrtData, rowData);

                    if (iRowIndex < iRowCount - 1)
                        jwrtData.WriteFieldDelimiter(false);

                    jwrtData.WriteNewLine();

                    iRowIndex++;
                }//next rowData
                
                //Writes closing ']' of table rows array.
                jwrtData.WriteEndArray();

                //Writes closing '}' object bracket of either a table in the Tables data array (of DataSet data file) or of the entire table data file.
                jwrtData.WriteEndObject();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataTable Overload 1 function of JsonDataSetSerializer class.");
            }
        }

        /// <summary>
        /// Serializes the data in a DataTable to a Tiferix JsonTable data stream.  The function will utilize the JsonDataWriter object to extract all data from the
        /// DataTable and write it the appropriate format into the Tiferix.JsonTable data stream.  Before serializing the DataTable, all formatting settings must be set 
        /// in the JsonDataSetSerializer class beforehand and it will be required that the same formatting settings bet set when deserializing the
        /// JsonTable data file.
        /// </summary>
        /// <param name="stream">JsonTable data stream that will be used to serialize data from the DataTable.</param>
        /// <param name="dtTable">The DataTable object to be serialized to JsonTable data stream.</param>
        public virtual void SerializeDataTable(Stream stream, DataTable dtTable)
        {
            try
            {                
                JsonDataWriter jwrtData = new JsonDataWriter(stream);
                InitJsonDataWriter(jwrtData);

                SerializeDataTable(jwrtData, dtTable);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataTable Overload 2 function of JsonDataSetSerializer class.");
            }
        }

        /// <summary>
        /// Serializes the data in a DataTable to a Tiferix JsonTable data file.  The function will utilize the JsonDataWriter object to extract all data from the
        /// DataTable and write it the appropriate format into the Tiferix.JsonTable data file.  Before serializing the DataTable, all formatting settings must be set 
        /// in the JsonDataSetSerializer class beforehand and it will be required that the same formatting settings bet set when deserializing the
        /// JsonTable data file.
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonTable data file that will be serialized with the data in the DataTable.</param>
        /// <param name="dtTable">The DataTable object to be serialized to JsonTable data stream.</param>
        public virtual void SerializeDataTable(string strFileName, DataTable dtTable)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                SerializeDataTable(fs, dtTable);

                fs.Close();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataTable Overload 3 function of JsonDataSetSerializer class.");
            }
        }

        #endregion

        #region Data Row/Data Column Data Serialization Properties, Functions

        /// <summary>
        /// Serializes the data in the current DataRow of the current DataTable to the JsonDataSet (or JsonTable) data stream being serialized in the
        /// JsonDataSetSerializer class.   The JsonDataWriter object passed to the function will be positioned at the next record of the current table 
        /// being written in the JsonDataSet data stream.  This function will use the JsonDataWriter to extract the field and value data out of the 
        /// current DataRow and convert it to the appropriate values to store in the JsonDataSet data stream.
        /// </summary>
        /// <param name="jwrtData">JsonDataWriter object used for serializing DataSet/DataTable data to JsonDataSet/JsonTable data stream.</param>
        /// <param name="rowData">The current row to be serialized to the JsonDataSet data stream.</param>
        protected virtual void SerializeDataRowValues(JsonDataWriter jwrtData, DataRow rowData)
        {
            try
            {
                //Write opening '{' object bracket for the data row.
                jwrtData.WriteBeginObject();

                //Loops through each column of the table schema and serializes the data to the stream.
                for (int iColIndex = 0; iColIndex < rowData.Table.Columns.Count; iColIndex++)
                {
                    jwrtData.WritePropDataValue(rowData.Table.Columns[iColIndex].ColumnName, rowData[iColIndex], false, false);

                    if (iColIndex < rowData.Table.Columns.Count - 1)
                    {
                        //Writes comma field delimiter after the column end object to separate each column of the data row.  Each field will be written on a 
                        //new line.
                        jwrtData.WriteFieldDelimiter(true);                        
                    }
                    else
                        //The final column of the data row will have a closing object bracket without a comma.
                        jwrtData.WriteNewLine();
                }//next iColIndex

                //Write closing '}' object bracket for the row data.
                jwrtData.WriteEndObject();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in SerializeDataRowValues function of JsonDataSetSerializer class.");
            }
        }

        #endregion

        #region Linked Json Data Parser and Writer Properties, Functions

        /// <summary>
        /// Initializes the JsonDataWriter object that will be used to serialize the DataSet with the default formatting, encoding and other serialization
        /// settings that will be used for serializing the DataSet data to JsonDataSet data file.
        /// </summary>
        /// <param name="jwrtData">JsonDataWriter object used for serializing DataSet/DataTable data to JsonDataSet/JsonTable data stream.</param>
        protected virtual void InitJsonDataWriter(JsonDataWriter jwrtData)
        {
            try
            {
                jwrtData.FieldDelimiter = FieldDelimiter;                
                jwrtData.DateTimeFormat = DateTimeFormat;
                jwrtData.DateTimeZone = DateTimeZone;
                jwrtData.Culture = Culture;
                jwrtData.AutoIdent = AutoIdent;
                jwrtData.IdentSpacing = IdentSpacing;
                jwrtData.IdentChar = IdentChar;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in InitJsonDataWriter function of JsonDataSetSerializer class.");
            }
        }        

        #endregion
    }
}
