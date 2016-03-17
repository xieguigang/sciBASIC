Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Module StackSolver

    ''' <summary>
    ''' Solves a tree
    ''' </summary>
    ''' <param name="tokens">Expression Model: Temporary placeholders for popped values</param>
    ''' <returns></returns>
    <Extension> Public Function Solve(tokens As List(Of Token(Of Tokens))) As Double
        Return New Pointer(Of Token(Of Tokens))(tokens).Solve
    End Function

    ''' <summary>
    ''' Solves a tree
    ''' </summary>
    ''' <param name="tokens">Expression Model: Temporary placeholders for popped values</param>
    ''' <returns></returns>
    <Extension> Public Function Solve(tokens As Pointer(Of Token(Of Tokens))) As Double
        ' This method uses a stack to solve the expression. The postfix
        '             * notation is tokenized and systematically added to the stack.
        '             * When the stack encounters an operation, it is executed and
        '             * modifies the contents on stack. The final item left on the 
        '             * stack (given the expression was valid) is the answer.
        '             
        ' Stack used to solve for a given tree.
        Dim stack As New Stack(Of Double)()
        Dim a As Double, b As Double
        Dim c As Double
        Dim e As Token(Of Tokens)

        Call stack.Push(0R)

        Do While Not tokens.EndRead         ' Tokenize the postfix output
            e = +tokens

            Select Case e.Type
                    ' For operation cases, the last two items added to the stack are
'                     * removed and acted upon. For any other case, the value is pushed
'                     * onto the stack.
'                     
                Case Mathematical.Tokens.Operator
                    b = stack.Pop
                    a = stack.Pop
                    c = Helpers.Arithmetic.Evaluate(a, b, e.Text.First)
                    stack.Push(c)

                Case Mathematical.Tokens.Number
                    stack.Push(Val(e.Text))

                Case Mathematical.Tokens.OpenBracket, Mathematical.Tokens.OpenStack
                    stack.Push(tokens.Solve)

                Case Mathematical.Tokens.CloseStack, Mathematical.Tokens.CloseBracket
                    Exit Do

                Case Mathematical.Tokens.UNDEFINE

            End Select
        Loop

        Return Val(stack.Pop())    ' Value left over is the result of the expression
    End Function
End Module
