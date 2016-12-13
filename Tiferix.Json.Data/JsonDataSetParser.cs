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
    /// The JsonDataSetParser class parses a Tiferix JsonDataSet data file that matches the schema of an ADO.NET DataSet object.  All data in the
    /// DataSet file, including each DataTable will be parsed from the JsonDataSet data file and loaded into the matching table in the DataSet.    
    /// </summary>
    public class JsonDataSetParser : IDisposable
    {
        #region Member Variables

        private bool m_blDisposed = false;

        #endregion

        #region Member Object Variables
        #endregion       
        
        #region ADO.NET Data Object Variables

        private object m_DataSource = null;

        private Dictionary<string, DataTable> m_dictTables = new Dictionary<string, DataTable>();
        private Dictionary<string, Dictionary<string, string>> m_dictTableColNames = new Dictionary<string, Dictionary<string, string>>();

        #endregion

        #region Json Data Variables

        private string m_strJsonData = "";

        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDataSetParser()
        {
            try
            {                
                DateTimeFormat = null;
                //DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss");
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 1 function of JsonDataSetParser class.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dsDataSet">The ADO.NET DataSet object that will be loaded from the JsonDataSet data file.  All schema must be loaded in
        /// the DataSet before parsing the JsonDataSet data file into the DataSet object.</param>
        public JsonDataSetParser(DataSet dsDataSet)            
        {
            try
            {
                DataSource = dsDataSet;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 4 function of JsonDataSetParser class.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dtTable">The ADO.NET DataTable object that will be loaded from the JsonDataSet data file.  All schema must be loaded in
        /// the DataTable before parsing the JsonDataSet data file into the DataTable object.</param>
        public JsonDataSetParser(DataTable dtTable)
        {
            try
            {
                DataSource = dtTable;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 5 function of JsonDataSetParser class.");
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
            m_strJsonData = "";            
            m_blDisposed = true;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~JsonDataSetParser()
        {
            Dispose(false);
        }

        #endregion

        #region JsonDataSet/DataTable Loading and Linking Properties, Functions

        /// <summary>
        /// The ADO.NET DataSource object that will be loaded from the JsonDataSet data file.  All schema must be loaded in the DataSource before 
        /// parsing the JsonDataSet data file into the DataSet or DataTable object.  Either a DataSet or DataTable can be directly linked and loaded 
        /// from the JsonDataSet parser class.
        /// </summary>
        public object DataSource
        {
            get
            {
                return m_DataSource;
            }
            set
            {
                try
                {
                    m_DataSource = value;

                    m_dictTables.Clear();
                    m_dictTableColNames.Clear();
                    int iTableCount = 0;

                    if (m_DataSource.GetType() == typeof(DataSet))
                    {                       
                        foreach (DataTable dtTable in ((DataSet)m_DataSource).Tables)
                        {
                            m_dictTables.Add(dtTable.TableName.ToUpper(), dtTable);
                            m_dictTableColNames.Add(dtTable.TableName.ToUpper(), new Dictionary<string, string>());                            
                        }//next dtTable

                        iTableCount = ((DataSet)m_DataSource).Tables.Count; 
                    }
                    else
                    {
                        DataTable dtTable = ((DataTable)m_DataSource);
                        m_dictTables.Add(dtTable.TableName.ToUpper(), dtTable);
                        m_dictTableColNames.Add(dtTable.TableName.ToUpper(), new Dictionary<string, string>());

                        iTableCount = 1;
                    }//end if
                    
                    for(int i = 0; i < iTableCount; i++)
                    {
                        Dictionary<string, string> dictColNames = null;
                        DataTable dtTable = null;

                        if (m_DataSource.GetType() == typeof(DataSet))
                            dtTable = ((DataSet)m_DataSource).Tables[i];
                        else
                            dtTable = (DataTable)m_DataSource;

                        dictColNames = m_dictTableColNames[dtTable.TableName.ToUpper()];

                        foreach (DataColumn colTable in dtTable.Columns)
                        {
                            dictColNames.Add(colTable.ColumnName.ToUpper(), colTable.ColumnName);
                        }//next colTable
                    }//next dictColNames
                }
                catch (Exception err)
                {
                    ErrorHandler.ShowErrorMessage(err, "Error in DataSource Set property of JsonDataSetParser class.");
                }                
            }
        }

        #endregion

        #region Json Data Set (Database Level) Parsing Properties, Functions

        /// <summary>
        /// Parses a stream of data linked to a Tiferix JsonDataSet data file and reads the data from the stream into the DataSet object linked to the 
        /// JsonDataSetParser class.  
        /// </summary>
        /// <param name="streamData">Stream pointing to JsonDataSet data.</param>
        /// <param name="encoding">Encoding of JsonDataSet data stream.  Default is UTF8.</param>
        /// <returns></returns>
        public virtual bool ParseJsonData(Stream streamData, Encoding encoding = null)
        {
            try
            {                
                if (DataSource.GetType() == typeof(DataTable))
                    throw new InvalidDataException("DataSet required as data source for parsing entire Json DataSet data file.");

                DataSet dsDataObj = (DataSet)DataSource;

                if (encoding == null)
                    encoding = Encoding.UTF8;

                StreamReader srdr = new StreamReader(streamData, encoding);
                m_strJsonData = srdr.ReadToEnd();
                srdr.Close();

                int iCurParseIndex = m_strJsonData.IndexOf(':', 0) + 1;
                int iEndParseIndex = 0;

                //Positions pointer of parser to the first table record in the Json Dataset file.
                iCurParseIndex = m_strJsonData.IndexOf('{', iCurParseIndex);

                string strTableName = "";

                bool blTableFound = true;                                
                
                while (blTableFound)
                {
                    iCurParseIndex = m_strJsonData.IndexOf("\"", iCurParseIndex) + 1;
                    iEndParseIndex = m_strJsonData.IndexOf("\"", iCurParseIndex);
                    strTableName = m_strJsonData.Substring(iCurParseIndex, iEndParseIndex - iCurParseIndex).ToUpper();
                    
                    if (!m_dictTables.ContainsKey(strTableName))
                        throw new InvalidDataException("Invalid data schema detected.  Table not found in Json data file.");

                    DataTable dtTable = m_dictTables[strTableName];

                    blTableFound = ParseJsonTable(dtTable, ref iCurParseIndex);
                }//end while

                return true;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonData Overload 1 function of JsonDataSetParser class.");
                return false;
            }
        }

        /// <summary>
        /// Parses the Tiferix JsonDataSet data file specified in the function and reads the data into the DataSet object linked to the 
        /// JsonDataSetParser class.  
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonDataSet data file.</param>
        /// <param name="encoding">Encoding of JsonDataSet data file.  Default is UTF8.</param>
        /// <returns></returns>
        public virtual bool ParseJsonData(string strFileName, Encoding encoding = null)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                return ParseJsonData(fs, encoding);                    
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonData Overload 2 function of JsonDataSetParser class.");
                return false;
            }
        }

        #endregion

        #region Json Data Table (Table Level) Parsing Properties, Functions

        /// <summary>
        /// An internal version of the ParseJsonTable function that parses the current DataTable of the JsonDataSet data file/stream being parsed by the 
        /// JsonDataSetParser class.  When a JsonDataSet DataSet or invididual DataTable is parsed, the current table in the Json DataSet file will 
        /// be parsed and extracted by this function into the appropriate DataTable object specified in the function's parameter.  The schema of the 
        /// DataTable must be set before parsing data from the JsonDataSet data file/stream.
        /// </summary>
        /// <param name="dtTable">Schema filled DataTable to be loaded with parsed data from JsonDataSet data.</param>
        /// <param name="iCurParseIndex">The current index of the JsonDataSet data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool ParseJsonTable(DataTable dtTable, ref int iCurParseIndex)
        {
            try
            {                                
                bool blRecordFound = true;
                
                while (blRecordFound)
                {
                    //Navigates to the next DataTable record in the JsonDataSet data stream.
                    iCurParseIndex = m_strJsonData.IndexOf('{', iCurParseIndex);

                    blRecordFound = ParseJsonRecord(dtTable, ref iCurParseIndex);
                }//end while
                
                bool blTableFound = MoveNextRecord(ref iCurParseIndex);

                return blTableFound;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonTable Overload 1 function of JsonDataSetParser class.");
                return false;
            }
        }

        /// <summary>
        /// Parses a stream of data linked to a Tiferix JsonDataSet data file containing only a single table record and reads the data from the stream into 
        /// the DataTable object linked to the JsonDataSetParser class.  
        /// </summary>
        /// <param name="streamData">Stream pointing to JsonDataSet data.</param>
        /// <param name="encoding">Encoding of JsonDataSet data stream.  Default is UTF8.</param>
        /// <returns></returns>
        public virtual bool ParseJsonTable(Stream streamData, Encoding encoding = null)
        {
            try
            {
                if (DataSource.GetType() == typeof(DataSet))
                    throw new InvalidDataException("DataTable required as data source for parsing entire Json DataSet data file.");

                DataTable dtTable = (DataTable)DataSource;

                StreamReader srdr = new StreamReader(streamData, encoding);
                m_strJsonData = srdr.ReadToEnd();
                srdr.Close();

                int iCurParseIndex = m_strJsonData.IndexOf(':', 0) + 1;

                ParseJsonTable(dtTable, ref iCurParseIndex);

                return true;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonTable Overload 2 function of JsonDataSetParser class.");
                return false;
            }
        }

        /// <summary>
        /// Parses a Tiferix JsonDataSet data file containing only a single table record and reads the data from the file into 
        /// the DataTable object linked to the JsonDataSetParser class.  
        /// </summary>
        /// <param name="strFileName">The full name and path of the JsonDataSet data file.</param>
        /// <param name="encoding">Encoding of JsonDataSet data file.  Default is UTF8.</param>
        /// <returns></returns>
        public virtual bool ParseJsonTable(string strFileName, Encoding encoding = null)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Open);
                ParseJsonTable(fs, encoding);

                return true;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonTable Overload 3 function of JsonDataSetParser class.");
                return false;
            }
        }

        #endregion

        #region Json Data Row (Record Level) Parsing Properties, Functions

        /// <summary>
        /// Parses the current record of a DataTable in the JsonDataSet data stream being parsed by the JsonDataSetParser class.  Each time a new 
        /// record is detected in the data stream, the ParseJsonRecord function will be called to parse the record from the current table in the JsonDataSet 
        /// data stream and extract the data into a row in the DataTable object passed to the function.  
        /// </summary>
        /// <param name="dtTable">Schema filled DataTable to be loaded with parsed data from JsonDataSet data.</param>
        /// <param name="iCurParseIndex">The current index of the JsonDataSet data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool ParseJsonRecord(DataTable dtTable, ref int iCurParseIndex)
        {
            try
            {                
                bool blFieldFound = true;
                                
                DataRow rowData = dtTable.NewRow();
                    
                while (blFieldFound)
                {
                    blFieldFound = ParseJsonField(dtTable, rowData, ref iCurParseIndex);
                }//end while

                dtTable.Rows.Add(rowData);

                bool blRecordFound = MoveNextRecord(ref iCurParseIndex);

                return blRecordFound;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonRecord function of JsonDataSetParser class.");
                return false;
            }
        }

        /// <summary>
        /// Parses the current field of the current record of a DataTable in the JsonDataSet data stream being parsed by the JsonDataSetParser class. 
        /// This function will be called to parse and extract the data from each field of each record being parsed by the class.  The ParseJsonField 
        /// will be called to prase every field detected for the record and extract the data into the DataRow object passed to the function which 
        /// will then be added to the DataTable in the ParseJsonRecord function. 
        /// </summary>
        /// <param name="dtTable">Schema filled DataTable to be loaded with parsed data from JsonDataSet data.</param>
        /// <param name="rowData">DataRow that will be loaded with parsed field data from the JsonDataSet data.</param>
        /// <param name="iCurParseIndex">The current index of the JsonDataSet data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool ParseJsonField(DataTable dtTable, DataRow rowData, ref int iCurParseIndex)
        {
            try
            {
                bool blFieldFound = true;

                string strFieldName = "";
                                
                iCurParseIndex = m_strJsonData.IndexOf('\"', iCurParseIndex) + 1;

                strFieldName = "";
                while (m_strJsonData[iCurParseIndex] != '\"')
                {
                    strFieldName += m_strJsonData[iCurParseIndex];
                    iCurParseIndex++;
                }//end while

                iCurParseIndex = m_strJsonData.IndexOf(':', iCurParseIndex) + 1;

                while (m_strJsonData[iCurParseIndex] == ' ')
                    iCurParseIndex++;

                strFieldName = m_dictTableColNames[dtTable.TableName.ToUpper()][strFieldName.ToUpper()];
                rowData[strFieldName] = GetFieldValue(dtTable.Columns[strFieldName].DataType, ref iCurParseIndex);

                if (m_strJsonData.IndexOf(',', iCurParseIndex) > -1)
                {
                    if (m_strJsonData.IndexOf(',', iCurParseIndex) < m_strJsonData.IndexOf('}', iCurParseIndex))
                    {
                        blFieldFound = true;
                        iCurParseIndex = m_strJsonData.IndexOf('\"', iCurParseIndex);
                    }
                    else
                    {
                        blFieldFound = false;
                        iCurParseIndex = m_strJsonData.IndexOf('}', iCurParseIndex) + 1;
                    }//end if
                }
                else
                {
                    blFieldFound = false;
                    iCurParseIndex = m_strJsonData.IndexOf('}', iCurParseIndex) + 1;
                }//end if

                return blFieldFound;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonField function of JsonDataSetParser class.");
                return false;
            }
        }

        /// <summary>
        /// Parses the data stored for the current field of the current record of the DataTable in the JsonDataSet data stream being parsed by the
        /// JsonDataSetParser class.   The function will parse the data string stored in the appropriate format in the JsonDataSet data file and extract 
        /// it into the appropriate .Net data type based on the .Net data type specified in the function's parameter (which should match that of the 
        /// DataTable column being loaded).   All types of data will be processed and appropriately converted, including null values, escape characters, 
        /// numeric, string, date/time and binary values.
        /// </summary>
        /// <param name="dataType">The .Net Data Type of the field to be extracted from the JsonDataSet data.</param>
        /// <param name="iCurParseIndex">The current index of the JsonDataSet data stream being parsed.</param>
        /// <returns></returns>
        protected virtual object GetFieldValue(Type dataType, ref int iCurParseIndex)
        {
            try
            {
                object oValue = null;
                string strValue = "";
                char chrNext = (char)0;
                bool blParseData = true;

                if (m_strJsonData[iCurParseIndex] == 'n')
                {
                    strValue = m_strJsonData.Substring(iCurParseIndex, iCurParseIndex + 4);

                    if (strValue == "null")
                    {
                        iCurParseIndex += 4;
                        return DBNull.Value;
                    }//end if                                
                }//end if

                if (dataType == typeof(string) || dataType == typeof(DateTime))
                {
                    iCurParseIndex++;

                    while (blParseData)
                    {
                        chrNext = m_strJsonData[iCurParseIndex];

                        if (chrNext == '\\')
                        {
                            string strEscapeChar = chrNext.ToString() + m_strJsonData[iCurParseIndex + 1].ToString();

                            if (JsonDataUtils.IsEscapeChar(strEscapeChar))
                            {
                                strValue += JsonDataUtils.ConvertEscapeChar(strEscapeChar);
                                iCurParseIndex += 2;
                            }
                            else
                            {
                                strValue += chrNext;
                                iCurParseIndex++;
                            }//end if
                        }
                        else if (chrNext == '\"')
                        {
                            blParseData = false;
                            iCurParseIndex++;
                        }
                        else
                        {
                            strValue += chrNext;
                            iCurParseIndex++;
                        }//end if                        
                    }//end while                                           
                }
                else
                {                    
                    while (blParseData)
                    {
                        chrNext = m_strJsonData[iCurParseIndex];

                        if (chrNext == ',' || chrNext == ' ' || chrNext == '}')                        
                            blParseData = false;                        
                        else
                        {
                            strValue += chrNext;
                            iCurParseIndex++;
                        }//end if
                    }//end while                    
                }//end if

                if (dataType == typeof(string))
                    return strValue;
                else
                {
                    oValue = JsonDataUtils.ConvertDataFromJson(strValue, dataType);
                    return oValue;
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in GetFieldValue function of JsonDataSetParser class.");
                return null;
            }
        }

        #endregion

        #region General Json Record Parsing Functions

        /// <summary>
        /// Scans the JsonDataSet data stream for the next record block of the parent object block being parsed, whether it be a table, record or field of a record.        
        /// Because Json files are stored in a hierarchical structure, the parsing operation will parse each set of object blocks at the current level and 
        /// treat them as invidividual records of the parent object block.  When the next record block of the parent object block is detected, the function 
        /// will return true.  If the final record has been reached in the set of records of the parent object block, then the function will return false. The 
        /// current parsing index will be advanced to either the next record or to past the last position of the parent object block, which will be positioned 
        /// at the next parent record.  An example can be positioning the parser at the next table record after the final row of the table has been detected.
        /// </summary>
        /// <param name="iCurParseIndex">The current index of the JsonDataSet data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool MoveNextRecord(ref int iCurParseIndex)
        {
            try
            {
                bool blRecFound = true;                
                
                if (m_strJsonData.IndexOf(',', iCurParseIndex) != -1)
                {
                    if (m_strJsonData.IndexOf(',', iCurParseIndex) < m_strJsonData.IndexOf('}', iCurParseIndex))
                    {
                        iCurParseIndex = m_strJsonData.IndexOf('{', iCurParseIndex);
                        blRecFound = true;
                    }
                    else
                    {
                        iCurParseIndex = m_strJsonData.IndexOf('}', iCurParseIndex) + 1;
                        blRecFound = false;
                    }//end if

                    return blRecFound;
                }
                else
                {
                    iCurParseIndex = m_strJsonData.IndexOf("}", iCurParseIndex) + 1;
                    return false;
                }//end if           
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in MoveNextRecord function of JsonDataSetParser class.");
                return false;
            }
        }      

        #endregion

        #region Data Type Specific Properties, Functions

        /// <summary>
        /// The DateTimeFormat that all date values are stored in the linked JsonDataSet data stream to be parsed by the function.  All date values 
        /// will use this format when parsing and extracting date values from the JsonDataSet data stream to the linked DataSet.
        /// </summary>
        public DateTimeFormat DateTimeFormat { get; set; }

        #endregion

        #region Generic/Unknown/Variant Type Data Parsing Properties, Functions

        /// <summary>
        /// Parses a value stored in a JsonDataSet or JsonDataSetSchema file where the data type is unknown.  The function will detect the data type 
        /// associated with the Json value and convert it into the appropriate .Net data type.   
        /// Returns: Success = Data Value (Converted to Appropriate Type), Failure = null.
        /// </summary>
        /// <param name="strValue">Json data value to parse.</param>
        /// <returns></returns>
        public static object ParseVarTypeJsonData(string strValue)
        {
            try
            {
                if (General.IsNumeric(strValue))
                {
                    object oValue;

                    if (!strValue.Contains('.'))
                    {                        
                        int iValue;
                        if (!Int32.TryParse(strValue, out iValue))
                        {
                            long lValue;
                            Int64.TryParse(strValue, out lValue);
                            oValue = lValue;
                        }
                        else
                            oValue = iValue;

                        return oValue;
                    }
                    else
                    {
                        float fValue;
                        if (!Single.TryParse(strValue, out fValue))
                        {
                            double dValue;
                            if (!double.TryParse(strValue, out dValue))
                            {                                
                                decimal mValue = decimal.Parse(strValue);
                                oValue = mValue;
                            }
                            else
                                oValue = dValue;
                        }
                        else
                            oValue = fValue;

                    }//end if
                }
                else if (strValue.ToUpper() == "TRUE")
                    return true;
                else if (strValue.ToUpper() == "FALSE")
                    return false;
                else if (strValue == "null")
                    return DBNull.Value;
                else if (strValue.StartsWith("\""))
                    return strValue.Substring(1, strValue.Length - 2);

                return null;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseVarTypeJsonData function of JsonDataSetParser class.");
                return null;
            }
        }

        #endregion
    }

    
}
