Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Java
Imports std = System.Math

Namespace dbn

    Public Class CrossValidation
        Private InstanceFieldsInitialized As Boolean = False

        Private Sub InitializeInstanceFields()
            r = New Random(randomSeedField)
        End Sub


        Friend randomSeedField As Integer = New Random().Next()
        Private r As Random

        Private o As Observations

        Private allData As Integer()()

        Private allPassiveData As String()()

        ' contains an extra column for fold identification
        Private stratifiedData As IList(Of Integer()())

        Private stratifiedPassiveData As IList(Of String()())

        Private numFolds As Integer

        Private Class Pair
            Private ReadOnly outerInstance As CrossValidation

            Friend a, b As Integer

            Friend Sub New(outerInstance As CrossValidation, a As Integer, b As Integer)
                Me.outerInstance = outerInstance
                Me.a = a
                Me.b = b
            End Sub
        End Class

        Public Overridable Function setRandomSeed(randomSeed As Integer) As CrossValidation
            randomSeedField = randomSeed
            r = New Random(randomSeed)
            Return Me
        End Function

        Public Overridable ReadOnly Property RandomSeed As Long
            Get
                Return randomSeedField
            End Get
        End Property

        Private Function countInstancesOfFold(fold As Integer) As Pair
            Dim n As Integer = o.numAttributes()
            Dim m = o.MarkovLag
            Dim countFold = 0
            Dim countNonFold = 0
            For c = 0 To stratifiedData.Count - 1
                For Each row In stratifiedData(c)
                    If row((m + 1) * n) = fold Then
                        countFold += 1
                    Else
                        countNonFold += 1
                    End If
                Next
            Next

            Return New Pair(Me, countFold, countNonFold)
        End Function

        Private Function calculateFoldIds(numInstances As Integer, numFolds As Integer) As IList(Of Integer)
            Dim foldIds As IList(Of Integer) = New List(Of Integer)(numInstances)

            Dim minFoldSize As Integer = numInstances / numFolds
            Dim rest = numInstances Mod numFolds

            For i = 0 To numFolds - 1
                For j = 0 To minFoldSize - 1
                    foldIds.Add(i)
                Next
            Next

            For i = 0 To rest - 1
                foldIds.Add(i)
            Next

            Collections.shuffle(foldIds, r)

            Return foldIds
        End Function

        Private Function countInstancesOfClass(classAttribute As Integer, value As Integer) As Integer
            Dim n As Integer = o.numAttributes()
            Dim m = o.MarkovLag
            Dim count = 0
            For i = 0 To allData.Length - 1
                If allData(i)(m * n + classAttribute) = value Then
                    count += 1
                End If
            Next
            Return count
        End Function

        Public Sub New(o As Observations, numFolds As Integer, classAttribute As Integer?)
            If Not InstanceFieldsInitialized Then
                InitializeInstanceFields()
                InstanceFieldsInitialized = True
            End If

            Me.o = o

            ' initialize allData
            Dim N = o.numObservations(-1)
            Dim lN As Integer = o.numAttributes()
            Dim lT As Integer = o.numTransitions()
            Dim m = o.MarkovLag
            Dim nPassive As Integer = o.numPassiveAttributes()

            allData = RectangularArray.Matrix(Of Integer)(N, (m + 1) * lN)
            allPassiveData = RectangularArray.Matrix(Of String)(N, (m + 1) * nPassive)

            Dim usefulObservations = o.ObservationsMatrix
            Dim passiveObservations = o.PassiveObservationsMatrix
            Dim i = 0
            For t = 0 To lT - 1
                For j = 0 To o.numObservations(t) - 1
                    allData(i) = usefulObservations(t)(j)
                    allPassiveData(i) = passiveObservations(t)(j)
                    i += 1
                Next
            Next

            ' stratify data
            If classAttribute IsNot Nothing Then

                Dim classRange As Integer = o.Attributes(classAttribute).size()

                stratifiedData = New List(Of Integer()())(classRange)

                stratifiedPassiveData = New List(Of String()())(classRange)

                For c = 0 To classRange - 1
                    stratifiedData.Add(RectangularArray.Matrix(Of Integer)(countInstancesOfClass(classAttribute, c), (m + 1) * lN + 1))
                    stratifiedPassiveData.Add(RectangularArray.Matrix(Of String)(countInstancesOfClass(classAttribute, c), (m + 1) * nPassive))
                    Dim classData = stratifiedData(c)
                    Dim classPassiveData = stratifiedPassiveData(c)
                    i = 0
                    For j = 0 To allData.Length - 1
                        Dim row = allData(j)
                        If row(m * lN + classAttribute) = c Then
                            classData(i) = copyOf(row, (m + 1) * lN + 1)
                            classPassiveData(i) = copyOf(allPassiveData(j), (m + 1) * nPassive)
                            i += 1
                        End If
                    Next

                Next
            Else
                stratifiedData = New List(Of Integer()())(1)
                stratifiedPassiveData = New List(Of String()())(1)
                stratifiedData.Add(RectangularArray.Matrix(Of Integer)(N, (m + 1) * lN + 1))
                Dim data = stratifiedData(0)

                stratifiedPassiveData.Add(allPassiveData)

                For j = 0 To allData.Length - 1
                    data(j) = copyOf(allData(j), (m + 1) * lN + 1)
                Next
            End If

            ' determining folds
            Me.numFolds = numFolds
            If numFolds > 0 Then
                Dim foldIds = calculateFoldIds(N, numFolds)

                i = 0
                For Each classData In stratifiedData
                    For j = 0 To classData.Length - 1
                        classData(j)((m + 1) * lN) = foldIds(std.Min(Threading.Interlocked.Increment(i), i - 1))
                    Next
                Next
            End If

        End Sub

        Private Function evaluateFold(train As Observations, test As Observations, numParents As Integer, s As ScoringFunction, dotOutput As Boolean, dotFileName As String, mostProbable As Boolean, ckg As Boolean, k As Integer) As Observations
            ' System.out.println("initializing scores");
            Dim s1 As Scores = New Scores(train, numParents, True, True)
            ' System.out.println("evaluating scores");
            s1.evaluate(s)
            Dim dbn1 As DynamicBayesNet

            If ckg Then
                dbn1 = s1.to_bcDBN(s, k)
            Else
                dbn1 = s1.toDBN()
            End If


            'System.out.println("Attributes "+dbn1.getAttributes());

            ' System.out.println("converting to DBN");

            If dotOutput Then
                Try
                    utils.Utils.writeToFile(dotFileName & ".dot", dbn1.toDot(False))
                Catch e As Exception
                    Console.WriteLine(e.ToString())
                    Console.Write(e.StackTrace)
                End Try
            End If

            Dim params = dbn1.learnParameters(train, True)

            If dotOutput Then
                ' for (Attribute a : train.getAttributes()) {
                ' System.out.print(a.getName() + ": ");
                ' System.out.println(a);
                ' }

                ' System.out.println(params);
                Return Nothing
            Else
                'System.out.println("testing network");
                Return dbn1.forecast(test, 1, True, mostProbable)
            End If

        End Function





        Public Overridable Function evaluate2(numParents As Integer, s As ScoringFunction, outputFileName As String, forecastAttributes As IList(Of Integer), mostProbable As Boolean, ckg As Boolean, k_ckg As Integer) As String


            Dim n As Integer = o.numAttributes()
            Dim nPassive As Integer = o.numPassiveAttributes()
            Dim m = o.MarkovLag
            Dim trainingData_0 As Integer()()()
            Dim trainingData_1 As Integer()()()
            Dim testData As Integer()()()
            Dim testPassiveData As String()()


            Dim output As StringBuilder = New StringBuilder()
            Dim ls = ","

            output.Append(randomSeedField.ToString() & ls)
            For Each predictor In forecastAttributes
                output.Append(o.Attributes(predictor).Name & vbTab)
            Next
            output.Append(vbTab & "actual_value" & ls)

            Dim accu As Double = 0
            Dim pre As Double = 0
            Dim rec As Double = 0
            Dim auc As Double = 0

            For f = 0 To numFolds - 1


                Dim accu_f As Double = 0
                Dim pre_f As Double = 0
                Dim rec_f As Double = 0
                Dim auc_f As Double = 0

                Dim clas = New Double(3) {}


                Dim fold = f + 1
                Console.WriteLine("Fold " & fold.ToString())

                Dim counts = countInstancesOfFold(f)
                Dim testSize = counts.a
                Dim trainingSize = counts.b

                trainingData_0 = RectangularArray.Cubic(Of Integer)(1, trainingSize, (m + 1) * n)

                trainingData_1 = RectangularArray.Cubic(Of Integer)(1, trainingSize, (m + 1) * n)

                testData = RectangularArray.Cubic(Of Integer)(1, testSize, (m + 1) * n)
                testPassiveData = RectangularArray.Matrix(Of String)(testSize, (m + 1) * nPassive)

                Console.WriteLine("Training size: " & trainingSize.ToString() & vbTab & "Test size: " & testSize.ToString())


                Console.WriteLine("size stratified data " & stratifiedData.Count.ToString())

                Dim i = 0
                Dim j = 0
                For c = 0 To stratifiedData.Count - 1
                    Dim classData = stratifiedData(c)
                    Dim classPassiveData = stratifiedPassiveData(c)
                    For k = 0 To classData.Length - 1
                        Dim row = classData(k)
                        If row((m + 1) * n) = f Then
                            testData(0)(i) = copyOf(row, (m + 1) * n)
                            testPassiveData(i) = copyOf(classPassiveData(k), (m + 1) * nPassive)
                            i += 1
                        Else
                            If c = 0 Then
                                trainingData_0(0)(std.Min(Threading.Interlocked.Increment(j), j - 1)) = CopyOf(row, (m + 1) * n)
                            Else
                                trainingData_1(0)(std.Min(Threading.Interlocked.Increment(j), j - 1)) = CopyOf(row, (m + 1) * n)
                            End If
                        End If
                    Next
                Next

                o.change0()

                Dim train_0 As Observations = New Observations(o, trainingData_0)
                Dim train_1 As Observations = New Observations(o, trainingData_1)

                Dim test As Observations = New Observations(o, testData)

                'Observations forecast = evaluateFold(train, test, numParents, s,false, null, mostProbable,ckg,k_ckg);


                Dim s0 As Scores = New Scores(train_0, numParents, True, True)

                Dim s1 As Scores = New Scores(train_1, numParents, True, True)


                s0.evaluate(s)
                s1.evaluate(s)

                Dim dbn0 As DynamicBayesNet

                Dim dbn1 As DynamicBayesNet

                ' System.out.println("evaluating scores");
                's1.evaluate(s);


                If ckg Then

                    ' Criar duas redes: transição 0=>0 e 0=>1


                    dbn0 = s0.to_bcDBN(s, k_ckg)

                    dbn1 = s1.to_bcDBN(s, k_ckg)
                Else
                    dbn0 = s0.toDBN()
                    dbn1 = s1.toDBN()
                End If



                Console.WriteLine(dbn0.ToString())
                Console.WriteLine(dbn1.ToString())


                dbn0.learnParameters(train_0)
                dbn1.learnParameters(train_1)




                'System.out.println(dbn0.transitionNets.get(0).getParameters().get(0));



                Console.WriteLine("-----------------------------------------------")


                'System.out.println(dbn1.transitionNets.get(0).getParameters().get(0));







                output.Append("---Fold-" & fold.ToString() & "---" & ls)

                Dim p0 As Double? = CDbl(1)

                Dim p1 As Double? = CDbl(1)



                For i = 0 To testSize - 1

                    Dim c0 As MutableConfiguration = New MutableConfiguration(dbn0.Attributes, 1, copyOfRange(testData(0)(i), 0, 18))


                    'for(BayesNet BN:dbn0.getTrans()) {

                    'System.out.println(BN.getTop());


                    Console.WriteLine(dbn0.Init.ToString())



                    For Each node In dbn0.Init.Top

                        Console.WriteLine("Node " & node.ToString())




                        Console.WriteLine("Attributes " & dbn0.Attributes.ToString())



                        Dim indexParameters = c0.applyMask(dbn0.Init.Parents(node), node)


                        Console.WriteLine("indexParameters " & indexParameters.ToString())


                        Console.WriteLine("One " & Arrays.toString(copyOfRange(testData(0)(i), 0, 18)))
                        'System.out.println("Two " + Arrays.toString(Arrays.copyOfRange(testData[0][i],18,36)));
                        'System.out.println("Three " + Arrays.toString(Arrays.copyOfRange(testData[0][i],18,34)));
                        'System.out.println("Four " + Arrays.toString(Arrays.copyOfRange(testData[0][i],18,37)));
                        Console.WriteLine(dbn0.Init.Parameters(node)(indexParameters))


                        'Double probability = BN.getParameters().get(node).get(indexParameters).get(testData[0][i][18+node]);

                        'p0=p0*probability;


                    Next





                    '}



                    Dim c1 As MutableConfiguration = New MutableConfiguration(dbn1.Attributes, 1, testData(0)(i))



                    For Each BN In dbn1.Trans

                        For Each node In BN.Top


                            Dim indexParameters = c1.applyMask(BN.Parents(node), node)




                            Dim probability As Double? = BN.Parameters(node)(indexParameters)(testData(0)(i)(node))

                            p1 = p1.Value * probability.Value


                        Next





                    Next

                    Dim p = -1



                    If p0 >= p1 Then
                        p = 0
                    Else
                        p = 1
                    End If


                    output.Append(p.ToString() & vbTab)



                    output.Append(vbTab)
                    Dim t = -1
                    'System.out.println("t "+testPassiveData[i][m * nPassive + 0]);
                    output.Append(testPassiveData(i)(m * nPassive + 0) & vbTab)

                    t = Integer.Parse(testPassiveData(i)(m * nPassive + 0))


                    If t = p AndAlso t = 2 Then
                        clas(0) += 1
                    End If

                    If t = p AndAlso t = 1 Then
                        clas(1) += 1
                    End If

                    If t <> p AndAlso t = 1 Then
                        clas(2) += 1
                    End If


                    If t <> p AndAlso t = 2 Then
                        clas(3) += 1
                    End If



                    output.Append(ls)
                Next

                Console.WriteLine("class  " & Arrays.toString(clas))




                accu_f = (clas(0) + clas(1)) / (clas(0) + clas(1) + clas(2) + clas(3))
                pre_f = clas(0) / (clas(0) + clas(2))
                rec_f = clas(0) / (clas(0) + clas(3))
                auc_f = 0.5 * (clas(0) / (clas(0) + clas(3)) + clas(1) / (clas(1) + clas(2)))

                Console.WriteLine("ACC " & accu_f.ToString())
                Console.WriteLine("PRE " & pre_f.ToString())
                Console.WriteLine("REC " & rec_f.ToString())
                Console.WriteLine("AUC " & auc_f.ToString())


                accu += accu_f
                pre += pre_f
                rec += rec_f
                auc += auc_f



            Next
            accu = accu / 10
            pre = pre / 10
            rec = rec / 10
            auc = auc / 10

            Console.WriteLine("Accuracy " & accu.ToString())
            Console.WriteLine("Precision " & pre.ToString())
            Console.WriteLine("Recall " & rec.ToString())
            Console.WriteLine("AUC " & auc.ToString())



            ' use all data for training and produce network graph

            ' System.out.println("---All-data---");
            ' 			Observations train = o;
            ' 			evaluateFold(train, null, numParents, s, true, outputFileName, mostProbable,ckg,k_ckg);
            ' 			output.append(ls);
            ' 	
            ' 			// output true values for baseline classifier
            ' 			for (int i = 0; i < allData.length; i++) {
            ' 				for (int j = 0; j < allPassiveData[0].length; j++)
            ' 					output.append(allPassiveData[i][j] + "\t");
            ' 				output.append(ls);
            ' 			}

            Return output.ToString()


        End Function























        Public Overridable Function evaluate(numParents As Integer, s As ScoringFunction, outputFileName As String, forecastAttributes As IList(Of Integer), mostProbable As Boolean, ckg As Boolean, k_ckg As Integer) As String


            Dim n As Integer = o.numAttributes()
            Dim nPassive As Integer = o.numPassiveAttributes()
            Dim m = o.MarkovLag
            Dim trainingData As Integer()()()
            Dim testData As Integer()()()
            Dim testPassiveData As String()()

            'double[] out = new double[3];


            'System.out.println("Random seed: " + randomSeed);
            'System.out.println("Number of observations: " + o.numObservations(-1));

            Dim output As StringBuilder = New StringBuilder()
            Dim ls = ","

            output.Append(randomSeedField.ToString() & ls)
            For Each predictor In forecastAttributes
                output.Append(o.Attributes(predictor).Name & vbTab)
            Next
            output.Append(vbTab & "actual_value" & ls)

            'System.out.println("Number of folds "+numFolds);






            Dim accu As Double = 0
            Dim pre As Double = 0
            Dim rec As Double = 0
            Dim auc As Double = 0

            For f = 0 To numFolds - 1


                Dim accu_f As Double = 0
                Dim pre_f As Double = 0
                Dim rec_f As Double = 0
                Dim auc_f As Double = 0

                Dim clas = New Double(3) {}


                Dim fold = f + 1
                Console.WriteLine("Fold " & fold.ToString())

                Dim counts = countInstancesOfFold(f)
                Dim testSize = counts.a
                Dim trainingSize = counts.b

                trainingData = RectangularArray.Cubic(Of Integer)(1, trainingSize, (m + 1) * n)
                testData = RectangularArray.Cubic(Of Integer)(1, testSize, (m + 1) * n)
                testPassiveData = RectangularArray.Matrix(Of String)(testSize, (m + 1) * nPassive)

                Console.WriteLine("Training size: " & trainingSize.ToString() & vbTab & "Test size: " & testSize.ToString())

                Dim i = 0
                Dim j = 0
                For c = 0 To stratifiedData.Count - 1
                    Dim classData = stratifiedData(c)
                    Dim classPassiveData = stratifiedPassiveData(c)
                    For k = 0 To classData.Length - 1
                        Dim row = classData(k)
                        If row((m + 1) * n) = f Then
                            testData(0)(i) = copyOf(row, (m + 1) * n)
                            testPassiveData(i) = copyOf(classPassiveData(k), (m + 1) * nPassive)
                            i += 1
                        Else
                            trainingData(0)(std.Min(Threading.Interlocked.Increment(j), j - 1)) = CopyOf(row, (m + 1) * n)
                        End If
                    Next
                Next



                Dim train As Observations = New Observations(o, trainingData)
                Dim test As Observations = New Observations(o, testData)

                Dim forecast = evaluateFold(train, test, numParents, s, False, Nothing, mostProbable, ckg, k_ckg)



                output.Append("---Fold-" & fold.ToString() & "---" & ls)



                For i = 0 To testSize - 1

                    Dim b = True
                    Dim fMatrix = forecast.ObservationsMatrix
                    Dim p = 0
                    'for (int predictor : forecastAttributes) {
                    'System.out.println("predictor "+predictor);


                    output.Append(o.Attributes(17).get(fMatrix(0)(i)(m * n + 17)) & vbTab)

                    If Equals(o.Attributes(17).get(fMatrix(0)(i)(m * n + 17)), Nothing) Then
                        b = False
                    End If

                    'System.out.println("p "+o.getAttributes().get(17).get(fMatrix[0][i][m * n + 17]));
                    If b Then
                        p = CInt(Double.Parse(o.Attributes(17).get(fMatrix(0)(i)(m * n + 17))))

                    End If


                    'System.out.println("p after "+p);


                    output.Append(vbTab)
                    Dim t = 0
                    'System.out.println("t "+testPassiveData[i][m * nPassive + 0]);
                    output.Append(testPassiveData(i)(m * nPassive + 0) & vbTab)


                    If ReferenceEquals(testPassiveData(i)(m * nPassive + 0), Nothing) Then
                        b = False
                    End If







                    'for (j = 0; j < nPassive; j++) {


                    'if(testPassiveData[i][m * nPassive + 0]==null) b=false;
                    If b Then
                        t = Integer.Parse(testPassiveData(i)(m * nPassive + 0)) '}


                    End If

                    'System.out.println("t after "+t);

                    'System.out.println("----------------------------------------");


                    If b Then
                        If t = p AndAlso t = 2 Then
                            clas(0) += 1
                        End If

                        If t = p AndAlso t = 1 Then
                            clas(1) += 1
                        End If

                        If t <> p AndAlso t = 1 Then
                            clas(2) += 1
                        End If


                        If t <> p AndAlso t = 2 Then
                            clas(3) += 1
                        End If

                    End If

                    output.Append(ls)
                Next

                Console.WriteLine("class  " & Arrays.toString(clas))




                accu_f = (clas(0) + clas(1)) / (clas(0) + clas(1) + clas(2) + clas(3))
                pre_f = clas(0) / (clas(0) + clas(2))
                rec_f = clas(0) / (clas(0) + clas(3))
                auc_f = 0.5 * (clas(0) / (clas(0) + clas(3)) + clas(1) / (clas(1) + clas(2)))

                Console.WriteLine("ACC " & accu_f.ToString())
                Console.WriteLine("PRE " & pre_f.ToString())
                Console.WriteLine("REC " & rec_f.ToString())
                Console.WriteLine("AUC " & auc_f.ToString())


                accu += accu_f
                pre += pre_f
                rec += rec_f
                auc += auc_f



            Next
            accu = accu / 10
            pre = pre / 10
            rec = rec / 10
            auc = auc / 10

            Console.WriteLine("Accuracy " & accu.ToString())
            Console.WriteLine("Precision " & pre.ToString())
            Console.WriteLine("Recall " & rec.ToString())
            Console.WriteLine("AUC " & auc.ToString())



            ' use all data for training and produce network graph

            ' System.out.println("---All-data---");
            ' 			Observations train = o;
            ' 			evaluateFold(train, null, numParents, s, true, outputFileName, mostProbable,ckg,k_ckg);
            ' 			output.append(ls);
            ' 	
            ' 			// output true values for baseline classifier
            ' 			for (int i = 0; i < allData.length; i++) {
            ' 				for (int j = 0; j < allPassiveData[0].length; j++)
            ' 					output.append(allPassiveData[i][j] + "\t");
            ' 				output.append(ls);
            ' 			}

            Return output.ToString()


        End Function


    End Class
End Namespace
