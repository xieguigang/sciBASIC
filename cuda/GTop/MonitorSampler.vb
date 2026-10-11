#Region "Microsoft.VisualBasic::gtop-sampler, cuda\GTop\MonitorSampler.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

''' <summary>
''' 按固定间隔轮询单个 NvmlDevice，累积定长样本缓冲。
''' 轮询为低频（默认 1s），采样成本极低；单点采样失败由 NvmlDevice 内部吞掉，
''' 不会因个别指标不可用而中断循环。
''' </summary>
Public Class MonitorSampler

    Private ReadOnly _device As NvmlDevice
    Private ReadOnly _intervalMs As Integer
    Private ReadOnly _maxRows As Integer
    Private ReadOnly _samples As New List(Of NvmlSample)()

    Public Sub New(device As NvmlDevice, intervalMs As Integer, maxRows As Integer)
        _device = device
        _intervalMs = Math.Max(50, intervalMs)
        _maxRows = Math.Max(1, maxRows)
    End Sub

    Public ReadOnly Property Device As NvmlDevice
        Get
            Return _device
        End Get
    End Property

    Public ReadOnly Property Samples As IReadOnlyList(Of NvmlSample)
        Get
            Return _samples.ToArray()
        End Get
    End Property

    ''' <summary>采集一次并追加到缓冲（超出上限时丢弃最旧样本）</summary>
    Public Function CollectOnce() As NvmlSample
        Dim s = _device.GetSample()
        _samples.Add(s)
        If _samples.Count > _maxRows Then
            _samples.RemoveAt(0)
        End If
        Return s
    End Function

    ''' <summary>
    ''' 循环采集直到 shouldStop 为真；每次采集后调用 onSample（可为空）。
    ''' </summary>
    Public Sub Run(shouldStop As Func(Of Boolean), Optional onSample As Action(Of NvmlSample) = Nothing)
        While Not shouldStop()
            Dim s = CollectOnce()
            If onSample IsNot Nothing Then onSample(s)
            If shouldStop() Then Exit While
            Threading.Thread.Sleep(_intervalMs)
        End While
    End Sub
End Class
