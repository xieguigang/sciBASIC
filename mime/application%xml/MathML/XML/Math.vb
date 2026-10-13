#Region "Microsoft.VisualBasic::8d1054ade1e9ffc67ad959a7dac21605, mime\application%xml\MathML\XML\Math.vb"

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

    '   Total Lines: 59
    '    Code Lines: 41 (69.49%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (30.51%)
    '     File Size: 1.55 KB


    '     Class Math
    ' 
    '         Properties: apply, lambda
    ' 
    '         Function: BuildExpressionString
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
    '         Function: ToString
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
Imports Microsoft.VisualBasic.Language

Namespace MathML

    <XmlRoot("math", Namespace:="http://www.w3.org/1998/Math/MathML")>
    <XmlType("math", Namespace:="http://www.w3.org/1998/Math/MathML")>
    Public Class Math

        Public Property apply As Apply
        Public Property lambda As lambda

        Public Shared Function BuildExpressionString(exp As Apply) As String
            Dim data = exp.apply.Select(Function(a) BuildExpressionString(a)).ToArray
            Dim op As String = exp.operator

            Return $"({data.JoinBy(op)})"
        End Function

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

        Public Overrides Function ToString() As String
            Return bvar.JoinBy(", ") & " => " & Math.BuildExpressionString(apply)
        End Function

    End Class

    Public Class symbols

        <XmlElement("ci")>
        Public Property ci As String()

        Public Overrides Function ToString() As String
            Return ci.JoinBy(" ")
        End Function
    End Class
End Namespace
