#Region "Microsoft.VisualBasic::5bf8c9c86beaff621843d83e451577f1, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\TreeShap\ShapAlgo1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 98
    '    Code Lines: 40 (40.82%)
    ' Comment Lines: 44 (44.90%)
    '    - Xml Docs: 93.18%
    ' 
    '   Blank Lines: 14 (14.29%)
    '     File Size: 3.65 KB


    '     Class ShapAlgo1
    ' 
    '         Function: (+2 Overloads) expValue, (+2 Overloads) g, isLeaf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

        ''' <summary>
        ''' feature values
        ''' </summary>
        Friend x As Double()
        ''' <summary>
        ''' vector of node values; = "internal" for internal nodes
        ''' </summary>
        Friend v As Double()
        ''' <summary>
        ''' thresholds for each internal node
        ''' </summary>
        Friend t As Double()
        ''' <summary>
        ''' cover of each node (ie. how many data samples fall in that sub-tree)
        ''' </summary>
        Friend r As Single()
        ''' <summary>
        ''' left and right node indexes for each internal node
        ''' </summary>
        Friend a As Integer()
        ''' <summary>
        ''' left and right node indexes for each internal node
        ''' </summary>
        Friend b As Integer()
        ''' <summary>
        ''' indexes of the features used for splitting in internal nodes
        ''' </summary>
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

