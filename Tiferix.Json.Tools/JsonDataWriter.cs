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
using Tiferix.Global;

namespace Tiferix.Json.Tools
{
    /// <summary>
    /// The JsonDataWriter class is used to write data from .Net objects, DataSets and Tiferix.Json schema objects to various Tieferix.Json data and schema 
    /// files.  This class will handle the the formatting and writing of data into a Tiferix.Json files, including object and array blocks, identation, spacing
    /// and the processing and conversion of various .Net data types into serialized Tiferix.Json strings that match the data type. 
    /// </summary>
    public class JsonDataWriter : IDisposable
    {
        #region Enumerations        

        #endregion        

        #region Member Variables

        private bool m_blDisposed = false;

        #endregion

        #region Member Object Variables
        #endregion

        #region Data Writer Position and Information Variables

        //private long m_lPos = 0;
        
        /// <summary>
        /// The current level of nesting of Json object/array block in the current JsonDataWriter when AutoIdent mode is active.
        /// </summary>
        private int m_iNestLevel = 0;                                

        #endregion

        #region Stream Object Variables

        private Stream m_jsonDataStream = null;

        private StreamWriter m_swrtJsonData = null;

        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">The underlying data stream used by the JsonDataWriter class to write all Tiferix.Json data output.</param>
        /// <param name="encoding">The character encoding used for writing output to the Tiferix.Json data stream.  The default character 
        /// encoding is UTF8.</param>
        public JsonDataWriter(Stream stream, Encoding encoding)
        {
            try
            {
                InitializeDataWriter(stream, encoding);                      
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 1 function of JsonDataWriter class.");
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">The underlying data stream used by the JsonDataWriter class to write all Tiferix.Json data output.</param>
        public JsonDataWriter(Stream stream)
            :this(stream, Encoding.UTF8)
        {            
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strFileName">The full path and name of the Tiferix.Json file where the JsonDataWriter class will write all data output.</param>
        /// <param name="encoding">The character encoding used for writing output to the Tiferix.Json data file.  The default character 
        /// encoding is UTF8.</param>
        public JsonDataWriter(string strFileName, Encoding encoding)
        {
            try
            {
                FileStream fs = File.Open(strFileName, FileMode.Create, FileAccess.ReadWrite);
                InitializeDataWriter(fs, encoding);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Constructor Overload 3 function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strFileName">The full path and name of the Tiferix.Json file where the JsonDataWriter class will write all data output.</param>
        public JsonDataWriter(string strFileName)
            :this(strFileName, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes the JsonDataWriter class with all default formatting, encoding and data conversion settings, as well as creates a new stream writer 
        /// object.  The streamwriter will be used to write all Tiferix.Json string data to the underlying data stream used by the class.
        /// </summary>
        /// <param name="stream">The underlying data stream used by the JsonDataWriter class to write all Tiferix.Json data output.</param>
        /// <param name="encoding">The character encoding used for writing output to the Tiferix.Json data stream.  The default character 
        /// encoding is UTF8.</param>
        protected virtual void InitializeDataWriter(Stream stream, Encoding encoding)
        {
            try
            {
                FieldDelimiter = ',';
                this.Encoding = encoding;
                DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ss");
                DateTimeZone = DateTimeKind.Unspecified;
                Culture = CultureInfo.InvariantCulture;
                AutoIdent = true;
                IdentChar = ' ';
                IdentSpacing = 2;                
                
                m_jsonDataStream = stream;
                m_swrtJsonData = new StreamWriter(stream, this.Encoding);
                m_swrtJsonData.AutoFlush = true;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in InitializeDataWriter function of JsonDataWriter class.");
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
                try
                {
                    // Free any other managed objects here.
                    //                
                    if (m_jsonDataStream != null)
                    {
                        m_jsonDataStream.Close();
                        m_jsonDataStream.Dispose();                              
                    }//end if
                }
                catch (Exception)
                {
                }
            }
            
            // Free any unmanaged objects here.
            //            
            m_blDisposed = true;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~JsonDataWriter()
        {
            Dispose(false);
        }

        #endregion

        #region Json Data Writer Stream State, Position and Stream Information Setting Properties, Functions

        /// <summary>
        /// The underlying data stream used by the JsonDataWriter class to write all Tiferix.Json data output.
        /// </summary>
        public Stream BaseStream
        {
            get
            {
                return m_jsonDataStream;
            }
        }

        /// <summary>
        /// Gets or sets the current position of the underlying data stream of the JsonDataWriter class.
        /// </summary>
        public long Position
        {
            get
            {
                return m_jsonDataStream.Position;
            }
            set
            {
                m_jsonDataStream.Position = value;
            }
        }

        /// <summary>
        /// Closes the JsonDataWriter's StreamWriter object and the underlying data stream.
        /// </summary>
        public void Close()
        {
            try
            {                
                m_swrtJsonData.Close();                
            }
            catch(Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in Close function JsonDataWriter class.");
            }
        }

        #endregion

        #region Json General Formatting and Encoding Properties, Functions
        
        /// <summary>
        /// The character used for separating invididual fields of a table in the Json data files.  The default fielf delimiter for separating table fields is a 
        /// comma character.
        /// </summary>
        public char FieldDelimiter { get; set; }

        /// <summary>
        /// The character encoding to be used for the Json data files, which are always stored in string format.  This will be the character encoding 
        /// used for the entire Json data file.  The default character encoding is UTF8.
        /// </summary>
        public Encoding Encoding { get; protected set; }

        /// <summary>
        /// The format to use for storing Date/Time values in the Json data file.
        /// </summary>
        public DateTimeFormat DateTimeFormat { get; set; }

        /// <summary>
        /// Indicates which time zone will be associated with the Date/Time values stored in the Json data file.
        /// </summary>
        public DateTimeKind DateTimeZone { get; set; }

        /// <summary>
        /// The specific culture or locale  associated with the data stored in the Json data file.
        /// </summary>
        public CultureInfo Culture { get; set; }

        #endregion

        #region Json Identation Formatting Properties, Functions

        /// <summary>
        /// Indicates if the JsonDataWriter class will automatically handling the identation of the Json data as it is written to the Json data file.  If 
        /// AutoIdent is set to true, then the JsonDataWriter class will keep track of the identation of the various nested blocks of data and handling 
        /// the identation of the various nested blocks will not be neccessary.  AutoIdent is enabled by default.   
        /// NOTE: Certain format restrictions apply, when AutoIdent mode is set to true.
        /// </summary>
        [DefaultValue(true)]
        public bool AutoIdent { get; set; }

        /// <summary>
        /// The number of identation characters (usually a space) to use to ident each line of data in the Json data file.  Each time an identation string 
        /// is written, it will use the number of identation spaces specified in this property.  The default number of indentation spaces is 2.
        /// </summary>
        [DefaultValue(2)]
        public int IdentSpacing { get; set; }

        /// <summary>
        /// The character used which will represent an identation space in the Json data file.  Each time an identation string is written, this character 
        /// will be written to the Json data file, using the number of identation spaces specified in the class.  The default identation character is a space.
        /// </summary>
        [DefaultValue(' ')]
        public char IdentChar { get; set; }

        /// <summary>
        /// Gets an identation string using either the IdentSpacing and IdentChar properties of the JsonDataWriter class or can optionally generate 
        /// an identation string using the identation character and spacing parameters of the function.  This function will also be called internally when 
        /// the AutoIdent feature is enabled and will keep track of the current nesting level/position of the Json data file.
        /// </summary>
        /// <param name="cIdentChar">Identation character.  This setting will override the IdentChar property of the class if set.</param>
        /// <param name="iSpacing">Identating spacing.  This setting will override the IdentSpacing property of the class is set.</param>
        /// <returns></returns>
        protected string GetIdentString(char cIdentChar = '\0', int iSpacing = -1)
        {
            try
            {
                if (cIdentChar == (char)0)
                    cIdentChar = IdentChar;

                if(iSpacing == -1)
                    iSpacing = AutoIdent ? (m_iNestLevel * IdentSpacing) : IdentSpacing;

                if (iSpacing > 0)
                    return new string(cIdentChar, iSpacing);
                else
                    return "";
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in GetIdentString function of JsonDataWriter class.");
                return "";
            }
        }
       
        #endregion

        #region Json Data Array and Object Writing Properties, Functions

        /// <summary>
        /// Writes the opening brace symbol "{" representing the start of a new Json object block to the Tiferix.Json data stream.
        /// </summary>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the opening brace symbol character.</param>
        public void WriteBeginObject(bool blNewLine = true)
        {
            try
            {
                string strOutput = "{";

                if (blNewLine)
                {
                    if (AutoIdent)
                        strOutput = GetIdentString() + strOutput;

                    m_swrtJsonData.WriteLine(strOutput);

                    if (AutoIdent)
                        m_iNestLevel++;                        
                }
                else
                    m_swrtJsonData.Write(strOutput);                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteBeginObject function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes the closing brace symbol "}" representing the end of a Json object block to the Tiferix.Json data stream.
        /// </summary>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the closing brace symbol character.</param>
        public void WriteEndObject(bool blNewLine = false)
        {
            try
            {
                string strOutput = "}";

                if (AutoIdent)
                {
                    m_iNestLevel--;

                    if (m_iNestLevel < 0)
                        m_iNestLevel = 0;

                    strOutput = GetIdentString() + strOutput;
                }//end if

                if (!blNewLine)
                {
                    m_swrtJsonData.Write(strOutput);                    
                }
                else
                {
                    m_swrtJsonData.WriteLine(strOutput);                    
                }//end if                                                   
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteEndObject function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes the opening bracket symbol "[" representing the start of a new Json array block to the Tiferix.Json data stream.
        /// </summary>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the opening bracket symbol character.</param>
        /// <param name="blStartOnNewLine">Indicates if the array block will start on a new line.</param>
        public void WriteBeginArray(bool blNewLine = true, bool blStartOnNewLine = false)
        {
            try
            {
                string strOutput = "[";

                if (blStartOnNewLine)
                {
                    if (AutoIdent)
                        strOutput = GetIdentString() + strOutput;
                }//end if                

                if (!blNewLine)
                    m_swrtJsonData.Write(strOutput);
                else
                    m_swrtJsonData.WriteLine(strOutput);
               
                if (AutoIdent)
                    m_iNestLevel++;                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteBeginArray function of JsonDataWriter class.");
            }            
        }

        /// <summary>
        /// Writes the closing bracket symbol "]" representing the end of a Json array block to the Tiferix.Json data stream.
        /// </summary>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the closing bracket symbol character.</param>
        public void WriteEndArray(bool blNewLine = true)
        {
            try
            {
                string strOutput = "]";

                if (AutoIdent)
                {
                    m_iNestLevel--;

                    if (m_iNestLevel < 0)
                        m_iNestLevel = 0;

                    strOutput = GetIdentString() + strOutput;
                }//end if

                if (!blNewLine)
                {
                    m_swrtJsonData.Write(strOutput);                    
                }
                else
                {
                    m_swrtJsonData.WriteLine(strOutput);                    
                }//end if               
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteEndArray function of JsonDataWriter class.");
            }
        }


        #endregion

        #region Json Data Format Writing Properties, Functions

        /// <summary>
        /// Writes an indentation string to the Tiferix.Json data stream.  The identation string will consist of the identation character and spacing set in
        /// the JsonDataWriter class.  If the NumSpaces parameter is set, it will override the identation spacing settings of the class.
        /// </summary>
        /// <param name="iNumSpaces">The number of characters (usually spaces) to write in the identation string.  If this parameter is set it willl 
        /// override the IdentSpacing setting of the class.</param>
        public void WriteIndent(int iNumSpaces = -1)
        {
            try
            {
                if (iNumSpaces == -1)
                    iNumSpaces = IdentSpacing;

                m_swrtJsonData.Write(GetIdentString('\0', iNumSpaces));
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteIndent function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes one or more new line characters to the Tiferix.Json data stream.  The default number of lines written is 1.
        /// </summary>
        /// <param name="iNumLines">Number of new line characters to write to the stream.</param>
        public void WriteNewLine(int iNumLines = 1)
        {
            try
            {
                for (int i = 0; i < iNumLines; i++)
                    m_swrtJsonData.WriteLine();                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteNewLine function of JsonDataWriter class.");
            }
        }

        #endregion

        #region Json Property Writing Properties, Functions

        /// <summary>
        /// Writes the name of the Tiferix.Json property to the Tiferix.Json data stream.  The property will be written in the aprropriate Json format of 
        /// "PropertyName": {data value}.
        /// NOTE: A space will be appended after the colon symbol of the property.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        public void WritePropertyName(string strPropName)
        {
            try
            {                 
                string strOutput = "\"" + strPropName + "\": ";

                if (AutoIdent)
                    strOutput = GetIdentString() + strOutput;

                m_swrtJsonData.Write(strOutput);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropertyName function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a field delimiter and optionally a new line character after the delimiter to Tiferix.Json data stream.  If the cDelimiter parameter is 
        /// not set, then the FieldDelimiter setting of the JsonDataWriter class will be written as the field delimiter to the stream.
        /// </summary>
        /// <param name="cDelimiter">The character used as the field delimiter.  If this paramter is set it will override the FieldDelimiter character setting 
        /// of the class.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the field delimiter character.</param>
        public void WriteFieldDelimiter(char cDelimiter, bool blNewLine = true)
        {
            try
            {
                if (cDelimiter == (char)0)
                    cDelimiter = FieldDelimiter;

                if (blNewLine)
                {
                    m_swrtJsonData.WriteLine(cDelimiter);                    
                }
                else
                {
                    m_swrtJsonData.Write(cDelimiter);                    
                }//end if
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteFieldDelimiter function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a field delimiter and optionally a new line character after the delimiter to Tiferix.Json data stream.  This version of the function
        /// will write the FieldDelimiter character setting of the JsonDataWriter class to the stream.
        /// </summary>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the field delimiter character.</param>
        public void WriteFieldDelimiter(bool blNewLine = true)
        {
            WriteFieldDelimiter('\0', blNewLine);            
        }

        #endregion

        #region Json Data General Writing Properties, Functions

        /// <summary>
        /// This internal function will be used to write all primitive .Net numeric data values to the Tiferix.Json data stream.  The numeric values will 
        /// be converted to a string and written as a Json formatted numeric value to the Tiferix.Json stream.  Optionally, a field delimiter and newline 
        /// character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="oValue">The numeric value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>        
        protected void WriteNumericValue(object oValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (!blNewLine || blAddFieldDelimiter)
                {
                    m_swrtJsonData.Write(oValue.ToString());

                    if (blAddFieldDelimiter)
                        WriteFieldDelimiter(blNewLine);
                }
                else
                    m_swrtJsonData.WriteLine(oValue.ToString());                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteNumericValue Overload 1 function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// This internal function will be used to write both the name of the property and its associated value for all primitive .Net numeric data values 
        /// to the Tiferix.Json data stream.  The property name and value will be written in the proper Json format of "PropertyName: " {data value}.  
        /// Numeric values will be converted to a string and written as a Json formatted numeric value to the Tiferix.Json stream.  Optionally, a field 
        /// delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="oValue">The numeric value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>        
        protected void WriteNumericValue(string strPropName, object oValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteNumericValue(oValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteNumericValue Overload 2 function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// This internal function will be used to write all .Net string data values to the Tiferix.Json data stream.  Any data type that needs to be 
        /// outputted in string format in the Json file to a string (including dates and even binarydata) will be passed as a string to this function
        /// and then written as a Json formatted string value to the Tiferix.Json stream.  Optionally, a field delimiter and newline 
        /// character can be written after the string value to the data stream.
        /// </summary>
        /// <param name="strValue">The string formatted value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>        
        protected void WriteStringValue(string strValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (!blNewLine || blAddFieldDelimiter)
                {
                    m_swrtJsonData.Write("\"" + strValue + "\"");

                    if(blAddFieldDelimiter)
                        WriteFieldDelimiter(blNewLine);
                }
                else
                    m_swrtJsonData.WriteLine("\"" + strValue + "\"");                                                            
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteStringValue Overload 1 function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// This internal function will be used to write oth the name of the property and its associated value for all .Net string data values to the 
        /// Tiferix.Json data stream.  The property name and value will be written in the proper Json format of "PropertyName: " "string value".  
        /// Any data type that needs to be outputted in string format in the Json file to a string (including dates and even binarydata) will be passed as
        /// a string to this function and then written as a Json formatted string value to the Tiferix.Json stream.  Optionally, a field delimiter and
        /// newline character can be written after the string value to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="strValue">The string formatted value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>        
        protected void WriteStringValue(string strPropName, string strValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteStringValue(strValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteStringValue Overload 2 function of JsonDataWriter class.");
            }
        }

        #endregion

        #region Json Data Value Primitive Data Type Writing Properties, Functions

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strValue">String value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>        
        public void WriteDataValue(string strValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteStringValue(strValue, blAddFieldDelimiter, blNewLine);                                                                          
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (string) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="bValue">Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>        
        public void WriteDataValue(byte bValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(bValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (byte) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="siValue">Int16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(short siValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(siValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Int16) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="iValue">Int32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(int iValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(iValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Int32) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="lValue">Int64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(long lValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(lValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Int64) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="fValue">Float value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(float fValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(fValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Float) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="dValue">Double value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(double dValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(dValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Double) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="mValue">Decimal value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(decimal mValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(mValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Decimal) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="sbValue">Signed Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(sbyte sbValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(sbValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (SByte) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="usiValue">UInt16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(ushort usiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(usiValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (UInt16) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="uiValue">UInt32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(uint uiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(uiValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (UInt32) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="ulValue">UInt64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(ulong ulValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(ulValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (UInt64) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Char values can either be written as Json formatted string or numeric values.  Optionally,
        /// a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="cValue">Char value to write to the Tiferix.Json data stream.</param>
        /// <param name="blSurroundQuotes"> If the SurroundQuotes parameter is set to true, then the char value will be written as a Json formatted 
        /// string with quotes surrounded by the char value.  If set to false, then the char will be written as a single character value to the stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(char cValue, bool blSurroundQuotes = false, bool blAddFieldDelimiter = false, bool blNewLine = false)
        {
            try
            {
                if (blSurroundQuotes)
                {
                    if (!blNewLine || blAddFieldDelimiter)
                    {
                        m_swrtJsonData.Write("\"" + cValue.ToString() + "\"");

                        if (blAddFieldDelimiter)
                            WriteFieldDelimiter(blNewLine);
                    }
                    else
                        m_swrtJsonData.WriteLine("\"" + cValue.ToString() + "\"");
                }
                else
                {
                    if (!blNewLine || blAddFieldDelimiter)
                        m_swrtJsonData.Write(cValue);
                    else
                    {
                        m_swrtJsonData.WriteLine(cValue);

                        if (blAddFieldDelimiter)
                            WriteFieldDelimiter(blNewLine);
                    }//end if
                }//end if                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Char) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  DateTime values will use the DateTimeFormat settings set in the JsonDataWriter class 
        /// to serialize the DateTime values as Json formatted strings.   Optionally, a field delimiter and newline character can be written after the 
        /// numeric value string to the data stream. 
        /// </summary>
        /// <param name="datValue">DateTime value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(DateTime datValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                DateTime datValueOutput = datValue;                
                string strDate = "";

                if (DateTimeZone != DateTimeKind.Unspecified)
                    datValueOutput = new DateTime(datValue.Year, datValue.Month, datValue.Day, datValue.Hour, datValue.Minute, datValue.Second, datValue.Millisecond, Culture.Calendar, DateTimeZone);

                if (DateTimeFormat != null)
                    strDate = datValueOutput.ToString(DateTimeFormat.FormatString, DateTimeFormat.FormatProvider);

                WriteStringValue(strDate, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (DateTime) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  The DateTime portion of DateTimeOffset values will use the DateTimeFormat settings set
        /// in the JsonDataWriter class to serialize the DateTime values as Json formatted strings.   Optionally, a field delimiter and newline character 
        /// can be written after the value string to the data stream. 
        /// </summary>
        /// <param name="dtoValue">DateTimeOffset value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(DateTimeOffset dtoValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                DateTimeOffset dtoValueOutput = dtoValue;
                string strDate = "";
                string strTimeSpan = "";

                if (DateTimeZone != DateTimeKind.Unspecified)
                    dtoValueOutput = new DateTimeOffset(
                                                new DateTime(dtoValue.DateTime.Year, dtoValue.DateTime.Month, dtoValue.DateTime.Day,
                                                                     dtoValue.DateTime.Hour, dtoValue.DateTime.Minute, dtoValue.DateTime.Second, dtoValue.DateTime.Millisecond,
                                                                     DateTimeZone), dtoValue.Offset);

                if (DateTimeFormat != null)
                    strDate = dtoValueOutput.DateTime.ToString(DateTimeFormat.FormatString, DateTimeFormat.FormatProvider);
                else
                    strDate = dtoValueOutput.DateTime.ToString();

                strTimeSpan = dtoValueOutput.Offset.ToString("c");

                WriteStringValue(strDate + " +" + strTimeSpan, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (DateTimeOffset) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="tsValue">TimeSpan value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(TimeSpan tsValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                string strTimeSpan = tsValue.ToString("c");

                WriteStringValue(strTimeSpan, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (TimeSpan) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="blValue">Boolean value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(bool blValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                string strOutput = blValue ? "true" : "false";

                if (!blNewLine || blAddFieldDelimiter)
                {
                    m_swrtJsonData.Write(strOutput);

                    if (blAddFieldDelimiter)
                        WriteFieldDelimiter(blNewLine);
                }
                else
                    m_swrtJsonData.WriteLine(strOutput);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Bool) function of JsonDataWriter class.");
            }
        }

        #endregion

        #region Json Data Value Nullable Primitive Data Type Writing Properties, Functions

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="bValue">Nullable Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<byte> bValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (bValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(bValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Byte?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="siValue">Nullable Int16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<short> siValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (siValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(siValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Int16?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="iValue">Nullable Int32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<int> iValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (iValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(iValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Int32?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="lValue">Nullable Int64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<long> lValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (lValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(lValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Int64?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="fValue">Nullable Float value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<float> fValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (fValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(fValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Float?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="dValue">Nullable Double value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<double> dValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (dValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(dValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Double?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="mValue">Nullable Decimal value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<decimal> mValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (mValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(mValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Decimal?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="sbValue">Nullable Signed Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<sbyte> sbValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (sbValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(sbValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (SByte?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="usiValue">Nullable UInt16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<ushort> usiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (usiValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(usiValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (UInt16?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="uiValue">Nullable UInt32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<uint> uiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (uiValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(uiValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (UInt32?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="ulValue">Nullable UInt64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<ulong> ulValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (ulValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(ulValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (UInt64?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Char values can either be written as Json formatted string or numeric values.  Null 
        /// values will be written according to the Json standards as the value of 'null', without quotes.  Optionally, a field delimiter and newline 
        /// character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="cValue">Nullable Char value to write to the Tiferix.Json data stream.</param>
        /// <param name="blSurroundQuotes"> If the SurroundQuotes parameter is set to true, then the char value will be written as a Json formatted 
        /// string with quotes surrounded by the char value.  If set to false, then the char will be written as a single character value to the stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<char> cValue, bool blSurroundQuotes = false, bool blAddFieldDelimiter = false, bool blNewLine = false)
        {
            try
            {
                if (cValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(cValue.Value, blSurroundQuotes, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Byte?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  DateTime values will use the DateTimeFormat settings set in the JsonDataWriter class 
        /// to serialize the DateTime values as Json formatted strings.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="datValue">Nullable DateTime value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<DateTime> datValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (datValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(datValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (DateTime?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  The DateTime portion of DateTimeOffset values will use the DateTimeFormat settings set
        /// in the JsonDataWriter class to serialize the DateTime values as Json formatted strings.  Null values will be written according to the Json 
        /// standards as the value of 'null', without quotes.  Optionally, a field delimiter and newline character can be written after the value 
        /// string to the data stream.
        /// </summary>
        /// <param name="dtoValue">Nullable DateTimeOffset value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<DateTimeOffset> dtoValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (dtoValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(dtoValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (DateTimeOffset?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="tsValue">Nullable TimeSpan value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<TimeSpan> tsValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (tsValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(tsValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (TimeSpan?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a nullable .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate 
        /// Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="blValue">Nullable Boolean value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Nullable<bool> blValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (blValue == null)
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                else
                    WriteDataValue(blValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Bool?) function of JsonDataWriter class.");
            }
        }

        #endregion

        #region Json Data Value Binary, Array and other Object Types Writing Properties, Functions

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  If the data value is passed as an object to the WriteDataValue function, then the 
        /// data type of the object first will be detected.  Once the data type is detected, the data then will be converted by the JsonDataWriter into the
        /// appropriate Json format and then written to the Tiferix.Json stream.  Null values will be written according to the Json standards as the value
        /// of 'null', without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="oValue">Data value of supported .Net data type passed as an object to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(object oValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (oValue == null)
                {
                    WriteDataNullValue(blAddFieldDelimiter, blNewLine);
                    return;
                }//end if                

                Type dataType = oValue.GetType();

                if (dataType == typeof(string))
                    WriteDataValue((string)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(byte))
                    WriteDataValue((string)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(short))
                    WriteDataValue((short)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(int))
                    WriteDataValue((int)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(long))
                    WriteDataValue((long)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(float))
                    WriteDataValue((float)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(double))
                    WriteDataValue((double)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(decimal))
                    WriteDataValue((decimal)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(sbyte))
                    WriteDataValue((sbyte)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(ushort))
                    WriteDataValue((ushort)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(uint))
                    WriteDataValue((uint)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(ulong))
                    WriteDataValue((ulong)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(char))
                    WriteDataValue((char)oValue, true, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(DateTime))
                    WriteDataValue((DateTime)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(DateTimeOffset))
                    WriteDataValue((DateTimeOffset)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(TimeSpan))
                    WriteDataValue((TimeSpan)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(bool))
                    WriteDataValue((bool)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(byte[]))
                    WriteDataValue((byte[])oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(Uri))
                    WriteDataValue((Uri)oValue, blAddFieldDelimiter, blNewLine);
                else if (dataType == typeof(Guid))
                    WriteDataValue((Guid)oValue, blAddFieldDelimiter, blNewLine);
                else
                    //Data types not supported will be converted and written as strings.
                    WriteDataValue(oValue.ToString(), blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Object) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a binary chunk of data to the Tiferix.Json data stream.  Because of the nature of how binary data is stored in string files, it will be 
        /// necessary to use a Base64 encoded string to store the binary data.  All binary values will be converted into Base64 encoded strings, which 
        /// can then be encoded back into .Net byte arrays.  Base64 encoding guarantees no loss of data of the binary data in the Json data file.  
        /// Once converted into Base64 encoded strings by the JsonDataWriterClass, the data will be then written to the Tiferix.Json stream.
        /// Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="bufValue">Binary value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(byte[] bufValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                string strData = Convert.ToBase64String(bufValue);
                WriteStringValue(strData, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Byte[]) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="uriValue">Uri value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Uri uriValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                string strUri = uriValue.ToString();
                WriteStringValue(strUri, blAddFieldDelimiter, blNewLine);                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Uri) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a .Net data value to the Tiferix.Json data stream.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="guidValue">Guid value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataValue(Guid guidValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                string strGuid = guidValue.ToString();
                WriteStringValue(strGuid, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataValue (Guid) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes a null value to the Tiferix.Json data stream.   Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>        
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WriteDataNullValue(bool blAddFieldDelimiter = false, bool blNewLine = true)        
        {
            try
            {
                if (!blNewLine || blAddFieldDelimiter)
                {
                    m_swrtJsonData.Write("null");

                    if (blAddFieldDelimiter)
                        WriteFieldDelimiter(blNewLine);
                }
                else
                    m_swrtJsonData.WriteLine("null");                
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WriteDataNullValue function of JsonDataWriter class.");
            }
        }

        #endregion

        #region Json Data Property/Key Value Combination Primitive Data Type Writing Properties, Functions

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="strValue">String value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, string strValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteStringValue(strPropName, strValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (String) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="bValue">Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, byte bValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, bValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Byte) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="siValue">Int16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, short siValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, siValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Int16) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="iValue">Int32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, int iValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, iValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Int32) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="lValue">Int64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, long lValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, lValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Int64) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="fValue">Float value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, float fValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, fValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Float) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="dValue">Double value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, double dValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, dValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Double) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="mValue">Decimal value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, decimal mValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, mValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Decimal) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="sbValue">Signed Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, sbyte sbValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, sbValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (SByte) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="usiValue">UInt16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, ushort usiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, usiValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (UInt16) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="uiValue">UInt32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, uint uiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, uiValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (UInt32) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="ulValue">UInt64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, ulong ulValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WriteNumericValue(strPropName, ulValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (UInt64) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Char values can either be written as Json formatted string or numeric values.  
        /// The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json stream.  
        /// Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="cValue">Char value to write to the Tiferix.Json data stream.</param>
        /// <param name="blSurroundQuotes"> If the SurroundQuotes parameter is set to true, then the char value will be written as a Json formatted 
        /// string with quotes surrounded by the char value.  If set to false, then the char will be written as a single character value to the stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, char cValue, bool blSurroundQuotes = false, bool blAddFieldDelimiter = false, bool blNewLine = false)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(cValue, blSurroundQuotes, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Char) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  DateTime values will use the DateTimeFormat settings set in the 
        /// JsonDataWriter class to serialize the DateTime values as Json formatted strings.  The data value will be converted by the JsonDataWriter into 
        /// the appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written 
        /// after the numeric value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="datValue">DateTime value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, DateTime datValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(datValue, blAddFieldDelimiter, blNewLine);              
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (DateTime) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The DateTime portion of DateTimeOffset values will use the DateTimeFormat settings set
        /// in the JsonDataWriter class to serialize the DateTime values as Json formatted strings.  The data value will be converted by the JsonDataWriter into 
        /// the appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written 
        /// after the numeric value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="dtoValue">DateTimeOffset value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, DateTimeOffset dtoValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(dtoValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (DateTimeOffset) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="tsValue">TimeSpan value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, TimeSpan tsValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(tsValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (TimeSpan) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="blValue">Boolean value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, bool blValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(blValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Bool) function of JsonDataWriter class.");
            }
        }

        #endregion        

        #region Json Data Property/Key Value Combination Nullable Primitive Data Type Writing Properties, Functions

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="bValue">Nullable Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<byte> bValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (bValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, bValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Byte?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="siValue">Nullable Int16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<short> siValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (siValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, siValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Int16?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="iValue">Nullable Int32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<int> iValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (iValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, iValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Int32?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="lValue">Nullable Int64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<long> lValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (lValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, lValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Int64?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="fValue">Nullable Float value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<float> fValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (fValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, fValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Float?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="dValue">Nullable Double value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<double> dValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (dValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, dValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Double?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="mValue">Nullable Decimal value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<decimal> mValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (mValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, mValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Decimal?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="sbValue">Nullable Signed Byte value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<sbyte> sbValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (sbValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, sbValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (SByte?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="usiValue">Nullable UInt16 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<ushort> usiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (usiValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, usiValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (UInt16?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="uiValue">Nullable UInt32 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<uint> uiValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (uiValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, uiValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (UInt32?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="ulValue">Nullable UInt64 value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<ulong> ulValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (ulValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, ulValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (UInt64?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Char values can either be written as Json formatted string or numeric values.
        /// Null values will be written according to the Json standards as the value of 'null', without quotes.  The data value will be converted by the JsonDataWriter
        /// into the appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be 
        /// written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="cValue">Char value to write to the Tiferix.Json data stream.</param>
        /// <param name="blSurroundQuotes"> If the SurroundQuotes parameter is set to true, then the char value will be written as a Json formatted 
        /// string with quotes surrounded by the char value.  If set to false, then the char will be written as a single character value to the stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<char> cValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (cValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, cValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Char?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  DateTime values will use the DateTimeFormat settings set in the 
        /// JsonDataWriter class to serialize the DateTime values as Json formatted strings.  Null values will be written according to the Json standards
        /// as the value of 'null', without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then 
        /// written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="datValue">Nullable DateTime value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<DateTime> datValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (datValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, datValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (DateTime?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The DateTime portion of DateTimeOffset values will use the DateTimeFormat
        /// settings set in the JsonDataWriter class to serialize the DateTime values as Json formatted strings.  Null values will be written according to 
        /// the Json standards as the value of 'null', without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json 
        /// format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value string to
        /// the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="dtoValue">Nullable DateTimeOffset value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<DateTimeOffset> dtoValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (dtoValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, dtoValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (DateTimeOffset?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="tsValue">Nullable TimeSpan value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<TimeSpan> tsValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (tsValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, tsValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (TimeSpan?) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated nullable .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the value of 'null',
        /// without quotes.  The data value will be converted by the JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json 
        /// stream.  Optionally, a field delimiter and newline character can be written after the value string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="blValue">Nullable Boolean value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Nullable<bool> blValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                if (blValue == null)
                    WritePropDataNullValue(strPropName, blAddFieldDelimiter, blNewLine);
                else
                    WriteNumericValue(strPropName, blValue.Value, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Bool?) function of JsonDataWriter class.");
            }
        }

        #endregion

        #region Json Data Property/Key Value Combination Binary, Array and other Object Types Writing Properties, Functions

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  If the data value is passed as an object to the WritePropDataValue
        /// function, then the data type of the object first will be detected.  Once the data type is detected, the data then will be converted by the 
        /// JsonDataWriter into the appropriate Json format and then written to the Tiferix.Json stream.  Null values will be written according to the 
        /// Json standards as the value of 'null', without quotes.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="oValue">Data value of supported .Net data type passed as an object to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, object oValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(oValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Object) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated binary chunk of data to the Tiferix.Json data stream.  The property name and value 
        /// will be written in the proper Json format of "PropertyName: " {data value}.  Because of the nature of how binary data is stored in string files,
        /// it will be necessary to use a Base64 encoded string to store the binary data.  All binary values will be converted into Base64 encoded strings,
        /// which can then be encoded back into .Net byte arrays.  Base64 encoding guarantees no loss of data of the binary data in the Json data file.  
        /// Once converted into Base64 encoded strings by the JsonDataWriterClass, the data will be then written to the Tiferix.Json stream.  Optionally,
        /// a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="bufValue">Binary value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, byte[] bufValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(bufValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Byte[]) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="uriValue">Uri value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Uri uriValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(uriValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Uri) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of the property and its associated .Net data value to the Tiferix.Json data stream.  The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  The data value will be converted by the JsonDataWriter into the 
        /// appropriate Json format and then written to the Tiferix.Json stream.  Optionally, a field delimiter and newline character can be written after the value
        /// string to the data stream.
        /// </summary>
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="guidValue">Guid value to write to the Tiferix.Json data stream.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataValue(string strPropName, Guid guidValue, bool blAddFieldDelimiter = false, bool blNewLine = true)
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataValue(guidValue, blAddFieldDelimiter, blNewLine);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataValue (Guid) function of JsonDataWriter class.");
            }
        }

        /// <summary>
        /// Writes both the name of a property and its associated null value to the Tiferix.Json data stream.   The property name and value will 
        /// be written in the proper Json format of "PropertyName: " {data value}.  Null values will be written according to the Json standards as the 
        /// value of 'null', without quotes.  Optionally, a field delimiter and newline character can be written after the value to the data stream.
        /// </summary>        
        /// <param name="strPropName">The name of the property.</param>
        /// <param name="blAddFieldDelimiter">Indicates if a field delimiter character will be written to the stream after the data value.</param>
        /// <param name="blNewLine">Indicates if a newline character will be written to the stream after the data value and field delimiter character
        /// (if a delimiter is used).</param>
        public void WritePropDataNullValue(string strPropName, bool blAddFieldDelimiter = false, bool blNewLine = true)        
        {
            try
            {
                WritePropertyName(strPropName);
                WriteDataNullValue(blAddFieldDelimiter);
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in WritePropDataNullValue function of JsonDataWriter class.");
            }
        }

        #endregion
    }
}
