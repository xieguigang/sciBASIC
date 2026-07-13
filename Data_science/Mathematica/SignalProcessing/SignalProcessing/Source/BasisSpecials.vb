#Region "Microsoft.VisualBasic::a5b6c7d8e9f00112233445566778899, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisSpecials.vb"

    ' Author:
    ' 
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed

    ' The composite & special basis functions:
    ' LogNormal, Sinc and Gompertz.

#End Region

Imports std = System.Math

Namespace Source.Generators

    ''' <summary>
    ''' log-normal function: an asymmetric bump, fast rise then long-tailed decay.
    ''' </summary>
    ''' <remarks>
    ''' Many physical quantities (particle sizes, income, reaction times) are not
    ''' normal but log-normal. Here Center maps to the log-mean mu and Scale maps
    ''' to the log-standard-deviation sigma:
    ''' shape(u) = exp(-(ln((x-Center)) - mu)^2 / (2 sigma^2)) / ((x-Center) sigma sqrt(2π)).
    ''' Returns 0 for x &lt;= Center to keep the domain valid.
    ''' </remarks>
    Public Class LogNormal : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            If u <= 0 Then
                Return 0.0
            End If
            Dim z = std.Log(u)
            ' normalized log-normal density shape (peak-adjusted to ~1 at mode)
            Dim density = std.Exp(-(z * z) / 2) / (u * std.Sqrt(2 * std.PI))
            ' rescale so the peak is near 1 for convenient amp handling
            Return density * std.Sqrt(2 * std.PI)
        End Function
    End Class

    ''' <summary>
    ''' sinc function: sin(π (x - Center) / Scale) / (π (x - Center) / Scale).
    ''' </summary>
    ''' <remarks>
    ''' The core function of signal processing; models the ideal low-pass filter
    ''' response or optical diffraction patterns. At the center it is defined as 1.
    ''' </remarks>
    Public Class Sinc : Inherits BasisFunction

        Protected Overrides Function Shape(u As Double) As Double
            If u = 0 Then
                Return 1.0
            End If
            Dim px = std.PI * u
            Return std.Sin(px) / px
        End Function
    End Class

    ''' <summary>
    ''' Gompertz function: an asymmetric S-curve, flatter at both ends than a logistic.
    ''' </summary>
    ''' <remarks>
    ''' Commonly used for tumor growth or biological population limits. Here
    ''' Scale is the asymptotic cap and the intrinsic growth rate is fixed;
    ''' Center shifts the inflection point. Value stays in (0, Scale].
    ''' </remarks>
    Public Class Gompertz : Inherits BasisFunction

        ''' <summary>the intrinsic growth rate (positive).</summary>
        Public Property Rate As Double = 1.0

        Protected Overrides Function Shape(u As Double) As Double
            If u >= 0 Then
                ' growth towards the cap
                Return Scale * std.Exp(-std.Exp(-Rate * u))
            Else
                ' decay toward 0 before the center
                Return Scale * (1 - std.Exp(-std.Exp(Rate * u)))
            End If
        End Function
    End Class
End Namespace
