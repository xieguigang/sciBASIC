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

Imports Microsoft.VisualBasic.Linq

Namespace Microsoft.VisualBasic.DataMining.Bonsai

    ''' <summary>
    ''' Pure numeric core of the Bonsai likelihood model. Everything factorises over dimensions, so the
    ''' methods below operate on D-length vectors and never touch full matrices. This is a direct
    ''' translation of the functions in ``bonsai_treeHelpers.py`` (getLoglikAndGradStarTree,
    ''' findNodeLtqsGivenLeafs, der2LeafTree, optimiseT3LeafStar, getDerivativesDownstream, ...).
    ''' </summary>
    Public Module Likelihood

        ''' <summary>
        ''' eps for numerical stability (mirrors the tiny regulariser used throughout the reference code).
        ''' </summary>
        Public Const EPS As Double = 1.0E-12

        ' =================================================================================
        ' Node-position combination (the Felsenstein pruning step for a star of children)
        ' =================================================================================

        ''' <summary>
        ''' Effective coordinate of the parent integrating out all children:
        '''   wbar_g = 1 / (ltqsVars_g + tParent_g)
        '''   W_g   = sum_i wbar_i
        '''   xr_g  = sum_i (ltqs_i * wbar_i) / W_g
        ''' Mirrors ``findNodeLtqsGivenLeafs`` returning xr_g.
        ''' </summary>
        Public Function findNodeLtqs(childs As List(Of BonsaiNode)) As Double()
            Dim D = childs(0).ltqs.Length
            Dim xr(D - 1) As Double
            Dim W(D - 1) As Double

            For Each child In childs
                Dim vars = child.getLtqsVars()
                For g = 0 To D - 1
                    Dim wbar = 1.0 / (vars(g) + child.tParent)
                    W(g) += wbar
                    xr(g) += child.ltqs(g) * wbar
                Next
            Next
            For g = 0 To D - 1
                xr(g) /= W(g)
            Next
            Return xr
        End Function

        ''' <summary>
        ''' Precision W_g = sum_i 1/(ltqsVars_i + tParent_i) (the complement of <see cref="findNodeLtqs"/>).
        ''' </summary>
        Public Function findNodeW(childs As List(Of BonsaiNode)) As Double()
            Dim D = childs(0).ltqs.Length
            Dim W(D - 1) As Double
            For Each child In childs
                Dim vars = child.getLtqsVars()
                For g = 0 To D - 1
                    W(g) += 1.0 / (vars(g) + child.tParent)
                Next
            Next
            Return W
        End Function

        ' =================================================================================
        ' Star-tree log-likelihood (used at every internal node)
        ' =================================================================================

        ''' <summary>
        ''' Log-likelihood of a star-tree rooted at position xr_g with children given either as node
        ''' objects or as explicit arrays. Mirrors ``getLoglikAndGradStarTree`` (returnGrad=False).
        ''' loglik = sum_g [ sum_i log(wbar_i) - log(W_g) - sum_i wbar_i (xr_g - ltqs_i)^2 ]
        ''' </summary>
        Public Function loglikStarTree(childs As List(Of BonsaiNode), xr_g As Double(), W_g As Double()) As Double
            Dim D = xr_g.Length
            Dim loglik = -VectorSum(VectorLog(W_g))

            For Each child In childs
                Dim vars = child.getLtqsVars()
                For g = 0 To D - 1
                    Dim wbar = 1.0 / (vars(g) + child.tParent)
                    Dim sq = (xr_g(g) - child.ltqs(g)) ^ 2
                    loglik += Math.Log(wbar) - wbar * sq
                Next
            Next
            Return loglik
        End Function

        ''' <summary>
        ''' Log-likelihood + gradient w.r.t. each child's diffusion time, used by the tree-search to
        ''' score candidate merges. Mirrors the gradient branch of ``getLoglikAndGradStarTree``.
        ''' grad[cInd] = sum_g wbar_g (wbar_g * sqDist_g - 1 + wbar_g / W_g)
        ''' </summary>
        Public Function loglikGradStarTree(childs As List(Of BonsaiNode), xr_g As Double(), W_g As Double()) As (loglik As Double, grad As Double())
            Dim D = xr_g.Length
            Dim nC = childs.Count
            Dim grad(nC - 1) As Double
            Dim loglik = -VectorSum(VectorLog(W_g))

            For cInd = 0 To nC - 1
                Dim child = childs(cInd)
                Dim vars = child.getLtqsVars()
                For g = 0 To D - 1
                    Dim wbar = 1.0 / (vars(g) + child.tParent)
                    Dim sq = (xr_g(g) - child.ltqs(g)) ^ 2
                    Dim term = wbar * sq
                    loglik += Math.Log(wbar) - term
                    grad(cInd) += wbar * (term - 1.0 + wbar / W_g(g))
                Next
            Next
            Return (loglik, grad)
        End Function

        ' =================================================================================
        ' Two-leaf optimal time (1-D root finding)
        ' =================================================================================

        ''' <summary>
        ''' Derivative of the 2-leaf log-likelihood with respect to the combined diffusion time,
        ''' mirroring ``der2LeafTree``.
        ''' der = sum_g (sqDists_g / totVar_g - 1) / totVar_g
        ''' </summary>
        Public Function der2LeafTree(t12 As Double, summedLtqsVars As Double(), sqDists As Double()) As Double
            Dim D = summedLtqsVars.Length
            Dim der = 0.0
            For g = 0 To D - 1
                Dim totVar = t12 + summedLtqsVars(g)
                der += (sqDists(g) / totVar - 1.0) / totVar
            Next
            Return der
        End Function

        ''' <summary>
        ''' Optimal total diffusion time between two leaves (on a star of two), found by bracketing the
        ''' root of <see cref="der2LeafTree"/> with bisection/brentq. Mirrors ``getOptTime2LeafTree``.
        ''' </summary>
        Public Function getOptTime2LeafTree(ltqs1 As Double(), ltqsVars1 As Double(), ltqs2 As Double(), ltqsVars2 As Double()) As (tOpt As Double, converged As Boolean)
            Dim D = ltqs1.Length
            Dim summedLtqsVars(D - 1) As Double
            Dim sqDists(D - 1) As Double
            For g = 0 To D - 1
                summedLtqsVars(g) = ltqsVars1(g) + ltqsVars2(g)
                sqDists(g) = (ltqs1(g) - ltqs2(g)) ^ 2
            Next

            Dim lb = 0.0
            If der2LeafTree(lb, summedLtqsVars, sqDists) <= 0 Then
                Return (0.0, True)
            End If

            Dim ub = 1.0
            Dim counter = 0
            While der2LeafTree(ub, summedLtqsVars, sqDists) >= 0
                counter += 1
                If counter > 10 OrElse lb > 1.0E6 Then
                    Return (Nothing, False)
                End If
                lb = ub
                ub *= 10
            End While

            Dim root = Optimizer.BrentZero(AddressOf der2LeafTree, lb, ub, summedLtqsVars, sqDists, 1.0E-7)
            Return (root, True)
        End Function

        ' =================================================================================
        ' Three-leaf star optimisation (used by the tree-search to score a candidate merge)
        ' =================================================================================

        ''' <summary>
        ''' Optimise the three branch times of a star of three children (the "merge" of two children plus
        ''' the rest of the tree as a third pseudo-child) in log-space with a bounded L-BFGS optimisation.
        ''' Returns the optimised times and the achieved log-likelihood. Mirrors ``optimiseT3LeafStar``.
        ''' </summary>
        Public Function optimiseT3LeafStar(ltqs_gi As Double()(), ltqsVars_gi As Double()(), t0_i As Double()) As (loglik As Double, tOpt As Double(), success As Boolean)
            Dim nChild = t0_i.Length
            Dim t0log = t0_i.Select(Function(t) Math.Log(Math.Max(t, 1.0E-4))).ToArray

            Dim lb = -16.118, ub = 10.0   ' log(1e-7) .. log(1e~4.5)
            Dim bounds As New List(Of (lo As Double, hi As Double))
            For i = 0 To nChild - 1
                bounds.Add((lb, ub))
            Next

            Dim res = Optimizer.Minimize(AddressOf logLGradStarTreeLogT, t0log, bounds,
                                      args:=(ltqsVars_gi, ltqs_gi))

            Dim topt = res.x.Select(Function(lt) Math.Exp(lt)).ToArray
            Return (-res.fun, topt, res.success)
        End Function

        ''' <summary>
        ''' Objective for <see cref="optimiseT3LeafStar"/>: negative log-likelihood and gradient in log-time
        ''' space. Mirrors ``logLGradStarTreeLogT``.
        ''' f = -sum_g [ sum_i log(wbar_i) - log(W_g) - sum_i wbar_i sqDists_i ]
        ''' grad = -t_i * sum_g wbar_i (sqDists_i wbar_i - 1 + wbar_i / W_g)
        ''' </summary>
        Private Function logLGradStarTreeLogT(logt_i As Double(), ParamArray args() As Object) As (f As Double, grad As Double())
            Dim ltqsVars_gi = DirectCast(args(0), Double()())
            Dim ltqs_gi = DirectCast(args(1), Double()())
            Dim nChild = logt_i.Length
            Dim D = ltqs_gi.Length
            Dim t_i = logt_i.Select(Function(lt) Math.Exp(lt)).ToArray

            Dim W_g(D - 1) As Double
            Dim xr_g(D - 1) As Double
            Dim wbar_gi(nChild - 1)() As Double
            For c = 0 To nChild - 1
                wbar_gi(c) = New Double(D - 1) {}
            Next

            For g = 0 To D - 1
                Dim W = 0.0, xr = 0.0
                For c = 0 To nChild - 1
                    Dim wbar = 1.0 / (ltqsVars_gi(g)(c) + t_i(c))
                    wbar_gi(c)(g) = wbar
                    W += wbar
                    xr += wbar * ltqs_gi(g)(c)
                Next
                W_g(g) = W
                xr_g(g) = xr / W
            Next

            Dim loglik = 0.0
            For g = 0 To D - 1
                loglik += Math.Log(W_g(g))
                For c = 0 To nChild - 1
                    loglik -= Math.Log(wbar_gi(c)(g))
                Next
            Next

            Dim grad(nChild - 1) As Double
            For c = 0 To nChild - 1
                For g = 0 To D - 1
                    Dim sq = (xr_g(g) - ltqs_gi(g)(c)) ^ 2
                    Dim term = wbar_gi(c)(g) * sq
                    grad(c) += wbar_gi(c)(g) * (term - 1.0 + wbar_gi(c)(g) / W_g(g))
                Next
                grad(c) *= -t_i(c)
            Next

            Return (-loglik, grad)
        End Function

        ' =================================================================================
        ' Downstream derivative (gradient of the full-tree log-likelihood w.r.t. each branch time)
        ' =================================================================================

        ''' <summary>
        ''' Compute the derivative of the total tree log-likelihood w.r.t. every non-root branch time by
        ''' propagating from the root down. Mirrors ``getDerivativesDownstream``. After this call every
        ''' node's <see cref="BonsaiNode.dLoglikdtParent"/> is populated.
        ''' </summary>
        Public Sub getDerivativesDownstream(root As BonsaiNode)
            Dim xrAsIfRoot = root.ltqs
            Dim WAsIfRoot = root.getW()
            For Each child In root.childs
                propagate(child, xrAsIfRoot, WAsIfRoot)
            Next
        End Sub

        Private Sub propagate(child As BonsaiNode, xrAsIfRoot_g As Double(), WAsIfRoot_g As Double())
            Dim D = xrAsIfRoot_g.Length
            Dim ltqsTimesW = New Double(D - 1) {}
            For g = 0 To D - 1
                ltqsTimesW(g) = xrAsIfRoot_g(g) * WAsIfRoot_g(g)
            Next

            Dim wbarChild_g = child.getLtqsVars().Select(Function(v) 1.0 / (v + child.tParent)).ToArray
            Dim WWOChild = New Double(D - 1) {}
            Dim ltqsWOChild = New Double(D - 1) {}
            For g = 0 To D - 1
                WWOChild(g) = WAsIfRoot_g(g) - wbarChild_g(g)
                ltqsWOChild(g) = (ltqsTimesW(g) - wbarChild_g(g) * child.ltqs(g)) / WWOChild(g)
            Next

            Dim sqDist = New Double(D - 1) {}
            Dim totalVars = New Double(D - 1) {}
            For g = 0 To D - 1
                totalVars(g) = child.getLtqsVars()(g) + ltqsWOChild(g)
                sqDist(g) = (ltqsWOChild(g) - child.ltqs(g)) ^ 2
            Next
            child.dLoglikdtParent = der2LeafTree(child.tParent, ltqsWOChild, sqDist)

            If Not child.isLeafNode() Then
                Dim wbarRoot_g = New Double(D - 1) {}
                For g = 0 To D - 1
                    wbarRoot_g(g) = 1.0 / (child.tParent + ltqsWOChild(g))
                Next
                Dim WChildWithRoot = New Double(D - 1) {}
                Dim ltqsChildWithRoot = New Double(D - 1) {}
                Dim cW = child.getW()
                For g = 0 To D - 1
                    WChildWithRoot(g) = cW(g) + wbarRoot_g(g)
                    ltqsChildWithRoot(g) = (child.ltqs(g) * cW(g) + wbarRoot_g(g) * ltqsWOChild(g)) / WChildWithRoot(g)
                Next
                For Each grandChild In child.childs
                    propagate(grandChild, ltqsChildWithRoot, WChildWithRoot)
                Next
            End If
        End Sub

        ' =================================================================================
        ' Full-tree complete log-likelihood (recursive Felsenstein pruning)
        ' =================================================================================

        ''' <summary>
        ''' Recursively integrate out all internal-node positions and return the complete tree
        ''' log-likelihood. Mirrors ``getLtqsComplete`` + ``calcLogLComplete``. After this call every
        ''' internal node's ltqs/W are up to date and node.prefactor holds the accumulated log-likelihood.
        ''' </summary>
        Public Function calcLogLComplete(root As BonsaiNode) As Double
            completeLtqs(root)
            Return root.prefactor
        End Function

        Private Sub completeLtqs(node As BonsaiNode)
            If node.isLeafNode() Then
                node.prefactor = 0.0
                Return
            End If

            node.prefactor = 0.0
            For Each child In node.childs
                completeLtqs(child)
                node.prefactor += child.prefactor
            Next

            node.ltqs = findNodeLtqs(node.childs)
            node.setLtqsVarsOrW(W_g:=findNodeW(node.childs))

            Dim xr = node.ltqs
            Dim W = node.getW()
            node.prefactor += loglikStarTree(node.childs, xr, W)
        End Sub

        ' =================================================================================
        ' Candidate-merge scoring (used by the tree search in BonsaiTree)
        ' =================================================================================

        ''' <summary>
        ''' dLogL of merging two candidate children child1, child2 under a root described by (xrAsIfRoot_g, WAsIfRoot_g).
        ''' Mirrors calcSingleDLogL: the pair is modelled as a 3-leaf star (the two candidates plus the rest of
        ''' the subtree as a single pseudo-leaf), the three times are optimised, and the gain is the difference in
        ''' star log-likelihood before/after creation of the ancestor. The returned value is 0.5 * dLogL (the
        ''' factor used by the reference implementation to avoid double counting).
        ''' </summary>
        Public Function calcSingleDLogL(xrAsIfRoot_g As Double(), WAsIfRoot_g As Double(), child1 As BonsaiNode, child2 As BonsaiNode) As Double
            Dim D = xrAsIfRoot_g.Length

            ' Equivalent leaf of the rest of the tree = root minus child1
            Dim vars1 = child1.getLtqsVars()
            Dim wbar1 = vars1.Select(Function(v) 1.0 / (v + child1.tParent)).ToArray
            Dim rootMinusFirstW = New Double(D - 1) {}
            Dim rootMinusFirstLtqs = New Double(D - 1) {}
            For g = 0 To D - 1
                rootMinusFirstW(g) = WAsIfRoot_g(g) - wbar1(g)
                rootMinusFirstLtqs(g) = xrAsIfRoot_g(g) * WAsIfRoot_g(g) - wbar1(g) * child1.ltqs(g)
            Next

            ' Then subtract child2 from that remainder -> the "R" pseudo-leaf
            Dim vars2 = child2.getLtqsVars()
            Dim wbar2 = vars2.Select(Function(v) 1.0 / (v + child2.tParent)).ToArray
            Dim WR_g = New Double(D - 1) {}
            Dim ltqsR = New Double(D - 1) {}
            For g = 0 To D - 1
                WR_g(g) = rootMinusFirstW(g) - wbar2(g)
                ltqsR(g) = (rootMinusFirstLtqs(g) - wbar2(g) * child2.ltqs(g)) / WR_g(g)
            Next

            ' Old likelihood: star with all three as direct children of root
            Dim oldLeafs = New List(Of BonsaiNode) From {child1, child2, makePseudoLeaf(ltqsR, WR_g)}
            Dim (oldLoglik, _) = loglikGradStarTree(oldLeafs, xrAsIfRoot_g, WAsIfRoot_g)

            ' New likelihood: star of (merged ancestor, R) where the ancestor is optimised
            Dim ltqs_gi = New Double(D - 1)() {child1.ltqs, child2.ltqs, ltqsR}
            Dim lv_gi = New Double(D - 1)() {vars1, vars2, WR_g.Select(Function(w) 1.0 / w).ToArray}
            Dim t0 = New Double() {child1.tParent, child2.tParent, 1.0}

            Dim (newLoglik, _, success) = optimiseT3LeafStar(ltqs_gi, lv_gi, t0)
            If Not success Then
                Return 0.0
            End If

            Dim dLogL = newLoglik - oldLoglik
            If dLogL < 0 Then
                dLogL = 0.0
            End If
            Return 0.5 * dLogL
        End Function

        ''' <summary>
        ''' Build a transient pseudo-leaf node wrapping an effective position/precision (used for scoring only).
        ''' </summary>
        Private Function makePseudoLeaf(ltqs As Double(), W As Double()) As BonsaiNode
            Dim n = New BonsaiNode With {
                .ltqs = ltqs,
                .isLeaf = True
            }
            n.setLtqsVarsOrW(W_g:=W)
            Return n
        End Function

        ' ----- small vector helpers -----

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function VectorLog(v As Double()) As Double()
            Return v.Select(Function(x) Math.Log(x)).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function VectorSum(v As Double()) As Double
            Dim s = 0.0
            For Each x In v
                s += x
            Next
            Return s
        End Function
    End Module
End Namespace
