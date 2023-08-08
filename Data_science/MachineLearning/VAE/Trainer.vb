Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Public Class Trainer

    Dim loss_vector As Vector = Nothing
    Dim vae As Vae

    Sub New(N1 As Integer, N2 As Integer)
        vae = New Vae(N1, N2)
    End Sub

    Public Sub train_vae(steps As Integer, dataset_input As Byte()())
        Dim input As Double() = New Double(dataset_input(0).Length - 1) {}
        Dim div As Double = 1 / 256

        For i As Integer = 0 To std.Min(dataset_input.Length, steps) - 1
            ' handle input
            If dataset_input(i).Length = 0 Then
                Continue For
            End If

            Dim mean As Double = 0

            For j As Integer = 0 To dataset_input(i).Length - 1
                mean += dataset_input(i)(j)
                input(j) = dataset_input(i)(j)
            Next
            For j As Integer = 0 To input.Length - 1
                input(j) -= mean
                input(j) *= div
            Next

            ' update weights
            vae.encode(input)
            vae.decode
            vae.update(input)
        Next
    End Sub
End Class
