#Easy Document in VisualBasic

Here are some of the data object that we want to save to file and read object from the file. In VisualBasic, there are two document format for the storage of simple document(_*.ini_, _*.Csv_), and 3 type of document format(_*.json_, _*.Xml_, _*.dat_) for the storage of complex object automatically.
In this document format guidelines, we want to introduce how easily that save object instance in the VisualBasic program.


>     <IniMapIO("#/test.ini")>
     Public Class Profiles
         Public Property Test As TestBin
     End Class

>     <ClassName("JSON")>
	<Serializable> Public Class TestBin
    	<DataFrameColumn> Public Property Property1 As String
    	<DataFrameColumn> Public Property D As Date
    	<DataFrameColumn> Public Property n As Integer
    	<DataFrameColumn> Public Property f As Double

>         Public Shared Function inst() As TestBin
          	Return New TestBin With {
               	.D = Now,
               	.f = RandomDouble(),
               	.n = RandomDouble() * 1000,
               	.Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
           	}
        End Function
	End Class

First of all, we define a object for the test example in this article:

>     Dim a As TestBin = TestBin.inst  ' Init test data

##INI

There are two Win32 API was used for ini profile file data read and write:



>     ''' <summary>
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

>     ''' <summary>
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


And the wrapper for the ini data serialization and deserialization is already been developed for the Class object in the VisualBasic. First just needs imports two namespace, and then let's see how simple it is:

>Imports **Microsoft.VisualBasic.ComponentModel.DataSourceModel**
>Imports **Microsoft.VisualBasic.ComponentModel.Settings.Inf**

Assuming that you have a **TestBin** type simple object, then you want to write this object as the program profile ini file, so that you just needs using **WriteClass** function, **if your object just stands for a section in the ini profile file.**


>     Call New Profiles With {.Test = a}.WriteProfile  ' Write profile file data
    Call a.WriteClass("./test2.ini")                 ' Write ini section data.
    a = Nothing
    a = "./test2.ini".LoadIni(Of TestBin)                        ' Load ini section data
    Dim pp As Profiles = "./test2.ini".LoadProfile(Of Profiles)  ' Load entire ini file

And in this example the **WriteClass** function produce the ini profile data as:

>[**JSON**]
>Property1=_{"_BufferLength":8,"_ChunkBuffer":[72,84,84,80,47,53,50,48],"_Protocol":520,"_ProtocolCategory":0,"_uid":0}_
>D=_5/20/2016 3:40:27 PM_
>n=_330_
>f=_0.33_

NOTE: the profile key in the ini file should be decorating with **&lt;DataFrameColumn>** attribute, and using **ClassName** attribute on the Class object definition, can makes tweaks on your section name and allow some identifier illegal character in VisualBasic is also able used as the section name, example is a section name is **"test-section"**, the character - is illegal in the VB identifier, so that you just needs using this attribute decorated as **&lt;ClassName("test-section")>**, the same of the usage of **&lt;DataFrameColumn>** attribute can be applied on the property.



##Structure Binary

In VisualBasic, there is a BinaryFormatter that can be used for the binary serialization, all you needs to do just imports a namespace **Microsoft.VisualBasic.Serialization.BinaryDumping.StructFormatter**, then you get two extension method for the serialization and deserialization.

>     Call a.Serialize("./test.dat")   ' test on the binary serialization
>     a = Nothing
>     a = "./test.dat".Load(Of TestBin)

> **StructFormatter.Serialize** for save any object as a binary file, and **StructFormatter.Load(of T)** function for load any object from a binary file.

In fact, except the **BinaryFormatter** class, there is another method that can be using for the binary serialization, by using **Marshal.StructureToPtr** and **Marshal.PtrToStructure** function and combine with **Marshal.Copy** for memory data copy, that we can serialize any structure object into binary stream, in theoretical, and from the test it actually works, but there is some problems in the serialization on the **String** value(As the **String** type is a reference type, so that when performance this serialization work, it actually serialize the memory address of your string, so that this function just works perfect on the **Integer, Long, Short, Enum, etc, value types.**):

>     ' Summary:
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

>     ' Summary:
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

##JSON







##XML

##Csv



A bug in the Microsoft Excel Csv Parser was found in this test: The filed Property1 is a json text in this test, but the Excel parser can not parsing this field correctly.



> ![The fields is not parsing successful in the Microsoft Excel](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/guides/ExcelBugs.png)

> ![The fields parsing successful in this library](https://raw.githubusercontent.com/xieguigang/VisualBasic_AppFramework/master/guides/ParserSuccess.png)



> All of the example test code can be download from [here](https://github.com/xieguigang/VisualBasic_AppFramework/tree/master/Example/EasyDocument)
