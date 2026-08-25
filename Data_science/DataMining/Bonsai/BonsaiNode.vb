' /********************************************************************************/

'   Author:
'
'       xie (genetics@smrucc.org)
'
'   Copyright (c) 2026 GPL3 Licensed
'
'
'   GNU GENERAL PUBLIC LICENSE (GPL3)
'
'
'   This program is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
'
'   This program is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.
'
'   You should have received a copy of the GNU General Public License
'   along with this program. If not, see <http://www.gnu.org/licenses/>.

' /********************************************************************************/

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports System.Runtime.CompilerServices

Namespace Microsoft.VisualBasic.DataMining.Bonsai

    ''' <summary>
    ''' A node in the Bonsai tree. This is a faithful translation of the python ``TreeNode`` class,
    ''' but with all single-cell semantics removed. The position of a node is described by a
    ''' per-dimension effective coordinate (<see cref="ltqs"/>) and a per-dimension variance
    ''' (<see cref="ltqsVars"/>); ``W_g = 1 / ltqsVars`` is the precision. Branches are described by
    ''' a diffusion time <see cref="tParent"/> measuring the distance to the parent.
    ''' </summary>
    ''' <remarks>
    ''' Every computation in Bonsai is factorised over dimensions: <see cref="ltqs"/> and
    ''' <see cref="ltqsVars"/> are plain D-length vectors, never full matrices. This keeps the
    ''' translation numerically identical to the reference implementation while being fast.
    ''' </remarks>
    Public Class BonsaiNode

        ''' <summary>
        ''' Unique node index, assigned by the tree builder.
        ''' </summary>
        Public nodeInd As Integer

        ''' <summary>
        ''' Stable identifier used for Newick export.
        ''' </summary>
        Public nodeId As String

        ''' <summary>
        ''' Diffusion time to the parent node (edge length). Null for the root.
        ''' </summary>
        Public tParent As Double

        ''' <summary>
        ''' Parent node.
        ''' </summary>
        Public par As BonsaiNode

        ''' <summary>
        ''' Child nodes.
        ''' </summary>
        Public childs As New List(Of BonsaiNode)

        ''' <summary>
        ''' True for a leaf (an observed sample).
        ''' </summary>
        Public isLeaf As Boolean

        ''' <summary>
        ''' True for the root of the tree.
        ''' </summary>
        Public isRoot As Boolean

        ''' <summary>
        ''' Effective per-dimension coordinate of the node (mean), length = D.
        ''' </summary>
        Public ltqs As Double()

        ' W_g = 1 / ltqsVars. We store both but keep them consistent: updating one invalidates the other.
        Private _ltqsVars As Double()
        Private _W_g As Double()

        ''' <summary>
        ''' Log-likelihood prefactor accumulated from downstream subtrees (used by calcLogLComplete).
        ''' </summary>
        Public prefactor As Double

        ''' <summary>
        ''' Derivative of the total tree log-likelihood w.r.t. the diffusion time to the parent.
        ''' </summary>
        Public dLoglikdtParent As Double

        ''' <summary>
        ''' Number of downstream (data) leaf nodes.
        ''' </summary>
        Public n_ds_nodes As Integer

        Sub New(Optional nodeInd As Integer = -1,
                Optional childs As List(Of BonsaiNode) = Nothing,
                Optional par As BonsaiNode = Nothing,
                Optional isLeaf As Boolean = False,
                Optional isRoot As Boolean = False,
                Optional ltqs As Double() = Nothing,
                Optional ltqsVars As Double() = Nothing,
                Optional tParent As Double? = Nothing,
                Optional nodeId As String = Nothing)

            Me.nodeInd = nodeInd
            Me.childs = If(childs Is Nothing, New List(Of BonsaiNode), childs)
            Me.par = par
            Me.isLeaf = isLeaf
            Me.isRoot = isRoot
            Me.ltqs = ltqs
            Me.setLtqsVarsOrW(ltqsVars)
            Me.tParent = If(tParent.HasValue, tParent.Value, 0.0)
            Me.nodeId = If(nodeId, "n" & nodeInd)
        End Sub

        ' ----- Position accessors keeping W_g / ltqsVars consistent -----

        Public Function getLtqsVars() As Double()
            If _ltqsVars Is Nothing AndAlso _W_g IsNot Nothing Then
                _ltqsVars = _W_g.Select(Function(w) 1.0 / w).ToArray
            End If
            Return _ltqsVars
        End Function

        Public Function getW() As Double()
            If _W_g Is Nothing AndAlso _ltqsVars IsNot Nothing Then
                _W_g = _ltqsVars.Select(Function(v) 1.0 / v).ToArray
            End If
            Return _W_g
        End Function

        Public Sub setLtqsVarsOrW(Optional ltqsVars As Double() = Nothing, Optional W_g As Double() = Nothing)
            If ltqsVars IsNot Nothing Then
                _ltqsVars = ltqsVars
                _W_g = Nothing
            ElseIf W_g IsNot Nothing Then
                _W_g = W_g
                _ltqsVars = Nothing
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function isLeafNode() As Boolean
            Return childs.IsNullOrEmpty
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function isRootNode() As Boolean
            Return par Is Nothing
        End Function

        ' ----- Tree traversal helpers -----

        ''' <summary>
        ''' Recursively collect all leaf nodes (observed samples) under this node.
        ''' </summary>
        Public Function getLeafs() As List(Of BonsaiNode)
            Dim out As New List(Of BonsaiNode)
            collectLeafs(Me, out)
            Return out
        End Function

        Private Shared Sub collectLeafs(node As BonsaiNode, out As List(Of BonsaiNode))
            If node.isLeafNode() Then
                out.Add(node)
            Else
                For Each c In node.childs
                    collectLeafs(c, out)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Gather (tParent, 1/(ltqsVars+tParent), ltqs, ltqsVars, nodeInd) for this node and its children,
        ''' mirroring python ``getInfo`` / ``getInfoChildren``.
        ''' </summary>
        Public Function getInfo() As (tParent As Double, wbar As Double(), ltqs As Double(), ltqsVars As Double(), nodeInd As Integer)
            Dim vars = getLtqsVars()
            Dim wbar = vars.Select(Function(v) 1.0 / (v + tParent)).ToArray
            Return (tParent, wbar, ltqs, vars, nodeInd)
        End Function

        ''' <summary>
        ''' Recompute the effective position (ltqs) and precision (W_g) of this node by integrating out
        ''' all of its children, exactly mirroring ``findNodeLtqsGivenLeafs``.
        ''' </summary>
        Public Sub getLtqsUponMerge()
            Me.ltqs = Likelihood.findNodeLtqs(Me.childs)
            Me.setLtqsVarsOrW(W_g:=Likelihood.findNodeW(Me.childs))
        End Sub

        ' ----- Newick export -----

        ''' <summary>
        ''' Serialise the subtree rooted at this node to Newick format.
        ''' </summary>
        Public Function toNewick(Optional useIds As Boolean = True) As String
            Dim children As New List(Of String)
            For Each child In childs
                children.Add(child.toNewick(useIds))
            Next

            Dim nwk As String
            If children.Count > 0 Then
                nwk = "(" & String.Join(",", children) & ")"
            Else
                nwk = ""
            End If

            If isRoot Then
                Dim own = If(useIds, nodeId, "N" & nodeInd)
                Return nwk & own & ";"
            Else
                Dim own = If(useIds, nodeId & ":" & tParent.ToString("G6"), "N" & nodeInd & ":" & tParent.ToString("G6"))
                Return nwk & own
            End If
        End Function

        ''' <summary>
        ''' Count the number of data-leaf nodes under this node.
        ''' </summary>
        Public Function countDataLeafs() As Integer
            If isLeafNode() Then
                Return 1
            End If
            Dim n = 0
            For Each c In childs
                n += c.countDataLeafs()
            Next
            Return n
        End Function
    End Class
End Namespace
