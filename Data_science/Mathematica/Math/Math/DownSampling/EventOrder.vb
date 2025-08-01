Imports std = System.Math

Namespace DownSampling


    Public Module EventOrder

        Public Function BY_TIME_ASC(e1 As [Event], e2 As [Event]) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.Time < e2.Time, -1, 1)
        End Function

        Public Function BY_VAL_ASC(e1 As [Event], e2 As [Event]) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.Value < e2.Value, -1, 1)
        End Function

        Public Function BY_VAL_DESC(e1 As [Event], e2 As [Event]) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.Value < e2.Value, 1, -1)
        End Function

        Public Function BY_ABS_VAL_ASC(e1 As [Event], e2 As [Event]) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(std.Abs(e1.Value) < std.Abs(e2.Value), -1, 1)
        End Function

        Public Function BY_ABS_VAL_DESC(e1 As [Event], e2 As [Event]) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(std.Abs(e1.Value) < std.Abs(e2.Value), 1, -1)
        End Function
    End Module

End Namespace