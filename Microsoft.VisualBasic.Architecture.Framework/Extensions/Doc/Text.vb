Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("Doc.TextFile", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Public Module TextDoc

    ''' <summary>
    ''' Read the first line of the text in the file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension> Public Function ReadFirstLine(path As String) As String
        Using reader As StreamReader = New StreamReader(New FileStream(path, FileMode.Open))
            Dim first As String = reader.ReadLine
            Return first
        End Using
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="FilePath"></param>
    ''' <param name="Encoding">Default value is UTF8</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Read.TXT")>
    <Extension>
    Public Function ReadAllText(FilePath As String, Optional Encoding As Encoding = Nothing) As String
        If Encoding Is Nothing Then
            Encoding = System.Text.Encoding.UTF8
        End If
        Return FileIO.FileSystem.ReadAllText(FilePath, encoding:=Encoding)
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="FilePath"></param>
    ''' <param name="Encoding">Default value is UTF8</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Read.Lines")>
    <Extension>
    Public Function ReadAllLines(FilePath As String, Optional Encoding As Encoding = Nothing) As String()
        If Encoding Is Nothing Then
            Encoding = System.Text.Encoding.UTF8
        End If
        If FilePath.FileExists Then
            Return IO.File.ReadAllLines(FilePath, encoding:=Encoding)
        Else
            Return New String() {}
        End If
    End Function

    ''' <summary>
    ''' Write the text file data into a file which was specific by the <paramref name="Path"></paramref> value,
    ''' this function not append the new data onto the target file.
    ''' (将目标文本字符串写入到一个指定路径的文件之中，但是不会在文件末尾追加新的数据)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <param name="TextValue"></param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(<Parameter("Text")> TextValue As String, Path As String, Optional Encoding As Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(Path) Then Return False
        If Encoding Is Nothing Then Encoding = System.Text.Encoding.Default
        Dim Dir As String
        Try
            Path = ProgramPathSearchTool.Long2Short(Path)
            Dir = FileIO.FileSystem.GetParentPath(Path)
        Catch ex As Exception
            Dim MSG As String = $" **** Directory string is illegal or string is too long:  [{NameOf(Path)}:={Path}] > 260".__DEBUG_ECHO
            Throw New Exception(MSG, ex)
        End Try

        If String.IsNullOrEmpty(Dir) Then
            Dir = FileIO.FileSystem.CurrentDirectory
        End If

        Try
            Call FileIO.FileSystem.CreateDirectory(Dir)
            Call FileIO.FileSystem.WriteAllText(Path, TextValue, append:=False, encoding:=Encoding)
        Catch ex As Exception
            ex = New Exception("[DIR]  " & Dir, ex)
            ex = New Exception("[Path]  " & Path, ex)
            Throw ex
        End Try

        Return True
    End Function

    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(value As XElement, path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return value.Value.SaveTo(path, encoding)
    End Function

    ''' <summary>
    ''' 由于可能会产生数据污染，所以并不推荐使用这个函数来写文件
    ''' </summary>
    ''' <param name="TextValue"></param>
    ''' <param name="Path"></param>
    ''' <param name="WaitForRelease">当其他的进程对目标文件产生占用的时候，函数是否等待其他进程的退出释放文件句柄之后在进行数据的写入</param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(TextValue As String, Path As String, WaitForRelease As Boolean, Optional Encoding As Encoding = Nothing) As Boolean
        If Path.FileOpened Then
            If WaitForRelease Then
                '假若文件被占用，则等待句柄的释放
                Do While Path.FileOpened
                    Call Threading.Thread.Sleep(10)
                Loop
            Else  '假若不等待句柄的释放的话，则直接返回失败
                Return False
            End If
        End If

        Return TextValue.SaveTo(Path, Encoding)
    End Function

    ''' <summary>
    ''' 判断是否是文本文件
    ''' </summary>
    ''' <param name="FilePath">文件全路径名称</param>
    ''' <returns>是返回True，不是返回False</returns>
    ''' <param name="ChunkLength">文件检查的长度，假若在这个长度内都没有超过null的阈值数，则认为该文件为文本文件，默认区域长度为4KB</param>
    ''' <remarks>2012年12月5日</remarks>
    '''
    <ExportAPI("IsTextFile")>
    <Extension> Public Function IsTextFile(FilePath As String, Optional ChunkLength As Integer = 4 * 1024) As Boolean
        Dim file As IO.FileStream = New System.IO.FileStream(FilePath, IO.FileMode.Open, IO.FileAccess.Read)
        Dim byteData(1) As Byte
        Dim i As Integer
        Dim p As Integer

        While file.Read(byteData, 0, byteData.Length) > 0
            If byteData(0) = 0 Then i += 1
            If p <= ChunkLength Then p += 1 Else Exit While
        End While

        Return i <= 0.1 * p
    End Function

    ''' <summary>
    ''' 将目标字符串数据全部写入到文件之中，当所写入的文件位置之上没有父文件夹存在的时候，会自动创建文件夹
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(array As IEnumerable(Of String), path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(path) Then Return False
        If encoding Is Nothing Then encoding = System.Text.Encoding.Default
        Dim Dir = FileIO.FileSystem.GetParentPath(path)
        Call FileIO.FileSystem.CreateDirectory(Dir)
        Call IO.File.WriteAllLines(path, array, encoding)
        Return True
    End Function

    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(sBuilder As StringBuilder, path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return sBuilder.ToString.SaveTo(path, encoding)
    End Function
End Module
