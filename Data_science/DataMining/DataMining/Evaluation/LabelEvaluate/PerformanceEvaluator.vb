#Region "Microsoft.VisualBasic::9d4261b9ce3d6254262d88b8b83ca8ec, Data_science\DataMining\DataMining\Evaluation\LabelEvaluate\PerformanceEvaluator.vb"

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

    '   Total Lines: 187
    '    Code Lines: 116 (62.03%)
    ' Comment Lines: 35 (18.72%)
    '    - Xml Docs: 54.29%
    ' 
    '   Blank Lines: 36 (19.25%)
    '     File Size: 5.90 KB


    '     Class PerformanceEvaluator
    ' 
    '         Properties: AP, AuC, PRCurve, ROCCurve
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: computeFPR, computePrecision, computeRecall, computeTPR
    ' 
    '         Sub: computePR, computeRoC, computeStatistics, findChanges
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

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Evaluation

    ''' <summary>
    ''' Class which evaluates an SVM model using several standard techniques.
    ''' </summary>
    Public Class PerformanceEvaluator

        Dim _data As List(Of RankPair)
        Dim _changes As List(Of ChangePoint)

        ''' <summary>
        ''' Receiver Operating Characteristic curve
        ''' </summary>
        Public ReadOnly Property ROCCurve As List(Of PointF)

        ''' <summary>
        ''' Returns the area under the ROC Curve
        ''' </summary>
        Public ReadOnly Property AuC As Double

        ''' <summary>
        ''' Precision-Recall curve
        ''' </summary>
        Public ReadOnly Property PRCurve As List(Of PointF)

        ''' <summary>
        ''' The average precision
        ''' </summary>
        Public ReadOnly Property AP As Double

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="set">A pre-computed ranked pair set</param>
        Public Sub New([set] As List(Of RankPair))
            _data = [set]
            computeStatistics()
        End Sub

        Private Sub computeStatistics()
            Call _data.Sort()

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
            If p.TP = 0 Then
                Return 0
            Else
                Return CSng(p.TP) / (p.TP + p.FP)
            End If
        End Function

        Private Function computeRecall(p As ChangePoint) As Single
            If p.TP = 0 Then
                Return 0
            Else
                Return CSng(p.TP) / (p.TP + p.FN)
            End If
        End Function

        Private Sub computePR()
            Dim precision = computePrecision(_changes(0))
            Dim recall = computeRecall(_changes(0))
            Dim precisionSum As Single = 0

            _PRCurve = New List(Of PointF)()
            _PRCurve.Add(New PointF(0, 1))

            If _changes(0).TP > 0 Then
                precisionSum += precision
                _PRCurve.Add(New PointF(recall, precision))
            End If

            For i = 1 To _changes.Count - 1
                precision = computePrecision(_changes(i))
                recall = computeRecall(_changes(i))

                If _changes(i).TP > _changes(i - 1).TP Then
                    precisionSum += precision
                    _PRCurve.Add(New PointF(recall, precision))
                End If
            Next

            Dim lastPR As Single = CSng(_changes(0).TP + _changes(0).FN) / (_changes(0).FP + _changes(0).TN)

            If Not (_PRCurve.Last.X = 1 AndAlso lastPR.IsNaNImaginary) Then
                _PRCurve.Add(New PointF(1, lastPR))
            End If

            _AP = precisionSum / (_changes(0).FN + _changes(0).TP)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function computeTPR(cp As ChangePoint) As Single
            Return computeRecall(cp)
        End Function

        Private Function computeFPR(cp As ChangePoint) As Single
            If cp.FP = 0 Then
                Return 0
            Else
                Return CSng(cp.FP) / (cp.FP + cp.TN)
            End If
        End Function

        Private Sub computeRoC()
            Dim tpr = computeTPR(_changes(0))
            Dim fpr = computeFPR(_changes(0))

            _ROCCurve = New List(Of PointF)()
            _ROCCurve.Add(New PointF(0, 0))
            _ROCCurve.Add(New PointF(fpr, tpr))
            _AuC = 0

            For i = 1 To _changes.Count - 1
                Dim newTPR = computeTPR(_changes(i))
                Dim newFPR = computeFPR(_changes(i))

                If _changes(i).TP > _changes(i - 1).TP Then
                    _AuC += tpr * (newFPR - fpr) + 0.5 * (newTPR - tpr) * (newFPR - fpr)
                    tpr = newTPR
                    fpr = newFPR
                    _ROCCurve.Add(New PointF(fpr, tpr))
                End If
            Next

            _ROCCurve.Add(New PointF(1, 1))
            _AuC += tpr * (1 - fpr) + 0.5 * (1 - tpr) * (1 - fpr)
        End Sub
    End Class
End Namespace
