#Region "Microsoft.VisualBasic::0e2bee6de84b7e0d2dee354b2f23fff5, mime\application%xml\MathML\XML\Apply.vb"

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

    '   Total Lines: 52
    '    Code Lines: 30 (57.69%)
    ' Comment Lines: 12 (23.08%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 10 (19.23%)
    '     File Size: 1.57 KB


    '     Class Apply
    ' 
    '         Properties: [operator], apply, cn, divide, plus
    '                     power, times
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace MathML

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' the math xml data should be parsed via the method:
    ''' 
    ''' <see cref="ContentBuilder.ParseXml(XmlElement)"/>
    ''' </remarks>
    Public Class Apply : Inherits symbols

        Public Property divide As mathOperator
        Public Property times As mathOperator
        Public Property plus As mathOperator
        Public Property power As mathOperator

        Public Property cn As constant

        ' 20221113 基于xml反序列化具有一个bug：
        ' 当节点同时存在ci符号名称以及apply表达式的时候
        ' xml反序列化是无法了解到具体的顺序的
        ' 这个会导致出现错误的表达式构建结果

        <XmlElement("apply")>
        Public Property apply As Apply()

        Public ReadOnly Property [operator] As String
            Get
                If Not divide Is Nothing Then
                    Return "/"
                ElseIf Not times Is Nothing Then
                    Return "*"
                ElseIf Not plus Is Nothing Then
                    Return "+"
                ElseIf Not power Is Nothing Then
                    Return "^"
                Else
                    Return "-"
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"({apply.JoinBy([operator])})"
        End Function

    End Class

End Namespace
