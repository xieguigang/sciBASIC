#Region "Microsoft.VisualBasic::99a71133c1240784a96476c6bdce8e23, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Extensions\Doc\Text.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("Doc.TextFile", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
Public Module TextDoc

    ''' <summary>
    ''' Enumerate all of the chars in the target text file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ForEachChar(path As String, Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of Char)
        Using file As New FileStream(path, FileMode.Open)
            Using reader As New IO.BinaryReader(file, encoding.GetEncodings)
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
    Public Function OpenWriter(path As String, Optional encoding As Encodings = Encodings.UTF8) As StreamWriter
        Call "".SaveTo(path)
        Return New StreamWriter(New FileStream(path, FileMode.OpenOrCreate), encoding.GetEncodings)
    End Function

    ''' <summary>
    ''' 通过具有缓存的流对象读取文本数据，使用迭代器来读取文件之中的所有的行，大文件推荐使用这个方法进行读取操作
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function IterateAllLines(path As String) As IEnumerable(Of String)
        Dim fs As New FileStream(path, FileMode.Open)
        Dim reader As New StreamReader(fs)

        Do While Not reader.EndOfStream
            Yield reader.ReadLine
        Loop
    End Function

    ''' <summary>
    ''' Read the first line of the text in the file.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension> Public Function ReadFirstLine(path As String) As String
        Using file As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using reader As StreamReader = New StreamReader(file)
                Dim first As String = reader.ReadLine
                Return first
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 这个函数只建议读取小文本文件的时候使用
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
    ''' 这个函数只建议读取小文本文件的时候使用
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="Encoding">Default value is UTF8</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Read.Lines")>
    <Extension>
    Public Function ReadAllLines(path As String, Optional Encoding As Encoding = Nothing) As String()
        If Encoding Is Nothing Then
            Encoding = System.Text.Encoding.UTF8
        End If
        If path.FileExists Then
            Return IO.File.ReadAllLines(path, encoding:=Encoding)
        Else
            Return New String() {}
        End If
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
                                       <Parameter("Text.Encoding")> Optional encoding As Encoding = Nothing) As Boolean

        If String.IsNullOrEmpty(path) Then Return False
        If encoding Is Nothing Then encoding = Encoding.Default

        Dim DIR As String

        Try
            path = ProgramPathSearchTool.Long2Short(path)
            DIR = FileIO.FileSystem.GetParentPath(path)
        Catch ex As Exception
            Dim MSG As String = $" **** Directory string is illegal or string is too long:  [{NameOf(path)}:={path}] > 260"
            Throw New Exception(MSG, ex)
        End Try

        If String.IsNullOrEmpty(DIR) Then
            DIR = FileIO.FileSystem.CurrentDirectory
        End If

        Try
            Call FileIO.FileSystem.CreateDirectory(DIR)
            Call FileIO.FileSystem.WriteAllText(path, text, append:=False, encoding:=encoding)
        Catch ex As Exception
            ex = New Exception("[DIR]  " & DIR, ex)
            ex = New Exception("[Path]  " & path, ex)
            Throw ex
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
    ''' 将目标字符串数据全部写入到文件之中，当所写入的文件位置之上没有父文件夹存在的时候，会自动创建文件夹
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(array As IEnumerable(Of String),
                                       path As String,
                                       Optional encoding As Encoding = Nothing) As Boolean

        If String.IsNullOrEmpty(path) Then Return False
        If encoding Is Nothing Then encoding = Encoding.Default

        Call "".SaveTo(path)

        Using file As New StreamWriter(New FileStream(path, FileMode.OpenOrCreate), encoding)
            For Each line As String In array.SafeQuery
                Call file.WriteLine(line)
            Next
        End Using

        Return True
    End Function

    <ExportAPI("Write.Text")>
    <Extension> Public Function SaveTo(sBuilder As StringBuilder, path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        Return sBuilder.ToString.SaveTo(path, encoding)
    End Function
End Module
