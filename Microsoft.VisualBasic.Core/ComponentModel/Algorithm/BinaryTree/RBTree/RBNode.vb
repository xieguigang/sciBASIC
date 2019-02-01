Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' 与键名所对应的数据是存储在<see cref="BinaryTree(Of K, V).Value"/>之中的
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    Public Class RBNode(Of K, V) : Inherits BinaryTree(Of K, V)

        Public Property Red As Boolean

        Public Property Child(dir As Boolean) As RBNode(Of K, V)
            Get
                If dir Then
                    Return Right
                Else
                    Return Left
                End If
            End Get
            Set(value As RBNode(Of K, V))
                If dir Then
                    Right = value
                Else
                    Left = value
                End If
            End Set
        End Property

        Public Sub New(key As K, value As V,
                       Optional parent As BinaryTree(Of K, V) = Nothing,
                       Optional toString As Func(Of K, String) = Nothing)

            MyBase.New(key, value, parent, toString)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{If(Red, "Red", "Black")}] {MyBase.ToString}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function IsRed(node As RBNode(Of K, V)) As Boolean
            Return Not node Is Nothing AndAlso node.Red
        End Function
    End Class
End Namespace