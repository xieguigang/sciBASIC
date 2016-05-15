Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Terminal.Utility

    Public Class EventProc

        Dim current As Integer
        Public Property Capacity As Integer
            Get
                Return _Capacity
            End Get
            Set(value As Integer)
                _Capacity = value
                delta = Capacity / 100
                current = CInt(_Capacity * p)
            End Set
        End Property

        Dim _Capacity As Integer
        Dim delta As Integer
        Dim TAG As String

        Sub New(n As Integer, <CallerMemberName> Optional TAG As String = "")
            Me.Capacity = n
            Me.TAG = TAG

            If String.IsNullOrEmpty(Me.TAG) Then
                Me.TAG = vbTab
            End If
        End Sub

        Public Function Tick() As Integer
            If delta = 0 Then
                Return 0
            End If

            current += 1
            If current Mod delta = 0 Then
                Call ToString.__DEBUG_ECHO
            Else
                Call Console.Write(".")
            End If

            Return current
        End Function

        Dim p As Double
        Dim sw As Stopwatch = Stopwatch.StartNew
        Dim pre As Long

        Public Overrides Function ToString() As String
            If Capacity = 0 Then
                Return ""
            End If

            Dim dt As Long = sw.ElapsedMilliseconds - pre
            pre = sw.ElapsedMilliseconds
            p = current / Capacity

            Return $" [{TAG}, {dt}ms] * ...... {Math.Round(100 * p, 2)}%"
        End Function
    End Class
End Namespace