Imports Microsoft.VisualBasic.Imaging.BitmapImage

''' <summary>
''' SLIC (Simple Linear Iterative Clustering) clusters pixels using pixel channels and image plane space
''' to efficiently generate compact, nearly uniform superpixels. The simplicity of approach makes it
''' extremely easy To use a lone parameter specifies the number Of superpixels And the efficiency Of
''' the algorithm makes it very practical.
''' </summary>
Public Class SLIC

    ReadOnly bitmap As BitmapBuffer

    Sub New(bitmap As BitmapBuffer)
        Me.bitmap = bitmap
    End Sub

    Public Function MeasureSegments()

    End Function

End Class

Public Class SLICPixel

    Public Property x As Integer
    Public Property y As Integer
    Public Property a As Double
    Public Property r As Double
    Public Property g As Double
    Public Property b As Double
    Public Property cluster As Integer

End Class