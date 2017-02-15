Namespace ComponentModel.DataStructures.Tree

    ''' <summary>
    ''' Generic Tree Node base class
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>https://www.codeproject.com/Articles/345191/Simple-Generic-Tree</remarks>
    Public MustInherit Class TreeNodeBase(Of T As {
                                              Class, ITreeNode(Of T)
                                          })
        Implements ITreeNode(Of T)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        Protected Sub New(name As String)
            Me.Name = name
            ChildNodes = New List(Of T)()
        End Sub

        ''' <summary>
        ''' Name
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' Parent
        ''' </summary>
        Public Property Parent() As T Implements ITreeNode(Of T).Parent

        ''' <summary>
        ''' Children
        ''' </summary>
        Public Property ChildNodes() As List(Of T)

        ''' <summary>
        ''' Me/this
        ''' </summary>
        Public MustOverride ReadOnly Property MySelf() As T

        ''' <summary>
        ''' True if a Leaf Node
        ''' </summary>
        Public ReadOnly Property IsLeaf() As Boolean Implements ITreeNode(Of T).IsLeaf
            Get
                Return ChildNodes.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' True if the Root of the Tree
        ''' </summary>
        Public ReadOnly Property IsRoot() As Boolean Implements ITreeNode(Of T).IsRoot
            Get
                Return Parent Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' List of Leaf Nodes
        ''' </summary>
        Public Function GetLeafNodes() As List(Of T)
            Return ChildNodes.Where(Function(x) x.IsLeaf).ToList()
        End Function

        ''' <summary>
        ''' List of Non Leaf Nodes
        ''' </summary>
        Public Function GetNonLeafNodes() As List(Of T)
            Return ChildNodes.Where(Function(x) Not x.IsLeaf).ToList()
        End Function

        ''' <summary>
        ''' Get the Root Node of the Tree
        ''' </summary>
        Public Function GetRootNode() As T Implements ITreeNode(Of T).GetRootNode
            If Parent Is Nothing Then
                Return MySelf
            End If

            Return Parent.GetRootNode()
        End Function

        ''' <summary>
        ''' Dot separated name from the Root to this Tree Node
        ''' </summary>
        Public ReadOnly Property FullyQualifiedName() As String Implements ITreeNode(Of T).FullyQualifiedName
            Get
                If Parent Is Nothing Then
                    Return Name
                End If

                Return String.Format("{0}.{1}", Parent.FullyQualifiedName(), Name)
            End Get
        End Property

        ''' <summary>
        ''' Add a Child Tree Node
        ''' </summary>
        ''' <param name="child"></param>
        Public Sub AddChild(child As T)
            child.Parent = MySelf
            ChildNodes.Add(child)
        End Sub

        ''' <summary>
        ''' Add a collection of child Tree Nodes
        ''' </summary>
        ''' <param name="children"></param>
        Public Sub AddChildren(children As IEnumerable(Of T))
            For Each child As T In children
                AddChild(child)
            Next
        End Sub
    End Class
End Namespace