Namespace ShapleyValue.TreeShape
    Public Class PkNode
        ''' <summary>
        ''' (v) Leaf value. If not leaf, contains NAN.
        ''' </summary>
        Public ReadOnly leafValue As Double

        ''' <summary>
        ''' (t) Split value (threshold). If not split node, contains NAN.
        ''' </summary>
        Public ReadOnly splitValue As Double

        ''' <summary>
        ''' (a) left node
        ''' </summary>
        Public ReadOnly yes As PkNode

        ''' <summary>
        ''' (b) right node
        ''' </summary>
        Public ReadOnly no As PkNode

        ''' <summary>
        ''' (d) index of feature used to split
        ''' </summary>
        Public ReadOnly splitFeatureIndex As Integer

        ''' <summary>
        ''' (r) cover
        ''' </summary>
        Public ReadOnly dataCount As Double

        Private Sub New(dataCount As Double, leafValue As Double, splitFeatureIndex As Integer, splitValue As Double, yes As PkNode, no As PkNode)
            Me.dataCount = dataCount
            Me.leafValue = leafValue
            Me.splitValue = splitValue
            Me.splitFeatureIndex = splitFeatureIndex
            Me.yes = yes
            Me.no = no
        End Sub

        Public Shared Function leaf(dataCount As Double, leafValue As Double) As PkNode
            Return New PkNode(dataCount, leafValue, -1, Double.NaN, Nothing, Nothing)
        End Function

        Public Shared Function split(dataCount As Double, splitFeatureIndex As Integer, splitValue As Double, yes As PkNode, no As PkNode) As PkNode
            Return New PkNode(dataCount, Double.NaN, splitFeatureIndex, splitValue, yes, no)
        End Function

        Public Overridable ReadOnly Property SplitProp As Boolean
            Get
                Return Double.IsNaN(leafValue)
            End Get
        End Property

        Public Overridable ReadOnly Property LeafProp As Boolean
            Get
                Return Not Double.IsNaN(leafValue)
            End Get
        End Property
    End Class

End Namespace
