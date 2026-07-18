#Region "Microsoft.VisualBasic::c5bd8029dda98d64aa149b7342f775f0, Data_science\MachineLearning\DeepQNetwork\QAgent.vb"

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

    '   Total Lines: 140
    '    Code Lines: 81 (57.86%)
    ' Comment Lines: 40 (28.57%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 19 (13.57%)
    '     File Size: 5.45 KB


    ' Class QAgent
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: act, GreedyAction
    ' 
    '     Sub: BeginEpisode, Learn, RecordReward, Store
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' DQN 智能体：ε-贪心选动作、经验回放、TD 目标（可选目标网络），
' 维护训练统计（回合奖励 / ε / 步数 / 当前 TD 误差）。

Imports System
Imports Microsoft.VisualBasic.MachineLearning.CNN

''' <summary>
''' Deep Q-Network agent.
''' Holds an online network (and optionally a target network for stable
''' bootstrapping), an experience replay buffer, and performs the standard
''' DQN update: sample a minibatch, build the TD target
'''   targetQ[a] = r + γ·max_a' Q(s')   (or r when done)
''' and fit the online network toward it via regression.
''' </summary>
Public Class QAgent

    ''' <summary>the online (behavior) Q-network being trained</summary>
    Public online As QNetwork
    ''' <summary>the slowly-updated target network (Nothing when disabled)</summary>
    Public target As QNetwork

    Private useTargetNet As Boolean
    Private buffer As ReplayBuffer
    Private rnd As New Random

    ' ----- hyper-parameters -----
    ''' <summary>discount factor</summary>
    Public gamma As Double = 0.99
    ''' <summary>exploration rate (ε-greedy)</summary>
    Public epsilon As Double = 1.0
    ''' <summary>minimum exploration rate</summary>
    Public epsilonMin As Double = 0.05
    ''' <summary>per-learn multiplicative decay of ε</summary>
    Public epsilonDecay As Double = 0.995
    ''' <summary>replay minibatch size</summary>
    Public batchSize As Integer = 32
    ''' <summary>target network sync interval (in stored steps)</summary>
    Public targetUpdateEvery As Integer = 200

    ' ----- running statistics -----
    ''' <summary>reward accumulated in the current episode</summary>
    Public episodeReward As Double
    ''' <summary>policy steps taken in the current episode</summary>
    Public stepsThisEpisode As Integer
    ''' <summary>total stored transitions</summary>
    Public totalSteps As Integer
    ''' <summary>last |TD error| observed during Learn()</summary>
    Public lastTDError As Double

    ''' <summary>
    ''' Create a DQN agent.
    ''' </summary>
    ''' <param name="online">the online Q-network (also used to seed the target net)</param>
    ''' <param name="useTargetNet">whether to maintain a periodically-synced target network</param>
    ''' <param name="replayCapacity">maximum replay buffer capacity</param>
    Sub New(online As QNetwork, Optional useTargetNet As Boolean = True, Optional replayCapacity As Integer = 50000)
        Me.online = online
        Me.useTargetNet = useTargetNet
        Me.buffer = New ReplayBuffer(replayCapacity)
        If useTargetNet Then
            Me.target = online.Clone()
        End If
    End Sub

    ''' <summary>ε-greedy action selection</summary>
    Public Function act(state As Double()) As Integer
        If rnd.NextDouble() < epsilon Then
            Return rnd.Next(online.ActionCount)
        End If
        Return online.argmaxAction(state)
    End Function

    ''' <summary>purely greedy action selection (for demonstration / evaluation)</summary>
    Public Function GreedyAction(state As Double()) As Integer
        Return online.argmaxAction(state)
    End Function

    ''' <summary>store a transition and periodically sync the target network</summary>
    Public Sub Store(s As Double(), a As Integer, r As Double, s2 As Double(), done As Boolean)
        buffer.Add(New Experience With {.state = s, .action = a, .reward = r, .nextState = s2, .done = done})
        totalSteps += 1
        stepsThisEpisode += 1
        If useTargetNet AndAlso totalSteps Mod targetUpdateEvery = 0 Then
            target.CopyWeightsFrom(online)
        End If
    End Sub

    ''' <summary>
    ''' perform one DQN learning step: sample a minibatch from replay and
    ''' fit the online network toward the TD targets.
    ''' </summary>
    Public Sub Learn()
        If buffer.Count < batchSize Then
            Return
        End If

        Dim batch = buffer.Sample(batchSize)

        For Each exp In batch
            Dim qvec = online.predictQ(exp.state)
            Dim targetVec = CType(qvec.Clone(), Double())

            Dim nextMax As Double
            If exp.done Then
                nextMax = 0.0
            Else
                Dim nq As Double()
                If useTargetNet Then
                    nq = target.predictQ(exp.nextState)
                Else
                    nq = online.predictQ(exp.nextState)
                End If
                nextMax = nq(0)
                For i As Integer = 1 To nq.Length - 1
                    If nq(i) > nextMax Then nextMax = nq(i)
                Next
            End If

            Dim td = exp.reward + gamma * nextMax
            lastTDError = td - targetVec(exp.action)
            targetVec(exp.action) = td

            online.trainOnTargets(exp.state, targetVec)
        Next

        epsilon = Math.Max(epsilonMin, epsilon * epsilonDecay)
    End Sub

    ''' <summary>reset per-episode counters (call at the start of each episode)</summary>
    Public Sub BeginEpisode()
        stepsThisEpisode = 0
        episodeReward = 0.0
    End Sub

    ''' <summary>accumulate reward for the current episode (for external tracking)</summary>
    Public Sub RecordReward(r As Double)
        episodeReward += r
    End Sub
End Class

