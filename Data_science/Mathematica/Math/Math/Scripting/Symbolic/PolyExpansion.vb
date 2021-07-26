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
            End If


        End Function
    End Module
End Namespace