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
    /// The JsonDataSetSchemaParser class parses a Tiferix JsonDataSchema schema file that will be used to load the schema of an ADO.NET DataSet 
    /// object.  All data in the DataSetSchema file, including each set of TableSchema data will be parsed from the JsonDataSetSchema file and will 
    /// be used to load the schema of each DataTable in the DataSet.  
    /// </summary>
    public class JsonDataSetSchemaParser : IDisposable 
    {
        #region Member Variables

        private bool m_blDisposed = false;

        #endregion

        #region Member Object Variables
        #endregion

        #region Schema Variables

        private string m_strJsonDSSchema = "";
        
        #endregion        
        
        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDataSetSchemaParser()
        {
            try
            {                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 1 function of JsonDataSetSchemaParser class.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDataSetSchemaParser(Stream streamSchema)
        {
            try
            {
                LoadSchema(streamSchema);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 2 function of JsonDataSetSchemaParser class.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonDataSetSchemaParser(string strSchemaFileName)
        {
            try
            {
                LoadSchema(strSchemaFileName);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 3 function of JsonDataSetSchemaParser class.");
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
            m_strJsonDSSchema = "";
            m_blDisposed = true;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~JsonDataSetSchemaParser()
        {
            Dispose(false);
        }

        #endregion        

        #region JsonDataSet Schema Loading Properties, Functions

        /// <summary>
        /// Loads the JsonDataSetSchema in the linked data stream into a string buffer in memory that will contain the entire contents of the
        /// JsonDataSetSchema data file to be parsed by the class.
        /// </summary>
        /// <param name="stream"></param>        
        public virtual void LoadSchema(Stream stream)
        {
            try
            {
                StreamReader srdr = new StreamReader(stream);

                srdr.BaseStream.Position = 0;
                m_strJsonDSSchema = srdr.ReadToEnd();
                srdr.Close();

                m_strJsonDSSchema = m_strJsonDSSchema.Trim();
                //m_strJsonDSSchema = m_strJsonDSSchema.ToUpper();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in LoadSchema Overload 1 function of JsonDataSetSchemaParser class.");
            }
        }

        /// <summary>
        /// Loads the JsonDataSetSchema contained in the specified schema file into a string buffer in memory that will contain the entire contents of the
        /// JsonDataSetSchema data file to be parsed by the class.
        /// </summary>
        /// <param name="strFileName"></param>        
        public virtual void LoadSchema(string strFileName)
        {
            try
            {
                FileStream fs = new FileStream(strFileName, FileMode.Open);
                LoadSchema(fs);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in LoadSchema Overload 2 function of JsonDataSetSchemaParser class.");
            }
        }

        #endregion

        #region Schema Parsing Properties, Functions

        /// <summary>
        /// Parses the buffered stream of data contained in the JsonDataSetSchemaParser class, which was loaded from a Tiferix JsonDataSetSchema 
        /// file.  The function will read the schema data from the buffered schema string into the linked JsonDataSetSchema object of the parser class,
        /// which then can be used to initialize the schema of the DataSet object.
        /// </summary>
        public virtual JsonDataSetSchema ParseJsonSchema()
        {
            try
            {
                JsonDataSetSchema schemaJsonDS = new JsonDataSetSchema();

                string strExtractData = "";

                int iCurParseIndex = 0;
                int iEndParseIndex = 0;

                iCurParseIndex = m_strJsonDSSchema.IndexOf("\"DataSet\"", iCurParseIndex, StringComparison.OrdinalIgnoreCase);
                iCurParseIndex = m_strJsonDSSchema.IndexOf(':', iCurParseIndex);
                iCurParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex) + 1;
                iEndParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex);

                //Extracts the name of the DataSet.
                strExtractData = m_strJsonDSSchema.Substring(iCurParseIndex, iEndParseIndex - iCurParseIndex);
                schemaJsonDS.DataSetName = strExtractData;

                iCurParseIndex = iEndParseIndex + 1;

                iCurParseIndex = m_strJsonDSSchema.IndexOf("\"Tables\"", iCurParseIndex, StringComparison.OrdinalIgnoreCase);
                iCurParseIndex = m_strJsonDSSchema.IndexOf(':', iCurParseIndex) + 1;

                if (m_strJsonDSSchema.IndexOf('[', iCurParseIndex) != -1)
                {
                    if (m_strJsonDSSchema.IndexOf('[', iCurParseIndex) < m_strJsonDSSchema.IndexOf('{', iCurParseIndex))
                        iCurParseIndex = m_strJsonDSSchema.IndexOf('[', iCurParseIndex) + 1;
                }//end if

                bool blTableDetected = true;

                while (blTableDetected)
                {
                    JsonTableSchema schemaJsonTable = null;
                    blTableDetected = ParseJsonTableSchema(ref schemaJsonTable, ref iCurParseIndex);

                    schemaJsonDS.Tables.Add(schemaJsonTable);
                }//end while

                return schemaJsonDS;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonSchema Overload 1 function of JsonDataSetSchemaParser class.");
                return null;
            }
        }

        /// <summary>
        /// Parses the specified stream of data in the function, which is linked to a Tiferix JsonDataSetSchema file and reads the schema data from the
        /// stream which will then be used to initialize the schema of the DataSet object linked to the JsonDataSetSchemaParser class.  
        /// </summary>
        public virtual JsonDataSetSchema ParseJsonSchema(Stream streamSchema)
        {
            try
            {
                LoadSchema(streamSchema);
                return ParseJsonSchema();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonSchema Overload 2 function of JsonDataSetSchemaParser class.");
                return null;
            }
        }

        /// <summary>
        /// Parses the data of the specified Tiferix JsonDataSetSchema file in the function and reads the schema data from the
        /// file which will then be used to initialize the schema of the DataSet object linked to the JsonDataSetSchemaParser class.  
        /// </summary>
        public virtual JsonDataSetSchema ParseJsonSchema(string strFileName)
        {
            try
            {
                LoadSchema(strFileName);
                return ParseJsonSchema();
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonSchema Overload 3 function of JsonDataSetSchemaParser class.");
                return null;
            }
        }

        /// <summary>
        /// Parses the current JsonTableSchema information contained in the JsonDataSetSchema data stream linked to the JsonDataSetSchemaParser
        /// class.  The function will read the table schema data at the current position of the data stream into the JsonTableSchema object 
        /// passed to the function.  This data can then be used to initialize the schema of a DataTable object.
        /// </summary>
        /// <param name="schemaJsonTable">The JsonTableSchema object that will </param>
        /// <param name="iCurParseIndex">The current index of the JsonDataSetSchema data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool ParseJsonTableSchema(ref JsonTableSchema schemaJsonTable, ref int iCurParseIndex)
        {
            try
            {
                schemaJsonTable = new JsonTableSchema();

                string strExtractData = "";
                int iEndParseIndex = 0;
                bool blTableSchemaFound = true;

                iCurParseIndex = m_strJsonDSSchema.IndexOf("\"Table\"", iCurParseIndex, StringComparison.OrdinalIgnoreCase);
                iCurParseIndex = m_strJsonDSSchema.IndexOf(":", iCurParseIndex);

                iCurParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex) + 1;
                iEndParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex);

                //Extracts the name of the table.
                strExtractData = m_strJsonDSSchema.Substring(iCurParseIndex, iEndParseIndex - iCurParseIndex);
                schemaJsonTable.TableName = strExtractData;

                iCurParseIndex = m_strJsonDSSchema.IndexOf("\"Columns\"", iCurParseIndex, StringComparison.OrdinalIgnoreCase);
                iCurParseIndex = m_strJsonDSSchema.IndexOf(':', iCurParseIndex) + 1;

                if (m_strJsonDSSchema.IndexOf('[', iCurParseIndex) != -1)
                {
                    if (m_strJsonDSSchema.IndexOf('[', iCurParseIndex) < m_strJsonDSSchema.IndexOf('{', iCurParseIndex))
                        iCurParseIndex = m_strJsonDSSchema.IndexOf('[', iCurParseIndex) + 1;
                }//end if

                bool blColDetected = true;

                while (blColDetected)
                {
                    JsonColumnSchema schemaJsonCol = null;
                    blColDetected = ParseJsonColumnSchema(ref schemaJsonCol, ref iCurParseIndex);

                    schemaJsonTable.Columns.Add(schemaJsonCol);
                }//end while

                blTableSchemaFound = MoveNextSchemaRecord(ref iCurParseIndex);

                return blTableSchemaFound;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonTableSchema Overload 1 function of JsonDataSetSchemaParser class.");
                return false;
            }
        }

        /// <summary>
        /// Parses the current JsonTableSchema information contained in the JsonDataSetSchema data stream linked to the JsonDataSetSchemaParser
        /// class.  The function will read the table schema data at the current position of the data stream into a newly instantiated JsonTableSchema object 
        /// which will then be returned by the function.  This data can then be used to initialize the schema of a DataTable object.        
        /// </summary>
        /// <returns></returns>
        public virtual JsonTableSchema ParseJsonTableSchema()
        {
            try
            {
                JsonTableSchema schemaJsonTable = null;
                int iCurParseIndex = 0;

                ParseJsonTableSchema(ref schemaJsonTable, ref iCurParseIndex);
                return schemaJsonTable;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonTableSchema Overload 2 function of JsonDataSetSchemaParser class.");
                return null;
            }
        }

        /// <summary>
        /// Parses the current JsonColumnSchema information of the current JsonTableSchema record contained in the JsonDataSetSchema data stream 
        /// linked o the JsonDataSetSchemaParser class.  The function will read the column schema and extract the column information into the 
        /// JstonColumnSchema object passed to the function.  All field schema properties and values will be converted into the appropriate format and 
        /// loaded into the JsonColumnSchema object associatd with the current field of the current table being parsed in the JsonDataSchemaParser 
        /// class.
        /// </summary>
        /// <param name="schemaJsonCol">A reference to the JsonColumnSchema object to be loaded with the column schema information from the 
        /// JsonDataSetSchema file being parsed in the class.</param>
        /// <param name="iCurParseIndex">The current index of the JsonDataSetSchema data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool ParseJsonColumnSchema(ref JsonColumnSchema schemaJsonCol, ref int iCurParseIndex)
        {
            try
            {
                schemaJsonCol = new JsonColumnSchema();

                string strExtractData = "";
                int iEndParseIndex = 0;                
                bool blColSchemaItemFound = true;

                while (blColSchemaItemFound)
                {
                    iCurParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex) + 1;
                    iEndParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex);
                    string strColSchemaItem = m_strJsonDSSchema.Substring(iCurParseIndex, iEndParseIndex - iCurParseIndex).ToUpper();
                    iCurParseIndex = m_strJsonDSSchema.IndexOf(':', iCurParseIndex) + 1;

                    switch (strColSchemaItem)
                    {
                        case "COLUMNNAME":
                        case "DATATYPE":
                        case "CAPTION":
                        case "EXPRESSION":
                        case "DATETIMEMODE":
                            iCurParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex) + 1;
                            iEndParseIndex = m_strJsonDSSchema.IndexOf('\"', iCurParseIndex);
                            strExtractData = m_strJsonDSSchema.Substring(iCurParseIndex, iEndParseIndex - iCurParseIndex);

                            if (strColSchemaItem == "COLUMNNAME")
                                schemaJsonCol.ColumnName = strExtractData;
                            else if (strColSchemaItem == "DATATYPE")
                                schemaJsonCol.DataType = JsonDataUtils.ConvertFromJsonDataType(strExtractData);
                            else if (strColSchemaItem == "CAPTION")
                                schemaJsonCol.Caption = strExtractData;
                            else if (strColSchemaItem == "EXPRESSION")
                                schemaJsonCol.Expression = strExtractData;
                            else if (strColSchemaItem == "DATETIMEMODE")
                                schemaJsonCol.DateTimeMode = JsonDataUtils.ConvertToADODataSetDateTimeEnum(strExtractData);

                            iCurParseIndex = iEndParseIndex + 1;

                            break;
                        case "PRIMARYKEY":
                        case "UNIQUE":
                        case "ALLOWDBNULL":
                        case "AUTOINCREMENT":
                        case "READONLY":
                            while (m_strJsonDSSchema[iCurParseIndex] != 'T' && m_strJsonDSSchema[iCurParseIndex] != 't' &&
                                 m_strJsonDSSchema[iCurParseIndex] != 'F' && m_strJsonDSSchema[iCurParseIndex] != 'f')
                                iCurParseIndex++;

                            bool blValue;

                            if (m_strJsonDSSchema[iCurParseIndex].ToString().ToUpper() == "T")
                            {
                                blValue = true;
                                iCurParseIndex += "true".Length;
                            }
                            else
                            {
                                blValue = false;
                                iCurParseIndex += "false".Length;
                            }//end if

                            if (strColSchemaItem == "PRIMARYKEY")
                                schemaJsonCol.PrimaryKey = blValue;
                            else if (strColSchemaItem == "UNIQUE")
                                schemaJsonCol.Unique = blValue;
                            else if (strColSchemaItem == "ALLOWDBNULL")
                                schemaJsonCol.AllowDBNull = blValue;
                            else if (strColSchemaItem == "AUTOINCREMENT")
                                schemaJsonCol.AutoIncrement = blValue;
                            else if (strColSchemaItem == "READONLY")
                                schemaJsonCol.ReadOnly = blValue;
                            break;
                        case "MAXLENGTH":
                        case "AUTOINCREMENTSEED":
                        case "AUTOINCREMENTSTEP":
                            while (m_strJsonDSSchema[iCurParseIndex] == ' ' || m_strJsonDSSchema[iCurParseIndex] == '\"')
                                iCurParseIndex++;

                            string strVal = "";

                            while (char.IsNumber(m_strJsonDSSchema[iCurParseIndex]))
                            {
                                strVal += m_strJsonDSSchema[iCurParseIndex];
                                iCurParseIndex++;
                            }//end while

                            schemaJsonCol.MaxLength = Convert.ToInt32(strVal);

                            break;
                        case "DEFAULTVALUE":
                            while (m_strJsonDSSchema[iCurParseIndex] != ' ')
                                iCurParseIndex++;

                            string strDefaultValue = "";
                            bool blDataFound = true;

                            while (blDataFound)
                            {
                                if (m_strJsonDSSchema[iCurParseIndex] != ',' && m_strJsonDSSchema[iCurParseIndex] != '}' &&
                                    m_strJsonDSSchema[iCurParseIndex] != ' ')
                                {
                                    strDefaultValue += m_strJsonDSSchema[iCurParseIndex];
                                }
                                else
                                    blDataFound = false;
                            }//end while

                            schemaJsonCol.DefaultValue = JsonDataSetParser.ParseVarTypeJsonData(strDefaultValue);

                            break;
                    }//end switch

                    if (m_strJsonDSSchema.IndexOf(',', iCurParseIndex) != -1)
                    {
                        if (m_strJsonDSSchema.IndexOf(',', iCurParseIndex) < m_strJsonDSSchema.IndexOf('}', iCurParseIndex))
                            iCurParseIndex = m_strJsonDSSchema.IndexOf(',', iCurParseIndex) + 1;
                        else
                            blColSchemaItemFound = false;
                    }
                    else
                        blColSchemaItemFound = false;

                    if(!blColSchemaItemFound)
                    {
                        iCurParseIndex = m_strJsonDSSchema.IndexOf('}', iCurParseIndex) + 1;                        
                    }//end if
                }//end while
               
                bool blColFound = MoveNextSchemaRecord(ref iCurParseIndex);

                return blColFound;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ParseJsonColumnSchema function of JsonDataSetSchemaParser class.");
                return false;
            }
        }

        #endregion                           

        #region General Json Schema Record Parsing Functions

        /// <summary>
        /// Scans the JsonDataSetSchema data stream for the next record block of the parent object block being parsed, whether it be a table schema or 
        /// columna schema record blocks.  Because Json files are stored in a hierarchical structure, the parsing operation will parse each set of object 
        /// blocks at the current level and treat them as invidividual records of the parent object block.  When the next record block of the parent object 
        /// block is detected, the function will return true.  If the final record has been reached in the set of records of the parent object block, then the 
        /// function will return false. The current parsing index will be advanced to either the next record or to past the last position of the parent object block, which will be positioned 
        /// at the next parent record.  An example can be positioning the parser at the next table schema record after the final row of the table schema has been detected.
        /// </summary>
        /// <param name="iCurParseIndex">The current index of the JsonDataSetSchema data stream being parsed.</param>
        /// <returns></returns>
        protected virtual bool MoveNextSchemaRecord(ref int iCurParseIndex)
        {
            try
            {
                bool blRecFound = true;

                if (m_strJsonDSSchema.IndexOf(',', iCurParseIndex) != -1)
                {
                    if (m_strJsonDSSchema.IndexOf(',', iCurParseIndex) < m_strJsonDSSchema.IndexOf('}', iCurParseIndex))
                    {
                        iCurParseIndex = m_strJsonDSSchema.IndexOf('{', iCurParseIndex);
                        blRecFound = true;
                    }
                    else
                    {
                        iCurParseIndex = m_strJsonDSSchema.IndexOf('}', iCurParseIndex) + 1;
                        blRecFound = false;
                    }//end if

                    return blRecFound;
                }
                else
                {
                    iCurParseIndex = m_strJsonDSSchema.IndexOf("}", iCurParseIndex) + 1;
                    return false;
                }//end if           
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in MoveNextSchemaRecord function of JsonDataSetSchemaParser class.");
                return false;
            }
        }        

        #endregion
    }
}
