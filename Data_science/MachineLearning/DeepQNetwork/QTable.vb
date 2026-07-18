#Region "Microsoft.VisualBasic::7a9804f1b2647451935892913b6fab02, Data_science\MachineLearning\DeepQNetwork\QTable.vb"

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

    '   Total Lines: 106
    '    Code Lines: 70 (66.04%)
    ' Comment Lines: 24 (22.64%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 12 (11.32%)
    '     File Size: 3.83 KB


    ' Class QTable
    ' 
    '     Properties: ActionCount
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: act, key, row
    ' 
    '     Sub: train
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 字面量离散 Q 表：将连续状态分桶离散化后维护 Q(s,a) 查表，
' 用于小规模 / 离散环境，并作为“QTable”概念的基线实现。

Imports System
Imports std = System.Math

''' <summary>
''' Literal tabular Q-learning. A continuous state is discretized into a fixed
''' number of bins per dimension and mapped to a string key; the Q(s,a) table
''' is then a dictionary of action-value vectors. This honours the "QTable"
''' concept directly and works as a baseline for small / discrete problems.
''' </summary>
Public Class QTable

    Private table As New Dictionary(Of String, Double())
    Private rnd As New Random
    Private bins As Integer
    Private stateDim As Integer

    ''' <summary>discount factor</summary>
    Public gamma As Double = 0.99
    ''' <summary>learning rate</summary>
    Public alpha As Double = 0.1
    ''' <summary>initial exploration rate</summary>
    Public epsilon As Double = 1.0
    ''' <summary>minimum exploration rate</summary>
    Public epsilonMin As Double = 0.05
    ''' <summary>per-train multiplicative decay of epsilon</summary>
    Public epsilonDecay As Double = 0.995

    ''' <summary>number of discrete actions</summary>
    Public ReadOnly Property ActionCount As Integer

    ''' <summary>
    ''' Create a Q-table.
    ''' </summary>
    ''' <param name="stateDim">dimension of the state vector</param>
    ''' <param name="actionCount">number of discrete actions</param>
    ''' <param name="bins">discretization bins per state dimension</param>
    Sub New(stateDim As Integer, actionCount As Integer, Optional bins As Integer = 10)
        Me.stateDim = stateDim
        Me.ActionCount = actionCount
        Me.bins = bins
    End Sub

    Private Function key(state As Double()) As String
        Dim parts(stateDim - 1) As String
        For i As Integer = 0 To stateDim - 1
            Dim v = state(i)
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                v = 0
            End If
            ' map roughly [-4, 4] into [0, bins-1]
            Dim b = CInt(std.Max(0, std.Min(bins - 1, std.Floor((v + 4.0) / 8.0 * bins))))
            parts(i) = b.ToString()
        Next
        Return String.Join(",", parts)
    End Function

    Private Function row(k As String) As Double()
        Dim r As Double() = Nothing
        If Not table.TryGetValue(k, r) Then
            r = New Double(ActionCount - 1) {}
            table(k) = r
        End If
        Return r
    End Function

    ''' <summary>epsilon-greedy action selection</summary>
    Public Function act(state As Double()) As Integer
        If rnd.NextDouble() < epsilon Then
            Return rnd.Next(ActionCount)
        End If
        Dim r = row(key(state))
        Dim best = 0, bestv = r(0)
        For i As Integer = 1 To ActionCount - 1
            If r(i) > bestv Then
                bestv = r(i)
                best = i
            End If
        Next
        Return best
    End Function

    ''' <summary>one Q-learning update toward the TD target</summary>
    Public Sub train(state As Double(), action As Integer, reward As Double, nextState As Double(), done As Boolean)
        Dim k = key(state)
        Dim r = row(k)
        Dim td As Double

        If done Then
            td = reward
        Else
            Dim nr = row(key(nextState))
            Dim mx = nr(0)
            For i As Integer = 1 To ActionCount - 1
                If nr(i) > mx Then mx = nr(i)
            Next
            td = reward + gamma * mx
        End If

        r(action) = r(action) + alpha * (td - r(action))
        epsilon = std.Max(epsilonMin, epsilon * epsilonDecay)
    End Sub
End Class

