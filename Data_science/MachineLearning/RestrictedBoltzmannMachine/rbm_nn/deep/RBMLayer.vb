Imports Microsoft.VisualBasic.Serialization.JSON

Namespace nn.rbm.deep

    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' 
    ''' A layer can have multiple RBMs, this allows convolution-like networks when configuring a deep rbm
    ''' </summary>
    Public Class RBMLayer

        Public ReadOnly rbms As RBM()

        Public Sub New(rbms As RBM())
            Me.rbms = rbms
        End Sub

        Public Overridable Function getRBM(r As Integer) As RBM
            Return rbms(r)
        End Function

        Public Overridable Function size() As Integer
            Return rbms.Length
        End Function

        Public Overrides Function ToString() As String
            Return "RBMLayer{" & "rbms=" & rbms.GetJson() & "}"c.ToString()
        End Function
    End Class


End Namespace
