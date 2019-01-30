
Namespace Layouts.Cola
    Public Class InputNode
        '*
        '     * index in nodes array, this is initialized by Layout.start()
        '     

        Private index As Integer
        '*
        '     * x and y will be computed by layout as the Node's centroid
        '     

        Private x As Double
        '*
        '     * x and y will be computed by layout as the Node's centroid
        '     

        Private y As Double
        '*
        '     * specify a width and height of the node's bounding box if you turn on avoidOverlaps
        '     

        Private width As Double
        '*
        '     * specify a width and height of the node's bounding box if you turn on avoidOverlaps
        '     

        Private height As Double
        '*
        '     * selective bit mask.  !=0 means layout will not move.
        '     

        Private fixed As Double
    End Class

    Class Node : Inherits InputNode
        ' Client-passed node may be missing these properties, which will be set
        ' upon ingestion
        Private y As Double
        Private x As Double
    End Class

    Class Group
        Private bounds As Rectangle2D
        Private leaves As Node()
        Private groups As Group()
        Private padding As Double

        Public Shared Function isGroup(g As any) As Boolean
            Return g.leaves IsNot Nothing OrElse g.groups IsNot Nothing
        End Function

    End Class


    Interface Link(Of NodeRefType)
        Property source() As NodeRefType
        Property target() As NodeRefType

        ' ideal length the layout should try to achieve for this link
        Property length() As Double

        ' how hard we should try to satisfy this link's ideal length
        ' must be in the range: 0 < weight <= 1
        ' if unspecified 1 is the default
        Property weight() As Double
    End Interface

    Public Delegate Function LinkNumericPropertyAccessor(t As Link(Of Node)) As Double

    Interface LinkLengthTypeAccessor
        Inherits LinkLengthAccessor(Of Link(Of Node))
        Overloads Function [getType]() As LinkNumericPropertyAccessor
    End Interface
End Namespace