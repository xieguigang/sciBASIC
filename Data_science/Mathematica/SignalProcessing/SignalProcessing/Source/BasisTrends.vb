#Region "Microsoft.VisualBasic::e3f4a5b6c7d8e901020304f5a6b7c8d9, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisTrends.vb"

    ' Author:
    ' 
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed

    ' The trend & gradient basis functions:
    ' Linear, Exponential, Logarithm and Power.

#End Region

Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' linear trend: Amp * (x - Center) / Scale + Offset.
    ''' </summary>
    ''' <remarks>The simplest constant growth / decline.</remarks>
    Public Class Linear : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            Return u
        End Function
    End Class

    ''' <summary>
    ''' exponential growth / decay: Amp * e^(alpha (x - Center) / Scale) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' With a positive <see cref="Rate"/> it models unbounded population growth,
    ''' radioactive decay or chemical concentration; with a negative rate it decays.
    ''' </remarks>
    Public Class Exponential : Inherits BasisFunction

        ''' <summary>the growth / decay rate (alpha).</summary>
        Public Property Rate As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            Return std.Exp(Rate * u)
        End Function
    End Class

    ''' <summary>
    ''' logarithmic trend: Amp * ln((x - Center) / Scale) + Offset.
    ''' </summary>
    ''' <remarks>
    ''' Models diminishing returns, learning curves or fast-then-flat evolution.
    ''' Returns <see cref="BasisFunction.Offset"/> for x &lt;= Center to avoid NaN.
    ''' </remarks>
    Public Class Logarithm : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            If u <= 0 Then
                Return 0.0
            End If
            Return std.Log(u)
        End Function
    End Class

    ''' <summary>
    ''' power-law trend: Amp * ((x - Center) / Scale)^alpha + Offset.
    ''' </summary>
    ''' <remarks>
    ''' 0 &lt; alpha &lt; 1 behaves like a log curve; alpha &gt; 1 behaves like
    ''' exponential growth. Extremely common in nature (fluid flow, fractals).
    ''' For fractional alpha the domain is restricted to x &gt;= Center.
    ''' </remarks>
    Public Class Power : Inherits BasisFunction

        ''' <summary>the exponent (alpha).</summary>
        Public Property Exponent As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            If u <= 0 Then
                Return 0.0
            End If
            Return std.Pow(u, Exponent)
        End Function
    End Class
End Namespace
