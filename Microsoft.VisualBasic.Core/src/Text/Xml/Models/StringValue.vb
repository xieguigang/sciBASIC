#Region "Microsoft.VisualBasic::e8744a6c59f7de0ab8941bd85c4a625f, Microsoft.VisualBasic.Core\src\Text\Xml\Models\StringValue.vb"

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

    '     Class StringValue
    ' 
    '         Properties: value
    ' 
    '         Function: ToString
    ' 
    '     Structure LineValue
    ' 
    '         Properties: line, text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Models

    Public Class StringValue : Implements Value(Of String).IValueOf

        ''' <summary>
        ''' A short text value without new line symbols
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property value As String Implements Value(Of String).IValueOf.Value

        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class

    ''' <summary>
    ''' 代码行的模型？
    ''' </summary>
    Public Structure LineValue

        <XmlAttribute>
        Public Property line As Integer
        <XmlText>
        Public Property text As String

        Public Overrides Function ToString() As String
            Return $"[{line}]  {text}"
        End Function
    End Structure
End Namespace
