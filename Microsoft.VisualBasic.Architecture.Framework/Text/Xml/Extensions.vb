#Region "Microsoft.VisualBasic::fe94e4a1fd86c3cca1fd04d59e7913bf, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Xml\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text

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
    End Module
End Namespace
