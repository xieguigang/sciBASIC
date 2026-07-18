#Region "Microsoft.VisualBasic::3ea93769df236f7e65bbf0e224f37019, Data_science\MachineLearning\DeepQNetwork\ReplayBuffer.vb"

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

    '   Total Lines: 72
    '    Code Lines: 48 (66.67%)
    ' Comment Lines: 12 (16.67%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 12 (16.67%)
    '     File Size: 1.99 KB


    ' Structure Experience
    ' 
    ' 
    ' 
    ' Class ReplayBuffer
    ' 
    '     Properties: Count
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Sample
    ' 
    '     Sub: Add, Clear
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 经验回放缓冲：环形缓冲存储 (s, a, r, s', done) 转移样本，支持小批量随机抽取。

Imports System

''' <summary>
''' A single transition experienced by the agent.
''' </summary>
Public Structure Experience
    Public state As Double()
    Public action As Integer
    Public reward As Double
    Public nextState As Double()
    Public done As Boolean
End Structure

''' <summary>
''' Fixed-capacity ring buffer of experiences for experience replay.
''' </summary>
Public Class ReplayBuffer

    Private ReadOnly capacity As Integer
    Private buffer As New List(Of Experience)
    Private head As Integer = 0
    Private filled As Integer = 0
    Private rnd As New Random

    Sub New(capacity As Integer)
        Me.capacity = capacity
    End Sub

    ''' <summary>number of stored experiences</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return filled
        End Get
    End Property

    ''' <summary>append a transition, overwriting the oldest when full</summary>
    Public Sub Add(exp As Experience)
        If filled < capacity Then
            buffer.Add(exp)
            filled += 1
        Else
            buffer(head) = exp
            head = (head + 1) Mod capacity
        End If
    End Sub

    ''' <summary>randomly sample <paramref name="batchSize"/> experiences (with replacement)</summary>
    Public Function Sample(batchSize As Integer) As List(Of Experience)
        Dim n = Math.Min(batchSize, filled)
        Dim outp As New List(Of Experience)

        If n = 0 Then
            Return outp
        End If

        For i As Integer = 1 To n
            outp.Add(buffer(rnd.Next(filled)))
        Next

        Return outp
    End Function

    ''' <summary>empty the buffer</summary>
    Public Sub Clear()
        buffer.Clear()
        head = 0
        filled = 0
    End Sub
End Class

