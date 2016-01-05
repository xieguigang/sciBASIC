Imports System.Text

''' <summary>
''' Database file in text file format.
''' (文本文件形式的数据库文件)
''' </summary>
''' <remarks></remarks>
<LINQ.Framework.Reflection.LINQEntity("string")>
Public Class File : Implements IDisposable
    Implements LINQ.Framework.ILINQCollection

    Friend _FileSize As Long = -1, _FilePath As String

    ''' <summary>
    ''' The source file
    ''' (源文件)
    ''' </summary>
    ''' <remarks></remarks>
    Public Data As String()

    Public ReadOnly Property FileSize As Long
        Get
            Return _FileSize
        End Get
    End Property

    Public Sub CopyTo(Of T As File)(ByRef e As T)
        e.Data = Me.Data
        e._FileSize = Me._FileSize
    End Sub

    Public Overrides Function ToString() As String
        Return _FilePath
    End Function

    Public Shared Narrowing Operator CType(e As File) As String
        Dim sBuilder As StringBuilder = New StringBuilder(2048)

        For Each Line As String In e.Data
            sBuilder.AppendLine(Line)
        Next

        Return sBuilder.ToString
    End Operator

    Public Shared Widening Operator CType(e As File) As String()
        Return e.Data
    End Operator

    Overridable Sub Save(Path As String)
        Call FileIO.FileSystem.WriteAllText(Path, CType(Me, String), append:=False)
    End Sub

    ''' <summary>
    ''' Read the data in the target database file.
    ''' (从目标文件中读取数据)
    ''' </summary>
    ''' <param name="Path">The file path of the target database file.(目标数据库文件的文件路径)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(Path As String) As File
        Return New File With {.Data = System.IO.File.ReadAllLines(Path), ._FileSize = FileIO.FileSystem.GetFileInfo(Path).Length}
    End Operator

    ''' <summary>
    ''' Contacts the multiple line text into one line string.
    ''' (将多行的文本连接为单行的字符串)
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Contact(Data As Generic.IEnumerable(Of String)) As String
        Dim sBuilder As StringBuilder = New StringBuilder(2048)
        For Each s As String In Data
            sBuilder.Append(s)
        Next

        Return sBuilder.ToString
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Public Overridable Function GetCollection(Path As String) As Object() Implements LINQ.Framework.ILINQCollection.GetCollection
        Return CType(Path, File).Data
    End Function

    Public Overridable Function GetEntityType() As Type Implements LINQ.Framework.ILINQCollection.GetEntityType
        Return GetType(System.String)
    End Function
End Class
