' Binary Tree Expression Solver
' * By James Brannan, 2005.
' * irregularexpression@gmail.com
' * You may copy and redistribute
' * this code free of charge as you
' * see fit.
' 

Imports System.Collections.Generic
Imports System.Text

''' <summary>
''' Node Class: Base for binary tree, holds data for left and right nodes.
''' </summary>
''' <remarks>
''' http://www.codeproject.com/Articles/10316/Binary-Tree-Expression-Solver
''' </remarks>
Public Class Node

    ' Stack used to solve for a given tree.
    Private stack As New Stack(Of String)()

    ' Solves a tree
    Public Function Solve() As Integer
        ' This method uses a stack to solve the expression. The postfix
        '             * notation is tokenized and systematically added to the stack.
        '             * When the stack encounters an operation, it is executed and
        '             * modifies the contents on stack. The final item left on the 
        '             * stack (given the expression was valid) is the answer.
        '             

        Dim a As String, b As String
        ' Temporary placeholders for popped values
        Dim tokens As String() = Postfix().Split(" "c)
        ' Tokenize the postfix output
        For Each e As String In tokens
            Select Case e
                    ' For operation cases, the last two items added to the stack are
'                     * removed and acted upon. For any other case, the value is pushed
'                     * onto the stack.
'                     

                Case "+"
                    b = stack.Pop()
                    a = stack.Pop()
                    stack.Push(Convert.ToString(Convert.ToInt16(a) + Convert.ToInt16(b)))
                    Exit Select
                Case "-"
                    b = stack.Pop()
                    a = stack.Pop()
                    stack.Push(Convert.ToString(Convert.ToInt16(a) - Convert.ToInt16(b)))
                    Exit Select
                Case "/"
                    b = stack.Pop()
                    a = stack.Pop()
                    stack.Push(Convert.ToString(Convert.ToInt16(a) \ Convert.ToInt16(b)))
                    Exit Select
                Case "*"
                    b = stack.Pop()
                    a = stack.Pop()
                    stack.Push(Convert.ToString(Convert.ToInt16(a) * Convert.ToInt16(b)))
                    Exit Select
                Case "%"
                    b = stack.Pop()
                    a = stack.Pop()
                    stack.Push(Convert.ToString(Convert.ToInt16(a) Mod Convert.ToInt16(b)))
                    Exit Select
                Case Else
                    stack.Push(e)
                    Exit Select
            End Select
        Next
        ' Value left over is the result of the expression
        Return Convert.ToInt16(stack.Pop())
    End Function

    ' Returns the prefix notation for the expression
    Public Function Prefix() As String
        ' Function recurses through the left then right
        '             * nodes after its value.
        '             

        Dim res As String = Me.Value & " "
        If Me.left IsNot Nothing Then
            ' If node is not a leaf, then recurse
            res += Me.left.Prefix()
            res += Me.right.Prefix()
        End If
        Return res
    End Function

    ' Returns the postfix notation for the expression
    Public Function Postfix() As String
        ' Function recurses through the left then right,
        '             * bottom-up. All leafs are returned before their
        '             * parent operators.
        '             

        Dim res As String = ""
        If Me.left IsNot Nothing Then
            ' If node is not a leaf, then recurse
            res += Me.left.Postfix() & " "
            res += Me.right.Postfix() & " "
        End If
        res += Me.Value
        Return res
    End Function

    ' Returns the (fully parenthesized) infix notation for the expression
    Public Function Infix() As String
        ' Function recurses through left, then returns
        '             * value, and recurses right. Each expression is
        '             * nested in parentheses.
        '             

        Dim res As String = ""
        If Me.left IsNot Nothing Then
            res = res & "(" & left.Infix() & " " & Value & " " & right.Infix() & ")"
        Else
            res += Value
        End If
        Return res
    End Function

    ' Constructor for subnodes
    Public Sub New(op As Char, l As Node, r As Node)
        left = l
        right = r
        Value = op.ToString()
    End Sub
    ' Constructor for leaf nodes
    Public Sub New(value__1 As String)
        ' Leaf nodes have no left or right subnodes
        left = Nothing
        right = Nothing
        Value = value__1
    End Sub

    ' Node connected on the left
    Private left As Node
    ' Node connected on the right
    Private right As Node
    ' Value (operator or term)
    Private Value As String

    ' Sample program:
    ' The code below demonstrates the use of the Node class. The expression being
    '         * tested is graphed as shown below. (Make sure you're using a monospace font)
    '         * (((1-2)-3) + (4*(5+6)))
    '         *        +
    '         *      /   \
    '         *     -     *
    '         *    / \   / \
    '         *   -   3 4   +
    '         *  / \       / \
    '         * 1   2     5   6
    '        


    Friend Shared Sub Main(args As String())
        Dim root As New Node("+"c, New Node("-"c, New Node("-"c, New Node("1"), New Node("2")), New Node("3")), New Node("*"c, New Node("4"), New Node("+"c, New Node("5"), New Node("6"))))
        Console.WriteLine("Prefix notation: " & vbTab & root.Prefix())
        Console.WriteLine("Postfix notation: " & vbTab & root.Postfix())
        Console.WriteLine("Infix notation: " & vbTab & root.Infix())
        Console.WriteLine("Solution for tree is:" & vbTab & root.Solve())
        Console.ReadKey(True)
    End Sub
End Class
