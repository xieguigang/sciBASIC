Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports std = System.Math

Namespace DownSampling


    Public Module EventOrder

        Public Function BY_TIME_ASC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.time < e2.time, -1, 1)
        End Function

        Public Function BY_VAL_ASC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.intensity < e2.intensity, -1, 1)
        End Function

        Public Function BY_VAL_DESC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.intensity < e2.intensity, 1, -1)
        End Function

        Public Function BY_ABS_VAL_ASC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(std.Abs(e1.intensity) < std.Abs(e2.intensity), -1, 1)
        End Function

        Public Function BY_ABS_VAL_DESC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(std.Abs(e1.intensity) < std.Abs(e2.intensity), 1, -1)
        End Function
    End Module

End Namespace