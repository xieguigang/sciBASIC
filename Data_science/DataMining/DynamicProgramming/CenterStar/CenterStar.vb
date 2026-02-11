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
Imports Microsoft.VisualBasic.Linq

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
    Dim multipleAlign$()
    Dim sequence$()
    Dim names$()
    Dim kband As KBandSearch
    Dim editScores As Integer()

    Public ReadOnly Property NameList As String()
        Get
            Return names
        End Get
    End Property

    Public Const GapChar As Char = "-"c

    Sub New(input As IEnumerable(Of NamedValue(Of String)), Optional kband As Integer = 32)
        With input.ToArray
            sequence = .Select(Function(fa) fa.Value) _
                       .ToArray
            names = .Select(Function(fa) fa.Name) _
                    .ToArray
        End With

        Me.kband = New KBandSearch(globalAlign:=New String(2) {}, kband)
        Me.editScores = New Integer(sequence.Length - 1) {}
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
    Public Function Compute(matrix As IScore(Of Char),
                            ByRef alignment As String(),
                            ByRef Optional edits As Integer() = Nothing) As Double
        Dim totalCost#

        If sequence.All(Function(s) s = sequence(Scan0)) Then
            ' 所输入的序列全部都是一样的？？
            alignment = sequence.ToArray
            totalCost = 0
            edits = New Integer(alignment.Length - 1) {}
        Else
            totalCost = computeInternal(matrix)
            edits = editScores.ToArray
            alignment = multipleAlign.ToArray
        End If

        Return totalCost
    End Function

    Private Function computeInternal(matrix As IScore(Of Char)) As Double
        Dim n As Integer = sequence.Length

        multipleAlign = New String(n - 1) {}

        Call FindStarIndex(n)
        Call MultipleAlignment(n)

        Return calculateTotalCost(matrix, n)
    End Function

    ''' <summary>
    ''' this Function calculate() the total cost
    ''' </summary>
    ''' <returns></returns>
    ''' 
    Private Function calculateTotalCost(matrix As IScore(Of Char), n%) As Double
        Dim length As Integer = multipleAlign(0).Length
        Dim totalScore# = 0

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                If j > i Then
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
    Private Sub MultipleAlignment(n As Integer)
        multipleAlign(starIndex) = sequence(starIndex)

        For i As Integer = 0 To n - 1
            If i = starIndex Then
                Continue For
            End If

            ' 执行双序列比对
            editScores(i) = kband.CalculateEditDistance(multipleAlign(starIndex), sequence(i))
            multipleAlign(i) = kband.globalAlign(1)

            ' 统一处理空格插入，确保所有序列长度一致'
            Call SyncGaps(multipleAlign, starIndex, i)
        Next

        Dim maxLen As Integer = Aggregate a As String
                                In multipleAlign
                                Into Max(a.Length)

        For i As Integer = 0 To multipleAlign.Length - 1
            multipleAlign(i) = multipleAlign(i).PadRight(maxLen, GapChar)
        Next
    End Sub

    ''' <summary>
    ''' 同步所有序列的空格插入位置，以新中心序列为基准统一对齐
    ''' </summary>
    ''' <param name="alignments">待同步的序列数组</param>
    ''' <param name="centerIndex">中心序列的索引</param>
    ''' <param name="i">当前新序列的索引</param>
    Private Sub SyncGaps(ByRef alignments As String(), centerIndex As Integer, i As Integer)
        Dim oldCenter As String = alignments(centerIndex)
        Dim newCenter As String = kband.globalAlign(0)
        Dim newSeq As String = kband.globalAlign(1)
        ' 1. 计算旧中心序列到新中心序列的空格插入位置
        '    按位置降序排序，避免插入时索引变动
        Dim gapPositions As Integer() = FindGapInsertPositions(oldCenter, newCenter) _
            .OrderByDescending(Function(p) p) _
            .ToArray

        If gapPositions.Length > 0 Then
            ' 2. 将所有序列（包括新序列）在指定位置插入空格
            Call ApplyGapInsertions(alignments, gapPositions, i)
        End If

        ' 3. 不更新中心序列，只更新当前序列i
        alignments(i) = newSeq
    End Sub

    ''' <summary>
    ''' 比较新旧中心序列，找出需要插入空格的位置
    ''' </summary>
    Private Shared Iterator Function FindGapInsertPositions(oldCenter As String, newCenter As String) As IEnumerable(Of Integer)
        Dim iOld As Integer = 0
        Dim iNew As Integer = 0

        While iOld < oldCenter.Length AndAlso iNew < newCenter.Length
            If iOld < oldCenter.Length AndAlso oldCenter(iOld) = newCenter(iNew) Then
                ' 字符匹配：移动双指针
                iOld += 1
                iNew += 1
            ElseIf newCenter(iNew) = GapChar Then
                ' 修复：避免在位置0插入gap，防止序列开头出现gap
                If iOld > 0 Then
                    Yield iOld
                End If
                iNew += 1
            Else
                ' 字符不匹配：同步移动指针
                iOld += 1
                iNew += 1
            End If
        End While

        ' 处理新中心序列末尾剩余的空格
        While iNew < newCenter.Length
            If newCenter(iNew) = GapChar Then
                ' 在旧序列末尾插入
                Yield iOld
            End If

            iNew += 1
        End While
    End Function

    ''' <summary>
    ''' 将所有序列在指定位置插入空格（从后向前处理避免索引偏移）
    ''' </summary>
    Private Sub ApplyGapInsertions(alignments As String(), sortedGaps As Integer(), i As Integer)
        ' 修复：只对其他序列（非中心序列）插入gap，保持中心序列固定
        For index As Integer = 0 To i
            ' 跳过中心序列
            If index <> starIndex Then
                Dim sb As New StringBuilder(alignments(index))

                For Each pos As Integer In sortedGaps
                    If pos <= sb.Length Then
                        sb.Insert(pos, GapChar)
                    Else
                        sb.Append(GapChar)
                    End If
                Next

                alignments(index) = sb.ToString()
            End If
        Next
    End Sub

    ''' <summary>
    ''' This Function finds the minimum star cost from all sequences
    ''' </summary>
    Private Sub FindStarIndex(n As Integer)
        Dim editDists As Integer() = New Integer(n - 1) {}
        Dim k As Integer = Me.kband.K

        Call System.Threading.Tasks.Parallel.For(0, n,
            Sub(i)
                Dim editDist As Integer = 0
                Dim kband As New KBandSearch(globalAlign:=New String(2) {}, k)

                For j As Integer = i + 1 To n - 1 ' 避免重复计算
                    editDist += kband.CalculateEditDistance(sequence(i), sequence(j))
                    editDist += kband.CalculateEditDistance(sequence(j), sequence(i))
                Next

                SyncLock editDists
                    editDists(i) = editDist
                End SyncLock
            End Sub)

        ' use the index of min score as the star index
        starIndex = which.Min(editDists)

        Call VBDebugger.EchoLine($"use [#{starIndex + 1}]{names(starIndex)} sequence as the start center sequence for make alignment!")
    End Sub
End Class
