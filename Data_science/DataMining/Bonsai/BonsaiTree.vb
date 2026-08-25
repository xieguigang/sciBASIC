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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

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
    Public root As BonsaiNode

    ''' <summary>
    ''' Dimensionality of the input points.
    ''' </summary>
    Public ReadOnly nGenes As Integer

    ' Box bounds for the log-time optimisation (mirrors scipy L-BFGS-B bounds in optTimes)
    Private Shared ReadOnly tLB As Double = System.Math.Log(0.000001)
    Private Shared ReadOnly tUB As Double = System.Math.Log(1000000.0)

    Private maxNodeInd As Integer = -1

    ''' <summary>
    ''' Master switch for the local rearrangement + re-rooting passes added alongside the NNI / SPR search.
    ''' Set to False to reproduce the pre-local-search build (useful for debugging / comparison).
    ''' </summary>
    Public Shared enableLocalSearch As Boolean = True

    ''' <summary>
    ''' Optional per-dimension diffusion scaling (global gene variance prior v_g). When set, every internal
    ''' node carries it so the transition variance becomes vars(g) + v_g(g)*tParent. Null reproduces the
    ''' default measurement-error-only behaviour. Populated from a <see cref="PointSet"/> when
    ''' <see cref="PointSet.useGlobalVariance"/> is on.
    ''' </summary>
    Private diffusionScale As Double() = Nothing

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
    '''  4. Resolve un-bifurcated star nodes with likelihood-optimal pairing (mergeZeroTimeChilds).
    '''  5. Local rearrangement search: NNI (random + greedy) then SPR (PerformNNI / PerformSPR).
    '''  6. Final edge/time optimisation (optTimes).
    '''  7. Dynamic re-rooting to the edge with the smallest internal-distance split (RerootToMinInternalDist).
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
            If verbose Then
                Console.WriteLine($"  merge round {merges}: {tree.root.childs.Count} root-children remaining, logL = {tree.calcLogLComplete():G4}")
            End If
            If maxMerges > 0 AndAlso merges >= maxMerges Then Exit Do
        Loop

        tree.mergeZeroTimeChilds(verbose)
        If verbose Then Console.WriteLine($"  [diag] leafs after mergeZeroTime = {tree.root.getLeafs().Count}")

        ' Local rearrangement to escape the greedy-merge local optimum (Bonsai paper: NNI then SPR).
        If enableLocalSearch Then
            tree.PerformNNI(randomPhase:=True, maxRounds:=3, verbose:=verbose)
            If verbose Then Console.WriteLine($"  [diag] leafs after NNI-rand = {tree.root.getLeafs().Count}")
            tree.PerformSPR(maxRounds:=2, verbose:=verbose)
            If verbose Then Console.WriteLine($"  [diag] leafs after SPR = {tree.root.getLeafs().Count}")
            tree.PerformNNI(randomPhase:=False, maxRounds:=5, verbose:=verbose)
            If verbose Then Console.WriteLine($"  [diag] leafs after NNI-greedy = {tree.root.getLeafs().Count}")
        End If

        tree.optTimes(maxiter, verbose)

        ' Choose a biologically meaningful root via the unsupervised first-split criterion.
        If enableLocalSearch Then
            tree.RerootToMinInternalDist(verbose)
            If verbose Then Console.WriteLine($"  [diag] leafs after reroot = {tree.root.getLeafs().Count}")
        End If

        If verbose Then
            Console.WriteLine($"Bonsai: final logL = {tree.calcLogLComplete():G4}, nodes = {tree.CountNodes()}")
        End If

        Return tree
    End Function

    ''' <summary>
    ''' Initialise a star tree: one root with every sample as a direct leaf child. Each leaf carries the
    ''' observed mean/var; the root is initialised through the star combination. When the point set enables
    ''' the global-variance prior, every node is tagged with the per-dimension diffusion scale v_g.
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
        Dim tree = New BonsaiTree(root, D) With {.maxNodeInd = ind - 1}
        If data.useGlobalVariance Then
            tree.diffusionScale = data.geneVariance
            For Each nd In tree.GetAllNodes()
                nd.diffusionScale = data.geneVariance
            Next
        End If
        Return tree
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

        If bestD <= 0.000001 OrElse bestI < 0 Then
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
        Dim lv_gi(D - 1)() As Double
        For g = 0 To D - 1
            ltqs_gi(g) = New Double() {child1.ltqs(g), child2.ltqs(g), ltqsR(g)}
            lv_gi(g) = New Double() {vars1(g), vars2(g), 1.0 / (WR_g(g) + Likelihood.EPS)}
        Next
        Dim t0 = New Double() {child1.tParent, child2.tParent, 1.0}
        Dim o3 = Likelihood.optimiseT3LeafStar(ltqs_gi, lv_gi, t0)
        Dim tOpt = o3.tOpt
        Dim success = o3.success
        If Not success Then
            Return
        End If

        Dim t1 = tOpt(0), t2 = tOpt(1), tar = tOpt(2)

        ' Decide whether we attach one child directly under the other (collapsed time)
        If t1 < 0.000001 Then
            AttachAsChild(child2, child1, t2, tar, verbose)
        ElseIf t2 < 0.000001 Then
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
            .par = root,
            .diffusionScale = Me.diffusionScale
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

    ''' <summary>
    ''' Temporarily group children i and j of <paramref name="node"/> under a fresh zero-time internal node
    ''' (the layout used to resolve a multifurcation). The change is applied in place; call <see cref="UndoPair"/>
    ''' to revert it. Only valid while the children have ~zero branch time (the multifurcation case).
    ''' </summary>
    Private Sub ApplyPair(node As BonsaiNode, i As Integer, j As Integer, ByRef midNode As BonsaiNode)
        Dim a = node.childs(i)
        Dim b = node.childs(j)
        ' Remove the higher index first so the lower index stays valid.
        node.childs.RemoveAt(j)
        node.childs.RemoveAt(i)

        maxNodeInd += 1
        midNode = New BonsaiNode With {
            .nodeInd = maxNodeInd,
            .nodeId = "N" & maxNodeInd,
            .isLeaf = False,
            .par = node,
            .tParent = 0.0,
            .diffusionScale = Me.diffusionScale
        }
        a.tParent = 0.0 : b.tParent = 0.0
        a.par = midNode : b.par = midNode
        midNode.childs.Add(a)
        midNode.childs.Add(b)
        midNode.getLtqsUponMerge()

        node.childs.Add(midNode)
    End Sub

    ''' <summary>
    ''' Revert a pairing applied by <see cref="ApplyPair"/>, restoring a and b as direct children of node.
    ''' </summary>
    Private Sub UndoPair(node As BonsaiNode, a As BonsaiNode, b As BonsaiNode, midNode As BonsaiNode)
        node.childs.Remove(midNode)
        a.tParent = 0.0 : b.tParent = 0.0
        a.par = node : b.par = node
        node.childs.Add(a)
        node.childs.Add(b)
        maxNodeInd -= 1
    End Sub

    ''' <summary>
    ''' Recursively split any internal node that has more than two children but all-zero branch times back
    ''' into proper bifurcations. Unlike the previous blind pairing of the first two children, we now
    ''' enumerate every pair of children and keep the pairing that maximises the complete tree
    ''' log-likelihood (the paper treats the multifurcating node as a local root and re-runs the
    ''' likelihood-maximising addition). Mirrors ``TreeNode.mergeZeroTimeChilds``.
    ''' </summary>
    Private Sub splitZeroTime(node As BonsaiNode)
        If node.isLeafNode() Then Return

        While node.childs.Count > 2
            Dim bestI = -1, bestJ = -1, bestLL = -Double.MaxValue
            Dim n = node.childs.Count
            For i = 0 To n - 2
                For j = i + 1 To n - 1
                    Dim mid As BonsaiNode = Nothing
                    ApplyPair(node, i, j, mid)
                    Dim ll = calcLogLComplete()
                    Dim a = mid.childs(0), b = mid.childs(1)
                    UndoPair(node, a, b, mid)
                    If ll > bestLL Then
                        bestLL = ll
                        bestI = i : bestJ = j
                    End If
                Next
            Next

            ' Permanently apply the likelihood-optimal pairing for this round.
            Dim finalMid As BonsaiNode = Nothing
            ApplyPair(node, bestI, bestJ, finalMid)
        End While

        For Each child In node.childs
            splitZeroTime(child)
        Next
    End Sub

    ' =================================================================================
    ' Local rearrangement: NNI (nearest-neighbour interchange)
    ' =================================================================================

    ''' <summary>
    ''' Saved topology state for an NNI move so it can be reverted exactly when the move does not improve
    ''' the likelihood.
    ''' </summary>
    Private Structure NNIState
        Public A As BonsaiNode, C As BonsaiNode, S As BonsaiNode, P As BonsaiNode
        Public tA As Double, tC As Double, tS As Double
        Public Pchilds As System.Collections.Generic.List(Of BonsaiNode), Achilds As System.Collections.Generic.List(Of BonsaiNode)
        Public Cpar As BonsaiNode, Spar As BonsaiNode, Apar As BonsaiNode
    End Structure

    Private Function SaveNNI(A As BonsaiNode, C As BonsaiNode, S As BonsaiNode, P As BonsaiNode) As NNIState
        Return New NNIState With {
            .A = A, .C = C, .S = S, .P = P,
            .tA = A.tParent, .tC = C.tParent, .tS = S.tParent,
            .Pchilds = New System.Collections.Generic.List(Of BonsaiNode)(P.childs),
            .Achilds = New System.Collections.Generic.List(Of BonsaiNode)(A.childs),
            .Cpar = C.par, .Spar = S.par, .Apar = A.par
        }
    End Function

    Private Sub RestoreNNI(st As NNIState)
        st.P.childs.Clear() : st.P.childs.AddRange(st.Pchilds)
        st.A.childs.Clear() : st.A.childs.AddRange(st.Achilds)
        st.A.tParent = st.tA : st.C.tParent = st.tC : st.S.tParent = st.tS
        st.C.par = st.Cpar : st.S.par = st.Spar : st.A.par = st.Apar
    End Sub

    ''' <summary>
    ''' Apply one NNI exchange around node A: move child C of A up to A's parent P, and bring A's sibling S
    ''' down under A. The move is self-inverse, so re-applying it reverts the topology.
    ''' </summary>
    Private Sub ApplyNNI(A As BonsaiNode, C As BonsaiNode, S As BonsaiNode, P As BonsaiNode)
        ' Nearest-neighbour interchange around edge (P, A): the edge itself is NOT broken, A stays a child of
        ' P. We swap one of A's children (C) up to P with A's sibling S (which moves down under A).
        '   - C leaves A and becomes a child of P.
        '   - S leaves P and becomes a child of A.
        P.childs.Add(C)
        C.par = P
        C.tParent = A.tParent

        P.childs.Remove(S)
        A.childs.Remove(C)
        A.childs.Add(S)
        S.par = A
        S.tParent = C.tParent
    End Sub

    ''' <summary>
    ''' Try every NNI exchange incident to <paramref name="A"/> (each child C swapped with A's sibling S) and
    ''' keep the one that most improves the complete log-likelihood. Returns true when a beneficial move was
    ''' applied. Mirrors the greedy NNI phase of the Bonsai local search.
    ''' </summary>
    Private Function TryNNI(A As BonsaiNode, maxiter As Integer, verbose As Boolean) As Boolean
        If A.isLeafNode() OrElse A.isRootNode() Then Return False
        Dim P = A.par
        If P.isRootNode() AndAlso P.childs.Count < 2 Then Return False
        ' The sibling S is any child of P other than A.
        Dim S = P.childs.Where(Function(c) c IsNot A).FirstOrDefault()
        If S Is Nothing Then Return False
        If A.childs.Count < 1 Then Return False

        Dim baseLL = calcLogLComplete()
        Dim baseLeafCount = root.getLeafs().Count
        Dim bestGain = 0.000001
        Dim bestC As BonsaiNode = Nothing
        Dim bestState As NNIState = Nothing

        For Each cnode In A.childs.ToArray
            Dim st = SaveNNI(A, cnode, S, P)
            ApplyNNI(A, cnode, S, P)
            Dim ll = calcLogLComplete()
            If ll - baseLL > bestGain Then
                bestGain = ll - baseLL
                bestC = cnode
                ' Keep this state as the candidate; revert for now and re-apply the best afterwards.
                bestState = st
            End If
            RestoreNNI(st)
            If verbose AndAlso root.getLeafs().Count <> baseLeafCount Then
                Console.WriteLine($"    [diag] RESTORE FAILED on A{A.nodeInd}: leaves {root.getLeafs().Count} != base {baseLeafCount}")
            End If
        Next

        If bestC Is Nothing Then Return False

        ' Re-apply the winning exchange and refine the affected branch times.
        ApplyNNI(A, bestC, S, P)
        calcLogLComplete()
        optTimes(maxiter, verbose)
        If verbose Then
            Console.WriteLine($"  NNI accepted: gain = {bestGain:G4}, logL = {calcLogLComplete():G4}")
        End If
        Return True
    End Function

    ''' <summary>
    ''' Nearest-neighbour interchange local search. When <paramref name="randomPhase"/> is true the internal
    ''' nodes are visited in a random order (the stochastic NNI phase); otherwise the order is deterministic
    ''' (the greedy NNI phase). Repeats until a full sweep makes no improvement or <paramref name="maxRounds"/>
    ''' is reached, exactly as described in the Bonsai paper.
    ''' </summary>
    Public Sub PerformNNI(Optional randomPhase As Boolean = True, Optional maxRounds As Integer = 3, Optional verbose As Boolean = False, Optional maxiter As Integer = 10)
        Dim rng = New Random(If(randomPhase, Guid.NewGuid().GetHashCode(), 12345))
        Dim rounds = 0
        Do
            rounds += 1
            Dim internals = GetAllNodes() _
                .Where(Function(n) Not n.isLeafNode() AndAlso Not n.isRootNode()) _
                .ToList()
            If randomPhase Then
                ' Fisher-Yates shuffle for a stochastic sweep.
                For i = internals.Count - 1 To 1 Step -1
                    Dim k = rng.Next(i + 1)
                    Dim tmp = internals(i)
                    internals(i) = internals(k)
                    internals(k) = tmp
                Next
            End If

            Dim improved = False
            For Each A In internals
                If TryNNI(A, maxiter, verbose) Then
                    improved = True
                    If verbose Then
                        Console.WriteLine($"    [diag] after NNI on A{A.nodeInd}: totalNodes={GetAllNodes().Count} leafs={root.getLeafs().Count}")
                    End If
                End If
            Next
            If Not improved OrElse rounds >= maxRounds Then Exit Do
        Loop
    End Sub

    ' =================================================================================
    ' Local rearrangement: SPR (subtree prune and regraft)
    ' =================================================================================

    ''' <summary>
    ''' Saved topology state for an SPR move (prune subtree X, regraft onto edge E).
    ''' </summary>
    Private Structure SPRState
        Public X As BonsaiNode, Epar As BonsaiNode, Echild As BonsaiNode
        Public tX As Double, tEchild As Double
        Public EparChilds As System.Collections.Generic.List(Of BonsaiNode), EchildChilds As System.Collections.Generic.List(Of BonsaiNode)
        Public Xpar As BonsaiNode, EchildPar As BonsaiNode
        Public newMid As BonsaiNode
    End Structure

    ''' <summary>
    ''' Prune the subtree rooted at X (X must not be the root) and regraft it onto the middle of the edge
    ''' (Epar -> Echild), creating a new zero-time internal node. Returns the saved state for exact revert.
    ''' </summary>
    Private Function ApplySPR(X As BonsaiNode, Epar As BonsaiNode, Echild As BonsaiNode) As SPRState
        Dim st As New SPRState With {
            .X = X, .Epar = Epar, .Echild = Echild,
            .tX = X.tParent, .tEchild = Echild.tParent,
            .EparChilds = New System.Collections.Generic.List(Of BonsaiNode)(Epar.childs),
            .EchildChilds = New System.Collections.Generic.List(Of BonsaiNode)(Echild.childs),
            .Xpar = X.par, .EchildPar = Echild.par
        }

        ' Detach X from its parent.
        X.par.childs.Remove(X)

        ' Split the target edge Epar->Echild with a fresh internal node.
        maxNodeInd += 1
        Dim mid = New BonsaiNode With {
            .nodeInd = maxNodeInd,
            .nodeId = "N" & maxNodeInd,
            .isLeaf = False,
            .par = Epar,
            .tParent = Echild.tParent,
            .diffusionScale = Me.diffusionScale
        }
        Epar.childs.Remove(Echild)
        Epar.childs.Add(mid)
        Echild.par = mid
        Echild.tParent = 0.0
        mid.childs.Add(Echild)

        ' Attach the pruned subtree X under the new internal node.
        X.par = mid
        X.tParent = 0.0
        mid.childs.Add(X)

        mid.getLtqsUponMerge()
        st.newMid = mid
        Return st
    End Function

    Private Sub RestoreSPR(st As SPRState)
        Dim mid = st.newMid
        ' Remove mid: reattach Echild directly under Epar and X back to its original parent.
        st.Epar.childs.Clear()
        st.Epar.childs.AddRange(st.EparChilds)
        st.Echild.childs.Clear()
        st.Echild.childs.AddRange(st.EchildChilds)
        st.Echild.par = st.EchildPar
        st.Echild.tParent = st.tEchild
        st.X.par = st.Xpar
        st.X.tParent = st.tX
        ' Reattach X into its original parent's child list (it was removed by ApplySPR).
        If Not st.Xpar.childs.Contains(st.X) Then
            st.Xpar.childs.Add(st.X)
        End If
        maxNodeInd -= 1
    End Sub

    ''' <summary>
    ''' Subtree prune-and-regraft local search. Every non-root subtree X is pruned and re-attached onto every
    ''' eligible edge of the tree (any edge not lying inside X's own subtree), keeping the graft that most
    ''' improves the complete log-likelihood. Runs up to <paramref name="maxRounds"/> times. Mirrors the SPR
    ''' phase of the Bonsai local search.
    ''' </summary>
    Public Sub PerformSPR(Optional maxRounds As Integer = 2, Optional verbose As Boolean = False, Optional maxiter As Integer = 10)
        Dim rounds = 0
        Do
            rounds += 1
            Dim improved = False
            Dim allNodes = GetAllNodes()
            Dim Xcandidates = allNodes.Where(Function(n) Not n.isRootNode() AndAlso Not n.isLeafNode()).ToList()

            For Each X In Xcandidates
                Dim subtree = X.getLeafs().Select(Function(l) l.nodeInd).ToHashSet()
                ' Ancestors of X (so we never regraft onto an edge that lies on X's own root path, which would
                ' sever X from the tree).
                Dim ancestors = New System.Collections.Generic.HashSet(Of Integer)
                Dim anc = X.par
                While anc IsNot Nothing
                    ancestors.Add(anc.nodeInd)
                    anc = anc.par
                End While
                Dim baseLeafCount = root.getLeafs().Count
                Dim baseLL = calcLogLComplete()
                Dim bestGain = 0.000001
                Dim bestEpar As BonsaiNode = Nothing, bestEchild As BonsaiNode = Nothing, bestSt As SPRState = Nothing

                For Each Epar In allNodes
                    If Epar.isLeafNode() Then Continue For
                    ' Copy the child list: ApplySPR mutates Epar.childs, so we must not enumerate it directly.
                    Dim echildren = New System.Collections.Generic.List(Of BonsaiNode)(Epar.childs)
                    For Each Echild In echildren
                        ' Cannot regraft into X's own subtree or onto X's root path (would detach X).
                        If subtree.Contains(Echild.nodeInd) OrElse ancestors.Contains(Echild.nodeInd) Then Continue For
                        Dim st = ApplySPR(X, Epar, Echild)
                        Dim ll = calcLogLComplete()
                        If ll - baseLL > bestGain Then
                            bestGain = ll - baseLL
                            bestEpar = Epar
                            bestEchild = Echild
                            bestSt = st
                        End If
                        RestoreSPR(st)
                        If verbose AndAlso root.getLeafs().Count <> baseLeafCount Then
                            Console.WriteLine($"    [diag] SPR RESTORE FAILED: leaves {root.getLeafs().Count} != base {baseLeafCount}")
                        End If
                    Next
                Next

                If bestEpar IsNot Nothing Then
                    ApplySPR(X, bestEpar, bestEchild)
                    calcLogLComplete()
                    optTimes(maxiter, verbose)
                    improved = True
                    If verbose Then
                        Console.WriteLine($"  SPR accepted: gain = {bestGain:G4}, leafs = {root.getLeafs().Count}")
                    End If
                End If
            Next

            If Not improved OrElse rounds >= maxRounds Then Exit Do
        Loop
    End Sub

    ' =================================================================================
    ' Dynamic re-rooting
    ' =================================================================================

    ''' <summary>
    ''' Re-root the tree onto the edge whose split minimises the sum of within-subtree squared distances
    ''' (the unsupervised "first cut" used by Bonsai). Only the topology pointers and the root marker are
    ''' changed; branch times and node positions are left untouched because the Gaussian-integral likelihood
    ''' is invariant to the choice of root.
    ''' </summary>
    Public Sub RerootToMinInternalDist(Optional verbose As Boolean = False)
        Dim allNodes = GetAllNodes().Where(Function(n) Not n.isRootNode()).ToList()
        If allNodes.Count = 0 Then Return

        ' Precompute each leaf's high-dimensional position and the data centroid.
        Dim leafs = root.getLeafs()
        Dim D = leafs(0).ltqs.Length
        Dim centroid(D - 1) As Double
        For Each lf In leafs
            For g = 0 To D - 1
                centroid(g) += lf.ltqs(g)
            Next
        Next
        For g = 0 To D - 1
            centroid(g) /= leafs.Count
        Next

        Dim bestEdge As BonsaiNode = Nothing
        Dim bestObj = Double.MaxValue

        For Each edge In allNodes
            ' Temporarily treat the edge as a root cut: left side = subtree under edge, right side = the rest.
            Dim leftLeafs = edge.getLeafs()
            Dim rightLeafs = leafs.Where(Function(l) Not leftLeafs.Contains(l)).ToList()
            If leftLeafs.Count = 0 OrElse rightLeafs.Count = 0 Then Continue For

            Dim obj = WithinSS(leftLeafs, centroid, D) + WithinSS(rightLeafs, centroid, D)
            If obj < bestObj Then
                bestObj = obj
                bestEdge = edge
            End If
        Next

        If bestEdge IsNot Nothing AndAlso bestEdge IsNot root Then
            RerootAtEdge(bestEdge)
            If verbose Then
                Console.WriteLine($"  rerooted onto edge above node N{bestEdge.nodeInd}")
            End If
        End If
    End Sub

    ''' <summary>
    ''' Re-root the tree at the edge leading into <paramref name="edge"/>, making that edge's child the new
    ''' root. The path from edge up to the old root is reversed (each node becomes the child of the one
    ''' below it) and the edge is detached from its old parent. Purely a pointer/flag re-arrangement; the
    ''' Gaussian-integral likelihood is invariant to the root position.
    ''' </summary>
    Private Sub RerootAtEdge(edge As BonsaiNode)
        ' Path from edge up to the current root (path(0) = edge, path(last) = old root).
        Dim path As New List(Of BonsaiNode)
        Dim n = edge
        While n IsNot Nothing
            path.Add(n)
            n = n.par
        End While

        ' Detach the new root from its old parent and mark it as the root.
        edge.par.childs.Remove(edge)
        edge.par = Nothing
        edge.isRoot = True
        edge.tParent = 0.0

        ' Reverse the rest of the path so every node becomes the child of the one below it.
        For i = 1 To path.Count - 1
            Dim child = path(i - 1)
            Dim parent = path(i)
            parent.childs.Remove(child)
            parent.par = child
            parent.tParent = child.tParent
            child.childs.Add(parent)
        Next

        ' The previous root is now an ordinary internal node (any off-path branches stay attached to it).
        If path.Count > 0 Then
            path(path.Count - 1).isRoot = False
        End If

        root = edge
    End Sub

    ''' <summary>
    ''' Sum of squared Euclidean distances from each leaf's position to its own side's mean (a proxy for the
    ''' within-cluster internal distance used to pick the re-rooting cut).
    ''' </summary>
    Private Shared Function WithinSS(leafs As System.Collections.Generic.List(Of BonsaiNode), centroid As Double(), D As Integer) As Double
        If leafs.Count = 0 Then Return 0.0
        Dim mean(D - 1) As Double
        For Each lf In leafs
            For g = 0 To D - 1
                mean(g) += lf.ltqs(g)
            Next
        Next
        For g = 0 To D - 1
            mean(g) /= leafs.Count
        Next
        Dim ss = 0.0
        For Each lf In leafs
            For g = 0 To D - 1
                Dim delta = lf.ltqs(g) - mean(g)
                ss += delta * delta
            Next
        Next
        Return ss
    End Function

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

