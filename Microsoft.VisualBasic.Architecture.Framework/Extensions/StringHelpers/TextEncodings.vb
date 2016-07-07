#Region "Microsoft.VisualBasic::da07161ff931829ffb0cfd4188ab81b8, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Extensions\StringHelpers\TextEncodings.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language

Public Module TextEncodings

    ''' <summary>
    ''' Default value or user specific?
    ''' </summary>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension> Public Function Assertion(encoding As Encoding) As Encoding
        If encoding Is Nothing Then
            Return System.Text.Encoding.Default
        Else
            Return encoding
        End If
    End Function

    ''' <summary>
    ''' The text document encodings constant for text file read and write
    ''' </summary>
    Public Enum Encodings As Byte
        ''' <summary>
        ''' <see cref="Encoding.Default"/>: Gets an encoding for the operating system's current ANSI code page.
        ''' </summary>
        [Default] = 0
        ASCII = 10
        Unicode
        UTF7
        UTF8
        UTF32

        ''' <summary>
        ''' Text encoding for simplify Chinese.
        ''' </summary>
        GB2312
    End Enum

    Public ReadOnly Property TextEncodings As IReadOnlyDictionary(Of Encodings, Encoding) =
        New Dictionary(Of Encodings, System.Text.Encoding) From {
 _
        {Encodings.ASCII, System.Text.Encoding.ASCII},
        {Encodings.GB2312, System.Text.Encoding.GetEncoding("GB2312")},
        {Encodings.Unicode, System.Text.Encoding.Unicode},
        {Encodings.UTF7, System.Text.Encoding.UTF7},
        {Encodings.UTF32, System.Text.Encoding.UTF32},
        {Encodings.UTF8, System.Text.Encoding.UTF8},
        {Encodings.Default, System.Text.Encoding.Default}
    }

    ''' <summary>
    ''' Get text file save encodings instance
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <Extension> Public Function GetEncodings(value As Encodings) As System.Text.Encoding
        If TextEncodings.ContainsKey(value) Then
            Return _TextEncodings(value)
        Else
            Return System.Text.Encoding.UTF8
        End If
    End Function

    Public Function GetEncodings(value As System.Text.Encoding) As TextEncodings.Encodings
        Dim Name As String = value.ToString.Split("."c).Last

        Select Case Name
            Case NameOf(Encodings.ASCII) : Return Encodings.ASCII
            Case NameOf(Encodings.GB2312) : Return Encodings.GB2312
            Case NameOf(Encodings.Unicode) : Return Encodings.Unicode
            Case NameOf(Encodings.UTF32) : Return Encodings.UTF32
            Case NameOf(Encodings.UTF7) : Return Encodings.UTF7
            Case NameOf(Encodings.UTF8) : Return Encodings.UTF8
            Case NameOf(Encodings.Default) : Return Encodings.Default
            Case Else
                Return Encodings.UTF8
        End Select
    End Function

    ''' <summary>
    ''' 有时候有些软件对文本的编码是有要求的，则可以使用这个函数进行文本编码的转换
    ''' 例如R程序默认是读取ASCII，而。NET的默认编码是UTF8，则可以使用这个函数将目标文本文件转换为ASCII编码的文本文件
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <param name="from"></param>
    ''' <returns></returns>
    <Extension>
    Public Function TransEncoding(path As String, encoding As Encodings, Optional from As System.Text.Encoding = Nothing) As Boolean
        If Not path.FileExists Then
            Call "".SaveTo(path, encoding.GetEncodings)
        End If

        Dim tmp As String = If(from Is Nothing, IO.File.ReadAllText(path), IO.File.ReadAllText(path, from))
        Return tmp.SaveTo(path, encoding.GetEncodings)
    End Function
End Module
