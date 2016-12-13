![](http://www.tiferix.com/Logos/TiferixJsonLogo.jpg)



# Tiferix.Json
The Tiferix.Json library contains a set of libraries that will allow you to directly read from and write to Json files on disk and data streams in .NET, as well as serialize and deserialize various types of .Net classes and objects to and from Json files.  In addition to serializing/deserializing Json files into .Net data objects, certain Tiferix.Json libraries will allow the use of schema files that can be used in conjunction with data serialization.  Schema files used with the Tiferix.Json library are robust and can be used to load data objects, like ADO.NET data object schemas and enforce proper data integrity when reading and writing Json data files.  The Tiferix.Json JsonDataSet library is a powerful feature that allows for parsing, serialization and deserialization of ADO.NET DataSets and DataTable objects stored in Json format.  In addition, to providing a powerful Json data writing class, the TiferixJson can perform certain automated functions, such as auto-identing your Json file output.  The Tiferix.Json library is very simple to use and allows you to take advantage of many powerful features of Json in .Net without the complexities of other Json libraries.
NOTES:
- As of the current time the only Tiferix.Json serialization library available in the JsonDataSet serialization libraries, which also includes a schema serialization library.  The Tiferix.Json is an ongoing project and support for full .Net object serialization and deserialization will be supported in future versions.
- Currently, the Tiferix.Json schema library cannot be used for validation of a Json data file, but this feature will be implemented in future versions.







#Tiferix.Json.Data
The Data library is used to handle the reading and writing of all ADO.Net DataSet and DataTable objects and their related schemas to Tiferix.Json data files.  The Tiferix.Json.Data library will allow you to both write ADO.Net data objects into Json files, as well as create ADO.Net schemas that can be stored and loaded from Json files.  Since the library allows full Json support for ADO.Net, it is also very simple to convert your Json data to XML and XML data to Json.

#Tiferix.Json.Serialization
The Serialization library handles the serialization and deserialization of various types of Tiferix.Json data and object libraries to and from Json files.  In addition, the library handles the serialization and deserialization of various schema libraries used by the Tiferix.Json lirary.  
NOTE: As of this version, only the serialization/deserialization of ADO.Net data ojects is supported.

#Tiferix.Json.Tools
The Tools library contains various classes to allow for the reading and writing of Json files.  In addition, to reader/writer classes, the Tools library contains various utility classes that will handle other tasks of the Tiferix.Json library.   
NOTE: As of this version, a JsonDataReader class is not supported.  In the next release a JsonDataReader class will be added to allow for raw reading of Json data files.  