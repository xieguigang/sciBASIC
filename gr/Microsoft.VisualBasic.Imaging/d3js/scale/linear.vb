Imports Microsoft.VisualBasic.ComponentModel.Ranges

Namespace d3js.scale

    ''' <summary>
    ''' 连续性的映射
    ''' </summary>
    Public Class LinearScale : Inherits IScale(Of LinearScale)

        Dim _domain As DoubleRange

        ''' <summary>
        ''' Constructs a new continuous scale with the unit domain [0, 1], the unit range [0, 1], 
        ''' the default interpolator and clamping disabled. Linear scales are a good default 
        ''' choice for continuous quantitative data because they preserve proportional differences. 
        ''' Each range value y can be expressed as a function of the domain value x: ``y = mx + b``.
        ''' </summary>
        Sub New()
        End Sub

        Default Public Overrides ReadOnly Property Value(x As Double) As Double
            Get
                Return _domain.ScaleMapping(x, _range)
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(term As String) As Double
            Get
                Return Me(Val(term))
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{_domain.Min}, {_domain.Max}] --> [{_range.Min}, {_range.Max}]"
        End Function

        Public Overrides Function domain(values As IEnumerable(Of Double)) As LinearScale
            _domain = values.ToArray
            Return Me
        End Function

        Public Overrides Function domain(values As IEnumerable(Of String)) As LinearScale
            Return domain(values.Select(AddressOf Val))
        End Function

        Public Overrides Function domain(values As IEnumerable(Of Integer)) As LinearScale
            Return domain(values.Select(Function(x) CDbl(x)))
        End Function
    End Class
End Namespace