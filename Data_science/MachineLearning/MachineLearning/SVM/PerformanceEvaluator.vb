#Region "Microsoft.VisualBasic::6e8f095f6a47f26f0e09950d5afaf54b, Data_science\MachineLearning\MachineLearning\SVM\PerformanceEvaluator.vb"

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

    '     Class RankPair
    ' 
    '         Properties: Label, Score
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, ToString
    ' 
    '     Class CurvePoint
    ' 
    '         Properties: X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class PerformanceEvaluator
    ' 
    '         Properties: AP, AuC, PRCurve, ROCCurve
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: computeFPR, computePrecision, computeRecall, computeTPR
    ' 
    '         Sub: computePR, computeRoC, computeStatistics, findChanges, parseResultsFile
    '              WritePRCurve, WriteROCCurve
    '         Class ChangePoint
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
' * SVM.NET Library
' * Copyright (C) 2008 Matthew Johnson
' * 
' * This program is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
' * 
' * This program is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
' * 
' * You should have received a copy of the GNU General Public License
' * along with this program.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualBasic.Text

Namespace SVM
    ''' <summary>
    ''' Class encoding a member of a ranked set of labels.
    ''' </summary>
    Public Class RankPair : Implements IComparable(Of RankPair)

        Private _score, _label As Double

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="score">Score for this pair</param>
        ''' <param name="label">Label associated with the given score</param>
        Public Sub New(score As Double, label As Double)
            _score = score
            _label = label
        End Sub

        ''' <summary>
        ''' The score for this pair.
        ''' </summary>
        Public ReadOnly Property Score As Double
            Get
                Return _score
            End Get
        End Property

        ''' <summary>
        ''' The Label for this pair.
        ''' </summary>
        Public ReadOnly Property Label As Double
            Get
                Return _label
            End Get
        End Property

#Region "IComparable<RankPair> Members"

        ''' <summary>
        ''' Compares this pair to another.  It will end up in a sorted list in decending score order.
        ''' </summary>
        ''' <param name="other">The pair to compare to</param>
        ''' <returns>Whether this should come before or after the argument</returns>
        Public Function CompareTo(other As RankPair) As Integer Implements IComparable(Of RankPair).CompareTo
            Return other.Score.CompareTo(Score)
        End Function

#End Region

        ''' <summary>
        ''' Returns a string representation of this pair.
        ''' </summary>
        ''' <returns>A string in the for Score:Label</returns>
        Public Overrides Function ToString() As String
            Return String.Format("{0}:{1}", Score, Label)
        End Function
    End Class

    ''' <summary>
    ''' Class encoding the point on a 2D curve.
    ''' </summary>
    Public Class CurvePoint
        Private _x, _y As Single

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="x">X coordinate</param>
        ''' <param name="y">Y coordinate</param>
        Public Sub New(x As Single, y As Single)
            _x = x
            _y = y
        End Sub

        ''' <summary>
        ''' X coordinate
        ''' </summary>
        Public ReadOnly Property X As Single
            Get
                Return _x
            End Get
        End Property

        ''' <summary>
        ''' Y coordinate
        ''' </summary>
        Public ReadOnly Property Y As Single
            Get
                Return _y
            End Get
        End Property

        ''' <summary>
        ''' Creates a string representation of this point.
        ''' </summary>
        ''' <returns>string in the form (x, y)</returns>
        Public Overrides Function ToString() As String
            Return String.Format("({0}, {1})", _x, _y)
        End Function
    End Class

    ''' <summary>
    ''' Class which evaluates an SVM model using several standard techniques.
    ''' </summary>
    Public Class PerformanceEvaluator
        Private Class ChangePoint
            Public Sub New(tp As Integer, fp As Integer, tn As Integer, fn As Integer)
                Me.TP = tp
                Me.FP = fp
                Me.TN = tn
                Me.FN = fn
            End Sub

            Public TP, FP, TN, FN As Integer

            Public Overrides Function ToString() As String
                Return String.Format("{0}:{1}:{2}:{3}", TP, FP, TN, FN)
            End Function
        End Class

        Private _prCurve As List(Of CurvePoint)
        Private _ap As Double
        Private _rocCurve As List(Of CurvePoint)
        Private _auc As Double
        Private _data As List(Of RankPair)
        Private _changes As List(Of ChangePoint)

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="set">A pre-computed ranked pair set</param>
        Public Sub New([set] As List(Of RankPair))
            _data = [set]
            computeStatistics()
        End Sub

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="model">Model to evaluate</param>
        ''' <param name="problem">Problem to evaluate</param>
        ''' <param name="category">Label to be evaluate for</param>
        Public Sub New(model As Model, problem As Problem, category As Double)
            Me.New(model, problem, category, "tmp.results")
        End Sub
        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="model">Model to evaluate</param>
        ''' <param name="problem">Problem to evaluate</param>
        ''' <param name="resultsFile">Results file for output</param>
        ''' <param name="category">Category to evaluate for</param>
        Public Sub New(model As Model, problem As Problem, category As Double, resultsFile As String)
            ' Predict(problem, resultsFile, model, True)
            ' parseResultsFile(resultsFile, problem.Y, category)
            ' computeStatistics()
            Throw New NotImplementedException
        End Sub

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="resultsFile">Results file</param>
        ''' <param name="correctLabels">The correct labels of each data item</param>
        ''' <param name="category">The category to evaluate for</param>
        Public Sub New(resultsFile As String, correctLabels As Double(), category As Double)
            parseResultsFile(resultsFile, correctLabels, category)
            computeStatistics()
        End Sub

        Private Sub parseResultsFile(resultsFile As String, labels As Double(), category As Double)
            Dim input As StreamReader = New StreamReader(resultsFile)
            Dim parts As String() = input.ReadLine().Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
            Dim confidenceIndex = -1

            For i = 1 To parts.Length - 1

                If Double.Parse(parts(i), CultureInfo.InvariantCulture) = category Then
                    confidenceIndex = i
                    Exit For
                End If
            Next

            _data = New List(Of RankPair)()

            For i = 0 To labels.Length - 1
                parts = input.ReadLine().Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
                Dim confidence = Double.Parse(parts(confidenceIndex), CultureInfo.InvariantCulture)
                _data.Add(New RankPair(confidence, If(labels(i) = category, 1, 0)))
            Next

            input.Close()
        End Sub

        Private Sub computeStatistics()
            _data.Sort()
            findChanges()
            computePR()
            computeRoC()
        End Sub

        Private Sub findChanges()
            Dim tp, fp, tn, fn As Integer
            fn = 0
            tn = 0
            fp = 0
            tp = 0

            For i = 0 To _data.Count - 1

                If _data(i).Label = 1 Then
                    fn += 1
                Else
                    tn += 1
                End If
            Next

            _changes = New List(Of ChangePoint)()

            For i = 0 To _data.Count - 1

                If _data(i).Label = 1 Then
                    tp += 1
                    fn -= 1
                Else
                    fp += 1
                    tn -= 1
                End If

                _changes.Add(New ChangePoint(tp, fp, tn, fn))
            Next
        End Sub

        Private Function computePrecision(p As ChangePoint) As Single
            Return CSng(p.TP) / (p.TP + p.FP)
        End Function

        Private Function computeRecall(p As ChangePoint) As Single
            Return CSng(p.TP) / (p.TP + p.FN)
        End Function

        Private Sub computePR()
            _prCurve = New List(Of CurvePoint)()
            _prCurve.Add(New CurvePoint(0, 1))
            Dim precision = computePrecision(_changes(0))
            Dim recall = computeRecall(_changes(0))
            Dim precisionSum As Single = 0

            If _changes(0).TP > 0 Then
                precisionSum += precision
                _prCurve.Add(New CurvePoint(recall, precision))
            End If

            For i = 1 To _changes.Count - 1
                precision = computePrecision(_changes(i))
                recall = computeRecall(_changes(i))

                If _changes(i).TP > _changes(i - 1).TP Then
                    precisionSum += precision
                    _prCurve.Add(New CurvePoint(recall, precision))
                End If
            Next

            _prCurve.Add(New CurvePoint(1, CSng(_changes(0).TP + _changes(0).FN) / (_changes(0).FP + _changes(0).TN)))
            _ap = precisionSum / (_changes(0).FN + _changes(0).TP)
        End Sub

        ''' <summary>
        ''' Writes the Precision-Recall curve to a tab-delimited file.
        ''' </summary>
        ''' <param name="filename">Filename for output</param>
        Public Sub WritePRCurve(filename As String)
            Dim output As StreamWriter = New StreamWriter(filename)
            output.WriteLine(_ap)

            For i = 0 To _prCurve.Count - 1
                output.WriteLine("{0}" & ASCII.TAB & "{1}", _prCurve(CInt(i)).X, _prCurve(CInt(i)).Y)
            Next

            output.Close()
        End Sub

        ''' <summary>
        ''' Writes the Receiver Operating Characteristic curve to a tab-delimited file.
        ''' </summary>
        ''' <param name="filename">Filename for output</param>
        Public Sub WriteROCCurve(filename As String)
            Dim output As StreamWriter = New StreamWriter(filename)
            output.WriteLine(_auc)

            For i = 0 To _rocCurve.Count - 1
                output.WriteLine("{0}" & ASCII.TAB & "{1}", _rocCurve(CInt(i)).X, _rocCurve(CInt(i)).Y)
            Next

            output.Close()
        End Sub

        ''' <summary>
        ''' Receiver Operating Characteristic curve
        ''' </summary>
        Public ReadOnly Property ROCCurve As List(Of CurvePoint)
            Get
                Return _rocCurve
            End Get
        End Property

        ''' <summary>
        ''' Returns the area under the ROC Curve
        ''' </summary>
        Public ReadOnly Property AuC As Double
            Get
                Return _auc
            End Get
        End Property

        ''' <summary>
        ''' Precision-Recall curve
        ''' </summary>
        Public ReadOnly Property PRCurve As List(Of CurvePoint)
            Get
                Return _prCurve
            End Get
        End Property

        ''' <summary>
        ''' The average precision
        ''' </summary>
        Public ReadOnly Property AP As Double
            Get
                Return _ap
            End Get
        End Property

        Private Function computeTPR(cp As ChangePoint) As Single
            Return computeRecall(cp)
        End Function

        Private Function computeFPR(cp As ChangePoint) As Single
            Return CSng(cp.FP) / (cp.FP + cp.TN)
        End Function

        Private Sub computeRoC()
            _rocCurve = New List(Of CurvePoint)()
            _rocCurve.Add(New CurvePoint(0, 0))
            Dim tpr = computeTPR(_changes(0))
            Dim fpr = computeFPR(_changes(0))
            _rocCurve.Add(New CurvePoint(fpr, tpr))
            _auc = 0

            For i = 1 To _changes.Count - 1
                Dim newTPR = computeTPR(_changes(i))
                Dim newFPR = computeFPR(_changes(i))

                If _changes(i).TP > _changes(i - 1).TP Then
                    _auc += tpr * (newFPR - fpr) + 0.5 * (newTPR - tpr) * (newFPR - fpr)
                    tpr = newTPR
                    fpr = newFPR
                    _rocCurve.Add(New CurvePoint(fpr, tpr))
                End If
            Next

            _rocCurve.Add(New CurvePoint(1, 1))
            _auc += tpr * (1 - fpr) + 0.5 * (1 - tpr) * (1 - fpr)
        End Sub
    End Class
End Namespace
