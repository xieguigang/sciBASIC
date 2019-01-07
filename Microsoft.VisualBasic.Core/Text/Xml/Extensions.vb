#Region "Microsoft.VisualBasic::38de72d159e0c303221b295b0b492b2d, Microsoft.VisualBasic.Core\Text\Xml\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: FormatHTML, StripInvalidCharacters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace Text.Xml

    Public Module Extensions

        ''' <summary>
        ''' 使用这个函数将html文档进行格式化，请注意，目标html文档应该是符合xml语法的
        ''' </summary>
        ''' <param name="html$"></param>
        ''' <returns></returns>
        <Extension> Public Function FormatHTML(html$) As String
            Dim xml$ = "<?xml version=""1.0""?>" & html
            Dim doc As New XmlDocument
            Dim ms As New MemoryStream

            Using writer As New StreamWriter(ms, UTF8WithoutBOM)
                Call doc.LoadXml(xml)
                Call doc.Save(writer)
                Call writer.Flush()
            End Using

            Dim out$ = UTF8WithoutBOM _
                .GetString(ms.ToArray) _
                .Trim("?"c)  ' 很奇怪，生成的字符串的开始的位置有一个问号

            Return out
        End Function
    End Module
End Namespace
