Imports System.Text
Imports DBNCode.utils
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Java

Namespace dbn

    ''' <summary>
    ''' Class that describes a Bayesian Network (BN).
    ''' 
    ''' @author josemonteiro
    ''' @author MargaridanarSousa
    ''' @author SSamDav
    ''' 
    ''' </summary>
    Public Class BayesNet

        ''' <summary>
        ''' List of attributes of this BN.
        ''' </summary>
        Private attributes As IList(Of Attribute)

        ''' <summary>
        ''' "processed" (unshifted) relations.
        ''' </summary>
        Private parentNodesPerSlice As IList(Of IList(Of IList(Of Integer)))

        ''' <summary>
        ''' "raw" (shifted) relations
        ''' </summary>
        Private parentNodes As IList(Of IList(Of Integer))

        ''' <summary>
        ''' Parameters of a BN (CPT).
        ''' </summary>
        Private parametersField As IList(Of IDictionary(Of Configuration, IList(Of Double)))

        Private topologicalOrder As IList(Of Integer)

        ''' <summary>
        ''' Markov Lag corresponding to this BN.
        ''' </summary>
        Private markovLagField As Integer

        ' for random sampling
        Private r As Random

        ''' <summary>
        ''' Getter for the parameters.
        ''' </summary>
        ''' <returns> List<Map/><Configuration/><Double>>> Returns the parameters of this
        '''         BN. </returns>    
        Public Overridable ReadOnly Property Parameters As IList(Of IDictionary(Of Configuration, IList(Of Double)))
            Get
                Return parametersField
            End Get
        End Property

        ''' <summary>
        ''' Getter for the topological order.
        ''' </summary>
        ''' <returns> List<Integer> Returns the topological order of the BN. </returns>  
        Public Overridable ReadOnly Property Top As IList(Of Integer)
            Get
                Return topologicalOrder
            End Get
        End Property

        ''' <summary>
        ''' Getter of the parent nodes.
        ''' </summary>
        ''' <returns> List<List/><Integer>> Returns the list of parents nodes of the BN. </returns>  
        Public Overridable ReadOnly Property Parents As IList(Of IList(Of Integer))
            Get
                Return parentNodes
            End Get
        End Property

        ' prior network
        Public Sub New(attributes As IList(Of Attribute), intraRelations As IList(Of Edge), r As Random)
            Me.New(attributes, 0, intraRelations, Nothing, r)
        End Sub

        Public Sub New(attributes As IList(Of Attribute), intraRelations As IList(Of Edge))
            Me.New(attributes, 0, intraRelations, Nothing, Nothing)
        End Sub

        ' transition network, standard Markov lag = 1
        Public Sub New(attributes As IList(Of Attribute), intraRelations As IList(Of Edge), interRelations As IList(Of Edge), r As Random)
            Me.New(attributes, 1, intraRelations, interRelations, r)
        End Sub

        Public Sub New(attributes As IList(Of Attribute), intraRelations As IList(Of Edge), interRelations As IList(Of Edge))
            Me.New(attributes, 1, intraRelations, interRelations, Nothing)
        End Sub

        ' transition network, arbitrary Markov lag
        Public Sub New(attributes As IList(Of Attribute), markovLag As Integer, intraRelations As IList(Of Edge), interRelations As IList(Of Edge))
            Me.New(attributes, markovLag, intraRelations, interRelations, Nothing)
        End Sub

        ''' <summary>
        ''' Constructor of a Bayesian Network. The edge heads are already unshifted
        ''' (i.e., in the interval [0, n[).
        ''' </summary>
        ''' <param name="attributes">     Attributes of the BN. </param>
        ''' <param name="markovLag">      Markov lag corresponding to the BN. </param>
        ''' <param name="intraRelations"> Intra-Relations between the attributes of this BN. The
        '''                       edge tails are unshifted. </param>
        ''' <param name="interRelations"> Inter-Relations between the attributes of this BN. The
        '''                       edge tails are shifted in Configuration style (i.e, [0,
        '''                       markovLag*n[). </param>
        ''' <param name="r"> </param>
        Public Sub New(attributes As IList(Of Attribute), markovLag As Integer, intraRelations As IList(Of Edge), interRelations As IList(Of Edge), r As Random)

            Me.attributes = attributes
            markovLagField = markovLag
            Dim n = attributes.Count

            Me.r = If(r IsNot Nothing, r, New Random())

            ' for topological sorting of t+1 slice
            Dim childNodes As IList(Of IList(Of Integer)) = New List(Of IList(Of Integer))(n)
            Dim i = n

            While Math.Max(Threading.Interlocked.Decrement(i), i + 1) > 0
                childNodes.Add(New List(Of Integer)(n))
            End While

            parentNodesPerSlice = New List(Of IList(Of IList(Of Integer)))(markovLag + 1)
            For slice As Integer = 0 To markovLag + 1 - 1
                parentNodesPerSlice.Add(New List(Of IList(Of Integer))(n))
                For i = 0 To n - 1
                    parentNodesPerSlice(slice).Add(New List(Of Integer)())
                Next
            Next

            parentNodes = New List(Of IList(Of Integer))(n)
            For i = 0 To n - 1
                parentNodes.Add(New List(Of Integer)())
            Next

            If interRelations IsNot Nothing Then
                For Each e As Edge In interRelations
                    ' tail is shifted and refers to a previous slice
                    Dim tail = e.Tail
                    Dim slice As Integer = tail / n
                    Dim unshiftedTail = tail Mod n
                    ' head refers to the foremost slice
                    Dim head = e.Head

                    parentNodesPerSlice(slice)(head).Add(unshiftedTail)
                    parentNodes(head).Add(tail)
                Next
            End If

            ' edges inside the same slice
            For Each e As Edge In intraRelations
                ' tail is unshifted
                Dim tail = e.Tail
                Dim shiftedTail = tail + n * markovLag
                Dim head = e.Head

                parentNodesPerSlice(markovLag)(head).Add(tail)
                parentNodes(head).Add(shiftedTail)
                childNodes(tail).Add(head)
            Next

            ' sort for when applying configuration mask
            i = n

            While Math.Max(Threading.Interlocked.Decrement(i), i + 1) > 0
                parentNodes(i) = New List(Of Integer)(parentNodes(i).Sort())
            End While

            ' obtain nodes by topological order
            topologicalOrder = utils.Utils.topologicalSort(childNodes)
        End Sub

        ''' <summary>
        ''' Function that generate the parameters of a given BN.
        ''' </summary>
        Public Overridable Sub generateParameters()
            Dim n = attributes.Count
            parametersField = New List(Of IDictionary(Of Configuration, IList(Of Double)))(n)

            For i = 0 To n - 1

                Dim c As LocalConfiguration = New LocalConfiguration(attributes, markovLagField, parentNodes(i), i)
                Dim parentsRange = c.ParentsRange
                If parentsRange = 0 Then
                    parametersField.Add(New Dictionary(Of Configuration, IList(Of Double))(2))
                    Dim range = c.ChildRange
                    parametersField(i)(New Configuration(c)) = generateProbabilities(range)
                Else
                    parametersField.Add(New Dictionary(Of Configuration, IList(Of Double))(Math.Ceiling(parentsRange / 0.75)))

                    Do
                        Dim range = c.ChildRange
                        parametersField(i)(New Configuration(c)) = generateProbabilities(range)
                    Loop While c.nextParents()
                End If

            Next
        End Sub

        ''' <summary>
        ''' Learns the parameters of a stationary BN.
        ''' </summary>
        ''' <param name="o"> Observations </param>
        Public Overridable Sub learnParameters(o As Observations)
            learnParameters(o, -1)
        End Sub

        ''' <summary>
        ''' Learns the parameters of an BN.
        ''' </summary>
        ''' <param name="o">          Observations </param>
        ''' <param name="transition"> of the BN. </param>
        ''' <returns> String Corresponding to the learnt CPT. </returns>
        Public Overridable Function learnParameters(o As Observations, transition As Integer) As String

            If Not o.Attributes.SequenceEqual(attributes) Then
                Throw New ArgumentException("Attributes of the observations don't" & "match the attributes of the BN")
            End If

            Dim n = attributes.Count
            parametersField = New List(Of IDictionary(Of Configuration, IList(Of Double)))(n)

            ' for each node, generate its local CPT
            For i = 0 To n - 1

                Dim c As LocalConfiguration = New LocalConfiguration(attributes, markovLagField, parentNodes(i), i)

                Dim parentsRange = c.ParentsRange

                ' node i has no parents
                If parentsRange = 0 Then
                    parametersField.Add(New Dictionary(Of Configuration, IList(Of Double))(2))
                    ' specify its priors
                    Dim range = c.ChildRange
                    Dim probabilities As IList(Of Double) = New List(Of Double)(range - 1)
                    ' count for all except one of possible child values
                    Dim j = range - 1

                    While Math.Max(Threading.Interlocked.Decrement(j), j + 1) > 0
                        Dim Nijk = o.count(c, transition)
                        probabilities.Add(1.0 * Nijk / o.numObservations(transition))
                        c.nextChild()
                    End While
                    ' important, configuration is indexed by parents only
                    ' child must be reset
                    c.resetChild()
                    parametersField(i)(New Configuration(c)) = probabilities
                Else
                    parametersField.Add(New Dictionary(Of Configuration, IList(Of Double))(Math.Ceiling(parentsRange / 0.75)))

                    Do
                        c.ConsiderChild = False
                        Dim Nij = o.count(c, transition)
                        c.ConsiderChild = True

                        Dim range = c.ChildRange
                        Dim probabilities As IList(Of Double) = New List(Of Double)(range - 1)

                        ' no data found for given configuration
                        If Nij = 0 Then
                            Dim j = range - 1

                            While Math.Max(Threading.Interlocked.Decrement(j), j + 1) > 0
                                ' assume uniform distribution
                                probabilities.Add(1.0 / range)
                            End While
                        Else
                            ' count for all except one of possible child values
                            Dim j = range - 1

                            While Math.Max(Threading.Interlocked.Decrement(j), j + 1) > 0
                                Dim Nijk = o.count(c, transition)
                                probabilities.Add(1.0 * Nijk / Nij)
                                c.nextChild()
                            End While
                        End If
                        ' important, configuration is index by parents only
                        ' child must be reset
                        c.resetChild()
                        parametersField(i)(New Configuration(c)) = probabilities
                    Loop While c.nextParents()
                End If

            Next

            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            For Each cpt In parametersField
                sb.Append(Arrays.toString(cpt.SetOfKeyValuePairs().ToArray()) & ls)
            Next

            Return sb.ToString()

        End Function

        ''' <summary>
        ''' Function that for a given configuration and for a given node calculates the
        ''' probability of that observation.
        ''' </summary>
        ''' <param name="node">   Node where we want to calculate the probability. </param>
        ''' <param name="config"> Configuration of the observation. </param>
        ''' <returns> List<Double> Returns the probability. </returns>
        Public Overridable Function getParameters(node As Integer, config As Integer()) As IList(Of Double)
            Dim prob As Double = 0
            Dim aux As IList(Of Double) = New List(Of Double)()
            Dim node_aux = attributes.Count * markovLagField + node
            Dim c As MutableConfiguration = New MutableConfiguration(attributes, markovLagField, config)
            Dim indexParameters = c.applyMask(parentNodes(node), node)
            If parametersField(node)(indexParameters).Count - 1 < config(node_aux) Then
                For i = 0 To attributes(node).size() - 1 - 1
                    prob += parametersField(node)(indexParameters)(i)
                Next
                prob = 1 - prob
                aux.Add(prob)
            Else
                prob = parametersField(node)(indexParameters)(config(node_aux))
                aux.Add(prob)
            End If
            '		System.out.println(" Prob: " + prob);
            Return aux
        End Function

        Private Function generateProbabilities(numValues As Integer) As IList(Of Double)
            Dim values As IList(Of Double) = New List(Of Double)(numValues)
            Dim probabilities As IList(Of Double)

            ' uniform sampling from [0,1[, more info at
            ' http://cs.stackexchange.com/questions/3227/uniform-sampling-from-a-simplex
            ' http://www.cs.cmu.edu/~nasmith/papers/smith+tromble.tr04.pdf
            ' generate n-1 random values in [0,1[, sort them
            ' use the distances between adjacent values as probabilities
            If numValues > 2 Then
                values.Add(0.0)
                Dim j = numValues - 1

                While Math.Max(Threading.Interlocked.Decrement(j), j + 1) > 0
                    values.Add(r.NextDouble())
                End While

                values = New List(Of Double)(values.Sort())

                probabilities = New List(Of Double)(numValues - 1)
                For j = 0 To numValues - 1 - 1
                    probabilities.Add(values(j + 1) - values(j))
                Next
            ElseIf numValues = 2 Then
                probabilities = New List(Of Double) From {
                    r.NextDouble()
                }
            Else
                probabilities = New List(Of Double) From {
                    1.0
                }
            End If
            Return probabilities
        End Function

        ''' <summary>
        ''' Calculates what is the next observation.
        ''' </summary>
        ''' <param name="previousObservation"> Previous Observation. </param>
        ''' <param name="mostProbable">        If true assigns the most probable values </param>
        ''' <returns> int[] Returns the next observation. </returns>
        Public Overridable Function nextObservation(previousObservation As Integer(), mostProbable As Boolean) As Integer()
            Dim c As MutableConfiguration = New MutableConfiguration(attributes, markovLagField, previousObservation)
            For Each node In topologicalOrder

                '			System.out.println("Node: " + node + " parents: " + parentNodes.get(node));
                '			System.out.println("previousOBS: " + Arrays.toString(previousObservation));
                '			System.out.println("Teste: " + Arrays.toString(c.configuration) );
                Dim indexParameters = c.applyMask(parentNodes(node), node)
                Dim probabilities = parametersField(node)(indexParameters)
                '			System.out.println("Probs: " + parameters.get(node));
                '			System.out.println("indexParameters "+ Arrays.toString(indexParameters.configuration));
                '			System.out.println("probabilities "+probabilities);
                '			
                ' System.out.println("Observation "+Arrays.toString(previousObservation));
                ' System.out.println("Attributes "+attributes);

                Dim size = probabilities.Count
                Dim value As Integer

                If mostProbable Then
                    Dim maxIndex = -1
                    Dim max As Double = 0
                    Dim sum As Double = 0
                    For i = 0 To size - 1
                        Dim p = probabilities(i)
                        sum += p
                        If max < p Then
                            max = p
                            maxIndex = i
                        End If
                    Next
                    If max < 1 - sum Then
                        maxIndex = size
                    End If

                    ' random sampling
                    value = maxIndex
                Else
                    Dim sample As Double = r.NextDouble()

                    Dim accum = probabilities(0)
                    value = 0

                    While sample > accum
                        If Not value < size - 1 Then
                            Threading.Interlocked.Increment(value)
                            Exit While
                        End If
                        accum += probabilities(Threading.Interlocked.Increment(value))
                    End While
                End If

                c.update(node, value)
            Next
            Dim n = attributes.Count
            Return copyOfRange(c.toArray(), markovLagField * n, (markovLagField + 1) * n)
        End Function

        Public Shared Function compare(original As BayesNet, recovered As BayesNet) As Double()
            Return compare(original, recovered, False)
        End Function

        ''' <summary>
        ''' Compares a network that was learned from observations (recovered) with the
        ''' original network used to generate those observations.
        ''' </summary>
        ''' <param name="original">  The original BN. </param>
        ''' <param name="recovered"> The recovered BN. </param>
        ''' <param name="verbose">   If set, prints net comparison. </param>
        ''' <returns> int[] Returns the precision, recall and f1 scores in the format
        '''         [precision, recall, f1]. </returns>
        Public Shared Function compare(original As BayesNet, recovered As BayesNet, verbose As Boolean) As Double()
            ' intra edges only, assume graph is a tree

            Diagnostics.Debug.Assert(original.attributes Is recovered.attributes)
            Dim n = original.attributes.Count
            ' maxParents
            ' assert (original.maxParents == recovered.maxParents);

            Dim parentNodesTruePositive As IList(Of IList(Of Integer)) = New List(Of IList(Of Integer))(n)
            For i = 0 To n - 1
                parentNodesTruePositive.Add(New List(Of Integer)(original.parentNodes(i)))
                parentNodesTruePositive(i).retainAll(recovered.parentNodes(i))
            Next

            Dim truePositive = 0
            Dim conditionPositive = 0
            Dim testPositive = 0
            For i = 0 To n - 1
                truePositive += parentNodesTruePositive(i).Count
                conditionPositive += original.parentNodes(i).Count
                testPositive += recovered.parentNodes(i).Count
            Next

            Dim precision = 1.0 * truePositive / testPositive
            Dim recall = 1.0 * truePositive / conditionPositive
            Dim f1 = 2 * precision * recall / (precision + recall)

            If verbose Then

                Console.WriteLine("Original network (" & conditionPositive.ToString() & ")")
                For i = 0 To n - 1
                    Console.Write(i.ToString() & ": ")
                    Console.WriteLine(original.parentNodes(i))
                Next

                Console.WriteLine("Learnt network (" & testPositive.ToString() & ")")
                For i = 0 To n - 1
                    Console.Write(i.ToString() & ": ")
                    Console.WriteLine(recovered.parentNodes(i))
                Next

                Console.WriteLine("In common (" & truePositive.ToString() & ")")
                For i = 0 To n - 1
                    Console.Write(i.ToString() & ": ")
                    Console.WriteLine(parentNodesTruePositive(i))
                Next

                Console.WriteLine("Precision = " & precision.ToString())
                Console.WriteLine("Recall  = " & recall.ToString())
                Console.WriteLine("F1 = " & f1.ToString())
            End If
            ' { truePositive, conditionPositive, testPositive };

            Return New Double() {precision, recall, f1}

        End Function

        Public Overridable ReadOnly Property MarkovLag As Integer
            Get
                Return markovLagField
            End Get
        End Property

        Public Overridable Function toDot(t As Integer, compactFormat As Boolean) As String

            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","

            Dim n = attributes.Count
            Dim presentSlice = t + markovLagField

            If compactFormat Then
                For head = 0 To n - 1
                    For Each tail As Integer? In parentNodesPerSlice(0)(head)
                        sb.Append("X" & tail.Value.ToString() & " -> " & "X" & head.ToString() & ls)
                    Next
                Next
            Else
                For ts = 0 To markovLagField + 1 - 1
                    Dim parentNodesOneSlice = parentNodesPerSlice(ts)
                    Dim slice = t + ts
                    For head = 0 To n - 1
                        For Each tail As Integer? In parentNodesOneSlice(head)
                            sb.Append("X" & tail.Value.ToString() & "_" & slice.ToString() & " -> " & "X" & head.ToString() & "_" & presentSlice.ToString() & ls)
                        Next
                    Next
                    sb.Append(ls)
                Next
            End If

            Return sb.ToString()
        End Function

        Public Overloads Function ToString(t As Integer, printParameters As Boolean) As String

            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","


            Dim n = attributes.Count
            Dim presentSlice = t + markovLagField

            For ts = 0 To markovLagField + 1 - 1
                Dim parentNodesOneSlice = parentNodesPerSlice(ts)
                Dim slice = t + ts
                For head = 0 To n - 1
                    For Each tail In parentNodesOneSlice(head)
                        sb.Append(attributes(tail).Name & "[" & slice.ToString() & "] -> " & attributes(head).Name & "[" & presentSlice.ToString() & "]" & ls)
                    Next
                Next
                sb.Append(ls)
            Next

            If printParameters Then
                sb.Append(ls)

                For i = 0 To n - 1
                    sb.Append(attributes(i).Name & ": " & attributes(i).ToString() & ls)
                    Dim cpt = parametersField(i)
                    Dim iter As IEnumerator(Of KeyValuePair(Of Configuration, IList(Of Double))) = SetOfKeyValuePairs(Of Configuration, Global.System.Collections.Generic.IList(Of Global.System.[Double]))(cpt).GetEnumerator()
                    While iter.MoveNext()
                        Dim e = iter.Current
                        sb.Append(e.Key.ToString())

                        sb.Append(": ")

                        Dim probabilities = e.Value
                        Dim sum As Double = 1
                        For Each p In probabilities
                            sb.Append(p.ToString() & " ")
                            sum -= p
                        Next
                        sb.Append(If(sum < 0, 0, sum.ToString()))

                        sb.Append(ls)
                    End While
                    sb.Append(ls)
                Next
            End If

            Return sb.ToString()
        End Function

        Public Overrides Function ToString() As String
            Return ToString(0, False)
        End Function

    End Class

End Namespace
