Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Public Module Doppler

    Public Function Calculate(signals As IEnumerable(Of ITimeSignal))
        ' 首先平移到最低点
        Dim raw = signals.ToArray
        Dim into = raw.Min(Function(x) x.intensity)

        If into < 0 Then
            into = stdNum.Abs(into)
            raw = raw _
                .Select(Function(a)
                            Return New TimeSignal With {
                                .time = a.time,
                                .intensity = a.intensity + into
                            }
                        End Function) _
                .As(Of ITimeSignal) _
                .ToArray
        End If

        ' 查找出所有的peaks
        Dim allPeaks = New ElevationAlgorithm(5, 0.65).FindAllSignalPeaks(raw).ToArray
        ' 计算出频率的变化趋势
        Throw New NotImplementedException
    End Function
End Module
