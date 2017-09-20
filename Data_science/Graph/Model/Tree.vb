Public Class Tree: inherits Vertex

    Public Property Childs As Tree()
	Public Property Parent As Tree
	
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

Public Class BinaryTree : Inherits Tree

Public Property Left As BinaryTree
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

Public Property Right As BinaryTree
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

End Class