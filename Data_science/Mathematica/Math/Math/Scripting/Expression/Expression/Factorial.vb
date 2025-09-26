#Region "Microsoft.VisualBasic::e0bd8b6504246b7ee457cea621b5d248, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\Factorial.vb"

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

    '   Total Lines: 28
    '    Code Lines: 19 (67.86%)
    ' Comment Lines: 3 (10.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 844 B


    '     Class Factorial
    ' 
    '         Properties: factor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetVariableSymbols, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' x!
    ''' </summary>
    Public Class Factorial : Inherits Expression

        Public ReadOnly Property factor As Expression

        Sub New(factor As String)
            Me.factor = Expression.Parse(factor)
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Dim factor As Double = env.Evaluate(factor)
            Dim result = VBMath.Factorial(CInt(factor))
            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"({factor})!"
        End Function

        Public Overrides Function GetVariableSymbols() As IEnumerable(Of String)
            Return factor.GetVariableSymbols
        End Function
    End Class
End Namespace
