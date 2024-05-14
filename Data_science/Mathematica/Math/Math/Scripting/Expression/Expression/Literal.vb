#Region "Microsoft.VisualBasic::2a8ced7d94b5e269a0ffb3cddc4d3f08, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\Literal.vb"

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

    '   Total Lines: 92
    '    Code Lines: 64
    ' Comment Lines: 11
    '   Blank Lines: 17
    '     File Size: 2.77 KB


    '     Class Literal
    ' 
    '         Properties: isInteger, number, One, Zero
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) Evaluate, GetNegative, GetReciprocal, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' A number literal
    ''' </summary>
    Public Class Literal : Inherits Expression

        Public ReadOnly Property number As Double

        Public ReadOnly Property isInteger As Boolean
            Get
                Return CDbl(CInt(number)) = number
            End Get
        End Property

        ''' <summary>
        ''' the literal value of 1
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property One As Literal
            Get
                Return New Literal(1)
            End Get
        End Property

        ''' <summary>
        ''' the literal value of 0
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Zero As Literal
            Get
                Return New Literal(0)
            End Get
        End Property

        Sub New(text As String)
            Me.number = Val(text)
        End Sub

        Sub New(x As Double)
            Me.number = x
        End Sub

        Public Function GetNegative() As Literal
            Return New Literal(-1 * number)
        End Function

        Public Function GetReciprocal() As Literal
            Return New Literal(1 / number)
        End Function

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return number
        End Function

        Public Overloads Shared Function Evaluate(left As Literal, op As Char, right As Literal) As Literal
            Dim a As Double = left.Evaluate(Nothing)
            Dim b As Double = right.Evaluate(Nothing)

            Select Case op
                Case "+" : Return a + b
                Case "-" : Return a - b
                Case "*" : Return a * b
                Case "/" : Return a / b
                Case "%" : Return a Mod b
                Case "^" : Return a ^ b
                Case Else
                    Throw New NotImplementedException(op)
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return number
        End Function

        Public Overloads Shared Widening Operator CType(x As Double) As Literal
            Return New Literal(x)
        End Operator

        Public Overloads Shared Operator =(literal As Literal, num As Double) As Boolean
            If literal Is Nothing Then
                Return num = 0.0
            End If

            Return literal.number = num
        End Operator

        Public Overloads Shared Operator <>(literal As Literal, num As Double) As Boolean
            Return Not (literal = num)
        End Operator
    End Class
End Namespace
