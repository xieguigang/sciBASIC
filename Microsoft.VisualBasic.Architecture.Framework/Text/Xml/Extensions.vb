#Region "Microsoft.VisualBasic::b1bb4c916d2b2d3ed75212bd230df7bb, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Xml\Extensions.vb"

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
Imports System.Xml

Namespace Text.Xml

    Public Module Extensions

        ''' <summary>
        ''' 使用这个函数删除xml文本字符串之中的无效的字符
        ''' </summary>
        ''' <param name="xml$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StripInvalidCharacters(xml$) As String
            Dim sb As New StringBuilder(xml)

            Call sb.Replace("&#x8;", ".")

            Return sb.ToString
        End Function

        <Extension>
        Public Function FormatHTML(html$) As String
            Dim xml$ = "<?xml version=""1.0""?>" & html
            Dim doc As New XmlDocument
            Dim ms As New MemoryStream

            Using writer As New StreamWriter(ms, Encoding.UTF8)
                Call doc.LoadXml(xml)
                Call doc.Save(writer)
                Call writer.Flush()
            End Using

            Dim out$ = Encoding.UTF8 _
                .GetString(ms.ToArray) _
                .Trim("?"c)  ' 很奇怪，生成的字符串的开始的位置有一个问号
            Return out
        End Function
    End Module
End Namespace
