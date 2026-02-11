#Region "Microsoft.VisualBasic::428c0b7e7483866e25b8b7fe4284e7c3, Data_science\DataMining\DynamicProgramming\CenterStar\CenterStar.vb"

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

'   Total Lines: 210
'    Code Lines: 147 (70.00%)
' Comment Lines: 33 (15.71%)
'    - Xml Docs: 78.79%
' 
'   Blank Lines: 30 (14.29%)
'     File Size: 7.08 KB


' Class CenterStar
' 
'     Properties: NameList
' 
'     Constructor: (+2 Overloads) Sub New
' 
'     Function: calculateTotalCost, Compute, computeInternal
' 
'     Sub: findStarIndex, multipleAlignmentImpl
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

''' <summary>
''' ##### Multiple-sequence-alignment
''' 
''' This program calculates the multiple sequence alignment of k>1 DNA sequences.
''' 
''' The program use the Matrix.txt file For the substitution matrix. The matrix 
''' can be changed, And it used With Default values As: 
''' 
''' + 0 - Match
''' + 1 - Missmatch
''' + 2 - Indel
''' 
''' Algorithm used For this purpose Is Center Star Algotrithm
''' 
''' > https://github.com/EranCohenSW/Multiple-sequence-alignment/blob/master/Project/src/CenterStar.java
''' </summary>
Public Class CenterStar

    Dim starIndex%
    Dim centerString$
    Dim multipleAlign$()
    Dim sequence$()
    Dim names$()
    Dim kband As KBandSearch

    Public ReadOnly Property NameList As String()
        Get
            Return names
        End Get
    End Property

    Sub New(input As IEnumerable(Of NamedValue(Of String)), Optional kband As Integer = 32)
        With input.ToArray
            sequence = .Select(Function(fa) fa.Value) _
                       .ToArray
            names = .Select(Function(fa) fa.Name) _
                    .ToArray
        End With

        Me.kband = New KBandSearch(globalAlign:=New String(2) {}, kband)
    End Sub

    ''' <summary>
    ''' auto encode sequence with title in format seq_id
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="kband"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(input As IEnumerable(Of String), Optional kband As Integer = 32)
        Call Me.New(input.Select(Function(seq, i) New NamedValue(Of String)($"seq{i + 1}", seq)), kband)
    End Sub

    ''' <summary>
    ''' Main
    ''' </summary>
    ''' <param name="matrix">得分矩阵</param>
    ''' <returns></returns>
    Public Function Compute(matrix As IScore(Of Char), ByRef alignment As String()) As Double
        Dim totalCost#

        If sequence.All(Function(s) s = sequence(Scan0)) Then
            ' 所输入的序列全部都是一样的？？
            alignment = sequence.ToArray
            totalCost = 0
        Else
#If DEBUG Then
            totalCost = computeInternal(matrix) 
#Else
            Try
                totalCost = computeInternal(matrix)
            Catch ex As Exception
                Throw New Exception(sequence.JoinBy(vbCrLf), ex)
            End Try
#End If
            alignment = multipleAlign.ToArray
        End If

        Return totalCost
    End Function

    Private Function computeInternal(matrix As IScore(Of Char)) As Double
        Dim n As Integer = sequence.Length

        findStarIndex()
        centerString = sequence(starIndex)
        multipleAlign = New String(n - 1) {}
        multipleAlignmentImpl()

        Return calculateTotalCost(matrix, n)
    End Function

    ''' <summary>
    ''' this Function calculate() the total cost
    ''' </summary>
    ''' <returns></returns>
    ''' 
    Private Function calculateTotalCost(matrix As IScore(Of Char), n%) As Double
        Dim length = multipleAlign(0).Length
        Dim totalScore# = 0

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                If (j > i) Then
                    For k As Integer = 0 To length - 1
                        Dim ic As Char = multipleAlign(i)(k)
                        Dim jc As Char = multipleAlign(j)(k)

                        totalScore += matrix.GetSimilarityScore(ic, jc)
                    Next
                End If
            Next
        Next

        Return totalScore
    End Function

    ''' <summary>
    ''' The Function do the multiple alignment according to the center string 
    ''' </summary>
    Private Sub multipleAlignmentImpl()
        Dim n As Integer = sequence.Length

        multipleAlign(starIndex) = centerString

        For i As Integer = 0 To n - 1
            If i = starIndex Then
                Continue For
            End If

            ' 执行双序列比对
            kband.CalculateEditDistance(multipleAlign(starIndex), sequence(i))
            multipleAlign(i) = kband.globalAlign(1)

            ' 统一处理空格插入，确保所有序列长度一致'
            Call SyncGaps(multipleAlign, starIndex, i)
        Next

        Dim maxLen As Integer = Aggregate a As String
                                In multipleAlign
                                Into Max(a.Length)

        For i As Integer = 0 To multipleAlign.Length - 1
            multipleAlign(i) = multipleAlign(i).PadRight(maxLen, "-"c)
        Next
    End Sub

    Private Sub SyncGaps(ByRef alignments As String(), centerIndex As Integer, i As Integer)
        If kband.globalAlign(0).Length > alignments(centerIndex).Length Then
            Dim j2 = 0

            For j1 As Integer = 0 To kband.globalAlign(0).Length - 1
                If (alignments(centerIndex).CharAtOrDefault(j2, "-"c) <> kband.globalAlign(0)(j1)) Then
                    Dim a As StringBuilder

                    For k As Integer = 0 To i - 1
                        With multipleAlign(k)
                            If .Length > j1 Then
                                a = New StringBuilder(multipleAlign(k))
                                a.Insert(j1, "-"c)
                                multipleAlign(k) = a.ToString
                            Else
                                multipleAlign(k) = .ToString & New String("-"c, j1 - .Length)
                            End If
                        End With
                    Next

                Else
                    j2 += 1
                End If
            Next
            alignments(centerIndex) = kband.globalAlign(0)
        Else
            Dim j2 = 0
            Dim globalAlign0 = kband.globalAlign(Scan0)

            For j1 As Integer = 0 To alignments(centerIndex).Length - 1
                If (alignments(centerIndex)(j1) <> globalAlign0.CharAtOrDefault(j2)) Then
                    With multipleAlign(i)
                        If .Length > j1 Then
                            Dim a As New StringBuilder(multipleAlign(i))
                            a.Insert(j1, "-"c)
                            multipleAlign(i) = a.ToString()
                        Else
                            multipleAlign(i) = .ToString & New String("-"c, j1 - .Length)
                        End If
                    End With
                Else
                    j2 += 1
                End If
            Next
        End If
    End Sub

    Private Sub SyncGapsAtPosition(pos As Integer)
        For idx As Integer = 0 To multipleAlign.Length - 1
            If multipleAlign(idx) IsNot Nothing AndAlso multipleAlign(idx).Length < pos Then
                ' 如果序列长度不足，在末尾补足空格直到该位置'
                multipleAlign(idx) = multipleAlign(idx).PadRight(pos, "-"c)
            ElseIf multipleAlign(idx) IsNot Nothing Then
                ' 在指定位置插入空格'
                Dim sb As New StringBuilder(multipleAlign(idx))
                sb.Insert(pos, "-"c)
                multipleAlign(idx) = sb.ToString()
            End If
        Next
    End Sub

    ''' <summary>
    ''' This Function finds the minimum star cost from all sequences
    ''' </summary>
    Private Sub findStarIndex()
        Dim editDist = 0
        Dim minEditDist = Integer.MaxValue
        Dim n = sequence.Length

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                editDist += kband.CalculateEditDistance(sequence(i), sequence(j))
            Next

            If (editDist < minEditDist) Then
                minEditDist = editDist
                starIndex = i
            End If

            editDist = 0
        Next

        Call VBDebugger.EchoLine($"use [#{starIndex + 1}]{names(starIndex)} sequence as the start center sequence for make alignment!")
    End Sub
End Class
