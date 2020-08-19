Imports Microsoft.VisualBasic.Linq

''' <summary>
''' data signal resampler for continuous signals
''' </summary>
Public Class Resampler

    Dim raw As GeneralSignal
    Dim x As Double()
    Dim y As Double()

    Public ReadOnly Property enumerateMeasures As Double()
        Get
            Return x.ToArray
        End Get
    End Property

    Public Function GetIntensity(x As Double) As Double
        Dim i As Integer = getPosition(x)

        If i = Me.x.Length Then
            Return Me.x(i - 1)
        ElseIf Me.x(i) = x OrElse i = Me.x.Length - 1 Then
            Return Me.y(i)
        Else
            Dim x1 = Me.x(i)
            Dim x2 = Me.x(i + 1)
            Dim y1 = Me.y(i)
            Dim y2 = Me.y(i + 1)
            Dim scale As Double = (x - x1) / (x2 - x1)
            Dim dy As Double = (y2 - y1) * scale

            Return y1 + dy
        End If
    End Function

    Private Function getPosition(x As Double) As Integer
        For i As Integer = 0 To Me.x.Length - 1
            If Me.x(i) >= x Then
                Return i
            End If
        Next

        Return Me.x.Length
    End Function

    Public Shared Function CreateSampler(raw As GeneralSignal) As Resampler
        Dim x = raw.Measures _
            .SeqIterator _
            .OrderBy(Function(xi) xi.value) _
            .ToArray
        Dim y = raw.Strength

        Return New Resampler With {
            .raw = raw,
            .x = x.Select(Function(xi) xi.value).ToArray,
            .y = x.Select(Function(xi) y(xi.i)).ToArray
        }
    End Function
End Class
