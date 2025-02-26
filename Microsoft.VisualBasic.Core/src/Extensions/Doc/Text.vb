#Region "Microsoft.VisualBasic::3f0a8f5b0f459050853e0380f80240bc, Microsoft.VisualBasic.Core\src\Extensions\Doc\Text.vb"

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

    '   Total Lines: 552
    '    Code Lines: 327 (59.24%)
    ' Comment Lines: 163 (29.53%)
    '    - Xml Docs: 91.41%
    ' 
    '   Blank Lines: 62 (11.23%)
    '     File Size: 21.09 KB


    ' Module TextDoc
    ' 
    '     Function: ForEachChar, IsTextFile, (+2 Overloads) IterateAllLines, LineIterators, LoadTextDoc
    '               OpenWriter, (+2 Overloads) ReadAllLines, ReadAllText, ReadFirstLine, SaveHTML
    '               SaveJson, (+4 Overloads) SaveTo, SaveTSV, SaveWithHTMLEncoding, SolveStream
    '               TsvHeaders
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports fs = Microsoft.VisualBasic.FileIO.FileSystem

''' <summary>
''' Extension helper function for the text documents
''' </summary>
Public Module TextDoc

    ''' <summary>
    ''' 默认是加载Xml文件的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="file"></param>
    ''' <param name="encoding"></param>
    ''' <param name="parser">default is Xml parser</param>
    ''' <param name="ThrowEx"></param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadTextDoc(Of T As IFileReference)(file$,
                                                        Optional encoding As Encoding = Nothing,
                                                        Optional parser As Func(Of String, Encoding, T) = Nothing,
                                                        Optional ThrowEx As Boolean = True) As T
        If parser Is Nothing Then
            parser = AddressOf LoadXml
        End If

        Dim FileObj As T

        Try
            FileObj = parser(file, encoding)
            FileObj.FilePath = file
        Catch ex As Exception
            Call App.LogException(New Exception(file.ToFileURL, ex))

            If ThrowEx Then
                Throw
            Else
#If DEBUG Then
                Call ex.PrintException
#End If
                Return Nothing
            End If
        End Try

        Return FileObj
    End Function

    ''' <summary>
    ''' 这个函数会自动判断所给定的数据是一个文件路径或者文本数据
    ''' 如果是文件路径则会返回该文本文件之中的所有的行数据
    ''' 反之将目标数据当作为文本返回所有文本行
    ''' </summary>
    ''' <param name="handle">
    ''' + 当这个参数为文件路径的时候会返回<see cref="Linq.IteratesALL(Of T)(IEnumerable(Of IEnumerable(Of T)))"/>函数的结果
    ''' + 当这个参数只是为文本字符串的时候，则会返回<see cref="LineTokens"/>函数的结果
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function LineIterators(handle$,
                                  Optional encoding As Encodings = Encodings.Default,
                                  Optional strictFile As Boolean = False) As IEnumerable(Of String)
        If handle.FileExists Then
            Return handle.IterateAllLines(encoding)
        ElseIf strictFile Then
            Throw New FileNotFoundException($"missing target file at location: {handle.GetFullPath}")
        Else
            Return handle.LineTokens
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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SaveJson(Of T)(obj As T, path$,
                                   Optional encoding As Encoding = Nothing,
                                   Optional indent As Boolean = False) As Boolean

        Return obj.GetJson(indent:=indent).SaveTo(path, encoding)
    End Function

    ''' <summary>
    ''' Enumerate all of the chars in the target text file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ForEachChar(path$, Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of Char)
        Using file As New FileStream(path, FileMode.Open)
            Using reader As New BinaryReader(file, encoding.CodePage)
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
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function OpenWriter(path$,
                               Optional encoding As Encodings = Encodings.UTF8,
                               Optional newLine As String = vbLf,
                               Optional append As Boolean = False,
                               Optional bufferSize As Integer = -1) As StreamWriter

        Return FileIO.OpenWriter(path, encoding.CodePage, newLine, append, bufferSize:=bufferSize)
    End Function

    ''' <summary>
    ''' Reading a super large size text file through stream method.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns>不存在的文件会返回空集合</returns>
    ''' <remarks>
    ''' (通过具有缓存的流对象读取文本数据，使用迭代器来读取文件之中的所有的行，大文件推荐使用这个方法进行读取操作)
    ''' </remarks>
    <Extension>
    Public Function IterateAllLines(path$,
                                    Optional encoding As Encodings = Encodings.Default,
                                    Optional verbose As Boolean = True,
                                    Optional unsafe As Boolean = False) As IEnumerable(Of String)

        Return path.IterateAllLines(encoding.CodePage, verbose:=verbose, unsafe:=unsafe)
    End Function

    ''' <summary>
    ''' Reading a super large size text file through stream method.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns>不存在的文件会返回空集合</returns>
    ''' <remarks>
    ''' (通过具有缓存的流对象读取文本数据，使用迭代器来读取文件之中的所有的行，大文件推荐使用这个方法进行读取操作)
    ''' </remarks>
    <Extension>
    Public Iterator Function IterateAllLines(path$, encoding As Encoding,
                                             Optional verbose As Boolean = True,
                                             Optional unsafe As Boolean = False) As IEnumerable(Of String)
        If path.IsURLPattern Then
            ' get request a html page
            For Each line As String In path.GET.LineTokens
                Yield line
            Next
        ElseIf Not path.FileExists Then
            Dim display_str As String = path

            If path.Contains(ASCII.CR) OrElse path.Contains(ASCII.LF) Then
                If unsafe Then
                    Throw New InvalidProgramException($"in-valid! ({path})")
                Else
                    If verbose Then
                        Call $"the given path is a text paragraph? ({path})".Warning
                    End If

                    ' returns an empty string collection
                    Return
                End If
            ElseIf path.Length > 60 Then
                display_str = path.Substring(0, 63) & "..."
            End If

            If verbose Then
                Call $"the given path ({display_str}) is not exists on your file system!".Warning
            End If

            ' returns an empty string collection
            Return
        End If

        ' path.Open is affects by the memory configuration
        Using fs As Stream = path.Open(FileMode.Open, doClear:=False, [readOnly]:=True, verbose:=verbose)
            Using reader As New StreamReader(fs, encoding Or DefaultEncoding)
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
    ''' <param name="encoding">
    ''' Parameter value is set to <see cref="DefaultEncoding"/> if this parameter is not specific.
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function ReadFirstLine(path$, Optional encoding As Encoding = Nothing) As String
        If path.FileLength <= 0 Then
            Return ""
        End If

        Using file As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using reader As New StreamReader(file, encoding Or DefaultEncoding)
                Dim first$ = reader.ReadLine
                Return first
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 自动进行判断解决所读取的数据源，当<paramref name="handle"/>为文件路径的时候，
    ''' 会读取文件内容，反之则会直接返回<paramref name="handle"/>的内容
    ''' </summary>
    ''' <param name="handle">The text content or file path string.(文本内容或者文件路径)</param>
    ''' <returns>Always returns a text content.</returns>
    ''' <remarks>
    ''' 不适用于大文本数据
    ''' </remarks>
    <Extension>
    Public Function SolveStream(handle$, Optional encoding As Encodings = Encodings.UTF8, Optional null$ = "null") As String
        If handle Is Nothing Then
            Return null
        ElseIf handle.IndexOf(ASCII.CR) > -1 OrElse handle.IndexOf(ASCII.LF) > -1 Then
            ' is text content, not path
            Return handle
        ElseIf handle.IsURLPattern Then
            ' http get text
            Return handle.GET
        ElseIf ILLEGAL_PATH_CHARACTERS _
            .Any(Function(i)
                     ' handle可能是绝对路径，在windows之中，绝对路径会含有盘符
                     ' 例如E:\，冒号会导致这里的判断出现BUG
                     ' 所以需要添加一个额外的判断条件
                     Return i <> ":"c AndAlso handle.IndexOf(i) > -1
                 End Function) Then

            ' is text content, not path
            Return handle
        ElseIf handle.Count(":"c) > 1 Then
            ' json?
            Return handle
        End If

        If handle.FileExists(True) Then
            Return handle.ReadAllText(encoding.CodePage)
        Else
            Return handle
        End If
    End Function

    ''' <summary>
    ''' This function just suite for read a small text file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding">Default value is <see cref="Encoding.UTF8"/></param>
    ''' <param name="suppress">Suppress error message??</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' (这个函数只建议读取小文本文件的时候使用)
    ''' </remarks>
    '''
    <ExportAPI("Read.TXT")>
    <Extension>
    <DebuggerStepThrough>
    Public Function ReadAllText(path$,
                                Optional encoding As Encoding = Nothing,
                                Optional throwEx As Boolean = True,
                                Optional suppress As Boolean = False) As String
        Try
            If Not path.FileExists Then
                If Not throwEx Then
                    Return ""
                End If
            End If

            Using s As Stream = path.OpenReadonly
                Return New StreamReader(s, encoding:=encoding Or UTF8).ReadToEnd
            End Using
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

    <Extension>
    Public Iterator Function ReadAllLines(s As Stream, Optional encoding As Encoding = Nothing) As IEnumerable(Of String)
        Using reader As New StreamReader(s, encoding Or UTF8)
            Do While Not reader.EndOfStream
                Yield reader.ReadLine
            Loop
        End Using
    End Function

    ''' <summary>
    ''' This function is recommend using for the small(probably smaller than 300MB) text file reading.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding">Default value is UTF8</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' (这个函数只建议读取小文本文件的时候使用)
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Read.Lines")>
    <Extension>
    Public Function ReadAllLines(path$,
                                 Optional encoding As Encoding = Nothing,
                                 Optional verbose As Boolean = True) As String()

        If (Not path.StringEmpty) AndAlso path.FileExists Then
            Return path _
                .IterateAllLines(encoding:=encoding, verbose:=verbose) _
                .ToArray
        Else
            If path.StringEmpty Then
                Call "empty file path!".Warning
            Else
                Try
                    Call $"missing text file: {path.GetFullPath}!".Warning
                Catch ex As Exception

                End Try
            End If

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
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function SaveWithHTMLEncoding(html$, path$) As Boolean
        Return html.SaveTo(path, Encoding.UTF8)
    End Function

    ''' <summary>
    ''' Write the text file data into a file which was specific by the <paramref name="path"></paramref> value,
    ''' this function not append the new data onto the target file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="text"></param>
    ''' <param name="encoding">这个函数会自动处理文本的编码的</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' default text encoding: utf-8(将目标文本字符串写入到一个指定路径的文件之中，但是不会在文件末尾追加新的数据)
    ''' </remarks>
    '''
    <Extension>
    Public Function SaveTo(<Parameter("Text")> text As String,
                           <Parameter("Path")> path As String,
                           <Parameter("Text.Encoding")>
                           Optional encoding As Encoding = Nothing,
                           Optional append As Boolean = False,
                           Optional throwEx As Boolean = True) As Boolean

        If String.IsNullOrEmpty(path) Then
            Return False
        End If

        Dim dir As String = Nothing

        Try
#If UNIX Then
            dir = System.IO.Directory.GetParent(path).FullName
#Else
            path = PathExtensions.Long2Short(path)
            dir = fs.GetParentPath(path)
#End If
        Catch ex As Exception
            Dim msg As String = $" **** Directory string is illegal or string is too long:  [{NameOf(path)}:={path}] > 260"
            ex = New Exception(msg, ex)
            App.LogException(ex)

            If throwEx Then
                Throw ex
            End If
        End Try

        If String.IsNullOrEmpty(dir) Then
            dir = App.CurrentDirectory
        Else
            dir.MakeDir(throwEx:=False)
        End If

        Try
            Call fs.WriteAllText(path, text Or EmptyString, append, encoding Or UTF8)
        Catch ex As Exception
            ex = New Exception("[DIR]  " & dir, ex)
            ex = New Exception("[Path]  " & path, ex)

            Call App.LogException(ex)

            If throwEx Then
                Throw ex
            Else
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
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SaveTo(value As XElement, path$, Optional encoding As Encoding = Nothing) As Boolean
        Return value.Value.SaveTo(path, encoding)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SaveHTML(html As XElement, path$, Optional encoding As Encodings = Encodings.UTF8WithoutBOM) As Boolean
        Return html.ToString.SaveTo(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' Determined that the target file is a text file or binary file?
    ''' (判断是否是文本文件)
    ''' </summary>
    ''' <param name="path">文件全路径名称</param>
    ''' <returns>是返回True，不是返回False</returns>
    ''' <param name="chunkSize">文件检查的长度，假若在这个长度内都没有超过null的阈值数，则认为该文件为文本文件，默认区域长度为4KB</param>
    ''' <remarks>2012年12月5日</remarks>
    '''
    <ExportAPI("IsTextFile")>
    <Extension>
    Public Function IsTextFile(path$, Optional chunkSize% = 4 * 1024) As Boolean
        Using file As New FileStream(path, FileMode.Open, FileAccess.Read)
            Dim byteData(1) As Byte
            Dim i%
            Dim p%

            While file.Read(byteData, 0, byteData.Length) > 0
                If byteData(0) = 0 Then i += 1

                If p <= chunkSize Then
                    p += 1
                Else
                    Exit While
                End If
            End While

            Return i <= 0.1 * p
        End Using
    End Function

    ''' <summary>
    ''' Save a collection of the text data in line by line
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="path">
    ''' 将目标字符串集合数据全部写入到文件之中，当所写入的文件位置之上没有父文件夹存在的时候，会自动创建文件夹
    ''' </param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这个函数是以For循环的方式来兼容大型数据集的文件写入操作的
    ''' </remarks>
    <Extension>
    Public Function SaveTo(array As IEnumerable(Of String), path$,
                           Optional encoding As Encoding = Nothing,
                           Optional newLine As Char = ASCII.LF) As Boolean

        If String.IsNullOrEmpty(path) Then
            Return False
        End If

        Call "".SaveTo(path)

        Using fs As New FileStream(path, FileMode.OpenOrCreate),
            file As New StreamWriter(fs, encoding Or DefaultEncoding) With {
                .NewLine = newLine
            }

            For Each line$ In array.SafeQuery
                Call file.WriteLine(line)
            Next
        End Using

        Return True
    End Function

    ''' <summary>
    ''' Save the text content in the <see cref="StringBuilder"/> object into a text file.
    ''' </summary>
    ''' <param name="sb"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SaveTo(sb As StringBuilder, path$, Optional encoding As Encoding = Nothing) As Boolean
        Return sb.ToString.SaveTo(path, encoding)
    End Function
End Module
