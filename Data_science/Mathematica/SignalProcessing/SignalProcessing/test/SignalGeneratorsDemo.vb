#Region "Microsoft.VisualBasic::46fd068e4807aba5eb07b612cc3d6e67, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\SignalGeneratorsDemo.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 76
    '    Code Lines: 48 (63.16%)
    ' Comment Lines: 13 (17.11%)
    '    - Xml Docs: 38.46%
    ' 
    '   Blank Lines: 15 (19.74%)
    '     File Size: 3.13 KB


    ' Module SignalGeneratorsDemo
    ' 
    '     Function: Grid
    ' 
    '     Sub: dumpExamples, formulaChecks, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Math.SignalProcessing.Source.Generators
Imports std = System.Math

Module SignalGeneratorsDemo

    ''' <summary>
    ''' run a few sanity checks that the basic formulas evaluate correctly,
    ''' and dump a couple of generated signals to CSV.
    ''' </summary>
    Sub Main()
        Call formulaChecks()
        Call dumpExamples()
        Pause()
    End Sub

    Sub formulaChecks()
        ' Gaussian peak should reach ~Amp at the center
        Dim g = Basis.Gaussian(amp:=5, center:=2, sigma:=0.5)
        Debug.Assert(std.Abs(g.Evaluate(2) - 5) < 0.001, "Gaussian peak failed")
        Debug.Assert(std.Abs(g.Evaluate(2) - g.Evaluate(2)) < 1E-9, "Gaussian symmetry failed")

        ' Sine at the center should be 0 (sin 0)
        Dim s = Basis.Sine(amp:=1, center:=0, scale:=1)
        Debug.Assert(std.Abs(s.Evaluate(0)) < 1E-9, "Sine(0) failed")

        ' Step should be 0 before center and ~1 after (Scale>0 => logistic)
        Dim st = Basis.[Step](amp:=1, center:=0, scale:=0.01)
        Debug.Assert(st.Evaluate(-1) < 0.001, "Step left failed")
        Debug.Assert(st.Evaluate(1) > 0.999, "Step right failed")

        ' Linear: f(center + scale) = offset + amp
        Dim l = Basis.Linear(amp:=2, center:=0, scale:=1, offset:=3)
        Debug.Assert(std.Abs(l.Evaluate(1) - 5) < 1E-9, "Linear failed")

        ' Sum combinator
        Dim sum = New Sum(Basis.Sine(1, 0, 1), Basis.Linear(2, 0, 1))
        Debug.Assert(std.Abs(sum.Evaluate(0) - 0) < 1E-9, "Sum failed")

        Console.WriteLine("All formula checks passed.")
    End Sub

    Sub dumpExamples()
        Dim x = Grid(0, 10, 0.01).ToArray

        ' A composite: trend + seasonality + gaussian peak + noise
        Dim sig = New SignalGenerator() _
            .Add(Basis.Linear(amp:=0.1, center:=0, scale:=10)) _
            .Add(Basis.Sine(amp:=2, center:=0, scale:=2)) _
            .Add(Basis.Gaussian(amp:=3, center:=5, sigma:=0.4)) _
            .Add(Basis.GaussianNoise(0.15))

        Call sig.ToGeneralSignal(x, reference:="composite", unit:="t").GetText.SaveTo("./composite_signal.csv")

        ' ECG preset
        Call Presets.ECG(period:=1, noise:=0.02) _
            .ToGeneralSignal(Grid(0, 3, 0.005).ToArray, reference:="ECG", unit:="s") _
            .GetText.SaveTo("./ecg_signal.csv")

        ' Weather preset
        Call Presets.Weather(days:=365) _
            .ToGeneralSignal(Grid(0, 365, 0.5).ToArray, reference:="weather", unit:="day") _
            .GetText.SaveTo("./weather_signal.csv")

        Console.WriteLine("Example signals written to ./composite_signal.csv, ./ecg_signal.csv, ./weather_signal.csv")
    End Sub

    ''' <summary>a tiny helper to build an evenly spaced sequence (like Python's range).</summary>
    Iterator Function Grid(min As Double, max As Double, stepSize As Double) As IEnumerable(Of Double)
        Dim x = min
        Do While x <= max
            Yield x
            x += stepSize
        Loop
    End Function
End Module
