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
