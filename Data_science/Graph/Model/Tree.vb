Public Class Tree(Of T): inherits Vertex

    Public Property Childs As Tree(Of T)()
	Public Property Parent As Tree(Of T)
	Public Property Data As T
	
	Public ReadOnly Property Count As Integer
	Get 
	If Childs.isNullorEmpty then 
	return 1  ' 自己算一个节点，所以数量总是1的
	else 
	Dim n% = Childs.Length	
	
	for each node in childs
		n += node .count ' 如果节点没有childs，则会返回1，因为他自身就是一个节点
	next 
	
	return n
	end if 
	
	End Get
	End Property
	
	Public ReadOnly Property QualifyName As String 
Get
If Not Parent Is nothing then
Return Parent.QualifyName & "." & Label
Else 
Return Label
end if 
End Get
End Property	

Public ReadOnly Property IsRoot As Boolean 
Get 
return Parent Is Nothing
End Get
End property

Public ReadOnly Property isLeaf As Boolean 
get
Return Childs.IsNullOrEmpty
end get 
end property

Public Overrides Function ToString As String 
Return QualifyName
End Function
End Class

Public Class BinaryTree(Of T As IComparable(Of T)) : Inherits Tree(Of T)

Public Property Left As BinaryTree(Of T)
Get 
Return Childs.ElementAtOrDefault(0)
End Get
Set 
If Childs.IsNullOrEmpty Then 
Childs = {value, nothing}
Else 
Childs(0) = value
End If
End Set
End Property

Public Property Right As BinaryTree(of T)
Get 
Return Childs.ElementAtOrDefault(1)
End Get
Set 
If Childs.IsNullOrEmpty OrElse Childs.Length < 2 Then 
Childs = {nothing ,value}
Else 
Childs(1) = value
End If
End Set
End Property

Const Duplicated$ = "Duplicated node was found!"

        ''' <summary>
        ''' Recursively locates an empty slot in the binary tree and inserts the node
        ''' </summary>
        ''' <param name="node"></param>
        ''' <param name="tree"></param>
        ''' <param name="[overrides]">
        ''' 0不复写，函数自动处理
        ''' &lt;0  LEFT
        ''' >0 RIGHT
        ''' </param>
        Private Sub InternalAdd(node As BinaryTree(Of T), ByRef tree As BinaryTree(Of T), overrides%)
            If tree Is Nothing Then
                tree = node
            Else
                ' If we find a node with the same name then it's 
                ' a duplicate and we can't continue
                Dim comparison%

                If [overrides] = 0 Then
                    comparison = Node.Data.CompareTo(tree.Data)
					
                    If comparison = 0 Then
                        Throw New Exception(Duplicated)
                    End If
                Else
                    comparison = [overrides]
                End If

				' 在这里进行递归的比较查找，直到二叉树节点是一个空节点为止
                If comparison < 0 Then
                   Call InternalAdd(node, tree.Left, comparison)
                Else
                  Call InternalAdd(node, tree.Right, comparison)
                End If
            End If
        End Sub

		        ''' <summary>
        ''' Add a symbol to the tree if it's a new one. Returns reference to the new
        ''' node if a new node inserted, else returns null to indicate node already present.
        ''' </summary>
        ''' <param name="name">Name of node to add to tree</param>
        ''' <param name="d">Value of node</param>
        ''' <returns> Returns reference to the new node is the node was inserted.
        ''' If a duplicate node (same name was located then returns null</returns>
        Public Function Insert(name$, data As T) As BinaryTree(Of T)
            Dim node As New BinaryTree(Of T) With { .Label = name, .Data = d, .ID = Count}
            Try
               Call InternalAdd(node, Me, 0)
                Return node
            Catch e As Exception
                Dim ex = New Exception(node.ToString, e)
                Return App.LogException(ex)
            End Try
        End Function
End Class