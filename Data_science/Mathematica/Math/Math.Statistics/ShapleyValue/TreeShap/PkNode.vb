#Region "Microsoft.VisualBasic::1d2439e7dd00c11de1db9e415c8fc786, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\TreeShap\PkNode.vb"

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

    '   Total Lines: 63
    '    Code Lines: 34 (53.97%)
    ' Comment Lines: 18 (28.57%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (17.46%)
    '     File Size: 2.12 KB


    '     Class PkNode
    ' 
    '         Properties: LeafProp, SplitProp
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: leaf, split
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
