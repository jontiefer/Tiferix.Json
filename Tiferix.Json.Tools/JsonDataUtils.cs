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
using System.Runtime.CompilerServices;
using Tiferix.Global;

namespace Tiferix.Json.Tools
{
    /// <summary>
    /// The JsonDataUtils class contains a variety of utility functions that will be used by various classes in the Tiferix.Json library.
    /// </summary>
    public static class JsonDataUtils
    {
        #region Data Type Conversion Properties, Functions

        /// <summary>
        /// Converts a data type stored in a Tiferix.Json file into its associated .Net data type.  All Tiferix.Json data types will be stored as Json 
        /// strings with a property name of "DataType".
        /// </summary>
        /// <param name="strSchemaDataType"></param>
        /// <returns></returns>
        public static Type ConvertFromJsonDataType(string strJsonDataType)
        {
            try
            {
                Type dataType = null;

                switch (strJsonDataType.ToUpper())
                {
                    case "STRING":
                    case "STR":
                    case "VARCHAR":
                        dataType = typeof(string);
                        break;
                    case "BYTE":
                        dataType = typeof(byte);
                        break;
                    case "SBYTE":
                        dataType = typeof(sbyte);
                        break;
                    case "CHAR":
                        dataType = typeof(char);
                        break;
                    case "SHORT":
                    case "INT16":
                        dataType = typeof(short);
                        break;
                    case "INT":
                    case "INT32":
                        dataType = typeof(int);
                        break;
                    case "LONG":
                    case "INT64":
                        dataType = typeof(long);
                        break;
                    case "USHORT":
                    case "UINT16":
                        dataType = typeof(ushort);
                        break;
                    case "UINT":
                    case "UINT32":
                        dataType = typeof(uint);
                        break;
                    case "ULONG":
                    case "UINT64":
                        dataType = typeof(ulong);
                        break;
                    case "FLOAT":
                    case "SINGLE":
                        dataType = typeof(float);
                        break;
                    case "DOUBLE":
                        dataType = typeof(double);
                        break;
                    case "DECIMAL":
                    case "CURRENCY":
                    case "MONEY":
                        dataType = typeof(decimal);
                        break;
                    case "DATETIME":
                    case "DATE":
                    case "TIME":
                        dataType = typeof(DateTime);
                        break;
                    case "DATETIMEOFFSET":
                        dataType = typeof(DateTimeOffset);
                        break;
                    case "TIMESPAN":
                        dataType = typeof(TimeSpan);
                        break;
                    case "BOOL":
                    case "BOOLEAN":
                        dataType = typeof(bool);
                        break;
                    case "BINARY":
                        dataType = typeof(byte[]);
                        break;
                }//end switch    

                return dataType;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertFromJsonDataType function of JsonDataUtils class.");
                return null;
            }
        }

        /// <summary>
        /// Converts a .Net data type into a format that can be stored in a Tiferix.Json file.  All Tiferix.Json data types will be stored as
        /// Json strings with a property name of "DataType".
        /// </summary>
        /// <param name="dataType">.Net data type to convert into a Tiferix.Json compatible data type.</param>
        /// <returns></returns>
        public static string ConvertToJsonDataType(Type dataType)
        {
            try
            {
                string strSchemaDataType = "";

                if (dataType == typeof(string))
                    strSchemaDataType = "string";
                else if (dataType == typeof(byte))
                    strSchemaDataType = "byte";
                else if (dataType == typeof(sbyte))
                    strSchemaDataType = "sbyte";
                else if (dataType == typeof(char))
                    strSchemaDataType = "char";
                else if (dataType == typeof(short))
                    strSchemaDataType = "short";
                else if (dataType == typeof(int))
                    strSchemaDataType = "int";
                else if (dataType == typeof(long))
                    strSchemaDataType = "long";
                else if (dataType == typeof(ushort))
                    strSchemaDataType = "ushort";
                else if (dataType == typeof(uint))
                    strSchemaDataType = "uint";
                else if (dataType == typeof(ulong))
                    strSchemaDataType = "ulong";
                else if (dataType == typeof(float))
                    strSchemaDataType = "float";
                else if (dataType == typeof(double))
                    strSchemaDataType = "double";
                else if (dataType == typeof(decimal))
                    strSchemaDataType = "decimal";
                else if (dataType == typeof(DateTime))
                    strSchemaDataType = "datetime";
                else if (dataType == typeof(DateTimeOffset))
                    strSchemaDataType = "datetimeoffset";
                else if (dataType == typeof(TimeSpan))
                    strSchemaDataType = "timespan";
                else if (dataType == typeof(bool))
                    strSchemaDataType = "bool";
                else if (dataType == typeof(byte[]))
                    strSchemaDataType = "binary";

                return strSchemaDataType;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertToJsonDataType function of JsonDataUtils class.");
                return "";
            }
        }

        /// <summary>
        /// Converts a DateSetDateTime value that is contained in the Tiferix.Json format into a DataSetDateTime .Net object which is used by the 
        /// DateTimeMode property of ADO.Net DataTables.
        /// </summary>
        /// <param name="strDSDateTimeVal"></param>
        /// <returns></returns>
        public static DataSetDateTime ConvertToADODataSetDateTimeEnum(string strDSDateTimeVal)
        {
            try
            {
                switch (strDSDateTimeVal.ToUpper())
                {
                    case "LOCAL":
                        return DataSetDateTime.Local;
                    case "UTC":
                        return DataSetDateTime.Utc;
                    case "UNSPECIFIED":
                        return DataSetDateTime.Unspecified;
                    case "UNSPECIFIEDLOCAL":
                        return DataSetDateTime.UnspecifiedLocal;
                }//end switch         

                return DataSetDateTime.Local;  
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertToADODataSetDateTimeEnum function of JsonDataUtils class.");
                return DataSetDateTime.Local;
            }
        }

        #endregion

        #region Data Value Conversion Properties, Functions

        /// <summary>
        /// Converts an escape character represented as a string (with '\\' characters) into actual .Net escape character represented as a single char value.
        /// All escape characters will be stored as strings in Tiferix.Json files.
        /// </summary>
        /// <param name="strValue">The string value to convert to a .Net escape character.</param>
        /// <returns></returns>
        public static char ConvertEscapeChar(string strValue)
        {
            try
            {
                switch (strValue)
                {
                    case "\\\'":
                        return '\'';
                    case "\\\"":
                        return '\"';
                    case "\\\\":
                        return '\\';
                    case "\\0":
                        return '\0';
                    case "\\b":
                        return '\b';
                    case "\\f":
                        return '\f';
                    case "\\n":
                        return '\n';
                    case "\\r":
                        return '\r';
                    case "\\t":
                        return '\t';
                    case "\\v":
                        return '\v';
                    default:
                        return (char)0;
                }//end switch
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertEscapeChar function of JsonDataUtils class.");
                return (char)0;
            }
        }

        /// <summary>
        /// Converts a data value stored in a Tiferix.Json file into the appropriate .Net value according to the data type of the specified value.  Since 
        /// all values are stored as strings in Json, certain data conversions and string decoding will be necessary for certain formats of data, such 
        /// as binary and date/time types of data.
        /// </summary>
        /// <param name="strValue">The Tiferix.Json data value string to convert into its .Net value that matches its data type.</param>
        /// <param name="dataType">The .Net data type that the Tiferix.Json data value will be converted to.</param>
        /// <param name="dateTimeFormat">The DateTimeFormat that DateTime, TimeSpan and other date related types are used for the Tiferix.Json 
        /// data value.</param>
        /// <returns></returns>
        public static object ConvertDataFromJson(string strValue, Type dataType, DateTimeFormat dateTimeFormat = null)
        {
            try
            { 
                if (dataType == typeof(string))
                    return strValue;
                else if (dataType == typeof(byte))
                    return byte.Parse(strValue);
                else if (dataType == typeof(sbyte))
                    return sbyte.Parse(strValue);
                else if (dataType == typeof(char))
                    return strValue[0];
                else if (dataType == typeof(short))
                    return Int16.Parse(strValue);
                else if (dataType == typeof(int))
                    return Int32.Parse(strValue);
                else if (dataType == typeof(long))
                    return Int64.Parse(strValue);
                else if (dataType == typeof(ushort))
                    return UInt16.Parse(strValue);
                else if (dataType == typeof(uint))
                    return UInt32.Parse(strValue);
                else if (dataType == typeof(ulong))
                    return UInt64.Parse(strValue);
                else if (dataType == typeof(float))
                    return Single.Parse(strValue);
                else if (dataType == typeof(double))
                    return Double.Parse(strValue);
                else if (dataType == typeof(decimal))
                    return Decimal.Parse(strValue);
                else if (dataType == typeof(DateTime))
                {
                    if (dateTimeFormat == null)
                        return DateTime.Parse(strValue);
                    else
                        return DateTime.ParseExact(strValue, dateTimeFormat.FormatString, dateTimeFormat.FormatProvider);
                }
                else if (dataType == typeof(DateTimeOffset))
                    return ConvertDateTimeOffsetFromJson(strValue, dateTimeFormat);
                else if (dataType == typeof(TimeSpan))
                {
                    DateTimeFormat formatTimeSpan = new DateTimeFormat("c");

                    return TimeSpan.ParseExact(strValue, formatTimeSpan.FormatString, formatTimeSpan.FormatProvider);
                }
                else if (dataType == typeof(bool))
                    return bool.Parse(strValue);
                else if (dataType == typeof(byte[]))
                {
                    byte[] bufData = Convert.FromBase64String(strValue);
                    return bufData;
                }//end if

                return null;
            }
            catch (FormatException)
            {
                if (dataType != typeof(DateTime) && dataType != typeof(bool) && dataType != typeof(byte[]))
                    return 0;
                else if (dataType == typeof(bool))
                    return false;
                else if (dataType == typeof(DateTime))
                    return DateTime.MinValue;
                
                return null;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertDataFromJson function of JsonDataUtils class.");
                return null;
            }
        }

        /// <summary>
        /// Converts a Tiferix.Json DateTimeOffset value into the equivalent .Net DateTimeOffset value.  The Tiferix.Json DateTimeOffset will be stored 
        /// as a concatenated Date/Time and TimeSpan value which will be used to generate the DateTimeOffset value.
        /// </summary>
        /// <param name="strValue">Tiferix.Json DateTimeOffset value to convert to .Net DateTimeOffset value.</param>
        /// <param name="dateTimeFormat">Date/Time format used for the Date/Time portion of the Tiferix.Json DateTimeOffset value.</param>
        /// <returns></returns>
        private static DateTimeOffset ConvertDateTimeOffsetFromJson(string strValue, DateTimeFormat dateTimeFormat = null)
        {
            try
            {
                DateTimeOffset dtoValue;
                DateTime datValue;
                TimeSpan tsValue;
                DateTimeFormat formatTimeSpan = new DateTimeFormat("c");

                string[] aryDatTimeOffset = strValue.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                if (dateTimeFormat == null)
                    datValue = DateTime.Parse(aryDatTimeOffset[0]);
                else
                    datValue = DateTime.ParseExact(aryDatTimeOffset[0], dateTimeFormat.FormatString, dateTimeFormat.FormatProvider);

                tsValue = TimeSpan.ParseExact(aryDatTimeOffset[1], formatTimeSpan.FormatString, formatTimeSpan.FormatProvider);

                dtoValue = new DateTimeOffset(datValue, tsValue);

                return dtoValue;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in ConvertDateTimeOffsetFromJson function of JsonDataUtils class.");
                return DateTimeOffset.MinValue;
            }
        }
        
        #endregion     

        #region Data Information Properties, Functions

        /// <summary>
        /// Checks to see if the Tiferix.Json string value is a valid .Net escape character.  Escape characters in Tiferix.Json files 
        /// will be stored as string literals (with two '\\') and must be converted into valid .Net escape character char values.
        /// </summary>
        /// <param name="strValue">The Tiferix.Json string to examine.</param>
        /// <returns></returns>
        public static bool IsEscapeChar(string strValue)
        {
            try
            {
                switch (strValue)
                {
                    case "\\\'":
                    case "\\\"":
                    case "\\\\":
                    case "\\\0":
                    case "\\\b":
                    case "\\\f":
                    case "\\\n":
                    case "\\\r":
                    case "\\\t":
                    case "\\\v":
                        return true;
                    default:
                        return false;
                }//end switch
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in IsEscapeChar function of JsonDataUtils class.");
                return false;
            }
        }

        #endregion

        #region General Extension Methods

        /// <summary>
        /// Checks to see if the .Net data type is an anonymous data type.  
        /// </summary>
        /// <param name="type">.Net data type.</param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            try
            {                
                bool blHasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
                bool blNameContainsAnonymousType = type.FullName.Contains("AnonymousType");
                Boolean blIsAnonymousType = blHasCompilerGeneratedAttribute && blNameContainsAnonymousType;

                return blIsAnonymousType;
            }
            catch (Exception err)
            {
                ErrorHandler.ShowErrorMessage(err, "Error in IsAnonymousType function of JsonDataUtils class.");
                return false;                    
            }
        }

        #endregion
    }
}
