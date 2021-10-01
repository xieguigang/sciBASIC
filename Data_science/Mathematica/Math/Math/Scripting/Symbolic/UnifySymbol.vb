#Region "Microsoft.VisualBasic::57bd05582d639f1491e93ee960f5a108, Data_science\Mathematica\Math\Math\Scripting\Symbolic\UnifySymbol.vb"

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

    '     Class UnifySymbol
    ' 
    '         Properties: factor, power
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Scripting.MathExpression

    ''' <summary>
    ''' a unify symbol model
    ''' 
    ''' a * (x ^ n)
    ''' </summary>
    Public Class UnifySymbol : Inherits SymbolExpression

        Public Property factor As Expression = New Literal(1)
        Public Property power As Expression = New Literal(1)

        Public Sub New(symbol As SymbolExpression)
            MyBase.New(symbol.symbolName)
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return factor.Evaluate(env) * (MyBase.Evaluate(env) ^ power.Evaluate(env))
        End Function

        Public Overrides Function ToString() As String
            Return $"({factor} * ({symbolName} ^ {power}))"
        End Function
    End Class
End Namespace
