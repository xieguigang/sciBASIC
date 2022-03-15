#Region "Microsoft.VisualBasic::63f24a05632213261be8c904f16f375d, sciBASIC#\docs\guides\Example\EasyDocument\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 354
    '    Code Lines: 230
    ' Comment Lines: 60
    '   Blank Lines: 64
    '     File Size: 13.76 KB


    '     Delegate Function
    ' 
    ' 
    '     Class DataStream
    ' 
    '         Properties: SchemaOridinal
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: AsLinq, BufferProvider, GetOrdinal, OpenHandle
    ' 
    '         Sub: (+2 Overloads) Dispose, ForEach, ForEachBlock
    '         Structure __taskHelper
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: RunTask
    ' 
    ' 
    ' 
    ' Class BlockTest
    ' 
    '     Properties: a
    ' 
    '     Function: ToString
    ' 
    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' Class Profiles
    ' 
    '     Properties: Test
    ' 
    ' Class TestBin
    ' 
    '     Properties: D, f, n, Property1
    ' 
    '     Function: inst
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping.StructFormatter

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.Linq

Namespace DocumentStream.Linq

    Public Delegate Function GetOrdinal(Column As String) As Integer

    ''' <summary>
    ''' Buffered large text dataset Table reader
    ''' </summary>
    Public Class DataStream : Inherits BufferedStream
        Implements ISchema
        Implements IDisposable

        ReadOnly _schema As Dictionary(Of String, Integer)
        ReadOnly _title As RowObject

        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal
            Get
                Return _schema
            End Get
        End Property

        Sub New()
            _schema = New Dictionary(Of String, Integer)
            _title = New RowObject
        End Sub

        Sub New(file As String, Optional encoding As Encoding = Nothing, Optional bufSize As Integer = 64 * 1024 * 1024)
            Call MyBase.New(file, encoding, bufSize)

            Dim first As String = file.ReadFirstLine

            _title = RowObject.TryParse(first)
            _schema = _title.ToArray(
                Function(colName, idx) New With {
                    .colName = colName,
                    .ordinal = idx}) _
                    .ToDictionary(Function(x) x.colName.ToLower,
                                  Function(x) x.ordinal)
            Me.FileName = file

            Call $"{file.ToFileURL} handle opened...".__DEBUG_ECHO
        End Sub

        Public Function GetOrdinal(Name As String) As Integer Implements ISchema.GetOrdinal
            If _schema.ContainsKey(Name.ToLower.ShadowCopy(Name)) Then
                Return _schema(Name)
            Else
                Return -1
            End If
        End Function

        Dim __firstBlock As Boolean = True

        ''' <summary>
        ''' Providers the data buffer for the <see cref="RowObject"/>
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数主要是为了处理第一行数据
        ''' 因为在构造函数部分已经读取了第一行来解析schema，所以在这里需要对第一个数据块做一些额外的处理
        ''' </remarks>
        Public Overrides Function BufferProvider() As String()
            Dim buffer As String() = MyBase.BufferProvider()

            If __firstBlock Then
                __firstBlock = False
                buffer = buffer.Skip(1).ToArray
            Else         '  不是第一个数据块，则不需要额外处理，直接返回
            End If

            Return buffer
        End Function

        ''' <summary>
        ''' For each item in the source data fram, invoke a specific task
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="invoke"></param>
        Public Sub ForEach(Of T As Class)(invoke As Action(Of T))
            Dim line As String = ""
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While True
                Dim buffer As String() = BufferProvider()
                Dim p As Integer = 0

                Do While Not buffer.Read(p, out:=line) Is Nothing
                    Dim row As RowObject = RowObject.TryParse(line)
                    Dim obj As T = Activator.CreateInstance(Of T)
                    obj = RowBuilder.FillData(Of T)(row, obj)
                    Call invoke(obj)
                Loop

                If EndRead Then
                    Exit Do
                Else
                    Call Console.WriteLine("Process next block....")
                End If
            Loop
        End Sub

        ''' <summary>
        ''' Processing large dataset in block partitions.(以分块任务的形式来处理一个非常大的数据集)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="invoke">task of this block buffer</param>
        ''' <param name="blockSize">Lines of the data source.(行数)</param>
        Public Sub ForEachBlock(Of T As Class)(invoke As Action(Of T()), Optional blockSize As Integer = 10240 * 5)
            Dim schema As SchemaProvider =
                SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject ' 生成schema映射模型
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While True
                Dim chunks As IEnumerable(Of String()) =
                    TaskPartitions.SplitIterator(BufferProvider(), blockSize)

                For Each block As String() In chunks
                    Dim LQuery As RowObject() = (From line As String
                                                 In block.AsParallel
                                                 Select RowObject.TryParse(line)).ToArray
                    Dim values As T() =
                        LinqAPI.Exec(Of T) <= From row As RowObject
                                              In LQuery.AsParallel
                                              Let obj As T = Activator.CreateInstance(Of T)
                                              Select RowBuilder.FillData(row, obj)

                    Call "Start processing block...".__DEBUG_ECHO
                    Call Time(AddressOf New __taskHelper(Of T)(values, invoke).RunTask)
                    Call Console.Write(".")
                Next

                If EndRead Then

                    Exit Do
                Else
                    Call Console.WriteLine("Process next block....")
                End If
            Loop
        End Sub

        ''' <summary>
        ''' 为了减少Lambda表达式所带来的性能损失而构建的一个任务运行帮助对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        Private Structure __taskHelper(Of T)

            ''' <summary>
            ''' 赋值任务和数据源
            ''' </summary>
            ''' <param name="source"></param>
            ''' <param name="invoke"></param>
            Sub New(source As T(), invoke As Action(Of T()))
                Me.__source = source
                Me.__task = invoke
            End Sub

            Dim __task As Action(Of T())
            Dim __source As T()

            ''' <summary>
            ''' 运行当前的这个任务
            ''' </summary>
            Public Sub RunTask()
                Call __task(__source)
            End Sub
        End Structure

        ''' <summary>
        ''' Csv to LINQ
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Iterator Function AsLinq(Of T As Class)() As IEnumerable(Of T)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While Not EndRead
                Dim LQuery As IEnumerable(Of T) = From line As String
                                                  In BufferProvider()
                                                  Let row As RowObject = RowObject.TryParse(line)
                                                  Let obj As T = Activator.CreateInstance(Of T)
                                                  Select RowBuilder.FillData(row, obj)
                For Each x As T In LQuery
                    Yield x
                Next
            Loop

            Call Reset()
        End Function

        ''' <summary>
        ''' Open the data frame reader for the specific csv document.
        ''' </summary>
        ''' <param name="file">*.csv data file.</param>
        ''' <param name="encoding">The text encoding. default is using <see cref="Encodings.Default"/></param>
        ''' <returns></returns>
        Public Shared Function OpenHandle(
                               file As String,
                      Optional encoding As Encoding = Nothing,
                      Optional bufSize As Integer = 64 * 1024 * 1024) As DataStream

            Return New DataStream(file, encoding, bufSize)
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Overloads Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    __innerBuffer = Nothing
                    __innerStream = Nothing

                    Call FlushMemory()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace


Public Class BlockTest
    Public Property a As Double

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Module Program

    Sub Main()

        Using writer As New WriteStream(Of BlockTest)("x:\bbbb.csv")
            Call DocumentStream.Linq.DataStream.OpenHandle(
                "F:\VisualBasic_AppFramework\Example\block_test.csv",
                bufSize:=128).ForEachBlock(Of BlockTest)(
                Sub(array2)
                    Call writer.Flush(array2)
                End Sub, 10)
        End Using



        Dim a As TestBin = TestBin.inst  ' Init test data

        Call a.Serialize("./test.dat")   ' test on the binary serialization  
        a = Nothing
        a = "./test.dat".Load(Of TestBin)

        Dim json As String = a.GetJson   ' JSON serialization test
        a = Nothing
        a = json.LoadObject(Of TestBin)
        Call json.__DEBUG_ECHO

        Call New Profiles With {.Test = a}.WriteProfile  ' Write profile file data
        Call a.WriteClass("./test2.ini")                 ' Write ini section data.
        a = Nothing
        a = "./test2.ini".LoadIni(Of TestBin)                        ' Load ini section data
        Dim pp As Profiles = "./test2.ini".LoadProfile(Of Profiles)  ' Load entire ini file
        Call a.GetJson.__DEBUG_ECHO
        Call pp.GetJson.__DEBUG_ECHO

        ' XML test
        Dim xml As String = a.GetXml   ' Convert object into Xml
        Call xml.__DEBUG_ECHO
        Call a.SaveAsXml("./testssss.Xml")   ' Save Object to Xml
        a = Nothing
        a = "./testssss.Xml".LoadXml(Of TestBin)  ' Load Object from Xml
        Call a.GetXml.__DEBUG_ECHO

        Dim array As TestBin() = {a, a, a, a, a, a, a, a, a, a}   ' We have a collection of object
        Call array.SaveTo("./test.Csv")    ' then wen can save this collection into Csv file 
        array = Nothing
        array = "./test.Csv".LoadCsv(Of TestBin)  ' test on load csv data
        Call array.GetJson.__DEBUG_ECHO

        Dim s As String = array.GetJson
        Call s.SaveTo("./tesssss.txt")

        Dim lines As String() = "./tesssss.txt".ReadAllLines()
        s = "./tesssss.txt".ReadAllText

        Pause()
    End Sub
End Module

<IniMapIO("#/test.ini")>
Public Class Profiles
    Public Property Test As TestBin
End Class

<ClassName("JSON")>
<Serializable> Public Class TestBin
    <DataFrameColumn> Public Property Property1 As String
    <DataFrameColumn> Public Property D As Date
    <DataFrameColumn> Public Property n As Integer
    <DataFrameColumn> Public Property f As Double

    Public Shared Function inst() As TestBin
        Return New TestBin With {
            .D = Now,
            .f = RandomDouble(),
            .n = RandomDouble() * 1000,
            .Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
        }
    End Function
End Class
