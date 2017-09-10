#Region "Microsoft.VisualBasic::6f83db4b6b05330c402fbadc01ddf63c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\LargeTextFile.vb"

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
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Wrapper for the file operations.
''' </summary>
''' <remarks></remarks>
<[Namespace]("Large_Text_File")>
Public Module LargeTextFile

    ''' <summary>
    ''' Iterates read all lines in a very large text file, using for loading a very large size csv/tsv file
    ''' </summary>
    ''' <param name="path$">file path</param>
    ''' <param name="title$">The header line of this large size csv/tsv file.</param>
    ''' <param name="skip%">Skip n lines, then start to populate data lines.</param>
    ''' <param name="encoding">Text file encoding.</param>
    ''' <returns></returns>
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

    <Extension>
    Public Iterator Function IteratesStream(s As StreamReader) As IEnumerable(Of String)
        Do While Not s.EndOfStream
            Yield s.ReadLine
        Loop
    End Function

    <ExportAPI("Partitioning")>
    Public Function TextPartition(data As IEnumerable(Of String)) As String()()
        Dim maxSize As Double = New StringBuilder(1024 * 1024).MaxCapacity
        Return __textPartitioning(data.ToArray, maxSize)
    End Function

    Private Function __textPartitioning(dat As String(), maxSize As Double) As String()()
        Dim currentSize As Double = (From s As String In dat.AsParallel Select CDbl(Len(s))).Sum
        If currentSize > maxSize Then
            Dim SplitTokens = dat.Split(CInt(dat.Length / 2))
            If SplitTokens.Length > 1 Then
                Return (From n In SplitTokens Select __textPartitioning(n, maxSize)).ToVector
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
    ''' Peek the tails of a large text file.(尝试查看大文件的尾部的数据)
    ''' </summary>
    ''' <param name="path">If the file is not exists, then this function will returns nothing.</param>
    ''' <param name="length">Peeks of the number of characters.(字符的数目)</param>
    ''' <param name="encoding">Default value is <see cref="DefaultEncoding"/></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Tails")>
    <Extension>
    Public Function Tails(path$, <Parameter("characters", "The number of the characters, not the bytes value.")> length%, Optional encoding As Encoding = Nothing) As String
        If Not path.FileExists Then
            Return Nothing
        Else
            length *= 8
        End If

        Using reader As New FileStream(path, FileMode.Open)
            If reader.Length < length Then
                length = reader.Length
            End If

            Dim chunkBuffer As Byte() = New Byte(length - 1) {}

            Call reader.Seek(reader.Length - length, SeekOrigin.Begin)
            Call reader.Read(chunkBuffer, 0, chunkBuffer.Length)

            Dim value$ = (encoding Or DefaultEncoding).GetString(chunkBuffer)
            Return value
        End Using
    End Function

    <ExportAPI(".Merge", Info:="Please make sure all of the file in the target directory is text file not binary file.")>
    Public Function Merge(<Parameter("Dir", "The default directory parameter value is the current directory.")> Optional dir$ = "./") As String
        Dim Texts = From file As String
                    In FileIO.FileSystem.GetFiles(dir, FileIO.SearchOption.SearchAllSubDirectories, "*.*")
                    Select FileIO.FileSystem.ReadAllText(file)
        Dim Merged As String = String.Join(vbCr, Texts)
        Return Merged
    End Function
End Module
