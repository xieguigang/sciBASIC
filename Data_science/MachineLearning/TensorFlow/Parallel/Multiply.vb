Imports Microsoft.VisualBasic.Parallel

Namespace Parallel

    Public Class MultiplyScale : Inherits VectorTask

        ReadOnly T As Tensor
        ReadOnly s As Double

        Sub New(tensor As Tensor, scale As Double)
            Call MyBase.New(tensor.nrValues)

            T = New Tensor(tensor, copy:=True)
            s = scale
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For c As Integer = start To ends
                T.values(c) = T.values(c) * s
            Next
        End Sub

        ''' <summary>
        ''' Scale all elements with a number
        ''' </summary>
        ''' <param name="tensor"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Function Scale(tensor As Tensor, s As Double) As Tensor
            If tensor.nrValues < 1000 Then
                Dim T As New Tensor(tensor, True)

                For c As Integer = 0 To tensor.nrValues - 1
                    T.values(c) = tensor.values(c) * s
                Next

                Return T
            Else
                Dim task As New MultiplyScale(tensor, s)
                Call task.Run()
                Return task.T
            End If
        End Function
    End Class
End Namespace