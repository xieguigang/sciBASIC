Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("IO")>
Public Module IOExtensions

    <Extension>
    Public Function ReadVector(path As String) As Double()
        Return IO.File.ReadAllLines(path).ToArray(Function(x) CDbl(x))
    End Function

    <ExportAPI("Open.File")>
    <Extension>
    Public Function Open(path As String, Optional mode As FileMode = FileMode.OpenOrCreate) As FileStream
        Return IO.File.Open(path, mode)
    End Function

    <ExportAPI("Open.Reader")>
    <Extension>
    Public Function OpenReader(path As String, Optional encoding As Encoding = Nothing) As StreamReader
        encoding = If(encoding Is Nothing, System.Text.Encoding.Default, encoding)
        Return New StreamReader(IO.File.Open(path, FileMode.OpenOrCreate), encoding)
    End Function

    <Extension>
    Public Function ReadBinary(path As String) As Byte()
        If Not path.FileExists Then
            Return {}
        End If
        Return IO.File.ReadAllBytes(path)
    End Function

    <Extension> Public Function FlushAllLines(Of T)(data As IEnumerable(Of T),
                                                    SaveTo As String,
                                                    Optional encoding As Encoding = Nothing) As Boolean
        Dim strings As String() = data.ToArray(Function(obj) Scripting.ToString(obj))

        Try
            Dim parent As String = FileIO.FileSystem.GetParentPath(SaveTo)
            Call FileIO.FileSystem.CreateDirectory(parent)
            Call IO.File.WriteAllLines(SaveTo, strings, If(encoding Is Nothing, System.Text.Encoding.Default, encoding))
        Catch ex As Exception
            Call App.LogException(New Exception(SaveTo, ex))
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Save the binary data into the filesystem.(保存二进制数据包值文件系统)
    ''' </summary>
    ''' <param name="buf">The binary bytes data of the target package's data.(目标二进制数据)</param>
    ''' <param name="path">The saved file path of the target binary data chunk.(目标二进制数据包所要进行保存的文件名路径)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("FlushStream")>
    <Extension> Public Function FlushStream(buf As IEnumerable(Of Byte), <Parameter("Path.Save")> path As String) As Boolean
        Dim parentDIR As String = If(String.IsNullOrEmpty(path),
            FileIO.FileSystem.CurrentDirectory,
            FileIO.FileSystem.GetParentPath(path))

        Call FileIO.FileSystem.CreateDirectory(parentDIR)
        Call FileIO.FileSystem.WriteAllBytes(path, buf.ToArray, False)

        Return True
    End Function

    <ExportAPI("FlushStream")>
    <Extension> Public Function FlushStream(stream As Net.Protocols.ISerializable, SavePath As String) As Boolean
        Dim rawStream As Byte() = stream.Serialize
        If rawStream Is Nothing Then
            rawStream = New Byte() {}
        End If
        Return rawStream.FlushStream(SavePath)
    End Function
End Module
