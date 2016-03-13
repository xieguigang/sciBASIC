Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DocumentStream.Linq

    Public Delegate Function GetOrdinal(Column As String) As Integer

    Public Class DataStream : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile
        Implements ISchema
        Implements System.IDisposable

        ReadOnly _schema As Dictionary(Of String, Integer)
        ReadOnly _title As RowObject

        Dim p As Integer

        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal
            Get
                Return _schema
            End Get
        End Property

        Sub New()
            _schema = New Dictionary(Of String, Integer)
            _title = New RowObject
        End Sub

        Sub New(file As String, Optional encoding As System.Text.Encoding = Nothing)
            Dim lines As String() = IO.File.ReadAllLines(file, getEncoding(encoding))
            _title = RowObject.TryParse(lines(Scan0))
            _schema = _title.ToArray(
                Function(colName, idx) New With {
                    .colName = colName,
                    .ordinal = idx}) _
                    .ToDictionary(Function(x) x.colName.ToLower,
                                  Function(x) x.ordinal)
            Me.FilePath = file
            Me._innerBuffer = lines.Skip(1).ToArray

            Call $"{file.ToFileURL} handle opened...".__DEBUG_ECHO
        End Sub

        Public Function GetOrdinal(Name As String) As Integer Implements ISchema.GetOrdinal
            If _schema.ContainsKey(Name.ToLower.ShadowCopy(Name)) Then
                Return _schema(Name)
            Else
                Return -1
            End If
        End Function

        Public Sub ForEach(Of T As Class)(invoke As Action(Of T))
            Dim line As String = ""
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While Not _innerBuffer.Read(p, out:=line) Is Nothing
                Dim row As RowObject = RowObject.TryParse(line)
                Dim obj As T = Activator.CreateInstance(Of T)
                obj = RowBuilder.FillData(Of T)(row, obj)
                Call invoke(obj)
            Loop
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="invoke"></param>
        ''' <param name="blockSize"></param>
        Public Sub ForEachBlock(Of T As Class)(invoke As Action(Of T()), Optional blockSize As Integer = 10240 * 5)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)
            Dim chunks As String()() = _innerBuffer.Split(blockSize)

            Call RowBuilder.Indexof(Me)
            Call $"{chunks.Length} data partitions, {NameOf(blockSize)}:={blockSize}..".__DEBUG_ECHO

            Dim i As Integer = 0

            For Each block As String() In chunks
                Dim LQuery = (From line As String In block.AsParallel Select RowObject.TryParse(line)).ToArray
                Dim values = (From row As RowObject In LQuery.AsParallel
                              Let obj As T = Activator.CreateInstance(Of T)
                              Select RowBuilder.FillData(row, obj)).ToArray
                Call invoke(values)
                Call $"{100 * i / chunks.Length}% ({i}/{chunks.Length})...".__DEBUG_ECHO
                Call i.MoveNext
            Next
        End Sub

        ''' <summary>
        ''' Csv to LINQ
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Function AsLinq(Of T As Class)() As IEnumerable(Of T)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Dim LQuery As IEnumerable(Of T) = From line As String In _innerBuffer
                                              Let row As RowObject = RowObject.TryParse(line)
                                              Let obj As T = Activator.CreateInstance(Of T)
                                              Select RowBuilder.FillData(row, obj)
            Return LQuery
        End Function

        Public Shared Function OpenHandle(file As String, Optional encoding As System.Text.Encoding = Nothing) As DataStream
            Return New DataStream(file, encoding)
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException()
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _innerBuffer = Nothing
                Call FlushMemory()
            End If
        End Sub
    End Class
End Namespace