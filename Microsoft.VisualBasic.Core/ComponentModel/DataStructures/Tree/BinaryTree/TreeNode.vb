#Region "Microsoft.VisualBasic::2512e6ceaf36abffed482f292c39e523, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Tree\BinaryTree\TreeNode.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class TreeNode
    ' 
    '         Properties: AllChilds, ChainPosition, Count, DisplayQualifiedName, IsLeaf
    '                     Left, Name, Parent, QualifiedName, Right
    '                     Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetEnumerator, ToString
    '         Operators: -, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' Define tree nodes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TreeNode(Of T) : Implements INamedValue
        Implements Value(Of T).IValueOf

        Public Property Name As String Implements INamedValue.Key
        Public Property Value As T Implements Value(Of T).IValueOf.Value
        Public Property Left As TreeNode(Of T)
        Public Property Right As TreeNode(Of T)
        Public Property Parent As TreeNode(Of T)

        Public ReadOnly Property QualifiedName As String
            Get
                If Parent Is Nothing Then
                    Return "/"
                Else
                    Return Parent.QualifiedName & "/" & Name
                End If
            End Get
        End Property

        Public ReadOnly Property ChainPosition As String
            Get
                If Parent Is Nothing Then
                    Return "/"
                Else
                    If Parent.Left Is Nothing Then
                        Return Parent.ChainPosition & "/+"
                    ElseIf Parent.Right Is Nothing Then
                        Return Parent.ChainPosition & "/-"
                    Else
                        If Me Is Parent.Right Then
                            Return Parent.ChainPosition & "/+"
                        Else
                            Return Parent.ChainPosition & "/-"
                        End If
                    End If
                End If
            End Get
        End Property

        <ScriptIgnore> Public ReadOnly Property IsLeaf As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Left Is Nothing AndAlso
                    Right Is Nothing
            End Get
        End Property

        <ScriptIgnore> Public ReadOnly Property AllChilds As List(Of TreeNode(Of T))
            Get
                Dim list As New List(Of TreeNode(Of T))

                For Each x In Me.GetEnumerator
                    Call list.Add(x)
                    Call list.AddRange(x.AllChilds)
                Next

                Return list
            End Get
        End Property

        ''' <summary>
        ''' 递归的得到子节点的数目
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore> Public ReadOnly Property Count As Integer
            Get
                Dim n As Integer

                If Not Left Is Nothing Then
                    n += 1
                    n += Left.Count
                End If

                If Not Right Is Nothing Then
                    n += 1
                    n += Right.Count
                End If

                Return n
            End Get
        End Property

        ''' <summary>
        ''' Constructor  to create a single node 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="obj"></param>
        Public Sub New(name As String, obj As T)
            With Me
                .Name = name
                .Value = obj
            End With
        End Sub

        Sub New()
        End Sub

        Public Shared Property DisplayQualifiedName As Boolean = True

        Public Overrides Function ToString() As String
            If DisplayQualifiedName Then
                Return QualifiedName
            Else
                If Value Is Nothing Then
                    Return Name
                Else
                    Return $"[{Name}] {Value}"
                End If
            End If
        End Function

        ''' <summary>
        ''' 最多只有两个元素
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerable(Of TreeNode(Of T))
            If Not Left Is Nothing Then
                Yield Left
            End If
            If Not Right Is Nothing Then
                Yield Right
            End If
        End Function

        Public Shared Operator +(parent As TreeNode(Of T), child As TreeNode(Of T)) As TreeNode(Of T)
            If parent.Left Is Nothing Then
                parent.Left = child
                Return parent
            End If
            If parent.Right Is Nothing Then
                parent.Right = child
                Return parent
            End If

            Throw New Exception("TreeNode is full, can not append any more!")
        End Operator

        Public Shared Operator -(parent As TreeNode(Of T), child As TreeNode(Of T)) As TreeNode(Of T)
            If Not parent.Left Is Nothing Then
                If parent.Left.Equals(child) Then
                    parent.Left = Nothing
                    Return parent
                End If
            End If
            If Not parent.Right Is Nothing Then
                If parent.Right.Equals(child) Then
                    parent.Right = Nothing
                    Return parent
                End If
            End If
            Return parent
        End Operator
    End Class
End Namespace
