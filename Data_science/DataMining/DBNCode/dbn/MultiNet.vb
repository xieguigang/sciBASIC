Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace dbn



    Public Class MultiNet

        Private networksField As IList(Of DynamicBayesNet)
        Private o As Observations
        Private clustering As Double()()()
        Private is_bcDBN As Boolean
        Private is_cDBN As Boolean
        Private spanning As Boolean
        Private intra_ind As Integer
        Private root As Integer
        Private maxParents As Integer
        Private stationaryProcess As Boolean
        Private multithread As Boolean


        ''' <param name="o"> Observations to cluster </param>
        ''' <param name="numClusters"> Number of clusters  </param>
        Public Sub New(o As Observations, numClusters As Integer, is_bcDBN As Boolean, is_cDBN As Boolean, spanning As Boolean, intra_ind As Integer, root As Integer, maxParents As Integer, stationaryProcess As Boolean, multithread As Boolean)
            MyBase.New()
            Me.o = o
            Dim numSubjects = o.NumSubjects
            Dim numTransitions = o.NumTransitionsProp
            Me.is_bcDBN = is_bcDBN
            Me.is_cDBN = is_cDBN
            Me.spanning = spanning
            Me.intra_ind = intra_ind
            Me.root = root
            Me.maxParents = maxParents
            Me.stationaryProcess = stationaryProcess
            Me.multithread = multithread

            Dim s As Scores
            Dim dbn As DynamicBayesNet

            networksField = New List(Of DynamicBayesNet)(numClusters)
            clustering = RectangularArray.Cubic(Of Double)(numTransitions, numSubjects, numClusters)

            For i = 0 To numClusters - 1
                s = New Scores(o, Me.maxParents, stationaryProcess, True, multithread)
                s.evaluate(New RandomScoringFunction())

                If Me.is_bcDBN Then
                    dbn = s.to_bcDBN(New RandomScoringFunction(), Me.intra_ind)
                ElseIf Me.is_cDBN Then
                    dbn = s.to_cDBN(New RandomScoringFunction(), Me.intra_ind)
                Else
                    dbn = s.toDBN(Me.root, Me.spanning)
                End If

                dbn.generateParameters()
                networksField.Add(dbn)
            Next
            clustering = computeClusters(networksField, stationaryProcess, False)
        End Sub

        Private Function computeClusters(net As IList(Of DynamicBayesNet), stationaryProcess As Boolean, mostProbable As Boolean) As Double()()()
            Dim observations = o.ObservationsMatrix
            Dim numSubjects = o.NumSubjects
            Dim numTransitions = o.NumTransitionsProp
            Dim numAttributes = o.Attributes.Count
            Dim numClusters = net.Count
            Dim probabilityAux As Double
            Dim probabilitySum As Double
            Dim probabilityMax As Double

            Dim newClustering = RectangularArray.Cubic(Of Double)(numTransitions, numSubjects, numClusters)
            Dim cluster As Integer
            Dim decimal_places As Double = 5
            Dim epsilon = Math.Pow(10, -decimal_places)
            Dim max_cluster As Integer
            Dim alpha = getAlpha(clustering)


            For s = 0 To numSubjects - 1
                probabilityMax = Double.NegativeInfinity
                cluster = 0
                max_cluster = 0
                For Each dbn As DynamicBayesNet In net
                    probabilityAux = 0
                    For t = 0 To numTransitions - 1
                        For n = 0 To numAttributes - 1
                            If stationaryProcess Then
                                probabilityAux += Math.Log(dbn.transitionNets(0).getParameters(n, observations(t)(s))(0))
                            Else
                                probabilityAux += Math.Log(dbn.transitionNets(t).getParameters(n, observations(t)(s))(0))
                            End If
                        Next
                    Next

                    For t = 0 To numTransitions - 1
                        newClustering(t)(s)(cluster) = probabilityAux
                    Next
                    If probabilityMax < probabilityAux Then
                        probabilityMax = probabilityAux
                        max_cluster = cluster
                    End If
                    cluster += 1
                Next

                For c = 0 To numClusters - 1
                    For t = 0 To numTransitions - 1
                        If newClustering(t)(s)(c) - probabilityMax >= Math.Log(epsilon) - Math.Log(numClusters) Then
                            newClustering(t)(s)(c) = Math.Exp(newClustering(t)(s)(c) - probabilityMax)
                        Else
                            newClustering(t)(s)(c) = 0
                        End If
                    Next
                Next

                probabilitySum = 0
                For c = 0 To numClusters - 1
                    probabilitySum += alpha(c) * Math.Ceiling(newClustering(0)(s)(c) * 1000000000000000R) / 1000000000000000R

                Next
                For c = 0 To numClusters - 1
                    For t = 0 To numTransitions - 1
                        newClustering(t)(s)(c) *= alpha(c) / probabilitySum
                    Next
                Next

                If mostProbable Then
                    For c = 0 To numClusters - 1
                        For t = 0 To numTransitions - 1
                            newClustering(t)(s)(c) = 0
                            If c = max_cluster Then
                                newClustering(t)(s)(c) = 1
                            End If
                        Next
                    Next
                Else
                    Dim r As Random = New Random()
                    Dim clust = 0
                    Dim probsum As Double = 0
                    Dim prob As Double = 0 + (1 - 0) * r.NextDouble()
                    For c = 0 To numClusters - 1
                        probsum += newClustering(0)(s)(c)
                        If probsum >= prob Then
                            clust = c
                            Exit For
                        End If
                    Next
                    For c = 0 To numClusters - 1
                        For t = 0 To numTransitions - 1
                            newClustering(t)(s)(c) = 0
                            If c = clust Then
                                newClustering(t)(s)(c) = 1
                            End If
                        Next
                    Next
                End If
            Next
            Return newClustering
        End Function

        Private Function getAlpha(counts As Double()()()) As Double()
            Dim alpha As Double()
            Dim numSubjects = o.NumSubjects
            Dim numClusters = networksField.Count
            Dim subSum As Double
            Dim sum As Double = 0

            alpha = New Double(numClusters - 1) {}
            For c = 0 To numClusters - 1
                subSum = 0
                For s = 0 To numSubjects - 1
                    subSum += counts(0)(s)(c)
                Next
                alpha(c) = subSum / numSubjects
                sum += alpha(c)
            Next
            If sum = 0 Then
                For c = 0 To numClusters - 1
                    alpha(c) = 1.0 / numClusters
                Next
            End If

            Return alpha
        End Function

        Private Function selectCluster(counts As Double()()(), cluster As Integer) As Double()()
            Dim numSubjects = o.NumSubjects
            Dim numTransitions = o.NumTransitionsProp

            Dim clusteringNew = RectangularArray.Matrix(Of Double)(numTransitions, numSubjects)

            For t = 0 To numTransitions - 1
                For s = 0 To numSubjects - 1
                    clusteringNew(t)(s) = counts(t)(s)(cluster)
                Next
            Next
            Return clusteringNew
        End Function



        Public Overridable ReadOnly Property Networks As IList(Of DynamicBayesNet)
            Get
                Return networksField
            End Get
        End Property

        Private Function trainNetworks(counts As Double()()()) As IList(Of DynamicBayesNet)
            Dim s As Scores
            Dim dbn As DynamicBayesNet
            Dim oNew As Observations
            Dim clust As Double()()
            Dim numClusters = networksField.Count
            Dim obs = o.ObservationsMatrix
            Dim attributes = o.Attributes
            Dim networksNew As IList(Of DynamicBayesNet) = New List(Of DynamicBayesNet)(numClusters)


            For c = 0 To numClusters - 1
                clust = selectCluster(counts, c)
                oNew = New Observations(attributes, obs, clust)
                s = New Scores(oNew, maxParents, stationaryProcess, False, multithread)
                s.evaluate(New LLScoringFunction())
                If is_bcDBN Then
                    dbn = s.to_bcDBN(New LLScoringFunction(), intra_ind)
                ElseIf is_cDBN Then
                    dbn = s.to_cDBN(New LLScoringFunction(), intra_ind)
                Else
                    dbn = s.toDBN(root, spanning)
                End If
                dbn.learnParameters(oNew, stationaryProcess)
                networksNew.Add(dbn)
            Next
            Return networksNew
        End Function

        Public Overridable Sub clust()
            Dim numClusters = networksField.Count
            Dim networkPrev = networksField
            Dim networkNew = networksField
            Dim counts = clustering
            Dim score = getScore(networkNew, counts, stationaryProcess)
            Dim score_prev = Double.NegativeInfinity
            Dim mostprobable = False
            Dim it = 0
            'System.out.println("Score: " + score);
            If Not mostprobable Then
                Console.WriteLine("Starting with stochastic EM.")
            Else
                Console.WriteLine("Starting with classification EM.")
            End If
            While score > score_prev
                If it >= 100 And Not mostprobable Then
                    mostprobable = True
                    Console.WriteLine("Changing to classification EM.")
                End If

                networkPrev = networkNew
                score_prev = score
                networkNew = trainNetworks(counts)
                counts = computeClusters(networkNew, stationaryProcess, mostprobable)
                score = getScore(networkNew, counts, stationaryProcess)
                it += 1
                '			System.out.println("Score: " + score  + " it: " + it);
            End While
            networksField = networkPrev
            clustering = computeClusters(networkPrev, True, False)
        End Sub

        Public Overridable ReadOnly Property BICScore As Double
            Get
                Dim oNew As Observations
                Dim attributes = o.Attributes
                Dim obs = o.ObservationsMatrix
                Dim c = 0
                Dim score As Double = 0
                Dim numParam As Double = 0
                Dim clust As Double()()


                For Each dbn As DynamicBayesNet In networksField
                    clust = selectCluster(clustering, c)
                    oNew = New Observations(attributes, obs, clust)
                    score += 2 * dbn.getScore(oNew, New LLScoringFunction(), stationaryProcess)
                    numParam += dbn.getNumberParameters(oNew)
                    c += 1
                Next

                '		System.out.println("LL: " + score + " Penalizing Term: " + Math.log(o.getNumSubjects()));
                score -= numParam * Math.Log(o.NumSubjects)
                Return score
            End Get
        End Property

        Public Overridable Function getScore(net As IList(Of DynamicBayesNet), clustering As Double()()(), stationaryProcess As Boolean) As Double
            Dim oNew As Observations
            Dim numSubjects = o.NumSubjects
            Dim obs = o.ObservationsMatrix
            Dim attributes = o.Attributes
            Dim numClusters = networksField.Count
            Dim alpha As Double()
            Dim clust As Double()()
            Dim netscore1 As Double = 0
            Dim netscore2 As Double = 0
            Dim c = 0
            Dim count As Double

            alpha = getAlpha(clustering)
            For Each dbn As DynamicBayesNet In net
                clust = selectCluster(clustering, c)
                oNew = New Observations(attributes, obs, clust)
                netscore1 += dbn.getScore(oNew, New LLScoringFunction(), stationaryProcess)
                c += 1
            Next

            For c = 0 To numClusters - 1
                count = 0
                For s = 0 To numSubjects - 1
                    count += clustering(0)(s)(c)
                Next
                netscore2 += count * Math.Log(alpha(c))
            Next

            Return netscore1 + netscore2
        End Function

        Public Overridable Sub writeToFile(outFileName As String)
            'CSVWriter writer;
            'int cluster = 0;
            'double prob;
            'double[] probs;
            'int numCluster = this.networks.Count;
            'IDictionary<string, bool[]> subjectIsPresent = this.o.SubjectIsPresent;

            'try
            '{

            '    File outputFile = new File(outFileName);
            '    outputFile.createNewFile();

            '    writer = new CSVWriter(new StreamWriter(outputFile));


            '    int numSubjects = this.o.NumSubjects;

            '    // compose header line
            '    IList<string> headerEntries = new List<string>(2);
            '    headerEntries.Add("subject_id");
            '    headerEntries.Add("Class");

            '    // write header line to file
            '    writer.writeNext(((List<string>)headerEntries).ToArray());

            '    // iterator over subject ids
            '    IEnumerator<string> subjectIterator = subjectIsPresent.Keys.GetEnumerator();

            '    int passiveSubject = -1;
            '    for (int s = 0; s < numSubjects; s++)
            '    {
            '        prob = double.NegativeInfinity;
            '        probs = this.clustering[0][s];
            '        for (int c = 0; c < numCluster; c++)
            '        {
            '            if (prob < probs[c])
            '            {
            '                cluster = c;
            '                prob = probs[c];
            '            }
            '        }

            '        IList<string> subjectEntries = new List<string>(2);

            '        // add subject id
            '        while (subjectIterator.MoveNext())
            '        {
            '            string subject = subjectIterator.Current;
            '            passiveSubject++;
            '            if (subjectIsPresent[subject][0])
            '            {
            '                subjectEntries.Add(subject);
            '                break;
            '            }
            '        }

            '        subjectEntries.Add(Convert.ToString(cluster));


            '        // write subject line to file
            '        writer.writeNext(((List<string>)subjectEntries).ToArray());

            '    }

            '    writer.Dispose();

            '}
            'catch (IOException e)
            '{
            '    Console.Error.WriteLine("Could not write to " + outFileName + ".");
            '    Console.WriteLine(e.ToString());
            '    Console.Write(e.StackTrace);
            '    Environment.Exit(1);
            '}

        End Sub

        Public Overrides Function ToString() As String
            Dim numClusters = networksField.Count
            Dim numSubjects = o.NumSubjects
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = "line.separator"

            sb.Append("Number of clusters : " & numClusters.ToString() & ls)
            sb.Append("Number of Observations : " & numSubjects.ToString() & ls & ls)


            Dim alpha = getAlpha(clustering)
            For c = 0 To numClusters - 1
                sb.Append("--- Cluster " & c.ToString() & " ---" & ls)
                '			for(int s = 0; s < numSubjects; s++) {
                '				sb.append(clustering[0][s][c] + ls);
                '			}
                sb.Append(networksField(c).ToString())
                sb.Append(ls & "Alpha: " & alpha(c).ToString() & ls)
            Next

            '		double score = getScore(networks, clustering);
            '		sb.append(ls + "Final Score: " + score + ls);

            Return sb.ToString()
        End Function



    End Class
End Namespace
