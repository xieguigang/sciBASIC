Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace ComponentModel.Algorithm.BinaryTree

    Public Class BinaryTree(Of K, V) : Implements Value(Of V).IValueOf

        ''' <summary>
        ''' 键名是唯一的，赋值之后就不可以改变了
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Key As K
        ''' <summary>
        ''' 与当前的这个键名相对应的键值可以根据需求发生改变，即可以被任意赋值
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As V Implements Value(Of V).IValueOf.Value
        Public Property Left As BinaryTree(Of K, V)
        Public Property Right As BinaryTree(Of K, V)

        ''' <summary>
        ''' Additional values that using for the binary tree algorithm.
        ''' </summary>
        ReadOnly additionals As New Dictionary(Of String, Object)

        ''' <summary>
        ''' Full name of current node
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property QualifiedName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return viewQualifiedName(Me)
            End Get
        End Property

        Default Public ReadOnly Property Item(key As String) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return additionals.TryGetValue(key)
            End Get
        End Property

        ReadOnly defaultView As New DefaultValue(Of Func(Of K, String))(Function(key) Scripting.ToString(key))

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <param name="parent"></param>
        ''' <param name="toString">Default debug view is <see cref="Scripting.ToString(Object, String)"/></param>
        Sub New(key As K, value As V,
                Optional parent As BinaryTree(Of K, V) = Nothing,
                Optional toString As Func(Of K, String) = Nothing)

            Me.Key = key
            Me.Value = value

            Call SetValue("name", (toString Or defaultView)(key))
            Call SetValue("parent", parent)
        End Sub

        ''' <summary>
        ''' Set <see cref="additionals"/> value by using a key value tuple.
        ''' </summary>
        ''' <param name="key$"></param>
        ''' <param name="value"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetValue(key$, value As Object)
            additionals(key) = value
        End Sub

        Private Shared Function viewQualifiedName(node As BinaryTree(Of K, V)) As String
            Dim additionals = node.additionals
            Dim parent = TryCast(additionals!parent, BinaryTree(Of K, V))
            Dim name$ = additionals!name

            If parent Is Nothing Then
                Return "/"
            Else
                Return parent.QualifiedName & "/" & name
            End If
        End Function

        ''' <summary>
        ''' Display debug view as: ``[key, value]``
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"[{additionals!name}, {Value}]"
        End Function
    End Class
End Namespace