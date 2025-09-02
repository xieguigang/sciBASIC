Namespace ShapleyValue.TreeShape

    ''' <summary>
    ''' See https://arxiv.org/pdf/1802.03888.pdf - Chapter 3.1, Algorithm 1.
    ''' 
    ''' v - vector of node values; = "internal" for internal nodes
    ''' a,b - left and right node indexes for each internal node
    ''' t - thresholds for each internal node
    ''' d - indexes of the features used for splitting in internal nodes
    ''' r - cover of each node (ie. how many data samples fall in that sub-tree)
    ''' w - weight, measures the proportion of the training samples matching the conditioning set S fall into each leaf
    ''' s - set of non-zero indexes in z', ie. known features
    ''' z' - for each feature, 0 if unknown, 1 if known
    ''' x - feature values
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/pkozelka/treeshap
    ''' </remarks>
    Friend Class ShapAlgo1
        Friend x As Double()
        Friend v As Double()
        Friend t As Double()
        Friend r As Single()
        Friend a As Integer()
        Friend b As Integer()
        Friend d As Integer()

        ''' <summary>
        ''' sincerely, I have no idea what this thing computes; I expected one computed contribution
        ''' weight per feature, and instead, this computes just one number.
        ''' Probably needs to be called repeatedly; more study of the paper needed here.
        ''' </summary>
        Friend Overridable Function expValue() As Double
            Return g(1, 1)
        End Function

        Friend Overridable Function g(j As Integer, w As Double) As Double
            If isLeaf(v(j)) Then
                Return w * v(j)
            End If

            Dim value = x(d(j))
            If Not Double.IsNaN(value) Then
                Return If(value <= t(j), g(a(j), w), g(b(j), w))
            End If

            Return g(a(j), w * r(a(j)) / r(j)) + g(b(j), w * r(b(j)) / r(j))
        End Function


        Private Shared Function isLeaf(leafValue As Double) As Boolean
            Return Not Double.IsNaN(leafValue)
        End Function

        Friend Overridable Function expValue(tree As PkTree) As Double
            Return g(tree.root, 1)
        End Function

        Friend Overridable Function g(node As PkNode, w As Double) As Double
            If node.LeafProp Then
                Return w * node.leafValue
            End If

            Dim value = x(node.splitFeatureIndex)
            If Not Double.IsNaN(value) Then
                ' follow decision path
                Return If(value <= node.splitValue, g(node.yes, w), g(node.no, w))
            End If

            ' take weighted average of both paths
            Return g(node.yes, w * node.yes.dataCount / node.dataCount) + g(node.no, w * node.no.dataCount / node.dataCount)
        End Function

    End Class

End Namespace
