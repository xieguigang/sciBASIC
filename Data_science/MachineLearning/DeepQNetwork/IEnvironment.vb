#Region "Microsoft.VisualBasic::e7f070a8e3965f089b4d3600e1fc15e6, Data_science\MachineLearning\DeepQNetwork\IEnvironment.vb"

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

    '   Total Lines: 46
    '    Code Lines: 16 (34.78%)
    ' Comment Lines: 19 (41.30%)
    '    - Xml Docs: 89.47%
    ' 
    '   Blank Lines: 11 (23.91%)
    '     File Size: 1.75 KB


    ' Interface IEnvironment
    ' 
    '     Properties: ActionCount, Actions, CurrentState, IsDone, LastReward
    '                 StateSize
    ' 
    '     Function: [Step], Reset
    ' 
    ' Structure StepResult
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 通用强化学习环境契约：任何环境只要实现该接口，即可接入 QAgent 进行训练。

Imports System

''' <summary>
''' Generic reinforcement-learning environment contract.
''' An environment exposes a fixed-size state vector and a discrete action set,
''' and can be stepped by the agent.
''' </summary>
Public Interface IEnvironment

    ''' <summary>size of the state vector fed to the agent</summary>
    ReadOnly Property StateSize As Integer

    ''' <summary>number of discrete actions</summary>
    ReadOnly Property ActionCount As Integer

    ''' <summary>enum values of the discrete action set (may be Nothing for index-based sets)</summary>
    ReadOnly Property Actions As Array

    ''' <summary>reset to a new episode, returns the initial state</summary>
    Function Reset() As Double()

    ''' <summary>apply an action, advance one decision step, return (nextState, reward, done)</summary>
    Function [Step](action As Integer) As StepResult

    ''' <summary>the current (post-step) state, for inspection</summary>
    ReadOnly Property CurrentState As Double()

    ''' <summary>the most recent reward</summary>
    ReadOnly Property LastReward As Double

    ''' <summary>whether the episode has terminated</summary>
    ReadOnly Property IsDone As Boolean
End Interface

''' <summary>the result of a single environment step</summary>
Public Structure StepResult
    ''' <summary>state observed after the action</summary>
    Public state As Double()
    ''' <summary>scalar reward received</summary>
    Public reward As Double
    ''' <summary>whether the episode ended</summary>
    Public done As Boolean
End Structure

