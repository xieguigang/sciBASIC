#Region "Microsoft.VisualBasic::30861de212105830185a42612ed76ee2, Data_science\DataMining\DBNCode\dbn\Scores.vb"

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

    '   Total Lines: 761
    '    Code Lines: 408 (53.61%)
    ' Comment Lines: 188 (24.70%)
    '    - Xml Docs: 12.77%
    ' 
    '   Blank Lines: 165 (21.68%)
    '     File Size: 29.52 KB


    '     Class Scores
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: Best_Past_Parents, evaluate, getScoresMatrix, prob, (+2 Overloads) to_bcDBN
    '                   (+2 Overloads) to_cDBN, (+3 Overloads) toDBN, ToString
    ' 
    '         Sub: generateCombinations
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.utils
Imports std = System.Math

Namespace dbn

    Public Class Scores

        Private multithread As Boolean

        Private observations As Observations

        ''' <summary>
        ''' scoresMatrix[t][i][j] is the score of the arc
        ''' Xj[t+markovLag]->Xi[t+markovLag].
        ''' </summary>
        Private scoresMatrix As Double()()()

        ''' <summary>
        ''' parentNodes.get(t).get(i) is the list of optimal parents in
        ''' {X[t],...,X[t+markovLag-1]} of Xi[t+markovLag] when there is no arc from
        ''' X[t+markovLag] to X[t+markovLag].
        ''' </summary>
        Private parentNodesPast As IList(Of IList(Of IList(Of Integer)))

        ''' <summary>
        ''' parentNodes.get(t).get(i).get(j) is the list of optimal parents in
        ''' {X[t],...,X[t+markovLag-1]} of Xi[t+markovLag] when the arc
        ''' Xj[t+markovLag]->Xi[t+markovLag] is present.
        ''' </summary>
        Private parentNodes As IList(Of IList(Of IList(Of IList(Of Integer))))

        ''' <summary>
        ''' Upper limit on the number of parents from previous time slices.
        ''' </summary>
        Private maxParents As Integer

        ''' <summary>
        ''' A list of all possible sets of parent nodes. Set cardinality lies within the
        ''' range [1, maxParents].
        ''' </summary>
        Private parentSets As IList(Of IList(Of Integer))

        ''' <summary>
        ''' If true, evaluates only one score matrix for all transitions.
        ''' </summary>
        Private stationaryProcess As Boolean

        Private evaluated As Boolean = False

        Private verbose As Boolean

        Private ancestors As IList(Of IList(Of Integer))

        Private PastParents As IList(Of IList(Of Integer))

        Private PresentParents As IList(Of IList(Of Integer))

        Public Sub New(observations As Observations, maxParents As Integer)
            Me.New(observations, maxParents, True, True)
        End Sub

        Public Sub New(observations As Observations, maxParents As Integer, stationaryProcess As Boolean, verbose As Boolean)
            Me.New(observations, maxParents, stationaryProcess, verbose, False)
        End Sub

        Public Sub New(observations As Observations, maxParents As Integer, stationaryProcess As Boolean, verbose As Boolean, multithread As Boolean)
            Me.observations = observations
            Me.maxParents = maxParents
            Me.stationaryProcess = stationaryProcess
            Me.verbose = verbose
            Me.multithread = multithread

            Dim n As Integer = Me.observations.numAttributes()
            Dim p = Me.maxParents
            Dim markovLag = observations.MarkovLag

            ' calculate sum_i=1^k nCi
            Dim size = n * markovLag
            Dim previous = n, i = 2

            While i <= p
                Dim current As Integer = previous * (n - i + 1) / i
                size += current
                previous = current
                i += 1
            End While
            size += 1 ' To count with the empty set!

            ' TODO: check for size overflow

            ' generate parents sets
            parentSets = New List(Of IList(Of Integer))(size)
            For i = 1 To p
                generateCombinations(n * markovLag, i)
            Next

            parentSets.Add(New List(Of Integer)())

            Dim numTransitions As Integer = If(stationaryProcess, 1, observations.numTransitions())
            parentNodesPast = New List(Of IList(Of IList(Of Integer)))(numTransitions)
            parentNodes = New List(Of IList(Of IList(Of IList(Of Integer))))(numTransitions)

            For t = 0 To numTransitions - 1

                parentNodesPast.Add(New List(Of IList(Of Integer))(n))
                ' allocate parentNodesPast
                Dim parentNodesPastTransition = parentNodesPast(t)
                For i = 0 To n - 1
                    parentNodesPastTransition.Add(New List(Of Integer)())
                Next

                parentNodes.Add(New List(Of IList(Of IList(Of Integer)))(n))
                ' allocate parentNodes
                Dim parentNodesTransition = parentNodes(t)
                For i = 0 To n - 1
                    parentNodesTransition.Add(New List(Of IList(Of Integer))(n))
                    Dim parentNodesTransitionHead = parentNodesTransition(i)
                    For j = 0 To n - 1
                        parentNodesTransitionHead.Add(New List(Of Integer)())
                    Next
                Next
            Next

            ' allocate scoresMatrix
            scoresMatrix = RectangularArray.Cubic(Of Double)(numTransitions, n, n)

        End Sub

        Public Overridable Function evaluate(sf As ScoringFunction) As Scores

            Dim n As Integer = observations.numAttributes()
            Dim numTransitions = scoresMatrix.Length

            Dim numBestScoresPast = New Integer(n - 1) {}
            Dim numBestScores = RectangularArray.Matrix(Of Integer)(n, n)

            Dim cores = CPUCoreNumbers
            Dim aux_sum As Integer = std.Ceiling(n / cores)
            Dim Threads = New ScoreCalculationThread(cores - 1) {}

            For t = 0 To numTransitions - 1
                'System.out.println("evaluating score in transition " + t + "/" + numTransitions);
                If Not multithread Then
                    For i = 0 To n - 1
                        '					// System.out.println("evaluating node " + i + "/" + n);
                        Dim bestScore = Double.NegativeInfinity

                        For Each parentSet In parentSets
                            Dim score = If(stationaryProcess, sf.evaluate(observations, parentSet, i), sf.evaluate(observations, t, parentSet, i))
                            ' System.out.println("Xi:" + i + " ps:" + parentSet + " score:" + score);
                            If bestScore < score Then
                                bestScore = score
                                parentNodesPast(t)(i) = parentSet
                                numBestScoresPast(i) = 1
                            ElseIf bestScore = score Then
                                numBestScoresPast(i) += 1
                            End If
                        Next
                        ' System.out.println("Finished parents past");
                        For j = 0 To n - 1
                            scoresMatrix(t)(i)(j) = -bestScore
                        Next
                    Next
                    For i = 0 To n - 1
                        For j = 0 To n - 1
                            If i <> j Then
                                Dim bestScore = Double.NegativeInfinity
                                For Each parentSet In parentSets
                                    Dim score = If(stationaryProcess, sf.evaluate(observations, parentSet, j, i), sf.evaluate(observations, t, parentSet, j, i))
                                    ' System.out.println("Xi:" + i + " Xj:" + j + " ps:" + parentSet + " score:" + score);
                                    If bestScore < score Then
                                        bestScore = score
                                        parentNodes(t)(i)(j) = parentSet
                                        numBestScores(i)(j) = 1
                                    ElseIf bestScore = score Then
                                        numBestScores(i)(j) += 1
                                    End If
                                Next
                                scoresMatrix(t)(i)(j) += bestScore
                            End If
                        Next
                    Next
                Else
                    Dim aux = 0
                    Dim i = 0

                    While i < n
                        Dim auxsum = i + aux_sum
                        If auxsum > n Then
                            auxsum = n
                        End If
                        Threads(aux) = New ScoreCalculationThread(t, i, auxsum, n, parentSets, observations, scoresMatrix, parentNodesPast, parentNodes, numBestScores, numBestScoresPast, sf, stationaryProcess)
                        Threads(aux).run()
                        aux += 1
                        i = i + aux_sum
                    End While
                End If
                If verbose Then
                    Dim numSolutions As Long = 1
                    For i = 0 To n - 1
                        numSolutions *= numBestScoresPast(i)
                    Next
                    For i = 0 To n - 1
                        For j = 0 To n - 1
                            If i <> j Then
                                numSolutions *= numBestScores(i)(j)
                            End If
                        Next
                    Next

                    Console.WriteLine("Number of networks with max score: " & numSolutions.ToString())
                End If

            Next

            evaluated = True

            Return Me

        End Function

        Public Overridable Function Best_Past_Parents(ancestors As IList(Of Integer), i As Integer, t As Integer, sf As ScoringFunction) As IList(Of Integer)

            Dim best_parent_set As IList(Of Integer) = New List(Of Integer)()
            Dim bestScore = Double.NegativeInfinity

            For Each parentSet In parentSets
                Dim score = If(stationaryProcess, sf.evaluate_2(observations, parentSet, ancestors, i), sf.evaluate_2(observations, t, parentSet, ancestors, i))
                If bestScore < score Then
                    bestScore = score
                    best_parent_set = parentSet
                End If

                Dim score_empty As Double = If(stationaryProcess, sf.evaluate_2(observations, New List(Of Integer)(), ancestors, i), sf.evaluate_2(observations, t, New List(Of Integer)(), ancestors, i))


                If score_empty > bestScore Then
                    bestScore = score
                    best_parent_set = New List(Of Integer)()
                End If

            Next

            Return best_parent_set
        End Function

        Public Overridable Function prob() As Double

            ' PARENTS PAST ????????

            Dim n As Integer = observations.numAttributes()
            Dim numTransitions = scoresMatrix.Length

            Dim score As Double = 0

            For t = 0 To numTransitions - 1
                ' System.out.println("evaluating score in transition " + t + "/" +
                ' numTransitions);
                For i = 0 To n - 1
                    ' System.out.println("evaluating node " + i + "/" + n);
                    For Each parentSet In parentSets

                        Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentSet, i)

                        Do
                            c.ConsiderChild = False
                            Dim Nij = observations.count(c, t)
                            c.ConsiderChild = True
                            Do
                                Dim Nijk = observations.count(c, t)
                                If Nijk <> 0 AndAlso Nijk <> Nij Then
                                    score += std.Log(Nijk) - std.Log(Nij)
                                End If
                            Loop While c.nextChild()
                        Loop While c.nextParents()

                    Next
                Next
            Next
            Return score

        End Function

        ' adapted from http://stackoverflow.com/a/7631893
        Private Sub generateCombinations(n As Integer, k As Integer)

            Dim comb = New Integer(k - 1) {}
            For i = 0 To comb.Length - 1
                comb(i) = i
            Next

            Dim done = False
            While Not done

                Dim intList As IList(Of Integer) = New List(Of Integer)(k)
                For Each i In comb
                    intList.Add(i)
                Next
                parentSets.Add(intList)

                Dim target = k - 1
                comb(target) += 1
                If comb(target) > n - 1 Then
                    ' carry the one
                    While comb(target) > n - 1 - (k - target)
                        target -= 1
                        If target < 0 Then
                            Exit While
                        End If
                    End While
                    If target < 0 Then
                        done = True
                    Else
                        comb(target) += 1
                        For i = target + 1 To comb.Length - 1
                            comb(i) = comb(i - 1) + 1
                        Next
                    End If
                End If
            End While
        End Sub

        Public Overridable Function getScoresMatrix(transition As Integer) As Double()()
            Return scoresMatrix(transition)
        End Function

        Public Overridable Function toDBN() As DynamicBayesNet
            Return toDBN(-1, False, False)
        End Function

        Public Overridable Function toDBN(root As Integer, spanning As Boolean) As DynamicBayesNet
            Return toDBN(root, spanning, False)
        End Function

        Public Overridable Function toDBN(root As Integer, spanning As Boolean, prior As Boolean) As DynamicBayesNet

            If Not evaluated Then
                Throw New InvalidOperationException("Scores must be evaluated before being converted to DBN")
            End If

            Dim n As Integer = observations.numAttributes()

            Dim numTransitions = scoresMatrix.Length

            Dim transitionNets As IList(Of BayesNet) = New List(Of BayesNet)(numTransitions)

            For t = 0 To numTransitions - 1

                Dim intraRelations As OptimumBranching = New OptimumBranching(scoresMatrix(t), root, spanning)

                If verbose Then
                    Dim score As Double = 0
                    Dim adj = utils.Utils.adjacencyMatrix(intraRelations.branchingField, n)

                    For i = 0 To n - 1
                        Dim isRoot = True
                        For j = 0 To n - 1
                            If adj(i)(j) Then
                                ' score
                                score += scoresMatrix(t)(i)(j) - scoresMatrix(t)(i)(i)
                                isRoot = False
                            End If
                        Next
                        If isRoot Then
                            ' subtract since sign was inverted
                            score -= scoresMatrix(t)(i)(i)
                        End If
                    Next

                    Console.WriteLine("Network score: " & score.ToString())
                End If

                Dim interRelations As IList(Of Edge) = New List(Of Edge)(n * maxParents)

                Dim hasParent = New Boolean(n - 1) {}

                For Each intra In intraRelations.branchingField
                    Dim tail = intra.Tail
                    Dim head = intra.Head
                    Dim parentNodesT = parentNodes(t)

                    For Each nodePast As Integer? In parentNodesT(head)(tail)
                        interRelations.Add(New Edge(nodePast.Value, head))
                        hasParent(head) = True
                    Next
                Next

                For i = 0 To n - 1
                    If Not hasParent(i) Then
                        Dim parentNodesPastT = parentNodesPast(t)
                        For Each nodePast In parentNodesPastT(i)
                            interRelations.Add(New Edge(nodePast, i))
                        Next
                    End If
                Next

                Dim bt As BayesNet = New BayesNet(observations.Attributes, observations.MarkovLag, intraRelations.branchingField, interRelations)

                transitionNets.Add(bt)
            Next
            If prior Then
                Dim prior_array As IList(Of Edge) = New List(Of Edge)()
                Dim a = observations.Attributes
                Dim b0 As BayesNet = New BayesNet(a, prior_array)
                Return New DynamicBayesNet(observations.Attributes, b0, transitionNets)
            End If

            Return New DynamicBayesNet(observations.Attributes, transitionNets)

        End Function

        Public Overridable Function to_bcDBN(sf As ScoringFunction, k As Integer) As DynamicBayesNet
            Return to_bcDBN(sf, k, False)
        End Function

        Public Overridable Function to_bcDBN(sf As ScoringFunction, k As Integer, prior As Boolean) As DynamicBayesNet

            If Not evaluated Then
                Throw New InvalidOperationException("Scores must be evaluated before being converted to DBN")
            End If

            Dim n As Integer = observations.numAttributes()

            Dim numTransitions = scoresMatrix.Length

            Dim transitionNets As IList(Of BayesNet) = New List(Of BayesNet)(numTransitions)

            For t = 0 To numTransitions - 1

                Dim intraRelations As OptimumBranching = New OptimumBranching(scoresMatrix(t))

                intraRelations.BFS()

                PastParents = New List(Of IList(Of Integer))(n)

                PresentParents = New List(Of IList(Of Integer))(n)

                For i = 0 To n - 1

                    Dim anc = intraRelations.ancestors(i)

                    PastParents.Add(New List(Of Integer)())

                    PresentParents.Add(New List(Of Integer)())

                    Dim bestScore = Double.NegativeInfinity

                    For Each parentSet In parentSets

                        For Each S In OptimumBranching.Subsets(anc, k)

                            Dim score = If(stationaryProcess, sf.evaluate_2(observations, parentSet, S, i), sf.evaluate_2(observations, t, parentSet, S, i))
                            If score > bestScore Then
                                bestScore = score
                                PastParents(i) = parentSet
                                PresentParents(i) = S
                            End If
                        Next
                    Next
                Next

                Dim intra As IList(Of Edge) = New List(Of Edge)()

                Dim inter As IList(Of Edge) = New List(Of Edge)()

                For node = 0 To n - 1

                    For j = 0 To PastParents(node).Count - 1

                        inter.Add(New Edge(PastParents(node)(j), node))
                    Next
                Next

                For node = 0 To n - 1

                    For j = 0 To PresentParents(node).Count - 1

                        intra.Add(New Edge(PresentParents(node)(j), node))
                    Next
                Next

                Dim bt As BayesNet = New BayesNet(observations.Attributes, observations.MarkovLag, intra, inter)

                transitionNets.Add(bt)
            Next

            If prior Then
                Dim prior_array As IList(Of Edge) = New List(Of Edge)()
                Dim a = observations.Attributes
                Dim b0 As BayesNet = New BayesNet(a, prior_array)
                Return New DynamicBayesNet(observations.Attributes, b0, transitionNets)
            End If

            Return New DynamicBayesNet(observations.Attributes, transitionNets)

        End Function

        Public Overridable Function to_cDBN(sf As ScoringFunction, k As Integer) As DynamicBayesNet
            Return to_cDBN(sf, k, False)
        End Function

        Public Overridable Function to_cDBN(sf As ScoringFunction, k As Integer, prior As Boolean) As DynamicBayesNet

            If Not evaluated Then
                Throw New InvalidOperationException("Scores must be evaluated before being converted to DBN")
            End If

            Dim n As Integer = observations.numAttributes()

            Dim numTransitions = scoresMatrix.Length

            Dim transitionNets As IList(Of BayesNet) = New List(Of BayesNet)(numTransitions)

            For t = 0 To numTransitions - 1

                Dim intraRelations As OptimumBranching = New OptimumBranching(scoresMatrix(t))

                PastParents = New List(Of IList(Of Integer))(n)

                PresentParents = New List(Of IList(Of Integer))(n)

                For i = 0 To n - 1

                    Dim anc = intraRelations.ancestors(i)

                    PastParents.Add(New List(Of Integer)())

                    PresentParents.Add(New List(Of Integer)())

                    Dim bestScore = Double.NegativeInfinity

                    For Each parentSet In parentSets

                        For Each S In OptimumBranching.Subsets(anc, k)

                            Dim score = If(stationaryProcess, sf.evaluate_2(observations, parentSet, S, i), sf.evaluate_2(observations, t, parentSet, S, i))

                            If score > bestScore Then
                                bestScore = score
                                PastParents(i) = parentSet
                                PresentParents(i) = S
                            End If

                        Next

                    Next

                Next

                Dim intra As IList(Of Edge) = New List(Of Edge)()

                Dim inter As IList(Of Edge) = New List(Of Edge)()

                For node = 0 To n - 1

                    For j = 0 To PastParents(node).Count - 1

                        inter.Add(New Edge(PastParents(node)(j), node))
                    Next
                Next

                For node = 0 To n - 1

                    For j = 0 To PresentParents(node).Count - 1

                        intra.Add(New Edge(PresentParents(node)(j), node))
                    Next
                Next

                Dim bt As BayesNet = New BayesNet(observations.Attributes, observations.MarkovLag, intra, inter)

                transitionNets.Add(bt)
            Next

            If prior Then
                Dim prior_array As IList(Of Edge) = New List(Of Edge)()
                Dim a = observations.Attributes
                Dim b0 As BayesNet = New BayesNet(a, prior_array)
                Return New DynamicBayesNet(observations.Attributes, b0, transitionNets)
            End If

            Return New DynamicBayesNet(observations.Attributes, transitionNets)

        End Function

        ' 
        ' 		 * public DynamicBayesNet toDBN_Ckg(ScoringFunction sf,int k) {
        ' 		 * 
        ' 		 * if (!evaluated) throw new
        ' 		 * IllegalStateException("Scores must be evaluated before being converted to DBN"
        ' 		 * );
        ' 		 * 
        ' 		 * int n = observations.numAttributes();
        ' 		 * 
        ' 		 * int numTransitions = scoresMatrix.length;
        ' 		 * 
        ' 		 * List<BayesNet> transitionNets= new ArrayList<BayesNet>(numTransitions);
        ' 		 * 
        ' 		 * 
        ' 		 * for (int t = 0; t < numTransitions; t++) {
        ' 		 * 
        ' 		 * 
        ' 		 * OptimumBranching intraRelations= new OptimumBranching(scoresMatrix[t]);
        ' 		 * 
        ' 		 * //System.out.println(intraRelations_before);
        ' 		 * 
        ' 		 * 
        ' 		 * intraRelations.Ckg(scoresMatrix[t], sf, observations, k);
        ' 		 * 
        ' 		 * 
        ' 		 * 
        ' 		 * 
        ' 		 * //System.out.println(intraRelations);
        ' 		 * 
        ' 		 * 
        ' 		 * if (verbose) { double score = 0;
        ' 		 * 
        ' 		 * boolean[][] adj = Utils.adjacencyMatrix(intraRelations.getBranching(), n);
        ' 		 * 
        ' 		 * for (int i = 0; i < n; i++) { boolean isRoot = true; for (int j = 0; j < n;
        ' 		 * j++) { if (adj[i][j]) { // score score += (scoresMatrix[t][i][j] -
        ' 		 * scoresMatrix[t][i][i]); isRoot = false; } } if (isRoot) // subtract since
        ' 		 * sign was inverted score -= scoresMatrix[t][i][i]; }
        ' 		 * 
        ' 		 * 
        ' 		 * }
        ' 		 * 
        ' 		 * List<Edge> interRelations = new ArrayList<Edge>(n * maxParents);
        ' 		 * 
        ' 		 * boolean[] hasParent = new boolean[n];
        ' 		 
        ' intraRelations_before
        ' 
        ' 		 * for (Edge intra : intraRelations) { int tail = intra.getTail(); int head =
        ' 		 * intra.getHead(); List<List<List<Integer>>> parentNodesT = parentNodes.get(t);
        ' 		 * for (Integer nodePast : parentNodesT.get(head).get(tail)) {
        ' 		 * interRelations.add(new Edge(nodePast, head)); hasParent[head] = true; } }
        ' 		 

        ' 
        ' 		 * ancestors=intraRelations.Anc(scoresMatrix[t],k);
        ' 		 * 
        ' 		 * for(int i=0;i<n;i++) {
        ' 		 * 
        ' 		 * List<Integer> anc = ancestors.get(i); if(anc.size()>0) {
        ' 		 * 
        ' 		 * hasParent[i]=true;
        ' 		 * 
        ' 		 * 
        ' 		 * List<Integer> past_parents = Best_Past_Parents(anc,i,t,sf); for(int j=0;
        ' 		 * j<past_parents.size();j++) { interRelations.add(new
        ' 		 * Edge(past_parents.get(j),i)); }
        ' 		 * 
        ' 		 * }
        ' 		 * 
        ' 		 * else hasParent[i]=false;
        ' 		 * 
        ' 		 * }
        ' 		 * 
        ' 		 * for (int i = 0; i < n; i++) if (!hasParent[i]) { List<List<Integer>>
        ' 		 * parentNodesPastT = parentNodesPast.get(t); for (int nodePast :
        ' 		 * parentNodesPastT.get(i)) interRelations.add(new Edge(nodePast, i)); }
        ' 		 

        ' 
        ' 		 * List<List<Integer>> parents = new ArrayList<List<Integer>>();
        ' 		 * 
        ' 		 * 
        ' 		 * for(int i=0; i<n;i++) { parents.add(new ArrayList<Integer>()); }
        ' 		 * 
        ' 		 * 
        ' 		 * 
        ' 		 * 
        ' 		 * for(Edge e : interRelations) {
        ' 		 * 
        ' 		 * for(int i=0; i<n;i++) {
        ' 		 * 
        ' 		 * if(e.getHead()==i) parents.get(i).add(e.getTail()); } }
        ' 		 

        ' 
        ' 		 * 
        ' 		 * 
        ' 		 * List<List<Edge>> best = OptimumBranching.Best2(ancestors,scoresMatrix[t], sf,
        ' 		 * observations, k);
        ' 		 * 
        ' 		 * interRelations=best.get(0);
        ' 		 * 
        ' 		 * intraRelations=best.get(1);
        ' 		 * 
        ' 		 * System.out.println("Inter relations "+interRelations);
        ' 		 * 
        ' 		 * System.out.println("Intra relations "+intraRelations);
        ' 		 
        ' 
        ' 		 * 
        ' 		 * BayesNet bt = new BayesNet(observations.getAttributes(),
        ' 		 * observations.getMarkovLag(), intraRelations.branching, interRelations);
        ' 		 * 
        ' 		 * transitionNets.add(bt); }
        ' 		 * 
        ' 		 * return new DynamicBayesNet(observations.getAttributes(), transitionNets);
        ' 		 * 
        ' 		 * }
        ' 		 

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            Dim n = scoresMatrix(0).Length

            Dim numTransitions = scoresMatrix.Length

            For t = 0 To numTransitions - 1
                ' sb.append("--- Transition " + t + " ---" + ls);
                ' sb.append("Maximum number of parents in t: " + maxParents + ls);
                '
                ' sb.append(ls);

                sb.Append("Scores matrix:" & ls)
                For i = 0 To n - 1
                    For j = 0 To n - 1
                        sb.Append(scoresMatrix(t)(i)(j).ToString() & " ")
                    Next
                    sb.Append(ls)
                Next

                ' sb.append(ls);
                '
                ' sb.append("Parents only in t:" + ls);
                ' for (int i = 0; i < n; i++) {
                ' sb.append(i + ": " + parentNodesPast.get(t).get(i) + ls);
                ' }
                '
                ' sb.append(ls);
                '
                ' sb.append("Parents in t for each parent in t+1:" + ls);
                ' sb.append("t+1: ");
                ' for (int i = 0; i < n; i++) {
                ' sb.append(i + " ");
                ' }
                ' sb.append(ls);
                ' for (int i = 0; i < n; i++) {
                ' sb.append(i + ": ");
                ' for (int j = 0; j < n; j++) {
                ' sb.append(parentNodes.get(t).get(i).get(j) + " ");
                ' }
                ' sb.append(ls);
                ' }
                '
                ' sb.append(ls);
            Next

            Return sb.ToString()
        End Function

    End Class

End Namespace

