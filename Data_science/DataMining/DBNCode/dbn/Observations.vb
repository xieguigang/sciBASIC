#Region "Microsoft.VisualBasic::2d329b01613edca30b00ebd427b2eff7, Data_science\DataMining\DBNCode\dbn\Observations.vb"

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

    '   Total Lines: 1061
    '    Code Lines: 662 (62.39%)
    ' Comment Lines: 261 (24.60%)
    '    - Xml Docs: 59.00%
    ' 
    '   Blank Lines: 138 (13.01%)
    '     File Size: 49.00 KB


    '     Class Observations
    ' 
    '         Properties: Attributes, First, MarkovLag, NumSubjects, NumTransitionsProp
    '                     ObservationsMatrix, PassiveObservationsMatrix, SubjectIsPresent
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: count, countMissingValues, fillMissingValues, generateMissingValues, getCombinations
    '                   imputeMissingValues, numAttributes, numMissings, (+2 Overloads) numObservations, numPassiveAttributes
    '                   numTransitions, observationIsOk, parseNumTimeSlices, processHeader, ToString
    '                   toTimeSeriesHorizontal, toTimeSeriesVertical
    ' 
    '         Sub: change0, readFromFiles, (+2 Overloads) writeToFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Java

Namespace dbn

    ''' <summary>
    ''' @author ssam_
    ''' 
    ''' </summary>
    Public Class Observations

        ''' <summary>
        ''' Three-dimensional matrix of coded observation data which will be used for
        ''' learning a dynamic Bayesian network.
        ''' <ul>
        ''' <li>the 1st index refers to the transition {t - markovLag + 1, ...
        ''' ,t}->t+1;
        ''' <li>the 2nd index refers to the the subject (set of observed attributes);
        ''' <li>the 3rd index refers to the attribute and lies within the range [0,
        ''' (1 + markovLag)*n[, where [0, markovLag*n[ refers to attributes in the
        ''' past and [markovLag*n, (1 + markovLag)*n[ refers to attributes in time
        ''' t+1.
        ''' </ul>
        ''' </summary>    
        Private usefulObservations As Integer()()()


        ''' <summary>
        ''' Three-dimensional matrix of non-coded observation data that will be
        ''' present in the output, but not used for network learning.
        ''' <ul>
        ''' <li>the 1st index refers to the transition {t - markovLag + 1, ...
        ''' ,t}->t+1;
        ''' <li>the 2nd index refers to the the subject (set of observed attributes);
        ''' <li>the 3rd index refers to the (not for learning) attribute.
        ''' </ul>
        ''' </summary>
        Private passiveObservations As String()()() = Nothing

        ''' <summary>
        ''' Indicates, for each subject, what observations are present. Subject ID is
        ''' the key, a boolean array of size equal to the number of transitions is
        ''' the value.
        ''' </summary>
        Private subjectIsPresentField As IDictionary(Of String, Boolean())

        ''' <summary>
        ''' Each column of the useful observation data refers to an attribute.
        ''' </summary>
        Private attributesField As IList(Of Attribute)

        ''' <summary>
        ''' Number of subjects per transition. Only those who have complete data for
        ''' a transition are stored.
        ''' </summary>
        Private numSubjectsField As Integer()

        ''' <summary>
        ''' Number of subjects per transition that are missing.
        ''' </summary>
        Private numMissing As Integer()

        ''' <summary>
        ''' File that contains observations that will be converted to attributes and
        ''' from which one can learn a DBN.
        ''' </summary>
        Private usefulObservationsFileName As String

        ''' <summary>
        ''' File that contains observations that will be included unchanged in the
        ''' output. These are ignored when learning a DBN.
        ''' </summary>
        Private passiveObservationsFileName As String

        ''' <summary>
        ''' Header line of input useful observations CSV file.
        ''' </summary>
        Private usefulObservationsHeader As String()

        ''' <summary>
        ''' Header line of input passive observations CSV file.
        ''' </summary>
        Private passiveObservationsHeader As String() = New String(-1) {}

        ''' <summary>
        ''' Order of the Markov process, which is the number of previous time slices
        ''' that influence the values in the following slice. Default is first-order
        ''' Markov.
        ''' </summary>
        Private markovLagField As Integer = 1

        ''' <summary>
        ''' Number of subjects with a certain observation
        ''' </summary>
        Private counts As Double()()

        ''' <summary>
        ''' Default constructor when reading observations from a file.
        ''' </summary>
        ''' <seealso cref=""/> 
        Public Sub New(usefulObsFileName As String, markovLag As Integer)
            Me.New(usefulObsFileName, Nothing, markovLag)
        End Sub


        Public Overridable Sub change0()
            Dim n = attributesField.Count
            attributesField.RemoveAt(n - 1)
        End Sub


        ''' <summary>
        ''' Default constructor when reading observations from a file.
        ''' <para>
        ''' Input files format is be the following:
        ''' <ul>
        ''' <li>First row is the header
        ''' <li>Each header entry, except the first, is in the form
        ''' "attributeName__t", where t is the time slice
        ''' <li>First column is the subject ID
        ''' <li>One subject per line
        ''' <li>No incomplete observations, a subject can only miss entire time
        ''' slices.
        ''' </ul>
        ''' Input file example: <br>
        ''' <code>subject_id,"resp__1","age__1","resp__2","age__2","resp__3","age__3"<br>
        ''' 121013,0,65.0,0,67.0,0,67.0<br>
        ''' 121113,0,24.0,0,29.0,0,29.0<br>
        ''' 121114,0,9.0,0,7.0,0,0,7.0<br></code>
        ''' 
        ''' </para>
        ''' </summary>
        ''' <param name="usefulObsFileName">
        '''            File which contains observations that will be converted to
        '''            attributes and from which a DBN is learnt. </param>
        ''' <param name="passiveObsFileName">
        '''            File which contains observations that will be included
        '''            unchanged in the output. These are ignored when learning a
        '''            DBN. </param>  
        Public Sub New(usefulObsFileName As String, passiveObsFileName As String, markovLag As Integer?)
            usefulObservationsFileName = usefulObsFileName
            passiveObservationsFileName = passiveObsFileName
            markovLagField = If(markovLag IsNot Nothing, markovLag.Value, 1)
            readFromFiles()
        End Sub

        Public Sub New(usefulObsFileName As String, passiveObsFileName As String)
            Me.New(usefulObsFileName, passiveObsFileName, Nothing)
        End Sub

        ''' <summary>
        ''' This constructor is used when generating observations from a user
        ''' specified DBN.
        ''' </summary>
        ''' <seealso cref="DynamicBayesNet"/> 
        Public Sub New(attributes As IList(Of Attribute), observationsMatrix As Integer()()(), counts As Double()())
            attributesField = attributes
            markovLagField = observationsMatrix(0)(0).Length / attributes.Count - 1
            usefulObservations = observationsMatrix
            Me.counts = counts
            numSubjectsField = New Integer(observationsMatrix.Length - 1) {}

            ' assume constant number of observations per transition
            '		int totalNumSubjects = observationsMatrix[0].length;
            Dim totalNumSubjects = 0
            For i = 0 To counts(0).Length - 1
                totalNumSubjects += CInt(counts(0)(i))
            Next
            numSubjectsField.fill(totalNumSubjects)

            ' generate header
            Dim n As Integer = numAttributes()
            usefulObservationsHeader = New String(n - 1) {}
            For i = 0 To n - 1
                usefulObservationsHeader(i) = attributes(i).Name
            Next

            ' assume same subjects over all transitions
            subjectIsPresentField = New Dictionary(Of String, Boolean())(System.Math.Ceiling(totalNumSubjects / 0.75))
            Dim allTrue As Boolean() = New Boolean(numTransitions() - 1) {}
            allTrue.fill(True)
            For i = 0 To counts(0).Length - 1
                subjectIsPresentField("" & i.ToString()) = allTrue
            Next
        End Sub

        ''' <summary>
        ''' This constructor is used when forecasting from existing observations.
        ''' </summary>
        ''' <seealso cref="DynamicBayesNet"/>
        Public Sub New(originalObservations As Observations, newObservationsMatrix As Integer()()())
            attributesField = originalObservations.attributesField
            markovLagField = originalObservations.markovLagField
            passiveObservations = originalObservations.passiveObservations
            passiveObservationsHeader = originalObservations.passiveObservationsHeader
            passiveObservationsFileName = originalObservations.passiveObservationsFileName
            subjectIsPresentField = originalObservations.subjectIsPresentField
            usefulObservations = newObservationsMatrix
            usefulObservationsHeader = originalObservations.usefulObservationsHeader
            usefulObservationsFileName = originalObservations.usefulObservationsFileName

            numSubjectsField = New Integer(usefulObservations.Length - 1) {}

            ' assume constant number of observations per transition
            numSubjectsField.fill(usefulObservations(0).Length)
        End Sub



        Public Overridable ReadOnly Property NumSubjects As Integer
            Get
                Return numSubjectsField(0)
            End Get
        End Property

        Public Overridable ReadOnly Property NumTransitionsProp As Integer
            Get
                Return usefulObservations.Length
            End Get
        End Property

        ''' <summary>
        ''' Reads the second and last column of the header, parses the integer time
        ''' value and returns the difference between the two, plus one. If parsing is
        ''' not possible, exits. Also performs error checking on the number of
        ''' columns.
        ''' </summary>
        ''' <returns> the number of time slices in input file </returns>
        Private Shared Function parseNumTimeSlices(header As String()) As Integer

            'int timeFirstColumn = 0, timeLastColumn = 0;

            'try
            '{
            '    // get first and last column time identifier
            '    timeFirstColumn = int.Parse(header[1].Split("__", true)[1]);
            '    timeLastColumn = int.Parse(header[header.Length - 1].Split("__", true)[1]);

            '}
            'catch (System.IndexOutOfRangeException)
            '{
            '    Console.Error.WriteLine(Arrays.deepToString(header));
            '    Console.Error.WriteLine("Input file header does not comply to the 'attribute__t' format.");
            '    Environment.Exit(1);
            '}
            'catch (System.FormatException)
            '{
            '    Console.Error.WriteLine(Arrays.deepToString(header));
            '    Console.Error.WriteLine("Input file header does not comply to the 'attribute__t' format.");
            '    Environment.Exit(1);
            '}

            'int numTimeSlices = timeLastColumn - timeFirstColumn + 1;

            ''' the number of columns per time slice must be constant
            ''' header contains an extra column with subject id
            'if ((header.Length - 1) % numTimeSlices != 0)
            '{
            '    Console.Error.WriteLine(Arrays.deepToString(header));
            '    Console.Error.WriteLine("Input file header does not have a number of columns" + " compatible with the number of time slices.");
            '    Console.Error.WriteLine("Header length: " + header.Length);
            '    Console.Error.WriteLine("Number of time slices: " + numTimeSlices);
            '    Environment.Exit(1);
            '}

            'return numTimeSlices;
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Counts the missing values in one string
        ''' </summary>
        Private Shared Function countMissingValues(dataLine As String()) As Integer

            Dim missing = 0

            For Each value In dataLine
                If value.Length = 0 OrElse value.Equals("?") Then
                    missing += 1
                End If
            Next

            Return missing
        End Function

        ''' <summary>
        ''' Checks for errors in an array of observed values, in order to decide if
        ''' they will be stored in the observations matrix. If all the values are
        ''' missing, returns false. If there are some missing values, exits. If no
        ''' values are missing, returns true.
        ''' </summary>
        Private Function observationIsOk(observation As String()) As Boolean

            Dim missingValues = countMissingValues(observation)
            Dim n As Integer = numAttributes()

            If missingValues = n Then
                ' missing observation (all values missing), skip
                Return False
            End If

            'System.out.println("number of missing values:"+missingValues);

            If missingValues > 0 Then
                ' some missing values, can't work like that
                'System.err.println(Arrays.deepToString(observation));
                'System.err.println("Observation contains missing values.");
                'System.exit(1);
                Return False
            End If

            Return True
        End Function



        Public Overridable ReadOnly Property SubjectIsPresent As IDictionary(Of String, Boolean())
            Get
                Return subjectIsPresentField
            End Get
        End Property

        Private Sub readFromFiles()

            Throw New NotImplementedException
        End Sub

        ''' <summary>
        ''' Gets the name of the attributes from an input header line and the number
        ''' of attributes.
        ''' </summary>
        Private Function processHeader(header As String(), numAttributes As Integer) As String()
            Dim newHeader = New String(numAttributes - 1) {}
            Dim stripFirstHeader = copyOfRange(header, 1, numAttributes + 1)
            Dim i = 0
            For Each column In stripFirstHeader
                Dim columnParts = column.StringSplit("__", True)
                newHeader(System.Math.Min(Threading.Interlocked.Increment(i), i - 1)) = columnParts(0)
            Next
            Return newHeader
        End Function

        Private Shared Function getCombinations(Of T)(lists As IList(Of IList(Of T))) As ISet(Of IList(Of T))
            Dim combinations As ISet(Of IList(Of T)) = New HashSet(Of IList(Of T))()
            Dim newCombinations As ISet(Of IList(Of T))

            Dim index = 0

            ' extract each of the integers in the first list
            ' and add each to ints as a new list
            For Each i In lists(0)
                Dim newList As IList(Of T) = New List(Of T)()
                newList.Add(i)
                combinations.Add(newList)
            Next
            index += 1
            While index < lists.Count
                Dim nextList = lists(index)
                newCombinations = New HashSet(Of IList(Of T))()
                For Each first As IList(Of T) In combinations
                    For Each second As T In nextList
                        Dim newList As IList(Of T) = New List(Of T)()
                        CType(newList, List(Of T)).AddRange(first)
                        newList.Add(second)
                        newCombinations.Add(newList)
                    Next
                Next
                combinations = newCombinations

                index += 1
            End While

            Return combinations
        End Function

        ''' <summary>
        ''' Function that given an observation generates missing values </summary>
        ''' <param name="missingObservations"> Percentage of observations with missing values </param>
        ''' <param name="missingVariables">	Percentage of attributes, for each observation, with missing values </param>
        Public Overridable Function generateMissingValues(missingObservations As Integer, missingVariables As Integer) As Observations
            Dim numSubjects = numObservations(0, True)
            Dim numMissingObservations As Integer = System.Math.Ceiling(missingObservations / 100 * numSubjects)
            Dim numMissingVariables As Integer = System.Math.Ceiling(missingVariables / 100 * (CDbl(attributesField.Count) * (markovLagField + 1)))
            Dim indicesObservation As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim add As Boolean
            Dim random As Random = New Random()
            Dim value As Integer
            Dim new_obs As Integer()()() = RectangularArray.Cubic(Of Integer)(numTransitions(), numSubjects, attributesField.Count * (markovLagField + 1))

            For t = 0 To numTransitions() - 1
                For s = 0 To numSubjects - 1
                    Array.Copy(usefulObservations(t)(s), 0, new_obs(t)(s), 0, usefulObservations(t)(s).Length)
                Next
            Next



            For i = 0 To numMissingObservations - 1
                Do
                    value = random.Next(numSubjects - 1 + 1 - 1) + 1
                    add = indicesObservation.Add(value)
                Loop While Not add
            Next

            For Each indice In indicesObservation
                Dim indicesVariables As ISet(Of Integer) = New HashSet(Of Integer)()
                For i = 0 To numMissingVariables - 1
                    Do
                        value = random.Next((attributesField.Count * (markovLagField + 1) - 1) * numTransitions() + 1 - 0) + 0
                        add = indicesVariables.Add(value)
                    Loop While Not add
                Next
                For Each indice2 In indicesVariables
                    new_obs(indice2 Mod numTransitions())(indice)(indice2 / numTransitions()) = -1
                Next
            Next

            Return New Observations(attributesField, new_obs, counts)

        End Function

        Public Overridable Function imputeMissingValues(dbn As DynamicBayesNet, stationaryProcess As Boolean, mostProbable As Boolean) As Observations
            Dim Attributes = dbn.Attributes
            Dim numAttributes = Attributes.Count
            Dim numSubjects = numSubjectsField(0)
            Dim numTransitions = usefulObservations.Length
            Dim newObservations As Integer()()()
            Dim observationAux As Integer()()()
            Dim probability As Double()
            Dim newCounts As Double()()
            Dim probabilitySum As Double
            Dim probabilityAux As Double
            Dim attributeValues As IList(Of Integer)
            Dim missingCombinations As IList(Of IList(Of Integer))
            Dim combinationSet As ISet(Of IList(Of Integer))
            Dim combinationList As IList(Of IList(Of Integer))
            Dim nodeAux As Integer
            Dim subjectCombinations As Integer

            newObservations = RectangularArray.Cubic(Of Integer)(numTransitions, numSubjects, numAttributes * (markovLagField + 1))
            newCounts = RectangularArray.Matrix(Of Double)(numTransitions, numSubjects)

            For t = 0 To numTransitions - 1
                newCounts(t).fill(1)
            Next

            For subject = 0 To numSubjects - 1
                subjectCombinations = 1
                For t = 0 To numTransitions - 1
                    If t = 0 Then
                        For n = 0 To numAttributes * (markovLagField + 1) - 1
                            If usefulObservations(t)(subject)(n) = -1 Then
                                subjectCombinations *= Attributes(n Mod numAttributes).size()
                            End If
                        Next
                    Else
                        For n = numAttributes To numAttributes * (markovLagField + 1) - 1
                            If usefulObservations(t)(subject)(n) = -1 Then
                                subjectCombinations *= Attributes(n Mod numAttributes).size()
                            End If
                        Next
                    End If

                Next

                If subjectCombinations = 1 Then
                    For t = 0 To numTransitions - 1
                        Array.Copy(usefulObservations(t)(subject), 0, newObservations(t)(subject), 0, numAttributes * (markovLagField + 1))
                    Next
                Else
                    observationAux = RectangularArray.Cubic(Of Integer)(numTransitions, subjectCombinations, numAttributes * (markovLagField + 1))
                    probability = New Double(subjectCombinations - 1) {}
                    missingCombinations = New List(Of IList(Of Integer))()
                    For t = 0 To numTransitions - 1
                        If t = 0 Then
                            For n = 0 To numAttributes * (markovLagField + 1) - 1
                                If usefulObservations(t)(subject)(n) = -1 Then
                                    attributeValues = New List(Of Integer)()
                                    For i = 0 To attributesField(n Mod numAttributes).size() - 1
                                        attributeValues.Add(i)
                                    Next
                                    missingCombinations.Add(attributeValues)
                                End If
                            Next
                        Else
                            For n = numAttributes To numAttributes * (markovLagField + 1) - 1
                                If usefulObservations(t)(subject)(n) = -1 Then
                                    attributeValues = New List(Of Integer)()
                                    For i = 0 To attributesField(n Mod numAttributes).size() - 1
                                        attributeValues.Add(i)
                                    Next
                                    missingCombinations.Add(attributeValues)
                                End If
                            Next
                        End If

                    Next
                    combinationSet = getCombinations(missingCombinations)
                    combinationList = New List(Of IList(Of Integer))(combinationSet)
                    probabilitySum = 0
                    For i = 0 To subjectCombinations - 1
                        probabilityAux = 1
                        nodeAux = 0
                        For t = 0 To numTransitions - 1
                            If t = 0 Then
                                For n = 0 To numAttributes * (markovLagField + 1) - 1
                                    If usefulObservations(t)(subject)(n) = -1 Then
                                        observationAux(t)(i)(n) = combinationList(i)(nodeAux)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            observationAux(n / numAttributes + t)(i)(n Mod numAttributes) = observationAux(t)(i)(n)
                                        End If
                                        nodeAux += 1
                                    Else
                                        observationAux(t)(i)(n) = usefulObservations(t)(subject)(n)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            observationAux(n / numAttributes + t)(i)(n Mod numAttributes) = observationAux(t)(i)(n)
                                        End If
                                    End If
                                Next
                            Else
                                For n = numAttributes To numAttributes * (markovLagField + 1) - 1
                                    If usefulObservations(t)(subject)(n) = -1 Then
                                        observationAux(t)(i)(n) = combinationList(i)(nodeAux)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            observationAux(n / numAttributes + t)(i)(n Mod numAttributes) = observationAux(t)(i)(n)
                                        End If
                                        nodeAux += 1
                                    Else
                                        observationAux(t)(i)(n) = usefulObservations(t)(subject)(n)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            observationAux(n / numAttributes + t)(i)(n Mod numAttributes) = observationAux(t)(i)(n)
                                        End If
                                    End If
                                Next
                            End If

                        Next



                        If stationaryProcess Then
                            For t = 0 To numTransitions - 1
                                For n = 0 To numAttributes - 1
                                    probabilityAux *= dbn.transitionNets(0).getParameters(n, observationAux(t)(i))(0)
                                Next
                            Next
                        Else
                            For t = 0 To numTransitions - 1
                                For n = 0 To numAttributes - 1
                                    probabilityAux *= dbn.transitionNets(t).getParameters(n, observationAux(t)(i))(0)
                                Next
                            Next
                        End If
                        probability(i) = probabilityAux
                        probabilitySum += probabilityAux
                    Next
                    For i = 0 To subjectCombinations - 1
                        probability(i) /= probabilitySum
                    Next
                    If mostProbable Then
                        Dim largest = probability(0)
                        Dim index = 0
                        For i = 1 To subjectCombinations - 1
                            If probability(i) > largest Then
                                largest = probability(i)
                                index = i
                            End If
                        Next
                        For t = 0 To numTransitions - 1
                            Array.Copy(observationAux(t)(index), 0, newObservations(t)(subject), 0, numAttributes * (markovLagField + 1))
                        Next
                    Else
                        Dim r As Random = New Random()
                        Dim sample As Double = r.NextDouble()
                        Dim index = 0
                        Dim accum = probability(index)
                        While sample > accum
                            If Not index < subjectCombinations - 1 Then
                                index += 1
                                Exit While
                            End If
                            accum += probability(Threading.Interlocked.Increment(index))
                        End While
                        For t = 0 To numTransitions - 1
                            Array.Copy(observationAux(t)(index), 0, newObservations(t)(subject), 0, numAttributes * (markovLagField + 1))
                        Next
                    End If
                End If
            Next
            Return New Observations(attributesField, newObservations, newCounts)
        End Function

        ''' <summary>
        ''' Do the Expected Sufficient Statistics of an set of observations with missing values. </summary>
        ''' <param name="dbn"> DBN to which it will be generated the ESS </param>
        ''' <returns> Observations Observations without missing values and with ESS.1 </returns>
        Public Overridable Function fillMissingValues(dbn As DynamicBayesNet, stationaryProcess As Boolean) As Observations
            Dim Attributes = dbn.Attributes
            Dim numAttributes = Attributes.Count
            Dim numSubjects = numSubjectsField(0)
            Dim numTransitions = usefulObservations.Length
            Dim totalCombinations = 0
            Dim subjectCombinations As Integer
            Dim newObservations As Integer()()()
            Dim newCounts As Double()()
            Dim observation As Integer()
            Dim probabilitySum As Double
            Dim probabilityAux As Double
            Dim attributeValues As IList(Of Integer)
            Dim missingCombinations As IList(Of IList(Of Integer))
            Dim combinationsSet As ISet(Of IList(Of Integer))
            Dim combinationsList As IList(Of IList(Of Integer))
            Dim subjectAux As Integer
            Dim subjectAux2 As Integer
            Dim nodeAux As Integer
            Dim probabilityMax As Double

            Dim decimal_places As Double = 5
            Dim epsilon = System.Math.Pow(10, -decimal_places)

            subjectAux = 0
            For subject = 0 To numSubjects - 1
                subjectCombinations = 1
                For t = 0 To numTransitions - 1
                    If t = 0 Then
                        For n = 0 To numAttributes * (markovLagField + 1) - 1
                            If usefulObservations(t)(subject)(n) = -1 Then
                                subjectCombinations *= Attributes(n Mod numAttributes).size()
                            End If
                        Next
                    Else
                        For n = numAttributes To numAttributes * (markovLagField + 1) - 1
                            If usefulObservations(t)(subject)(n) = -1 Then
                                subjectCombinations *= Attributes(n Mod numAttributes).size()
                            End If
                        Next
                    End If

                Next
                totalCombinations += subjectCombinations
            Next

            newObservations = RectangularArray.Cubic(Of Integer)(numTransitions, totalCombinations, numAttributes * (markovLagField + 1))
            newCounts = RectangularArray.Matrix(Of Double)(numTransitions, totalCombinations)

            For subject = 0 To numSubjects - 1
                missingCombinations = New List(Of IList(Of Integer))()
                For t = 0 To numTransitions - 1
                    If t = 0 Then
                        For n = 0 To numAttributes * (markovLagField + 1) - 1
                            If usefulObservations(t)(subject)(n) = -1 Then
                                attributeValues = New List(Of Integer)()
                                For i = 0 To attributesField(n Mod numAttributes).size() - 1
                                    attributeValues.Add(i)
                                Next
                                missingCombinations.Add(attributeValues)
                            End If
                        Next
                    Else
                        For n = numAttributes To numAttributes * (markovLagField + 1) - 1
                            If usefulObservations(t)(subject)(n) = -1 Then
                                attributeValues = New List(Of Integer)()
                                For i = 0 To attributesField(n Mod numAttributes).size() - 1
                                    attributeValues.Add(i)
                                Next
                                missingCombinations.Add(attributeValues)
                            End If
                        Next
                    End If

                Next
                If missingCombinations.Count = 0 Then
                    For t = 0 To numTransitions - 1
                        Array.Copy(usefulObservations(t)(subject), 0, newObservations(t)(subjectAux), 0, usefulObservations(t)(subject).Length)
                        newCounts(t)(subjectAux) = 1
                    Next
                    subjectAux += 1
                Else
                    combinationsSet = getCombinations(missingCombinations)
                    combinationsList = New List(Of IList(Of Integer))(combinationsSet)
                    subjectAux2 = subjectAux
                    probabilityMax = Double.NegativeInfinity
                    For i = 0 To combinationsList.Count - 1
                        '					probabilityAux = 1;
                        probabilityAux = 0
                        nodeAux = 0
                        For t = 0 To numTransitions - 1
                            If t = 0 Then
                                For n = 0 To numAttributes * (markovLagField + 1) - 1
                                    If usefulObservations(t)(subject)(n) = -1 Then
                                        newObservations(t)(subjectAux)(n) = combinationsList(i)(nodeAux)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            newObservations(n / numAttributes + t)(subjectAux)(n Mod numAttributes) = newObservations(t)(subjectAux)(n)
                                        End If
                                        nodeAux += 1
                                    Else
                                        newObservations(t)(subjectAux)(n) = usefulObservations(t)(subject)(n)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            newObservations(n / numAttributes + t)(subjectAux)(n Mod numAttributes) = usefulObservations(t)(subject)(n)
                                        End If
                                    End If
                                Next
                            Else
                                For n = numAttributes To numAttributes * (markovLagField + 1) - 1
                                    If usefulObservations(t)(subject)(n) = -1 Then
                                        newObservations(t)(subjectAux)(n) = combinationsList(i)(nodeAux)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            newObservations(n / numAttributes + t)(subjectAux)(n Mod numAttributes) = combinationsList(i)(nodeAux)
                                        End If
                                        nodeAux += 1
                                    Else
                                        newObservations(t)(subjectAux)(n) = usefulObservations(t)(subject)(n)
                                        If n >= numAttributes And n / numAttributes + t < numTransitions Then
                                            newObservations(n / numAttributes + t)(subjectAux)(n Mod numAttributes) = usefulObservations(t)(subject)(n)
                                        End If
                                    End If
                                Next
                            End If
                        Next

                        If stationaryProcess Then
                            For t = 0 To numTransitions - 1
                                For n = 0 To numAttributes - 1
                                    '								probabilityAux *= dbn.transitionNets.get(0).getParameters(n, newObservations[t][subjectAux]).get(0);
                                    probabilityAux += System.Math.Log(dbn.transitionNets(0).getParameters(n, newObservations(t)(subjectAux))(0))
                                Next
                            Next
                        Else
                            For t = 0 To numTransitions - 1
                                For n = 0 To numAttributes - 1
                                    '								probabilityAux *= dbn.transitionNets.get(t).getParameters(n, newObservations[t][subjectAux]).get(0);
                                    probabilityAux += System.Math.Log(dbn.transitionNets(t).getParameters(n, newObservations(t)(subjectAux))(0))

                                Next
                            Next
                        End If
                        For t = 0 To numTransitions - 1
                            newCounts(t)(subjectAux) = probabilityAux
                        Next
                        '					probabilitySum += probabilityAux;
                        If probabilityMax < probabilityAux Then
                            probabilityMax = probabilityAux
                        End If

                        subjectAux += 1
                    Next



                    For i = subjectAux2 To subjectAux - 1
                        For t = 0 To numTransitions - 1
                            If newCounts(t)(i) - probabilityMax >= System.Math.Log(epsilon) - System.Math.Log(combinationsList.Count) Then
                                newCounts(t)(i) = System.Math.Exp(newCounts(t)(i) - probabilityMax)
                            Else
                                newCounts(t)(i) = 0
                            End If
                        Next
                    Next
                    probabilitySum = 0
                    For i = subjectAux2 To subjectAux - 1
                        probabilitySum += System.Math.Ceiling(newCounts(0)(i) * 1000000000000000R) / 1000000000000000R

                    Next
                    For i = subjectAux2 To subjectAux - 1
                        For t = 0 To numTransitions - 1
                            newCounts(t)(i) /= probabilitySum
                        Next
                    Next
                    '				for(int i = subjectAux2; i < subjectAux; i++) {
                    '					for(int t = 0; t < numTransitions; t++) {
                    '						newCounts[t][i] /= probabilitySum;
                    '					}
                    '				}
                End If
            Next

            '		System.out.println("--------- antes da gera��o ---------");
            '		for(int t = 0; t < numTransitions; t++) {
            '			System.out.println("---Transition " + t + " ---");
            '			for(int s = 0; s < numSubjects; s++) {
            '				System.out.print("[ ");
            '				for(int n = 0; n < numAttributes * (markovLag + 1); n++) {
            '					if(this.usefulObservations[t][s][n] == -1) {
            '						System.out.print("? ");
            '					}else {
            '						System.out.print(attributes.get(n%numAttributes).get(this.usefulObservations[t][s][n]) + " ");
            '					}
            '				}
            '				System.out.println("]  count: " + this.counts[t][s]);
            '			}
            '		}
            '''		
            '		System.out.println("--------- depois da gera��o ---------");
            '		for(int t = 0; t < numTransitions; t++) {
            '			System.out.println("---Transition " + t + " ---");
            '			for(int s = 0; s < totalCombinations; s++) {
            '				System.out.print("[ ");
            '				for(int n = 0; n < numAttributes * (markovLag + 1); n++) {
            '					System.out.print(attributes.get(n%numAttributes).get(newObservations[t][s][n]) + " " );
            '				}
            '				System.out.println("]  count: " + newCounts[t][s]);
            '			}
            '		}
            Return New Observations(attributesField, newObservations, newCounts)
        End Function

        ''' <summary>
        ''' Gets the number of transitions of the observations.
        ''' </summary>
        Public Overridable Function numTransitions() As Integer
            Return usefulObservations.Length
        End Function

        ''' <summary>
        ''' Gets the number of observations in one transition.
        ''' </summary>
        Public Overridable Function numObservations(transition As Integer) As Integer

            Return numObservations(transition, False)
        End Function

        ''' <summary>
        ''' Gets the number of observations in one transition utilizing the expected sufficient statistics.
        ''' </summary>
        Public Overridable Function numObservations(transition As Integer, withoutcounts As Boolean) As Integer

            ' stationary process
            If transition < 0 Then
                Dim numObs = 0
                Dim lT As Integer = numTransitions()
                For t = 0 To lT - 1
                    If withoutcounts Then
                        numObs += counts(t).Length
                    Else
                        numObs += numSubjectsField(t)
                    End If
                Next
                Return numObs
            End If

            ' time-varying process
            If withoutcounts Then
                Return counts(transition).Length
            Else
                Return numSubjectsField(transition)
            End If
        End Function

        ''' <summary>
        ''' Gets the number of observations with missing values.
        ''' </summary>
        Public Overridable Function numMissings(transition As Integer) As Integer

            ' stationary process
            If transition < 0 Then
                Dim numObs = 0
                Dim lT As Integer = numTransitions()
                For t = 0 To lT - 1
                    numObs += numMissing(t)
                Next
                Return numObs
            End If

            ' time-varying process
            Return numMissing(transition)
        End Function

        Public Overridable Function numAttributes() As Integer
            Return attributesField.Count
        End Function

        Public Overridable ReadOnly Property Attributes As IList(Of Attribute)
            Get
                Return attributesField
            End Get
        End Property

        ''' <summary>
        ''' Returns a representation of the first observations (#markovLag time
        ''' slices) of all subjects.
        ''' </summary>
        Public Overridable ReadOnly Property First As IList(Of Integer())
            Get
                Dim numSubjects = numSubjectsField(0)
                Dim initialObservations As IList(Of Integer()) = New List(Of Integer())(numSubjects)
                For s = 0 To numSubjects - 1
                    initialObservations.Add(copyOfRange(usefulObservations(0)(s), 0, markovLagField * numAttributes()))
                Next
                Return initialObservations
            End Get
        End Property

        Public Overridable ReadOnly Property ObservationsMatrix As Integer()()()
            Get
                Return usefulObservations
            End Get
        End Property

        Public Overridable ReadOnly Property PassiveObservationsMatrix As String()()()
            Get
                Return passiveObservations
            End Get
        End Property

        ''' <summary>
        ''' Given a network configuration (parents and child values), counts all
        ''' observations in some transition that are compatible with it. If
        ''' transition is negative, counts matches in all transitions.
        ''' </summary>
        Public Overridable Function count(c As LocalConfiguration, transition As Integer) As Double

            ' stationary process
            If transition < 0 Then
                Dim allMatches As Double = 0
                Dim lT As Integer = numTransitions()
                For t = 0 To lT - 1
                    allMatches += count(c, t)
                Next
                Return allMatches
            End If

            ' time-varying process
            Dim matches As Double = 0
            Dim N = numObservations(transition, True)
            For i = 0 To N - 1
                If c.matches(usefulObservations(transition)(i)) Then
                    matches += counts(transition)(i)
                End If
            Next
            Return matches
        End Function

        Public Overridable Sub writeToFile()
            Dim outFileName = usefulObservationsFileName.Replace(".csv", "-out.csv")

            ' test for file name without extension
            If outFileName.Equals(usefulObservationsFileName) Then
                outFileName = usefulObservationsFileName & "-out"
            End If

            writeToFile(outFileName)
        End Sub

        Public Overridable Sub writeToFile(outFileName As String)

        End Sub

        Public Overridable Function numPassiveAttributes() As Integer
            Return If(passiveObservations IsNot Nothing, passiveObservations(0)(0).Length / (markovLagField + 1), 0)
        End Function

        Public Overridable ReadOnly Property MarkovLag As Integer
            Get
                Return markovLagField
            End Get
        End Property

        Public Overridable Function toTimeSeriesHorizontal() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            Dim numTransitions As Integer = Me.numTransitions()
            Dim numAttributes As Integer = Me.numAttributes()

            sb.Append("Attribute_ID" & vbTab)
            For t = 0 To numTransitions - 1
                sb.Append("OBS" & t.ToString() & vbTab)
            Next
            sb.Append("OBS" & numTransitions.ToString() & ls)
            For j = 0 To numAttributes - 1
                sb.Append("A" & j.ToString() & vbTab)
                For t = 0 To numTransitions - 1
                    sb.Append(usefulObservations(t)(0)(j).ToString() & vbTab)
                Next
                sb.Append(usefulObservations(numTransitions - 1)(0)(j + numAttributes).ToString() & ls)

            Next
            sb.Append(ls)
            Return sb.ToString()
        End Function

        Public Overridable Function toTimeSeriesVertical() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            Dim numTransitions As Integer = Me.numTransitions()
            Dim numAttributes As Integer = Me.numAttributes()

            For t = 0 To numTransitions - 1
                For j = 0 To numAttributes - 1
                    sb.Append(usefulObservations(t)(0)(j).ToString() & vbTab)
                Next
                sb.Append(ls)
            Next
            For j = 0 To numAttributes - 1
                sb.Append(usefulObservations(numTransitions - 1)(0)(j + numAttributes).ToString() & vbTab)
            Next
            sb.Append(ls)

            Return sb.ToString()
        End Function

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            Dim numTransitions As Integer = Me.numTransitions()
            Dim numAttributes As Integer = Me.numAttributes()
            ' int numColumns = numAttributes*2;

            sb.Append("Input file: " & usefulObservationsFileName & ls & ls)

            sb.Append("Number of transitions: " & numTransitions.ToString() & ls)
            sb.Append("Number of attributes: " & numAttributes.ToString() & ls)

            sb.Append(ls)

            For t = 0 To numTransitions - 1

                sb.Append("--- Transition " & t.ToString() & " ---" & ls)
                Dim numObservations = Me.numObservations(t)

                sb.Append(numObservations.ToString() & " observations." & ls)
                If numMissing IsNot Nothing Then
                    Dim numMissing = numMissings(t)
                    sb.Append(numMissing.ToString() & " have missing values." & ls)
                End If

                ' sb.append("Observations matrix:"+ls);
                ' for (int i=0; i<numObservations; i++) {
                ' for(int j=0; j<numColumns; j++) {
                ' int attributeId = j%numAttributes;
                ' sb.append(attributes.get(attributeId).get(observationsMatrix[t][i][j])+" ");
                ' }
                ' sb.append(ls);
                ' }
                '
                ' sb.append(ls);
                '
                ' sb.append("Coded observations matrix:"+ls);
                ' for (int i=0; i<numObservations; i++) {
                ' for(int j=0; j<numColumns; j++) {
                ' sb.append(observationsMatrix[t][i][j]+" ");
                ' }
                ' sb.append(ls);
                ' }
                ' sb.append(ls);
            Next

            sb.Append(ls)

            sb.Append("Attributes:" & ls)
            For i = 0 To numAttributes - 1
                sb.Append(attributesField(i).ToString() & ls)
            Next

            Return sb.ToString()

        End Function

    End Class

End Namespace

