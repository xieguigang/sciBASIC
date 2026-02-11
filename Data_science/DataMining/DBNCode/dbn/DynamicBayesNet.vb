#Region "Microsoft.VisualBasic::39786cc1ad6e4d390f94e465765cecd4, Data_science\DataMining\DBNCode\dbn\DynamicBayesNet.vb"

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

    '   Total Lines: 356
    '    Code Lines: 258 (72.47%)
    ' Comment Lines: 13 (3.65%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 85 (23.88%)
    '     File Size: 14.13 KB


    '     Class DynamicBayesNet
    ' 
    '         Properties: Attributes, Init, Trans
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) compare, (+2 Overloads) forecast, (+2 Overloads) generateObservations, generateObservationsMatrix, generateParameters
    '                   getNumberParameters, getScore, learnParameters, Mean, parameterEM
    '                   StandDes, toDot, (+2 Overloads) ToString
    ' 
    '         Sub: learnParameters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Java
Imports std = System.Math

Namespace dbn

    Public Class DynamicBayesNet

        Private attributesField As IList(Of Attribute)

        Private markovLag As Integer

        Private initialNet As BayesNet

        Public transitionNets As IList(Of BayesNet)

        Public Sub New(attributes As IList(Of Attribute), initialNet As BayesNet, transitionNets As IList(Of BayesNet))
            attributesField = attributes
            Me.initialNet = initialNet
            Me.transitionNets = transitionNets
            markovLag = transitionNets(0).MarkovLag
        End Sub

        Public Overridable ReadOnly Property Attributes As IList(Of Attribute)
            Get
                Return attributesField
            End Get
        End Property


        Public Overridable ReadOnly Property Trans As IList(Of BayesNet)
            Get
                Return transitionNets
            End Get
        End Property


        Public Overridable ReadOnly Property Init As BayesNet
            Get

                Return initialNet
            End Get
        End Property




        Public Sub New(attributes As IList(Of Attribute), transitionNets As IList(Of BayesNet))
            Me.New(attributes, Nothing, transitionNets)
        End Sub

        Public Overridable Function generateParameters() As DynamicBayesNet
            If initialNet IsNot Nothing Then
                initialNet.generateParameters()
            End If
            For Each transitionNet In transitionNets
                transitionNet.generateParameters()
            Next
            Return Me
        End Function

        Public Overridable Sub learnParameters(o As Observations)
            learnParameters(o, False)
        End Sub

        Public Overridable Function learnParameters(o As Observations, stationaryProcess As Boolean) As String

            If stationaryProcess Then
                ' assert there is only one transition network
                If transitionNets.Count > 1 Then
                    Throw New ArgumentException("DBN has more than one transition network, cannot " & "learn parameters considering a stationary process")
                End If

                Return transitionNets(0).learnParameters(o, -1)
            Else
                Dim lT = transitionNets.Count
                For t = 0 To lT - 1
                    transitionNets(t).learnParameters(o, t)
                Next
            End If
            Return Nothing
        End Function

        Public Overridable Function parameterEM(o As Observations, stationaryProcess As Boolean) As DynamicBayesNet
            Dim dbn = Me
            Dim dbnOld As DynamicBayesNet
            Dim o_new As Observations
            Dim score = Double.NegativeInfinity
            Dim scorePrev As Double

            generateParameters()
            Dim i = 0
            Do
                scorePrev = score
                o_new = o.fillMissingValues(dbn, stationaryProcess)
                dbn.learnParameters(o_new, stationaryProcess)
                score = dbn.getScore(o_new, New LLScoringFunction(), stationaryProcess)
                i += 1
                Console.WriteLine("Parameter EM step: " & i.ToString() & " score: " & score.ToString())
            Loop While score > scorePrev

            Return dbn
        End Function

        Public Overridable Function getNumberParameters(o As Observations) As Double
            Dim numParam As Double = 0
            Dim ParentsPresent As Integer? = Nothing

            For n = 0 To attributesField.Count - 1
                Dim c As LocalConfiguration = New LocalConfiguration(o.Attributes, o.MarkovLag, transitionNets(0).Parents(n), ParentsPresent, n)
                numParam += c.NumParameters
            Next

            Return numParam
        End Function

        Public Overridable Function getScore(o As Observations, sf As ScoringFunction, stationaryProcess As Boolean) As Double
            Dim score As Double = 0
            If stationaryProcess Then
                ' assert there is only one transition network
                If transitionNets.Count > 1 Then
                    Throw New ArgumentException("DBN has more than one transition network, cannot " & "learn parameters considering a stationary process")
                End If
                For n = 0 To attributesField.Count - 1
                    score += sf.evaluate(o, transitionNets(0).Parents(n), n)
                Next
            Else
                Dim lT = transitionNets.Count
                For t = 0 To lT - 1
                    For n = 0 To attributesField.Count - 1
                        score += sf.evaluate(o, transitionNets(t).Parents(n), n)
                    Next
                Next
            End If
            Return score
        End Function

        Public Overridable Function generateObservations(numIndividuals As Integer) As Observations
            Return generateObservations(numIndividuals, transitionNets.Count, False)
        End Function

        Public Overridable Function generateObservations(numIndividuals As Integer, numTransitions As Integer, stationaryProcess As Boolean) As Observations
            Dim obsMatrix = generateObservationsMatrix(Nothing, numIndividuals, numTransitions, stationaryProcess, False)

            Dim counts = RectangularArray.Matrix(Of Double)(numTransitions, numIndividuals)
            For i = 0 To numTransitions - 1
                counts(i).fill(1)
            Next
            Return New Observations(attributesField, obsMatrix, counts)
        End Function

        Public Overridable Function forecast(originalObservations As Observations, numTransitions As Integer, stationaryProcess As Boolean, mostProbable As Boolean) As Observations
            If stationaryProcess Then
                ' assert there is only one transition network
                If transitionNets.Count > 1 Then
                    Throw New ArgumentException("DBN has more than one transition network, cannot " & "learn parameters considering a stationary process")
                End If

            End If
            Dim initialObservations = originalObservations.First
            Dim obsMatrix = generateObservationsMatrix(initialObservations, initialObservations.Count, numTransitions, stationaryProcess, mostProbable)
            Return New Observations(originalObservations, obsMatrix)
        End Function

        Public Overridable Function forecast(originalObservations As Observations) As Observations
            Return forecast(originalObservations, transitionNets.Count, False, False)
        End Function

        Private Function generateObservationsMatrix(initialObservations As IList(Of Integer()), numIndividuals As Integer, numTransitions As Integer, stationaryProcess As Boolean, mostProbable As Boolean) As Integer()()()
            ' System.out.println("generating observations");

            If Not stationaryProcess AndAlso numTransitions > transitionNets.Count Then
                Throw New ArgumentException("DBN only has " & transitionNets.Count.ToString() & " " & "transitions defined, cannot generate " & numTransitions.ToString() & ".")
            End If

            Dim n = attributesField.Count

            Dim obsMatrix = RectangularArray.Cubic(Of Integer)(numTransitions, numIndividuals, (markovLag + 1) * n)

            For subject = 0 To numIndividuals - 1

                Dim observation0 = If(initialObservations IsNot Nothing, initialObservations(subject), initialNet.nextObservation(Nothing, mostProbable))
                Dim observationT = observation0
                For transition = 0 To numTransitions - 1
                    Array.Copy(observationT, 0, obsMatrix(transition)(subject), 0, n * markovLag)

                    Dim observationTplus1 = If(stationaryProcess, transitionNets(0).nextObservation(observationT, mostProbable), transitionNets(transition).nextObservation(observationT, mostProbable))
                    Array.Copy(observationTplus1, 0, obsMatrix(transition)(subject), n * markovLag, n)

                    observationT = copyOfRange(obsMatrix(transition)(subject), n, (markovLag + 1) * n)
                Next
            Next

            Return obsMatrix
        End Function



        Public Shared Function compare(original As DynamicBayesNet, recovered As DynamicBayesNet) As Double()
            Return compare(original, recovered, False)
        End Function


        'List<int[]> 

        Public Shared Function compare(original As DynamicBayesNet, recovered As DynamicBayesNet, verbose As Boolean) As Double()
            Diagnostics.Debug.Assert(original.transitionNets.Count = recovered.transitionNets.Count)
            'int numTransitions = original.transitionNets.size();
            'List<int[]> counts = new ArrayList<int[]>(numTransitions);
            ' for (int t = 0; t < numTransitions; t++) {
            ' 				counts.add(BayesNet.compare(original.transitionNets.get(t), recovered.transitionNets.get(t), verbose));
            ' 			}
            Return BayesNet.compare(original.transitionNets(0), recovered.transitionNets(0), verbose)
        End Function

        Public Overridable Function toDot(compactFormat As Boolean) As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            Dim dl = ls & ls
            Dim n = attributesField.Count
            Dim lT = transitionNets.Count

            If compactFormat AndAlso (lT <> 1 OrElse markovLag <> 1) Then
                Throw New InvalidOperationException("More than one transition network or Markov lag larger than 1, cannot create compact graph.")
            End If

            ' digraph init
            sb.Append("digraph dbn{" & dl)

            If compactFormat Then
                For i = 0 To n - 1
                    sb.Append("X" & i.ToString())
                    Dim attributeName = attributesField(i).Name
                    If Not ReferenceEquals(attributeName, Nothing) Then
                        sb.Append("[label=""" & attributeName)
                    Else
                        sb.Append("[label=""X" & i.ToString())
                    End If
                    sb.Append("""];" & ls)
                Next
                sb.Append(ls)
            Else
                For t = 0 To lT + markovLag - 1
                    ' slice t attributes
                    For i = 0 To n - 1
                        sb.Append("X" & i.ToString() & "_" & t.ToString())
                        Dim attributeName = attributesField(i).Name
                        If Not ReferenceEquals(attributeName, Nothing) Then
                            sb.Append("[label=""" & attributeName)
                        Else
                            sb.Append("[label=""X" & i.ToString())
                        End If
                        sb.Append("[" & t.ToString() & "]"", group=g" & t.ToString() & "];" & ls)
                    Next
                    sb.Append(ls)
                Next
                For i = 0 To n - 1
                    sb.Append("{ rank=same ")
                    For t = 0 To lT + markovLag - 1
                        sb.Append("X" & i.ToString() & "_" & t.ToString() & " ")
                    Next
                    sb.Append("}")
                    sb.Append(ls)
                Next
            End If
            sb.Append(ls)

            ' transition and intra-slice (t>0) edges
            For t = 0 To lT - 1
                sb.Append(transitionNets(t).toDot(t, compactFormat))
            Next

            If Not compactFormat Then
                sb.Append("edge[style=invis];")
                For t = 0 To lT + markovLag - 1

                    sb.Append(ls)
                    sb.Append("X" & 0.ToString() & "_" & t.ToString())
                    For i = 1 To n - 1
                        sb.Append(" -> X" & i.ToString() & "_" & t.ToString())
                    Next
                    sb.Append(";")
                    sb.Append(ls)
                Next
            End If


            sb.Append(ls & "}" & ls)

            Return sb.ToString()
        End Function

        Public Overloads Function ToString(printParameters As Boolean) As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","

            If initialNet IsNot Nothing Then
                sb.Append(initialNet.ToString(-1, printParameters))
            End If

            Dim i = 0
            Dim iter As IEnumerator(Of BayesNet) = transitionNets.GetEnumerator()

            While iter.MoveNext()
                sb.Append(iter.Current.ToString(i, printParameters))
                i += 1

                If iter.MoveNext() Then
                    sb.Append("-----------------" & ls & ls)
                End If
            End While

            Return sb.ToString()
        End Function

        Public Overrides Function ToString() As String
            Return toString(False)
        End Function



        Public Shared Function Mean(obs As Double()) As Double

            Dim sum As Double = 0

            For i = 0 To obs.Length - 1
                sum += obs(i)

            Next


            sum = sum / obs.Length
            Return sum
        End Function


        Public Shared Function StandDes(obs As Double(), mean As Double) As Double

            Dim sum As Double = 0

            For i = 0 To obs.Length - 1
                sum += std.Pow(obs(i) - mean, 2)

            Next
            sum = sum / obs.Length
            sum = std.Sqrt(sum) / std.Sqrt(obs.Length) * 1.96
            Return sum
        End Function


    End Class



End Namespace

