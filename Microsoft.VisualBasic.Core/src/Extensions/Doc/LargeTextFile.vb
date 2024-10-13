#Region "Microsoft.VisualBasic::442d9072bd109450a268fd058fd3f4d3, Microsoft.VisualBasic.Core\src\Extensions\Doc\LargeTextFile.vb"

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

    '   Total Lines: 202
    '    Code Lines: 97 (48.02%)
    ' Comment Lines: 79 (39.11%)
    '    - Xml Docs: 94.94%
    ' 
    '   Blank Lines: 26 (12.87%)
    '     File Size: 7.78 KB


    ' Module LargeTextFile
    ' 
    '     Function: FixEscapes, GetLastLine, IteratesStream, IteratesTableData, Merge
    '               Peeks, (+2 Overloads) Tails
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

''' <summary>
''' Wrapper for the file operations.
''' </summary>
''' <remarks></remarks>
Public Module LargeTextFile

    ''' <summary>
    ''' 函数返回结果文件的临时文件的文件路径
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="escape">
    ''' 这个函数输入文本文件之中的一行数据,然后处理完转义之后将改行的数据返回
    ''' </param>
    ''' <returns></returns>
    Public Function FixEscapes(path$, escape As Func(Of String, String), Optional encoding As Encodings = Encodings.UTF8WithoutBOM) As String
        Dim temp$ = TempFileSystem.GetAppSysTempFile(".txt", App.PID)

        Using output As StreamWriter = temp.OpenWriter(encoding)
            For Each line As String In path.IterateAllLines(encoding)
                Call output.WriteLine(escape(line))
            Next
        End Using

        Return temp
    End Function

    ''' <summary>
    ''' Iterates read all lines in a very large text file, 
    ''' using for loading a very large size csv/tsv file
    ''' </summary>
    ''' <param name="path">file path</param>
    ''' <param name="title">The header line of this large size csv/tsv file.</param>
    ''' <param name="skip">Skip n lines, then start to populate data lines.</param>
    ''' <param name="encoding">Text file encoding.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' A helper function for read the csv/tsv table file
    ''' </remarks>
    <Extension>
    Public Function IteratesTableData(path$, ByRef title$, Optional skip% = -1, Optional encoding As Encodings = Encodings.ASCII) As IEnumerable(Of String)
        Using reader As StreamReader = path.OpenReader(encoding.CodePage)
            Dim i% = skip

            ' skip lines
            Do While i > 0
                reader.ReadLine()
                i -= 1
            Loop

            title = reader.ReadLine

            Return reader.IteratesStream
        End Using
    End Function

    ''' <summary>
    ''' Populate all lines of the text data from current 
    ''' stream reader object
    ''' </summary>
    ''' <param name="s">The stream connection to the target text file data resource.</param>
    ''' <returns>
    ''' Try to pull all text lines from the given text stream data
    ''' </returns>
    <Extension>
    Public Iterator Function IteratesStream(s As StreamReader) As IEnumerable(Of String)
        Do While Not s.EndOfStream
            Yield s.ReadLine
        Loop
    End Function

    ''' <summary>
    ''' 当一个文件非常大以致无法使用任何现有的文本编辑器查看的时候，可以使用本方法查看其中的一部分数据 
    ''' </summary>
    ''' <param name="length">字节长度</param>
    ''' <returns></returns>
    ''' <remarks></remarks> 
    <ExportAPI("Peeks")>
    <Extension>
    Public Function Peeks(path As String, Optional length% = 5 * 1024) As String
        Dim buffer As Char() = New Char(length - 1) {}

        Using reader As StreamReader = FileIO.FileSystem.OpenTextFileReader(path)
            Call reader.ReadBlock(buffer, 0, buffer.Length)
        End Using

        Return New String(value:=buffer)
    End Function

    ''' <summary>
    ''' Peek the tails of a large text file.(尝试查看大文件的尾部的数据)
    ''' </summary>
    ''' <param name="path">If the file is not exists, then this function will returns nothing.</param>
    ''' <param name="length">
    ''' Peeks of the number of characters. The number of the characters, not the bytes value.
    ''' (字符的数目)
    ''' </param>
    ''' <param name="encoding">Default value is <see cref="DefaultEncoding"/></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 请注意，如果字符编码是不定长的，则返回的字符串可能会出现乱码的问题
    ''' </remarks>
    <Extension>
    Public Function Tails(path$, length%, Optional encoding As Encoding = Nothing) As String
        If Not path.FileExists Then
            Return Nothing
        Else
            Using reader As New FileStream(path, FileMode.Open)
                Return reader.Tails(length, encoding)
            End Using
        End If
    End Function

    ''' <summary>
    ''' Peek the tails of a large text file.(尝试查看大文件的尾部的数据)
    ''' </summary>
    ''' <param name="length">
    ''' Peeks of the number of characters. The number of the characters, not the bytes value.
    ''' (字符的数目)
    ''' </param>
    ''' <param name="encoding">Default value is <see cref="DefaultEncoding"/></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 请注意，如果字符编码是不定长的，则返回的字符串可能会出现乱码的问题
    ''' </remarks>
    <Extension>
    Public Function Tails(file As Stream, length%, Optional encoding As Encoding = Nothing) As String
        Dim textEncoder As Encoding = encoding Or DefaultEncoding

        length *= (textEncoder.GetBytes("a").Length + 1)

        If file.Length < length Then
            length = file.Length
        End If

        Dim buffer As Byte() = New Byte(length - 1) {}

        Call file.Seek(file.Length - length, SeekOrigin.Begin)
        Call file.Read(buffer, 0, buffer.Length)

        Dim value$ = textEncoder.GetString(buffer)
        Return value
    End Function

    ''' <summary>
    ''' Get last line of the target text file.
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <param name="newLine$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetLastLine(path$, Optional encoding As Encoding = Nothing, Optional newLine$ = vbLf) As String
        Using sr As New StreamReader(path, encoding Or UTF8)
            Dim lastline As String
            Dim i As Integer = 2

            Call sr.DiscardBufferedData()

            Do
                If i <= sr.BaseStream.Length Then
                    sr.BaseStream.Seek(sr.BaseStream.Length - i, SeekOrigin.Begin)
                    lastline = sr.ReadToEnd

                    If lastline.StartsWith(newLine) Then
                        Exit Do
                    End If

                    i += 1
                Else
                    ' 目标文本文件只有一行数据
                    sr.BaseStream.Seek(Scan0, SeekOrigin.Begin)
                    Return sr.ReadToEnd
                End If
            Loop

            ' 因为空格可能是所需要的字符串的数据
            ' 所以在这里只取出前后的newline字符串
            Return lastline.Trim(ASCII.CR, ASCII.LF)
        End Using
    End Function

    ''' <summary>
    ''' Please make sure all of the file in the target directory is text file not binary file.
    ''' </summary>
    ''' <param name="dir$"></param>
    ''' <returns></returns>
    Public Function Merge(<Parameter("Dir", "The default directory parameter value is the current directory.")> Optional dir$ = "./") As String
        Dim Texts = From file As String
                    In FileIO.FileSystem.GetFiles(dir, FileIO.SearchOption.SearchAllSubDirectories, "*.*")
                    Select FileIO.FileSystem.ReadAllText(file)
        Dim Merged As String = String.Join(vbCr, Texts)
        Return Merged
    End Function
End Module
