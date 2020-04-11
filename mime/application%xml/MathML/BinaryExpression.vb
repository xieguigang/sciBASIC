#Region "Microsoft.VisualBasic::cbceb58e860ae6b8a7345b4ac36cbb05, mime\application%xml\MathML\BinaryExpression.vb"

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

'     Class BinaryExpression
' 
'         Properties: [operator], applyleft, applyright
' 
'         Function: ToString
' 
'     Class SymbolExpression
' 
'         Properties: isNumericLiteral, text
' 
'         Function: ToString
' 
'     Class LambdaExpression
' 
'         Properties: lambda, parameters
' 
'         Function: FromMathML, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace MathML

    Public Class BinaryExpression : Inherits MathExpression

        Public Property [operator] As String

        Public Property applyleft As MathExpression
        Public Property applyright As MathExpression

        Public Overrides Function ToString() As String
            Return ContentBuilder.ToString(Me)
        End Function

    End Class

    Public Class SymbolExpression : Inherits MathExpression

        Public Property text As String
        Public Property isNumericLiteral As Boolean

        Public Overrides Function ToString() As String
            Return text
        End Function

    End Class

    Public MustInherit Class MathExpression
    End Class

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class LambdaExpression

        Public Property parameters As String()
        Public Property lambda As MathExpression

        Public Overrides Function ToString() As String
            Return $"function({parameters.JoinBy(", ")}) {{
    return {lambda};
}}"
        End Function

        Public Shared Function FromMathML(xmlText As String) As LambdaExpression
            Return XmlParser.ParseXml(xmlText).ParseXml
        End Function

        Public Shared Function FromMathML(xml As XmlElement) As LambdaExpression
            Return xml.ParseXml
        End Function

    End Class

    Public Class MathFunctionExpression : Inherits MathExpression

        Public Property name As String
        Public Property parameters As MathExpression()

        Public Overrides Function ToString() As String
            Return $"{name}({parameters.JoinBy(", ")})"
        End Function
    End Class
End Namespace
