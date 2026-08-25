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
    ''' The Bonsai tree-reconstruction engine. Given a <see cref="PointSet"/> of high-dimensional observations it
    ''' builds a tree whose non-root edges are diffusion times and whose internal nodes are effective positions,
    ''' then optimises every edge length and internal-node coordinate to maximise the (continuous Felsenstein)
    ''' tree log-likelihood. This is a faithful, biology-free translation of the python ``Tree`` class in
    ''' bonsai_treeHelpers.py.
    ''' </summary>
    Public Class BonsaiTree

        ''' <summary>
        ''' Root of the reconstructed tree.
        ''' </summary>
        Public ReadOnly root As BonsaiNode

        ''' <summary>
        ''' Dimensionality of the input points.
        ''' </summary>
        Public ReadOnly nGenes As Integer

        ' Box bounds for the log-time optimisation (mirrors scipy L-BFGS-B bounds in optTimes)
        Private Shared ReadOnly tLB As Double = System.Math.Log(0.000001)
        Private Shared ReadOnly tUB As Double = System.Math.Log(1000000.0)

        Private maxNodeInd As Integer = -1

        Sub New(root As BonsaiNode, nGenes As Integer)
            Me.root = root
            Me.nGenes = nGenes
        End Sub

        ' =================================================================================
        ' Construction entry point
        ' =================================================================================

        ''' <summary>
        ''' Build a Bonsai tree from a point set. Algorithm (mirrors bonsai_main.buildTree):
        '''  1. Initialise a star tree (all samples directly under the root).
        '''  2. Optimise every branch time (optTimes).
        '''  3. Recursively merge children to increase the log-likelihood (mergeChildrenUB).
        '''  4. Resolve un-bifurcated star nodes (mergeZeroTimeChilds).
        '''  5. Final edge/time optimisation (optTimes).
        ''' </summary>
        Public Shared Function Build(data As PointSet,
                                     Optional maxMerges As Integer = -1,
                                     Optional maxiter As Integer = 20,
                                     Optional verbose As Boolean = False) As BonsaiTree
            Dim tree = InitialiseStarTree(data, verbose)
            tree.optTimes(maxiter, verbose)

            ' Repeatedly merge until no positive-dLogL pair remains (or a cap is reached).
            Dim merges = 0
            Do
                Dim improved = tree.mergeChildrenUB(verbose)
                If Not improved Then Exit Do
                merges += 1
                If maxMerges > 0 AndAlso merges >= maxMerges Then Exit Do
            Loop

            tree.mergeZeroTimeChilds(verbose)
            tree.optTimes(maxiter, verbose)

            If verbose Then
                Console.WriteLine($"Bonsai: final logL = {tree.calcLogLComplete():G4}, nodes = {tree.CountNodes()}")
            End If

            Return tree
        End Function

        ''' <summary>
        ''' Initialise a star tree: one root with every sample as a direct leaf child. Each leaf carries the
        ''' observed mean/var; the root is initialised through the star combination.
        ''' </summary>
        Private Shared Function InitialiseStarTree(data As PointSet, verbose As Boolean) As BonsaiTree
            Dim D = data.nGenes
            Dim root = New BonsaiNode(nodeInd:=0, isRoot:=True) With {.nodeId = "ROOT"}
            root.tParent = 0.0

            Dim ind = 1
            For i = 0 To data.nSamples - 1
                Dim leaf = New BonsaiNode With {
                    .nodeInd = ind,
                    .nodeId = data.names(i),
                    .isLeaf = True,
                    .ltqs = data.GetMean(i),
                    .tParent = 1.0
                }
                leaf.setLtqsVarsOrW(ltqsVars:=data.GetVar(i))
                leaf.par = root
                root.childs.Add(leaf)
                ind += 1
            Next

            root.getLtqsUponMerge()
            Return New BonsaiTree(root, D) With {.maxNodeInd = ind - 1}
        End Function

        ' =================================================================================
        ' Edge / time optimisation (global L-BFGS-B over all branch times)
        ' =================================================================================

        ''' <summary>
        ''' Optimise all non-root branch times simultaneously. Mirrors ``Tree.optTimes``: optimise the times in
        ''' log-space with a bound-constrained L-BFGS, using the analytic gradient from
        ''' <see cref="Likelihood.getDerivativesDownstream"/>. Returns the achieved log-likelihood.
        ''' </summary>
        Public Function optTimes(Optional maxiter As Integer = 20, Optional verbose As Boolean = False) As Double
            Dim allNodes = GetAllNodes()
            ' collect non-root nodes (each defines one branch time)
            Dim branchNodes = allNodes.Where(Function(nd) Not nd.isRootNode()).ToList
            If branchNodes.Count = 0 Then
                Return calcLogLComplete()
            End If

            Dim n = branchNodes.Count
            Dim x0 = branchNodes.Select(Function(nd) System.Math.Log(System.Math.Max(nd.tParent, 0.000001))).ToArray
            Dim bounds As New List(Of (lo As Double, hi As Double))
            For i = 0 To n - 1
                bounds.Add((tLB, tUB))
            Next

            Dim res = Optimizer.Minimize(AddressOf logLGradAllTimes, x0, bounds, root)

            For i = 0 To n - 1
                branchNodes(i).tParent = System.Math.Exp(res.x(i))
            Next

            ' After updating times, refresh internal node positions
            calcLogLComplete()
            Return res.fun
        End Function

        ''' <summary>
        ''' Objective for <see cref="optTimes"/>: negative complete log-likelihood and its gradient w.r.t. each
        ''' branch time (in log-time space). grad_lt_i = t_i * dLoglik/dt_i.
        ''' </summary>
        Private Shared Function logLGradAllTimes(logt As Double(), args() As Object) As (f As Double, grad As Double())
            Dim root = DirectCast(args(0), BonsaiNode)
            Dim allNodes = GetAllNodesFlat(root)
            Dim branchNodes = allNodes.Where(Function(ni) Not ni.isRootNode()).ToList
            Dim n = branchNodes.Count

            ' apply the proposed times
            For i = 0 To n - 1
                branchNodes(i).tParent = System.Math.Exp(logt(i))
            Next

            Dim loglik = Likelihood.calcLogLComplete(root)
            Likelihood.getDerivativesDownstream(root)

            Dim grad(n - 1) As Double
            For i = 0 To n - 1
                ' Minimise (-loglik): gradient of the objective is -dLoglik/d(logt) = -t * dLoglik/dt.
                grad(i) = -branchNodes(i).tParent * branchNodes(i).dLoglikdtParent
            Next
            Return (-loglik, grad)
        End Function

        ' =================================================================================
        ' Tree search: merge the best child pair (UB pruning, full correctness)
        ' =================================================================================

        ''' <summary>
        ''' One round of the tree search: evaluate every candidate pair of children of the root, choose the pair
        ''' with the largest positive dLogL, and merge it. Returns false when no positive-dLogL pair remains
        ''' (the tree can no longer be improved). Mirrors the numeric core of ``Tree.mergeChildrenUB`` (the
        ''' MPI/NNI parallel machinery is intentionally omitted; a full O(C^2) sweep keeps the result exact).
        ''' </summary>
        Public Function mergeChildrenUB(Optional verbose As Boolean = False) As Boolean
            Dim childs = root.childs
            Dim nC = childs.Count
            If nC < 2 Then Return False

            ' Root as-if-root position
            Likelihood.calcLogLComplete(root)
            Dim xrAsIfRoot_g = root.ltqs
            Dim WAsIfRoot_g = root.getW()

            Dim bestD = 0.0
            Dim bestI = -1, bestJ = -1
            For i = 0 To nC - 2
                For j = i + 1 To nC - 1
                    Dim d = Likelihood.calcSingleDLogL(xrAsIfRoot_g, WAsIfRoot_g, childs(i), childs(j))
                    If d > bestD Then
                        bestD = d
                        bestI = i : bestJ = j
                    End If
                Next
            Next

            If bestD <= 1.0E-6 OrElse bestI < 0 Then
                Return False
            End If

            MergePair(childs(bestI), childs(bestJ), xrAsIfRoot_g, WAsIfRoot_g, verbose)
            Return True
        End Function

        ''' <summary>
        ''' Execute the merge of child1 and child2 into a new ancestor (or, when one optimised time collapses to
        ''' ~0, attach the other directly under it). Mirrors ``Tree.mergeNodes``.
        ''' </summary>
        Private Sub MergePair(child1 As BonsaiNode, child2 As BonsaiNode, xrAsIfRoot_g As Double(), WAsIfRoot_g As Double(), verbose As Boolean)
            ' Build the 3-leaf star (child1, child2, rest-of-tree) and optimise the three times
            Dim D = nGenes
            Dim vars1 = child1.getLtqsVars()
            Dim wbar1 = vars1.Select(Function(v) 1.0 / (v + child1.tParent)).ToArray
            Dim rootMinusFirstW = New Double(D - 1) {}
            Dim rootMinusFirstLtqs = New Double(D - 1) {}
            For g = 0 To D - 1
                rootMinusFirstW(g) = WAsIfRoot_g(g) - wbar1(g)
                rootMinusFirstLtqs(g) = xrAsIfRoot_g(g) * WAsIfRoot_g(g) - wbar1(g) * child1.ltqs(g)
            Next
            Dim vars2 = child2.getLtqsVars()
            Dim wbar2 = vars2.Select(Function(v) 1.0 / (v + child2.tParent)).ToArray
            Dim WR_g = New Double(D - 1) {}
            Dim ltqsR = New Double(D - 1) {}
            For g = 0 To D - 1
                WR_g(g) = rootMinusFirstW(g) - wbar2(g)
                ltqsR(g) = (rootMinusFirstLtqs(g) - wbar2(g) * child2.ltqs(g)) / WR_g(g)
            Next

            Dim ltqs_gi(D - 1)() As Double
            ltqs_gi(0) = child1.ltqs : ltqs_gi(1) = child2.ltqs : ltqs_gi(2) = ltqsR
            Dim lv_gi(D - 1)() As Double
            lv_gi(0) = vars1 : lv_gi(1) = vars2 : lv_gi(2) = WR_g.Select(Function(w) 1.0 / w).ToArray
            Dim t0 = New Double() {child1.tParent, child2.tParent, 1.0}
            Dim o3 = Likelihood.optimiseT3LeafStar(ltqs_gi, lv_gi, t0)
            Dim tOpt = o3.tOpt
            Dim success = o3.success
            If Not success Then
                Return
            End If

            Dim t1 = tOpt(0), t2 = tOpt(1), tar = tOpt(2)

            ' Decide whether we attach one child directly under the other (collapsed time)
            If t1 < 1.0E-6 Then
                AttachAsChild(child2, child1, t2, tar, verbose)
            ElseIf t2 < 1.0E-6 Then
                AttachAsChild(child1, child2, t1, tar, verbose)
            Else
                CreateAncestor(child1, child2, t1, t2, tar, verbose)
            End If
        End Sub

        Private Sub AttachAsChild(gchild As BonsaiNode, anc As BonsaiNode, tGC As Double, tAnc As Double, verbose As Boolean)
            ' gchild becomes a child of anc (no new node); anc stays a child of root
            root.childs.Remove(gchild)
            gchild.par = anc
            gchild.tParent = tGC
            anc.childs.Add(gchild)
            anc.tParent = tAnc
            anc.getLtqsUponMerge()
            calcLogLComplete()
        End Sub

        Private Sub CreateAncestor(child1 As BonsaiNode, child2 As BonsaiNode, t1 As Double, t2 As Double, tar As Double, verbose As Boolean)
            maxNodeInd += 1
            Dim anc = New BonsaiNode With {
                .nodeInd = maxNodeInd,
                .nodeId = "N" & maxNodeInd,
                .isLeaf = False,
                .par = root
            }

            root.childs.Remove(child1)
            root.childs.Remove(child2)
            child1.par = anc : child1.tParent = t1
            child2.par = anc : child2.tParent = t2
            anc.childs.Add(child1)
            anc.childs.Add(child2)
            anc.tParent = tar
            anc.getLtqsUponMerge()

            root.childs.Add(anc)
            calcLogLComplete()

            If verbose Then
                Console.WriteLine($"Merged N{child1.nodeInd} and N{child2.nodeInd} into N{anc.nodeInd} (dLogL pair)")
            End If
        End Sub

        ' =================================================================================
        ' Resolve multi-furcating star nodes (split into binary branches)
        ' =================================================================================

        ''' <summary>
        ''' Recursively split any internal node that has more than two children but all-zero branch times back
        ''' into proper bifurcations. Mirrors ``TreeNode.mergeZeroTimeChilds``.
        ''' </summary>
        Public Sub mergeZeroTimeChilds(Optional verbose As Boolean = False)
            splitZeroTime(root)
        End Sub

        Private Sub splitZeroTime(node As BonsaiNode)
            If node.isLeafNode() Then Return

            ' Re-arrange children of node into a binary chain when there are > 2 of them.
            While node.childs.Count > 2
                Dim a = node.childs(0)
                Dim b = node.childs(1)
                node.childs.RemoveAt(0)
                node.childs.RemoveAt(0)

                maxNodeInd += 1
                Dim mid = New BonsaiNode With {
                    .nodeInd = maxNodeInd,
                    .nodeId = "N" & maxNodeInd,
                    .isLeaf = False,
                    .par = node,
                    .tParent = 0.0
                }
                a.tParent = 0.0 : b.tParent = 0.0
                a.par = mid : b.par = mid
                mid.childs.Add(a)
                mid.childs.Add(b)
                mid.getLtqsUponMerge()

                node.childs.Add(mid)
                calcLogLComplete()
            End While

            For Each child In node.childs.ToArray
                splitZeroTime(child)
            Next
        End Sub

        ' =================================================================================
        ' Queries / export
        ' =================================================================================

        ''' <summary>
        ''' Complete tree log-likelihood with all internal positions integrated out.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function calcLogLComplete() As Double
            Return Likelihood.calcLogLComplete(root)
        End Function

        ''' <summary>
        ''' Number of nodes in the tree (internal + leaves).
        ''' </summary>
        Public Function CountNodes() As Integer
            Return GetAllNodesFlat(root).Count
        End Function

        ''' <summary>
        ''' Serialise the tree to Newick format (edge lengths = branch times).
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToNewick() As String
            Return root.toNewick(useIds:=True)
        End Function

        ''' <summary>
        ''' Low-dimensional coordinates for every data sample: the root-relative position of each leaf, expressed
        ''' as the cumulative (summed) branch times along the path from the root, one coordinate per latent
        ''' dimension along the tree (here returned as the D-dimensional effective position of each leaf under the
        ''' optimised tree). Mirrors the "tree coordinates" visualised by Bonsai.
        ''' </summary>
        Public Function GetLowDimCoords() As Double()()
            Dim leafs = root.getLeafs()
            Dim out(leafs.Count - 1)() As Double
            For i = 0 To leafs.Count - 1
                out(i) = leafs(i).ltqs
            Next
            Return out
        End Function

        ''' <summary>
        ''' Branch-time (1-D) coordinate along the tree for every leaf: depth = sum of ancestor branch times.
        ''' Useful for a one-dimensional embedding / pseudotime-like axis.
        ''' </summary>
        Public Function GetBranchTimeCoords() As Double()
            Dim leafs = root.getLeafs()
            Dim out(leafs.Count - 1) As Double
            For i = 0 To leafs.Count - 1
                Dim d = 0.0, n = leafs(i)
                While n.par IsNot Nothing
                    d += n.tParent
                    n = n.par
                End While
                out(i) = d
            Next
            Return out
        End Function

        ' ----- node enumeration helpers -----

        Private Shared Function GetAllNodesFlat(r As BonsaiNode) As List(Of BonsaiNode)
            Dim out As New List(Of BonsaiNode)
            collect(r, out)
            Return out
        End Function

        Private Function GetAllNodes() As List(Of BonsaiNode)
            Return GetAllNodesFlat(root)
        End Function

        Private Shared Sub collect(n As BonsaiNode, out As List(Of BonsaiNode))
            out.Add(n)
            For Each child In n.childs
                collect(child, out)
            Next
        End Sub
    End Class
End Namespace

