Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    Module Derivative

        <Extension>
        Public Function GetDerivative(exp As Expression) As Expression
            If TypeOf exp Is Literal Then
                ' num ' = 0 
                Return Literal.Zero
            ElseIf TypeOf exp Is UnifySymbol Then

            ElseIf TypeOf exp Is SymbolExpression Then
                ' x ' = 1
                Return Literal.One
            End If
        End Function
    End Module
End Namespace