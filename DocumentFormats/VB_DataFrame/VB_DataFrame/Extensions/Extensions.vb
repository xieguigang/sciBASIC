#Region "Microsoft.VisualBasic::d3689aeab39d626292aa3305ab21bf35, ..\VisualBasic_AppFramework\DocumentFormats\VB_DataFrame\VB_DataFrame\Extensions\Extensions.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.ComponentModel
Imports System.Data.Common
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.Linq
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.InputHandler
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' The shortcuts operation for the common csv document operations.
''' </summary>
''' <remarks></remarks>
'''
<PackageNamespace("IO_Device.Csv.Extensions",
                  Description:="The shortcuts operation for the common csv document operations.",
                  Publisher:="xie.guigang@gmail.com")>
Public Module Extensions

    Sub New()
        Call InitHandle()
    End Sub

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="path">Csv file path</param>
    ''' <returns></returns>
    Public Function GetLocusMapName(path As String) As String
        Dim first As String = path.ReadFirstLine
        Dim tokens = DocumentStream.CharsParser(first)
        Return tokens.FirstOrDefault
    End Function

    <Extension>
    Public Function TabExport(Of T As Class)(source As IEnumerable(Of T), saveTo As String, Optional noTitle As Boolean = False, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim doc As DocumentStream.File = StorageProvider.Reflection.Reflector.Save(source, False)
        Dim lines As DocumentStream.RowObject() = If(noTitle, doc.Skip(1).ToArray, doc.ToArray)
        Dim slines As String() = lines.ToArray(Function(x) x.AsLine(vbTab))
        Dim sdoc As String = String.Join(vbCrLf, slines)
        Return sdoc.SaveTo(saveTo, encoding.GetEncodings)
    End Function

    <Extension> Public Sub ForEach(Of T As Class)(path As String, invoke As Action(Of T))
        Call DataStream.OpenHandle(path).ForEach(Of T)(invoke)
    End Sub

    ''' <summary>
    ''' As query source for the LINQ or PLINQ, this function is much save time for the large data set query!
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension> Public Function AsLinq(Of T As Class)(path As String) As IEnumerable(Of T)
        Return DataStream.OpenHandle(path).AsLinq(Of T)
    End Function

    ''' <summary>
    ''' Convert a database table into a dynamics dataframe in VisualBasic.(将数据库之中所读取出来的数据表转换为表格对象)
    ''' </summary>
    ''' <param name="DataSource"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI(NameOf(DataFrame),
               Info:="Convert a database table into a dynamics dataframe in VisualBasic.")>
    <Extension> Public Function DataFrame(DataSource As DbDataReader) As DataFrame
        Dim File As DocumentStream.File = New DocumentStream.File
        Dim Fields As Integer() = DataSource.FieldCount.Sequence
        Call File.AppendLine((From i As Integer In Fields Select DataSource.GetName(i)).ToArray)

        Do While DataSource.Read
            Call File.AppendLine((From i As Integer In Fields Select s = DataSource.GetValue(i).ToString).ToArray)
        Loop

        Return DataFrame.CreateObject(File)
    End Function

    <ExportAPI("Write.Csv")>
    <Extension> Public Function SaveTo(data As IEnumerable(Of Object), path As String, Optional encoding As Encoding = Nothing) As Boolean
        Return Save(data, False).Save(path, False, encoding)
    End Function

    <ExportAPI("Write.Csv")>
    <Extension> Public Function SaveTo(data As IEnumerable(Of DynamicObjectLoader), Path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Dim Headers = data.First.Schema
        Dim LQuery = (From item In data Select CType((From p In Headers Select item.GetValue(p.Value)).ToList, RowObject)).ToArray
        Dim File As New DocumentStream.File

        Call File.AppendLine((From p In Headers Select p.Key).ToList)
        Call File.AppendRange(LQuery)

        Return File.Save(Path, False, encoding)
    End Function

    <ExportAPI("Write.Csv")>
    <Extension> Public Function SaveTo(dat As IEnumerable(Of DocumentStream.RowObject), Path As String, Optional encoding As Encoding = Nothing) As Boolean
        Dim Csv As DocumentStream.File = CType(dat, DocumentStream.File)
        Return Csv.Save(Path, Encoding:=encoding)
    End Function

    <ExportAPI("Row.Parsing")>
    <Extension> Public Function ToCsvRow(data As Generic.IEnumerable(Of String)) As RowObject
        Return CType(data.ToList, Csv.DocumentStream.RowObject)
    End Function

    ''' <summary>
    ''' Create a dynamics data frame object from a csv document object.(从Csv文件之中创建一个数据框容器)
    ''' </summary>
    ''' <param name="CsvData"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI(NameOf(DataFrame), Info:="Create a dynamics data frame object from a csv document object.")>
    <Extension> Public Function DataFrame(CsvData As Csv.DocumentStream.File) As Csv.DocumentStream.DataFrame
        Return Csv.DocumentStream.DataFrame.CreateObject(CsvData)
    End Function

    ''' <summary>
    ''' Convert the csv data file to a type specific collection.(将目标Csv文件转换为特定类型的集合数据)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dataSet"></param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function AsDataSource(Of T As Class)(dataSet As DocumentStream.File, Optional explicit As Boolean = False) As T()
        Dim dataFrame As DataFrame =
            DocumentStream.DataFrame.CreateObject(dataSet)
        Dim source As T() = Reflector.Convert(Of T)(dataFrame, explicit).ToArray
        Return source
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
        Dim DataCollection As T() = Reflector.Convert(Of T)(Wrapper, explicit).ToArray
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
    <Extension> Public Function AsDataSource(Of T As Class)(strDataLines As IEnumerable(Of String), Optional Delimiter As String = ",", Optional explicit As Boolean = True) As T()
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
    ''' <param name="maps">``Csv.Field -> <see cref="PropertyInfo.Name"/>``</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function LoadCsv(Of T As Class)(Path As String,
                                                       Optional explicit As Boolean = False,
                                                       Optional encoding As Encoding = Nothing,
                                                       Optional fast As Boolean = False,
                                                       Optional maps As Dictionary(Of String, String) = Nothing) As List(Of T)
        Call "Start to load csv data....".__DEBUG_ECHO
        Dim st = Stopwatch.StartNew
        Dim bufs = Reflector.Load(Of T)(Path, explicit, encoding, fast, maps)
        Dim ms As Long = st.ElapsedMilliseconds
        Dim fs As String = If(ms > 1000, (ms / 1000) & "sec", ms & "ms")
        Call $"[CSV.Reflector::{GetType(T).FullName}]
Load {bufs.Count} lines of data from ""{Path.ToFileURL}""! ...................{fs}".__DEBUG_ECHO
        Return bufs
    End Function

    ''' <summary>
    ''' Load object data set from the text lines stream.(从文本行之中加载数据集)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    <Extension> Public Function LoadStream(Of T As Class)(
                                              source As IEnumerable(Of String),
                                              Optional explicit As Boolean = True) As T()
        Dim dataFrame As DocumentStream.File =
            DocumentStream.File.Load(source.ToArray)
        Dim buf As T() = dataFrame.AsDataSource(Of T)(explicit)
        Return buf
    End Function

    ''' <summary>
    ''' Save the object collection data dump into a csv file.(将一个对象数组之中的对象保存至一个Csv文件之中，请注意，这个方法仅仅会保存简单的基本数据类型的属性值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="path"></param>
    ''' <param name="explicit">If true then all of the simple data type property its value will be save to the data file,
    ''' if not then only save the property with the <see cref="Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.ColumnAttribute"></see>
    ''' </param>
    ''' <param name="encoding"></param>
    ''' <param name="maps">{meta_define -> custom}</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SaveTo(Of T)(source As IEnumerable(Of T),
                                             path As String,
                                             Optional explicit As Boolean = False,
                                             Optional encoding As Encoding = Nothing,
                                             Optional metaBlank As String = "",
                                             Optional nonParallel As Boolean = False,
                                             Optional maps As Dictionary(Of String, String) = Nothing) As Boolean

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call Console.WriteLine("[CSV.Reflector::{0}]" & vbCrLf & "Save data to file:///{1}", GetType(T).FullName, path)
        Call Console.WriteLine("[CSV.Reflector] Reflector have {0} lines of data to write.", source.Count)

        Dim df As DocumentStream.File =
            Reflector.Save(source,
                           explicit,
                           metaBlank,
                           maps,
                           Not nonParallel)
        Dim lazy As Boolean = df.RowNumbers > 20000

        If nonParallel Then
            lazy = False
        End If

        Call df.Save(path, LazySaved:=lazy, encoding:=encoding)
        Call "CSV saved!".__DEBUG_ECHO

        Return True
    End Function

    <Extension> Public Function SaveTo(Of T)(source As IEnumerable(Of T),
                                             path As String,
                                             encoding As Encodings,
                                             Optional explicit As Boolean = False) As Boolean
        Return source.SaveTo(path, explicit, encoding.GetEncodings)
    End Function

    ''' <summary>
    ''' Generate a csv document from a object collection.(从一个特定类型的数据集合之中生成一个Csv文件，非并行化的以保持数据原有的顺序)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="explicit">默认导出所有的可用属性</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToCsvDoc(Of T As Class)(source As IEnumerable(Of T), Optional explicit As Boolean = False) As DocumentStream.File
        Return Reflector.Save(source, explicit)
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
    <Extension> Public Function SaveTo(data As IEnumerable(Of Double), path As String) As Boolean
        Dim row As IEnumerable(Of String) = From n As Double
                                            In data
                                            Select s =
                                                n.ToString
        Dim buf As New DocumentStream.File({New RowObject(row)})
        Return buf.Save(path, LazySaved:=False)
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
        Dim buf As DocumentStream.File = DocumentStream.File.Load(path)
        Dim FirstRow As RowObject = buf.First
        Dim data As Double() = FirstRow.ToArray(AddressOf Val)
        Return data
    End Function

    <Extension> Public Sub Cable(Of T)(Method As LoadObject(Of T))
        Dim type As Type = GetType(T)
        Dim name As String = type.FullName
        Dim helper As New __loadHelper(Of T) With {
            .handle = Method
        }

        Call CapabilityPromise(name, type, AddressOf helper.LoadObject)
    End Sub

    Private Structure __loadHelper(Of T)
        Public handle As LoadObject(Of T)

        Public Function LoadObject(s As String) As T
            Return handle(s)
        End Function
    End Structure
End Module
