Imports Microsoft.VisualBasic.Language.JavaScript

Namespace Layouts.Cola

    Public Interface Indexed
        Property id As Integer
    End Interface

    Public Interface IGroup(Of TGroups, TLeaves) : Inherits Indexed
        Property groups As List(Of TGroups)
        Property leaves As List(Of TLeaves)
    End Interface

    Public Class IndexGroup : Inherits JavaScriptObject
        Implements IGroup(Of Integer, Integer)

        Public Property leaves As List(Of Integer) Implements IGroup(Of Integer, Integer).leaves
        Public Property groups As List(Of Integer) Implements IGroup(Of Integer, Integer).groups
        Public Property id As Integer Implements IGroup(Of Integer, Integer).id
        Public Property padding As Double
        Public Property index As Integer
    End Class

    Public Class Group : Inherits Node
        Implements IGroup(Of Group, Node)

        Public Property groups As List(Of Group) Implements IGroup(Of Group, Node).groups
        Public Property leaves As List(Of Node) Implements IGroup(Of Group, Node).leaves
        Public Property padding As Double?

        Public Shared Function isGroup(g As Group) As Boolean
            Return g.leaves IsNot Nothing OrElse g.groups IsNot Nothing
        End Function
    End Class

End Namespace