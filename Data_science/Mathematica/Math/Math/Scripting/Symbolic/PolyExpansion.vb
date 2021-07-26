Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Scripting.MathExpression

    ''' <summary>
    ''' (a+b)^2 = a^2 + ab + ab + b ^ 2
    ''' </summary>
    Public Module PolyExpansion

        <Extension>
        Public Function Expands(expression As Expression) As Expression
            If Not TypeOf expression Is BinaryExpression Then
                Return expression
            Else
                Return ExpandsBinary(expression)
            End If
        End Function

        Private Function ExpandsBinary(bin As BinaryExpression) As Expression
            Select Case bin.operator
                Case "^"
                    If TypeOf bin.right Is Literal AndAlso DirectCast(bin.right, Literal).isInteger Then
                        If TypeOf bin.left Is BinaryExpression Then
                            Return ExpandsPower(bin.left, bin.right.Evaluate(Nothing))
                        Else
                            Return bin
                        End If
                    Else
                        Return bin
                    End If
                Case "+", "-"
                    Return bin
                Case "*", "/"
                    Return bin
                Case Else
                    Throw New NotImplementedException(bin.operator)
            End Select
        End Function

        Private Function ExpandsPower(base As BinaryExpression, power As Integer) As Expression
            If Not (base.operator = "+" OrElse base.operator = "-") Then
                Return BinaryExpression.Power(base, power)
            End If

            Dim a As Expression = base.left
            Dim b As Expression = base.right

            If base.operator = "-" Then

            End If
        End Function
    End Module
End Namespace