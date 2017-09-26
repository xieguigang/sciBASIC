Imports System.Drawing

Namespace d3js.Layout

    Public Class Label

        ''' <summary>
        ''' the x-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
        ''' <summary>
        ''' the y-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
        ''' <summary>
        ''' the width of the label (approximating the label as a rectangle).
        ''' </summary>
        ''' <returns></returns>
        Public Property width As Double
        ''' <summary>
        ''' the height of the label (same approximation).
        ''' </summary>
        ''' <returns></returns>
        Public Property height As Double
        ''' <summary>
        ''' the label text.
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        Public Overrides Function ToString() As String
            Return $"{text}@({X.ToString("F2")},{Y.ToString("F2")})"
        End Function

        Public Shared Narrowing Operator CType(label As Label) As PointF
            Return New PointF With {
                .X = label.X,
                .Y = label.Y
            }
        End Operator
    End Class
End Namespace