#Region "Microsoft.VisualBasic::7316b87e52da389b3ad1214c9dfc8cab, mime\application%xml\MathML\Math.vb"

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

    '     Class Math
    ' 
    '         Properties: apply, lambda
    ' 
    '     Class Apply
    ' 
    '         Properties: [operator], apply, cn, divide, plus
    '                     power, times
    ' 
    '     Class mathOperator
    ' 
    ' 
    ' 
    '     Class constant
    ' 
    '         Properties: type, value
    ' 
    '         Function: ToString
    ' 
    '     Class lambda
    ' 
    '         Properties: apply, bvar
    ' 
    '     Class symbols
    ' 
    '         Properties: ci
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace MathML

    <XmlRoot("math", Namespace:="http://www.w3.org/1998/Math/MathML")>
    <XmlType("math", Namespace:="http://www.w3.org/1998/Math/MathML")>
    Public Class Math

        Public Property apply As Apply
        Public Property lambda As lambda

    End Class

    Public Class Apply : Inherits symbols

        Public Property divide As mathOperator
        Public Property times As mathOperator
        Public Property plus As mathOperator
        Public Property power As mathOperator

        Public Property cn As constant

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
    End Class

    Public Class mathOperator
    End Class

    Public Class constant

        <XmlAttribute>
        Public Property type As String
        <XmlText>
        Public Property value As String

        Public Overrides Function ToString() As String
            Return $"({type}) {value}"
        End Function

    End Class

    Public Class lambda

        <XmlElement("bvar")>
        Public Property bvar As symbols()
        Public Property apply As Apply
    End Class

    Public Class symbols

        <XmlElement("ci")>
        Public Property ci As String()

        Public Overrides Function ToString() As String
            Return ci.JoinBy(" ")
        End Function
    End Class
End Namespace
