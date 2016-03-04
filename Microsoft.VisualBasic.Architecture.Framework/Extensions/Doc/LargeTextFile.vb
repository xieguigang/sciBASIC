Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports System.Runtime.CompilerServices

''' <summary>
''' Wrapper for the file operations.
''' </summary>
''' <remarks></remarks>
<[Namespace]("Large_Text_File")>
Public Module LargeTextFile

    <ExportAPI("Partitioning")>
    Public Function TextPartition(data As Generic.IEnumerable(Of String)) As String()()
        Dim maxSize As Double = New StringBuilder(1024 * 1024).MaxCapacity
        Return __textPartitioning(data.ToArray, maxSize)
    End Function

    Private Function __textPartitioning(dat As String(), maxSize As Double) As String()()
        Dim currentSize As Double = (From s As String In dat.AsParallel Select CDbl(Len(s))).ToArray.Sum
        If currentSize > maxSize Then
            Dim SplitTokens = dat.Split(CInt(dat.Length / 2))
            If SplitTokens.Length > 1 Then
                Return (From n In SplitTokens Select __textPartitioning(n, maxSize)).ToArray.MatrixToVector
            Else
                Return SplitTokens
            End If
        Else
            Return New String()() {dat}
        End If
    End Function

    ''' <summary>
    ''' 当一个文件非常大以致无法使用任何现有的文本编辑器查看的时候，可以使用本方法查看其中的一部分数据 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Peeks")>
    Public Function Peeks(path As String, length As Integer) As String
        Dim ChunkBuffer As Char() = New Char(length - 1) {}
        Using Reader = FileIO.FileSystem.OpenTextFileReader(path)
            Call Reader.ReadBlock(ChunkBuffer, 0, ChunkBuffer.Length)
        End Using
        Return New String(value:=ChunkBuffer)
    End Function

    ''' <summary>
    ''' 尝试查看大文件的尾部的数据
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="length">字符的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Tails")>
    Public Function Tails(path As String,
                          <Parameter("characters", "The number of the characters, not the bytes value.")>
                          length As Integer) As String
        length *= 8

        Using Reader = New IO.FileStream(path, IO.FileMode.OpenOrCreate)
            If Reader.Length < length Then
                length = Reader.Length
            End If

            Dim ChunkBuffer As Byte() = New Byte(length - 1) {}

            Call Reader.Seek(Reader.Length - length, IO.SeekOrigin.Begin)
            Call Reader.Read(ChunkBuffer, 0, ChunkBuffer.Length)

            Dim value As String = System.Text.Encoding.Default.GetString(ChunkBuffer)
            Return value
        End Using
    End Function

    <ExportAPI(".Merge", Info:="Please make sure all of the file in the target directory is text file not binary file.")>
    Public Function Merge(<Parameter("Dir", "The default directory parameter value is the current directory.")> Optional dir As String = "./") As String
        Dim Texts = From file As String
                    In FileIO.FileSystem.GetFiles(dir, FileIO.SearchOption.SearchAllSubDirectories, "*.*")
                    Select FileIO.FileSystem.ReadAllText(file)
        Dim Merged As String = String.Join(vbCr, Texts)
        Return Merged
    End Function
End Module
