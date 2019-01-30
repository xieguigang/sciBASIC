Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class InputNode

        ''' <summary>
        ''' index in nodes array, this is initialized by Layout.start()
        ''' </summary>
        Public index As Integer

        ''' <summary>
        ''' x and y will be computed by layout as the Node's centroid
        ''' </summary>
        Public x As Double

        ''' <summary>
        ''' x and y will be computed by layout as the Node's centroid
        ''' </summary>
        Public y As Double

        ''' <summary>
        ''' specify a width and height of the node's bounding box if you turn on avoidOverlaps
        ''' </summary>
        Public width As Double

        ''' <summary>
        ''' specify a width and height of the node's bounding box if you turn on avoidOverlaps
        ''' </summary>
        Public height As Double

        ''' <summary>
        ''' selective bit mask.  !=0 means layout will not move.
        ''' </summary>
        Public fixed As Double
    End Class

    ''' <summary>
    ''' Client-passed node may be missing these properties, which will be set
    ''' upon ingestion
    ''' </summary>
    Public Class Node : Inherits InputNode

        Public prev As RBNode(Of Node, Object)
        Public [next] As RBNode(Of Node, Object)

        Public r As Rectangle2D
        Public pos As Double

        Public Shared Function makeRBTree() As RBNode(Of Node, Object)
            Return New RBNode(Of Node, Object)(Nothing, Nothing)
        End Function

    End Class

    Public Class Group

        Public bounds As Rectangle2D
        Public leaves As Node()
        Public groups As Group()
        Public padding As Double

        Public Shared Function isGroup(g As Group) As Boolean
            Return g.leaves IsNot Nothing OrElse g.groups IsNot Nothing
        End Function
    End Class


    Public Class Link(Of NodeRefType)

        Public Property source() As NodeRefType
        Public Property target() As NodeRefType

        ''' <summary>
        ''' ideal length the layout should try to achieve for this link 
        ''' </summary>
        ''' <returns></returns>
        Public Property length() As Double

        ''' <summary>
        ''' how hard we should try to satisfy this link's ideal length
        ''' must be in the range: ``0 &lt; weight &lt;= 1``
        ''' if unspecified 1 is the default
        ''' </summary>
        ''' <returns></returns>
        Public Property weight() As Double
    End Class

    Public Delegate Function LinkNumericPropertyAccessor(t As Link(Of Node)) As Double

    Interface LinkLengthTypeAccessor
        Inherits LinkLengthAccessor(Of Link(Of Node))
        Overloads Function [getType]() As LinkNumericPropertyAccessor
    End Interface
End Namespace