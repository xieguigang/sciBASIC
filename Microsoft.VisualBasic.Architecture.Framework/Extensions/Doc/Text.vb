#Region "Microsoft.VisualBasic::16319b5533c3a6f92dc89991a5a33435, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\Text.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

<Package("Doc.TextFile", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Public Module TextDoc

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="handle$">
    ''' + 当这个参数为文件路径的时候会返回<see cref="Linq.IteratesALL(Of T)(IEnumerable(Of IEnumerable(Of T)))"/>函数的结果
    ''' + 当这个参数只是为文本字符串的时候，则会返回<see cref="lTokens"/>函数的结果
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function LineIterators(handle$) As IEnumerable(Of String)
        If handle.FileExists Then
            Return handle.IterateAllLines
        Else
            Return handle.lTokens
        End If
    End Function

    ''' <summary>
    ''' 解析出TSV文件的头部并且生成index数据
    ''' </summary>
    ''' <param name="path$">``*.tsv``文件路径</param>
    ''' <returns></returns>
    <Extension>
    Public Function TsvHeaders(path$) As Index(Of String)
        Dim header$() = path.ReadFirstLine.Split(ASCII.TAB)
        Dim index As New Index(Of String)(header)
        Return index
    End Function

    ''' <summary>
    ''' 将IDmapping数据保存为tsv文件
    ''' </summary>
    ''' <param name="tsv"></param>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveTSV(tsv As IEnumerable(Of IDMap), path$, Optional encoding As Encodings = Encodings.ASCII) As Boolean
        Dim lines = tsv.Select(Function(x) x.TSV)
        Return lines.SaveTo(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' Enumerate all of the chars in the target text file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ForEachChar(path As String, Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of Char)
        Using file As New FileStream(path, FileMode.Open)
            Using reader As New IO.BinaryReader(file, encoding.CodePage)
                Dim bs As Stream = reader.BaseStream
                Dim l As Long = bs.Length

                Do While bs.Position < l
                    Yield reader.ReadChar
                Loop
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Open text file writer, this function will auto handle all things.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function OpenWriter(path$,
                               Optional encoding As Encodings = Encodings.UTF8,
                               Optional newLine$ = ASCII.LF,
                               Optional append As Boolean = False) As StreamWriter
        Return FileIO.OpenWriter(path, encoding.CodePage, newLine, append)
    End Function

    ''' <summary>
    ''' Reading a super large size text file through stream method.
    ''' (通过具有缓存的流对象读取文本数据，使用迭代器来读取文件之中的所有的行，大文件推荐使用这个方法进行读取操作)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function IterateAllLines(path As String, Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of String)
        Using fs As New FileStream(path, FileMode.Open, access:=FileAccess.Read, share:=FileShare.Read)
            Using reader As New StreamReader(fs, encoding.CodePage)

                Do While Not reader.EndOfStream
                    Yield reader.ReadLine
                Loop
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Read the first line of the text in the file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension> Public Function ReadFirstLine(path$, Optional encoding As Encoding = Nothing) As String
        Using file As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using reader As New StreamReader(file, encoding Or DefaultEncoding)
                Dim first As String = reader.ReadLine
                Return first
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 自动进行判断解决所读取的数据源，当<paramref name="handle"/>为文件路径的时候，会读取文件内容，反之则会直接返回<paramref name="handle"/>的内容
    ''' </summary>
    ''' <param name="handle$">文本内容或者文件路径</param>
    ''' <returns></returns>
    <Extension> Public Function SolveStream(handle$) As String
        If handle.FileExists(True) Then
            Return handle.ReadAllText
        Else
            Return handle
        End If
    End Function

    ''' <summary>
    ''' This function just suite for read a small text file.(这个函数只建议读取小文本文件的时候使用)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding">Default value is <see cref="Encoding.UTF8"/></param>
    ''' <param name="suppress">Suppress error message??</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Read.TXT")>
    <Extension>
    Public Function ReadAllText(path$,
                                Optional encoding As Encoding = Nothing,
                                Optional throwEx As Boolean = True,
                                Optional suppress As Boolean = False) As String
        Try
            Return FileIO.FileSystem.ReadAllText(path, encoding:=encoding Or UTF8)
        Catch ex As Exception
            ex = New Exception(path.ToFileURL, ex)
            If throwEx Then
                Throw ex
            Else
                Call App.LogException(ex)

                If Not suppress Then
                    Call ex.PrintException
                End If
            End If
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' This function is recommend using for the small(probably smaller than 300MB) text file reading.
    ''' (这个函数只建议读取小文本文件的时候使用)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="Encoding">Default value is UTF8</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Read.Lines")>
    <Extension>
    Public Function ReadAllLines(path As String, Optional Encoding As Encoding = Nothing) As String()
        If path.FileExists Then
            Return IO.File.ReadAllLines(path, encoding:=Encoding Or DefaultEncoding)
        Else
            Return New String() {}
        End If
    End Function

    ''' <summary>
    ''' 使用html文本的默认编码格式<see cref="Encodings.UTF8"/>来保存这个文本文件
    ''' </summary>
    ''' <param name="html$"></param>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveWithHTMLEncoding(html$, path$) As Boolean
        Return html.SaveTo(path, Encoding.UTF8)
    End Function

    ''' <summary>
    ''' Write the text file data into a file which was specific by the <paramref name="path"></paramref> value,
    ''' this function not append the new data onto the target file.
    ''' (将目标文本字符串写入到一个指定路径的文件之中，但是不会在文件末尾追加新的数据)
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="text"></param>
    ''' <param name="encoding">这个函数会自动处理文本的编码的</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(<Parameter("Text")> text As String,
                                       <Parameter("Path")> path As String,
                                       <Parameter("Text.Encoding")> Optional encoding As Encoding = Nothing,
                                       Optional append As Boolean = False,
                                       Optional throwEx As Boolean = True) As Boolean

        If String.IsNullOrEmpty(path) Then
            Return False
        End If

        If text Is Nothing Then
            text = ""
        End If

        Dim DIR As String

        Try
            path = ProgramPathSearchTool.Long2Short(path)
            DIR = FileIO.FileSystem.GetParentPath(path)
        Catch ex As Exception
            Dim msg As String = $" **** Directory string is illegal or string is too long:  [{NameOf(path)}:={path}] > 260"
            Throw New Exception(msg, ex)
        End Try

        If String.IsNullOrEmpty(DIR) Then
            DIR = FileIO.FileSystem.CurrentDirectory
        End If

        Try
            Call FileIO.FileSystem.CreateDirectory(DIR)
            Call File.WriteAllText(path, text, encoding Or DefaultEncoding)
        Catch ex As Exception
            ex = New Exception("[DIR]  " & DIR, ex)
            ex = New Exception("[Path]  " & path, ex)

            If throwEx Then
                Throw ex
            Else
                Call App.LogException(ex)
                Return False
            End If
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Save the inner text value of a xml element
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(value As XElement, path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return value.Value.SaveTo(path, encoding)
    End Function

    ''' <summary>
    ''' Determined that the target file is a text file or binary file?
    ''' (判断是否是文本文件)
    ''' </summary>
    ''' <param name="FilePath">文件全路径名称</param>
    ''' <returns>是返回True，不是返回False</returns>
    ''' <param name="chunkSize">文件检查的长度，假若在这个长度内都没有超过null的阈值数，则认为该文件为文本文件，默认区域长度为4KB</param>
    ''' <remarks>2012年12月5日</remarks>
    '''
    <ExportAPI("IsTextFile")>
    <Extension> Public Function IsTextFile(FilePath As String, Optional chunkSize As Integer = 4 * 1024) As Boolean
        Dim file As IO.FileStream = New FileStream(FilePath, IO.FileMode.Open, IO.FileAccess.Read)
        Dim byteData(1) As Byte
        Dim i As Integer
        Dim p As Integer

        While file.Read(byteData, 0, byteData.Length) > 0
            If byteData(0) = 0 Then i += 1
            If p <= chunkSize Then p += 1 Else Exit While
        End While

        Return i <= 0.1 * p
    End Function

    ''' <summary>
    ''' 将目标字符串集合数据全部写入到文件之中，当所写入的文件位置之上没有父文件夹存在的时候，会自动创建文件夹
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(array As IEnumerable(Of String), path$, Optional encoding As Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(path) Then
            Return False
        End If

        Call "".SaveTo(path)

        Using file As New StreamWriter(New FileStream(path, FileMode.OpenOrCreate), encoding Or DefaultEncoding)
            For Each line As String In array.SafeQuery
                Call file.WriteLine(line)
            Next
        End Using

        Return True
    End Function

    ''' <summary>
    ''' Save the text content in the <see cref="StringBuilder"/> object into a text file.
    ''' </summary>
    ''' <param name="sBuilder"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(sBuilder As StringBuilder, path As String, Optional encoding As Encoding = Nothing) As Boolean
        Return sBuilder.ToString.SaveTo(path, encoding)
    End Function
End Module
