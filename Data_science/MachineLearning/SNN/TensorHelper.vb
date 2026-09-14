Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Public Module TensorHelper

    Public Function BatchTensor(data As Double()(), idx As Integer(), offset As Integer, count As Integer) As Tensor
        Dim f = data(0).Length
        Dim flat(f * count - 1) As Double
        For b = 0 To count - 1
            Array.Copy(data(idx(offset + b)), 0, flat, b * f, f)
        Next
        Return New Tensor(flat, count, f)
    End Function

    Public Function AllTensor(d As NumericTable) As Tensor
        Dim n = d.nsamples
        Dim idx(n - 1) As Integer
        For i = 0 To n - 1
            idx(i) = i
        Next
        Return BatchTensor(d.features, idx, 0, n)
    End Function
End Module
