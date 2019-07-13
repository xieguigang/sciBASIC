#Region "Microsoft.VisualBasic::cf604f709a0ada068726574cd3cccdda, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Tree\BinaryTree\Abstract.vb"

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

    '     Class TreeMap
    ' 
    '         Properties: Key, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures.BinaryTree

    Public MustInherit Class TreeMap(Of K, V)
        Implements IKeyedEntity(Of K)
        Implements Value(Of V).IValueOf
        Implements IComparable(Of K)

        Public Property Key As K Implements IKeyedEntity(Of K).Key
        Public Property value As V Implements Value(Of V).IValueOf.Value

        Public MustOverride Function CompareTo(other As K) As Integer Implements IComparable(Of K).CompareTo

    End Class
End Namespace
