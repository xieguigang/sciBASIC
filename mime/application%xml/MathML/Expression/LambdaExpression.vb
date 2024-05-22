#Region "Microsoft.VisualBasic::57a6d00e6b220b9ecbaed37a203c9e4e, mime\application%xml\MathML\Expression\LambdaExpression.vb"

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

    '   Total Lines: 40
    '    Code Lines: 21 (52.50%)
    ' Comment Lines: 12 (30.00%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 7 (17.50%)
    '     File Size: 1.28 KB


    '     Class LambdaExpression
    ' 
    '         Properties: lambda, parameters
    ' 
    '         Function: (+2 Overloads) FromMathML, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace MathML

    ''' <summary>
    ''' A lambda expression that consist with the 
    ''' <see cref="parameters"/> and <see cref="lambda"/>
    ''' body.
    ''' 
    ''' example as: ``f(x) = x ^ 2``
    ''' </summary>
    Public Class LambdaExpression

        Public Property parameters As String()
        Public Property lambda As MathExpression

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"function({parameters.JoinBy(", ")}) {{
    return {lambda};
}}"
        End Function

        ''' <summary>
        ''' lambda expression parsed from the given xml text
        ''' </summary>
        ''' <param name="xmlText"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromMathML(xmlText As String) As LambdaExpression
            Return XmlParser.ParseXml(xmlText).ParseXml
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromMathML(xml As XmlElement) As LambdaExpression
            Return xml.ParseXml
        End Function

    End Class
End Namespace
