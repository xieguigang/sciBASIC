# Easy Document in VisualBasic

Here are some of the data object that we want to save to file and read object from the file. In VisualBasic, there are two document format for the storage of simple document(_*.ini_, _*.Csv_), and 3 type of document format(_*.json_, _*.Xml_, _*.dat_) for the storage of complex object automatically.
In this document format guidelines, we want to introduce how easily that save object instance in the VisualBasic program.

```vbnet
<IniMapIO("#/test.ini")>
Public Class Profiles
    Public Property Test As TestBin
End Class
```
```vbnet
<ClassName("JSON")>
<Serializable> Public Class TestBin
    <DataFrameColumn> Public Property Property1 As String
    <DataFrameColumn> Public Property D As Date
    <DataFrameColumn> Public Property n As Integer
    <DataFrameColumn> Public Property f As Double
```
```vbnet
    Public Shared Function inst() As TestBin
        Return New TestBin With {
            .D = Now,
            .f = RandomDouble(),
            .n = RandomDouble() * 1000,
            .Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
        }
    End Function
End Class
```

First of all, we define a object for the test example in this article:

```vbnet
Dim a As TestBin = TestBin.inst  ' Init test data
```

##INI

There are two Win32 API was used for ini profile file data read and write:

```vbnet
''' <summary>
''' Write a string value into a specific section in a specifc ini profile.(在初始化文件指定小节内设置一个字串)
''' </summary>
''' <param name="section">
''' <see cref="String"/>，要在其中写入新字串的小节名称。这个字串不区分大小写
''' </param>
''' <param name="key">
''' <see cref="String"/>，要设置的项名或条目名。这个字串不区分大小写。
''' 用<see cref="vbNullString"/>可删除这个小节的所有设置项
''' </param>
''' <param name="val">
''' <see cref="String"/>，指定为这个项写入的字串值。用<see cref="vbNullString"/>表示删除这个项现有的字串
''' </param>
''' <param name="filePath">
''' <see cref="String"/>，初始化文件的名字。如果没有指定完整路径名，则windows会在windows目录查找文件。
''' 如果文件没有找到，则函数会创建它</param>
''' <returns>Long，非零表示成功，零表示失败。会设置<see cref="GetLastErrorAPI.GetLastError()"/></returns>
<DllImport("kernel32")>
Public Shared Function WritePrivateProfileString(section As String,
                                                 key As String,
                                                 val As String,
                                                 filePath As String) As Long
End Function

''' <summary>
''' 为初始化文件中指定的条目取得字串
''' </summary>
''' <param name="section">
''' String，欲在其中查找条目的小节名称。这个字串不区分大小写。如设为vbNullString，就在lpReturnedString
''' 缓冲区内装载这个ini文件所有小节的列表。
''' </param>
''' <param name="key">
''' String，欲获取的项名或条目名。这个字串不区分大小写。如设为vbNullString，就在lpReturnedString
''' 缓冲区内装载指定小节所有项的列表
''' </param>
''' <param name="def">String，指定的条目没有找到时返回的默认值。可设为空（""）</param>
''' <param name="retVal">String，指定一个字串缓冲区，长度至少为nSize</param>
''' <param name="size">Long，指定装载到lpReturnedString缓冲区的最大字符数量</param>
''' <param name="filePath">
''' String，初始化文件的名字。如没有指定一个完整路径名，windows就在Windows目录中查找文件
''' </param>
''' <returns>
''' Long，复制到lpReturnedString缓冲区的字节数量，其中不包括那些NULL中止字符。如lpReturnedString
''' 缓冲区不够大，不能容下全部信息，就返回nSize-1（若lpApplicationName或lpKeyName为NULL，则返回nSize-2）
''' </returns>
<DllImport("kernel32")>
Public Shared Function GetPrivateProfileString(section As String,
                                               key As String,
                                               def As String,
                                               retVal As StringBuilder,
                                               size As Integer,
                                               filePath As String) As Integer
End Function
```

And the wrapper for the ini data serialization and deserialization is already been developed for the Class object in the VisualBasic. First just needs imports two namespace, and then let's see how simple it is:

```vbnet
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
```

Assuming that you have a **TestBin** type simple object, then you want to write this object as the program profile ini file, so that you just needs using **WriteClass** function, **if your object just stands for a section in the ini profile file.**

```vbnet
Call New Profiles With {.Test = a}.WriteProfile  ' Write profile file data
Call a.WriteClass("./test2.ini")                 ' Write ini section data.
a = Nothing
a = "./test2.ini".LoadIni(Of TestBin)                        ' Load ini section data
Dim pp As Profiles = "./test2.ini".LoadProfile(Of Profiles)  ' Load entire ini file
```

And in this example the **WriteClass** function produce the ini profile data as:

>[**JSON**]<br />
>Property1=_{"_BufferLength":8,"_ChunkBuffer":[72,84,84,80,47,53,50,48],"_Protocol":520,"_ProtocolCategory":0,"_uid":0}_<br />
>D=_5/20/2016 3:40:27 PM_<br />
>n=_330_<br />
>f=_0.33_<br />

NOTE: the profile key in the ini file should be decorating with **&lt;DataFrameColumn>** attribute, and using **ClassName** attribute on the Class object definition, can makes tweaks on your section name and allow some identifier illegal character in VisualBasic is also able used as the section name, example is a section name is **"test-section"**, the character - is illegal in the VB identifier, so that you just needs using this attribute decorated as **&lt;ClassName("test-section")>**, the same of the usage of **&lt;DataFrameColumn>** attribute can be applied on the property.

## Structure Binary

In VisualBasic, there is a BinaryFormatter that can be used for the binary serialization, all you needs to do just imports a namespace **Microsoft.VisualBasic.Serialization.BinaryDumping.StructFormatter**, then you get two extension method for the serialization and deserialization.

```vbnet
Call a.Serialize("./test.dat")   ' test on the binary serialization
a = Nothing
a = "./test.dat".Load(Of TestBin)
```

**StructFormatter.Serialize** for save any object as a binary file, and **StructFormatter.Load(of T)** function for load any object from a binary file.

In fact, except the **BinaryFormatter** class, there is another method that can be using for the binary serialization, by using **Marshal.StructureToPtr** and **Marshal.PtrToStructure** function and combine with **Marshal.Copy** for memory data copy, that we can serialize any structure object into binary stream, in theoretical, and from the test it actually works, but there is some problems in the serialization on the **String** value(As the **String** type is a reference type, so that when performance this serialization work, it actually serialize the memory address of your string, so that this function just works perfect on the **Integer, Long, Short, Enum, etc, value types.**):

```vbnet
' Summary:
'     [Supported in the .NET Framework 4.5.1 and later versions] Marshals data from
'     a managed object of a specified type to an unmanaged block of memory.
'
' Parameters:
'   structure:
'     A managed object that holds the data to be marshaled. The object must be a structure
'     or an instance of a formatted class.
'
'   ptr:
'     A pointer to an unmanaged block of memory, which must be allocated before this
'     method is called.
'
'   fDeleteOld:
'     true to call the System.Runtime.InteropServices.Marshal.DestroyStructure``1(System.IntPtr)
'     method on the ptr parameter before this method copies the data. The block must
'     contain valid data. Note that passing false when the memory block already contains
'     data can lead to a memory leak.
'
' Type parameters:
'   T:
'     The type of the managed object.
'
' Exceptions:
'   T:System.ArgumentException:
'     structure is a reference type that is not a formatted class.
<SecurityCritical>
Public Shared Sub StructureToPtr(Of T)([structure] As T, ptr As IntPtr, fDeleteOld As Boolean)
```

```vbnet
' Summary:
'     Marshals data from an unmanaged block of memory to a newly allocated managed
'     object of the specified type.
'
' Parameters:
'   ptr:
'     A pointer to an unmanaged block of memory.
'
'   structureType:
'     The type of object to be created. This object must represent a formatted class
'     or a structure.
'
' Returns:
'     A managed object containing the data pointed to by the ptr parameter.
'
' Exceptions:
'   T:System.ArgumentException:
'     The structureType parameter layout is not sequential or explicit.-or-The structureType
'     parameter is a generic type.
'
'   T:System.ArgumentNullException:
'     structureType is null.
'
'   T:System.MissingMethodException:
'     The class specified by structureType does not have an accessible default constructor.
<ComVisible(True)> <SecurityCritical>
Public Shared Function PtrToStructure(ptr As IntPtr, structureType As Type) As Object
```

##JSON

The json format is a popular data format on the network, in my job, the d3js with VisualBasic hybrids solution required of json data,

This document format in VisualBasic needs imports this namespace at first:

```vbnet
Imports Microsoft.VisualBasic.Serialization
```

And the json serialization extension method is based on the System Json serialization solution:

```vbnet
Imports System.Runtime.Serialization.Json
Imports System.Web.Script.Serialization
```

```vbnet
''' <summary>
''' Gets the json text value of the target object, the attribute <see cref="ScriptIgnoreAttribute"/> 
''' can be used for block the property which is will not serialize to the text.
''' (使用<see cref="ScriptIgnoreAttribute"/>来屏蔽掉不想序列化的属性)
''' </summary>
''' <typeparam name="T"></typeparam>
''' <param name="obj"></param>
''' <returns></returns>
<Extension> Public Function GetJson(Of T)(obj As T) As String
```

```vbnet
''' <summary>
''' JSON反序列化
''' </summary>
<Extension> Public Function LoadObject(Of T)(json As String) As T
```

And just using this two extension that can enable you to serialize any object in to Json document and deserialize Json document for instance any object type:

```vbnet
Dim json As String = a.GetJson   ' JSON serialization test
a = Nothing
a = json.LoadObject(Of TestBin)
Call json.__DEBUG_ECHO
```

And there is another perfect fast Json serialization solution for VisualBasic: [Newton.Json](https://github.com/JamesNK/Newtonsoft.Json), but in this article I just want to introduce the System json serialization solution as this solution no needs for referecne of the third-part library.

##XML

Generates the Xml document is very easy in the VisualBasic, just using your object's **GetXml** extension function, and then using function **LoadXml(of T)** can be easily load your saved object from a Xml file:

```vbnet
' XML test
Dim xml As String = a.GetXml   ' Convert object into Xml
Call xml.__DEBUG_ECHO
Call a.SaveAsXml("./testssss.Xml")   ' Save Object to Xml
a = Nothing
a = "./testssss.Xml".LoadXml(Of TestBin)  ' Load Object from Xml
Call a.GetXml.__DEBUG_ECHO
```

###### Simple Usage

1. <T>.GetXml<br />
	+ _Gets the object generated Xml document text. By combine using of the **String.SaveTo(path)** Extension function, that you can easily save the object as a Xml text file._
2. <path>.LoadXml(of T)<br />
	+ _Load object from Xml_

## Csv

By using this serialization feature, you should imports this namespace at first:

```vb.net
Imports Microsoft.VisualBasic.DocumentFormat.Csv
```

Assuming that you have a type specific collection, and you want to save this collection into Csv data file and for the data exchange with R language or d3js data visualization, so that you just needs simple by using SaveTo extension function applied on your data collection, then you are save successfully your data collection into a csv data file.

```vbnet
Dim array As TestBin() = {a, a, a, a, a, a, a, a, a, a}   ' We have a collection of object
Call array.SaveTo("./test.Csv")    ' then wen can save this collection into Csv file
array = Nothing
array = "./test.Csv".LoadCsv(Of TestBin)  ' test on load csv data
Call array.GetJson.__DEBUG_ECHO
```

If you want to load the csv data to a collection, so that you just needs using LoadCsv(Of T) function:

```vbnet
<path>.LoadCsv(Of <type>)
```

A bug in the Microsoft Excel Csv Parser was found in this test: The filed Property1 is a json text in this test, but the Excel parser can not parsing this field correctly.

> ![The fields is not parsing successful in the Microsoft Excel](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/guides/ExcelBugs.png)

> ![The fields parsing successful in this library](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/guides/ParserSuccess.png)

### Additional
+ Read and Write text document

**Read/Write** text document in the VisualBasic is so easy!, just one method for write text file and two method for read text file, here is a very simple example:

```vbnet
Dim s As String = array.GetJson
Call s.SaveTo("./tesssss.txt")
```

```vbnet
Dim lines As String() = "./tesssss.txt".ReadAllLines()
s = "./tesssss.txt".ReadAllText
```

######Usage

You can not believe how easily that you can do on the text document read/write! *By using the system default text file read/write function, you should determined that the parent directory of your text file is exists or not, or if not exist when you save you text file data, then your program will crash*. But using these VisualBasic text file read/write function, no worried about this problem, the function is already do it for you.

>1. _&lt;String>_.**SaveTo**(_path_)
>2. _&lt;path>_.**ReadAllText()**
>3. _&lt;path>_.**ReadAllLines()**

+ Read/Write binary data

>The usage of the binary data **Byte()** read/write is as easy as the same of text file read/write:

>1. Byte().**FlushStream**(_&lt;path>_) ' Write binary data
>2. _&lt;path>_.**ReadBinary()** ' Read binary bytes from file.

NOTE

> All of the example test code can be download from [here](https://github.com/xieguigang/VisualBasic_AppFramework/tree/master/Example/EasyDocument)