#Region "Microsoft.VisualBasic::1b6696441a2046b70e2472e46d333064, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Tree\ITreeNode.vb"

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

    '     Interface ITreeNode
    ' 
    '         Properties: ChildNodes, FullyQualifiedName, IsLeaf, IsRoot, Parent
    ' 
    '         Function: GetRootNode, IteratesAllChilds
    ' 
    '         Sub: ChildCountsTravel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataStructures.Tree

    Public Interface ITreeNode(Of T)

        Property Parent() As T
        ''' <summary>
        ''' Children
        ''' </summary>
        Property ChildNodes() As List(Of T)

        ReadOnly Property FullyQualifiedName() As String
        ''' <summary>
        ''' Is this node have no childs
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsLeaf() As Boolean
        ''' <summary>
        ''' I this node have no parents
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsRoot() As Boolean

        Function GetRootNode() As T
        Function IteratesAllChilds() As IEnumerable(Of T)
        Sub ChildCountsTravel(distribute As Dictionary(Of String, Double), Optional getID As Func(Of T, String) = Nothing)

    End Interface
End Namespace
