Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("IO")>
Public Module IOExtensions

    <ExportAPI("Open.File")>
    <Extension>
    Public Function Open(path As String, Optional mode As FileMode = FileMode.OpenOrCreate) As FileStream
        Return File.Open(path, mode)
    End Function

    <ExportAPI("Open.Reader")>
    <Extension>
    Public Function OpenReader(path As String, Optional encoding As Encoding = Nothing) As StreamReader
        encoding = If(encoding Is Nothing, System.Text.Encoding.Default, encoding)
        Return New StreamReader(File.Open(path, FileMode.OpenOrCreate), encoding)
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
    ''' <param name="ChunkBuffer">The binary bytes data of the target package's data.(目标二进制数据)</param>
    ''' <param name="SavePath">The saved file path of the target binary data chunk.(目标二进制数据包所要进行保存的文件名路径)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("FlushStream")>
    <Extension> Public Function FlushStream(ChunkBuffer As IEnumerable(Of Byte), <Parameter("Path.Save")> SavePath As String) As Boolean
        Dim ParentDir As String = If(String.IsNullOrEmpty(SavePath),
            FileIO.FileSystem.CurrentDirectory,
            FileIO.FileSystem.GetParentPath(SavePath))

        Call FileIO.FileSystem.CreateDirectory(ParentDir)
        Call FileIO.FileSystem.WriteAllBytes(SavePath, ChunkBuffer.ToArray, False)

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
