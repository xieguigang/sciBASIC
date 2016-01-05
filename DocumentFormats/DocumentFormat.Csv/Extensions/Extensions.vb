Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Drawing
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.Linq

''' <summary>
''' The shortcuts operation for the common csv document operations.
''' </summary>
''' <remarks></remarks>
''' 
<PackageNamespace("IO_Device.Csv.Extensions",
                  Description:="The shortcuts operation for the common csv document operations.",
                  Publisher:="xie.guigang@gmail.com")>
Public Module Extensions

    <Extension> Public Sub ForEach(Of T As Class)(path As String, invoke As Action(Of T))
        Call DataStream.OpenHandle(path).ForEach(Of T)(invoke)
    End Sub

    ''' <summary>
    ''' As query source for the LINQ or PLINQ, this function is much save time for the large data set query!
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension> Public Function AsQuery(Of T As Class)(path As String) As Generic.IEnumerable(Of T)
        Return DataStream.OpenHandle(path).AsLinq(Of T)
    End Function

    ''' <summary>
    ''' Convert a database table into a dynamics dataframe in VisualBasic.(将数据库之中所读取出来的数据表转换为表格对象)
    ''' </summary>
    ''' <param name="DataSource"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("DataFrame", Info:="Convert a database table into a dynamics dataframe in VisualBasic.")>
    <Extension> Public Function DataFrame(DataSource As System.Data.Common.DbDataReader) As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.DataFrame
        Dim File As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File = New Csv.DocumentStream.File
        Dim Fields As Integer() = DataSource.FieldCount.Sequence
        Call File.AppendLine((From i As Integer In Fields Select DataSource.GetName(i)).ToArray)

        Do While DataSource.Read
            Call File.AppendLine((From i As Integer In Fields Select s = DataSource.GetValue(i).ToString).ToArray)
        Loop

        Return Csv.DocumentStream.DataFrame.CreateObject(File)
    End Function

    <ExportAPI("Write.Csv")>
    <Extension> Public Function SaveTo(data As Generic.IEnumerable(Of Object), path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return DocumentFormat.Csv.StorageProvider.Reflection.Save(data, False).Save(path, False, encoding)
    End Function

    <ExportAPI("Write.Csv")>
    <Extension> Public Function SaveTo(data As Generic.IEnumerable(Of DocumentFormat.Csv.StorageProvider.ComponentModels.DynamicObjectLoader), Path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Dim Headers = data.First.Schema
        Dim LQuery = (From item In data Select CType((From p In Headers Select item.GetValue(p.Value)).ToList, DocumentFormat.Csv.DocumentStream.RowObject)).ToArray
        Dim File As New Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File

        Call File.AppendLine((From p In Headers Select p.Key).ToList)
        Call File.AppendRange(LQuery)

        Return File.Save(Path, False, encoding)
    End Function

    <ExportAPI("Write.Csv")>
    <Extension> Public Function SaveTo(dat As Generic.IEnumerable(Of DocumentFormat.Csv.DocumentStream.RowObject), Path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Dim Csv As DocumentFormat.Csv.DocumentStream.File = CType(dat, DocumentFormat.Csv.DocumentStream.File)
        Return Csv.Save(Path, Encoding:=encoding)
    End Function

    <ExportAPI("Row.Parsing")>
    <Extension> Public Function ToCsvRow(data As Generic.IEnumerable(Of String)) As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.RowObject
        Return CType(data.ToList, Csv.DocumentStream.RowObject)
    End Function

    ''' <summary>
    ''' Create a dynamics data frame object from a csv document object.(从Csv文件之中创建一个数据框容器)
    ''' </summary>
    ''' <param name="CsvData"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("DataFrame", Info:="Create a dynamics data frame object from a csv document object.")>
    <Extension> Public Function DataFrame(CsvData As Csv.DocumentStream.File) As Csv.DocumentStream.DataFrame
        Return Csv.DocumentStream.DataFrame.CreateObject(CsvData)
    End Function

    ''' <summary>
    ''' Convert the csv data file to a type specific collection.(将目标Csv文件转换为特定类型的集合数据) 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="CsvData"></param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function AsDataSource(Of T As Class)(CsvData As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File, Optional explicit As Boolean = True) As T()
        Dim DataCollection As T() = Csv.StorageProvider.Reflection.Reflector.Convert(Of T)(DocumentFormat.Csv.DocumentStream.DataFrame.CreateObject(CsvData), explicit).ToArray
        Return DataCollection
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="ImportsFile">The file path of the text doucment which will be imports as a csv document.</param>
    ''' <param name="Delimiter">The delimiter to parsing a row in the csv document.</param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function AsDataSource(Of T As Class)(ImportsFile As String, Optional Delimiter As String = ",", Optional explicit As Boolean = True) As T()
        Dim Wrapper = DocumentFormat.Csv.DocumentStream.DataFrame.CreateObject([Imports](ImportsFile, Delimiter))
        Dim DataCollection As T() = Csv.StorageProvider.Reflection.Reflector.Convert(Of T)(Wrapper, explicit).ToArray
        Return DataCollection
    End Function

    ''' <summary>
    ''' Convert the string collection as the type specific collection, please make sure the first element 
    ''' in this collection is stands for the title row.
    ''' (将字符串数组转换为数据源对象，注意：请确保第一行为标题行)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="strDataLines"></param>
    ''' <param name="Delimiter"></param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function AsDataSource(Of T As Class)(strDataLines As Generic.IEnumerable(Of String), Optional Delimiter As String = ",", Optional explicit As Boolean = True) As T()
        Dim Expression As String = String.Format(DocumentFormat.Csv.DataImports.SplitRegxExpression, Delimiter)
        Dim LQuery = (From line As String In strDataLines Select RowParsing(line, Expression)).ToArray
        Return CType(LQuery, Csv.DocumentStream.File).AsDataSource(Of T)(explicit)
    End Function

    ''' <summary>
    ''' Load a csv data file document using a specific object type.(将某一个Csv数据文件加载仅一个特定类型的对象集合中，空文件的话会返回一个空集合，这是一个安全的函数，不会返回空值)
    ''' </summary>
    ''' <typeparam name="T">The type parameter of the element in the returns collection data.</typeparam>
    ''' <param name="Path">The csv document file path.(目标Csv数据文件的文件路径)</param>
    ''' <param name="explicit"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function LoadCsv(Of T As Class)(Path As String, Optional explicit As Boolean = False, Optional encoding As System.Text.Encoding = Nothing) As List(Of T)
        Call "Start to load csv data....".__DEBUG_ECHO
        Dim st = Stopwatch.StartNew
        Dim ChunkBuffer = Csv.StorageProvider.Reflection.Reflector.Load(Of T)(Path, explicit, encoding)
        Call Console.WriteLine(
$"[CSV.Reflector::{GetType(T).FullName}]
Load {ChunkBuffer.Count} lines of data from ""{Path.ToFileURL}""! ...................{st.ElapsedMilliseconds}ms")
        Return ChunkBuffer
    End Function

    <Extension> Public Function LoadCsv(Of T As Class)(DataChunk As Generic.IEnumerable(Of String), Optional explicit As Boolean = True) As T()
        Dim Expression As String = String.Format(DocumentFormat.Csv.DataImports.SplitRegxExpression, ",")
        Dim LQuery = (From line As String In DataChunk Select RowParsing(line, Expression)).ToArray
        Return CType(LQuery, Csv.DocumentStream.File).AsDataSource(Of T)(explicit)
    End Function

    ''' <summary>
    ''' Save the object collection data dump into a csv file.(将一个对象数组之中的对象保存至一个Csv文件之中，请注意，这个方法仅仅会保存简单的基本数据类型的属性值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="path"></param>
    ''' <param name="explicit">If true then all of the simple data type property its value will be save to the data file, 
    ''' if not then only save the property with the <see cref="Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.ColumnAttribute"></see>
    ''' </param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SaveTo(Of T As Class)(Collection As Generic.IEnumerable(Of T),
                                                      path As String,
                                                      Optional explicit As Boolean = False,
                                                      Optional encoding As System.Text.Encoding = Nothing) As Boolean

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call Console.WriteLine("[CSV.Reflector::{0}]" & vbCrLf & "Save data to file:///{1}", GetType(T).FullName, path)
        Call Console.WriteLine("[CSV.Reflector] Reflector have {0} lines of data to write.", Collection.Count)

        If Collection.Count > 20000 Then
            Call Csv.StorageProvider.Reflection.Reflector.Save(Collection, explicit).Save(path, LazySaved:=True, encoding:=encoding)
        Else
            Call Csv.StorageProvider.Reflection.Reflector.Save(Collection, explicit).Save(path, LazySaved:=False, encoding:=encoding)
        End If

        Call Console.WriteLine("CSV saved!")

        Return True
    End Function

    ''' <summary>
    ''' Generate a csv document from a object collection.(从一个特定类型的数据集合之中生成一个Csv文件，非并行化的以保持数据原有的顺序)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="explicit">默认导出所有的可用属性</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToCsvDoc(Of T As Class)(source As Generic.IEnumerable(Of T), Optional explicit As Boolean = False) As DocumentStream.File
        Return Csv.StorageProvider.Reflection.Reflector.Save(source, explicit)
    End Function

    ''' <summary>
    ''' Save the data collection vector as a csv document.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Write.Csv", Info:="Save the data collection vector as a csv document.")>
    <Extension> Public Function SaveTo(data As Generic.IEnumerable(Of Double), path As String) As Boolean
        Dim Row As List(Of String) = (From n In data Select s = n.ToString).ToList
        Dim Csv = CType({CType(Row, Csv.DocumentStream.RowObject)}, Csv.DocumentStream.File)
        Return Csv.Save(path, LazySaved:=False)
    End Function

    ''' <summary>
    ''' Load the data from the csv document as a double data type vector. 
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("DblVector.LoadCsv", Info:="Load the data from the csv document as a double data type vector. ")>
    <Extension> Public Function LoadDblVector(path As String) As Double()
        Dim Csv = Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File.Load(path)
        Dim FirstRow = Csv.First
        Dim DblCollection = (From s As String In FirstRow Select Val(s)).ToArray
        Return DblCollection
    End Function

    <Extension> Public Function Cable(Of T)(Method As Microsoft.VisualBasic.Scripting.LoadObject(Of T)) As Boolean
        Dim typeDef As Type = GetType(T)
        Call Microsoft.VisualBasic.Scripting.UpdateHandle(typeDef.FullName, typeDef, Function(field) Method(field))
        Return True
    End Function
End Module