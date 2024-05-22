#Region "Microsoft.VisualBasic::4db54b1b66562f9f59557f843c497302, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\BinaryExpression.vb"

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

    '   Total Lines: 46
    '    Code Lines: 24 (52.17%)
    ' Comment Lines: 15 (32.61%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 7 (15.22%)
    '     File Size: 1.57 KB


    '     Class BinaryExpression
    ' 
    '         Properties: [operator], left, right
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, Power, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' left op right
    ''' </summary>
    Public Class BinaryExpression : Inherits Expression

        Public ReadOnly Property left As Expression
        Public ReadOnly Property right As Expression
        Public ReadOnly Property [operator] As Char

        ''' <summary>
        ''' construct a new binary expression of:
        ''' 
        ''' ```
        ''' <paramref name="left"/> <paramref name="op"/> <paramref name="right"/>
        ''' ```
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="op">
        ''' the binary math operator
        ''' </param>
        Sub New(left As Expression, right As Expression, op As Char)
            Me.left = left
            Me.right = right
            Me.operator = op
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Dim left As Double = Me.left.Evaluate(env)
            Dim right As Double = Me.right.Evaluate(env)
            Dim result As Double = Arithmetic.Evaluate(left, right, [operator])

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"({left} {[operator]} {right})"
        End Function

        Public Shared Function Power(x As Expression, y As Integer) As Expression
            Return New BinaryExpression(x, New Literal(y), "^"c)
        End Function
    End Class
End Namespace
