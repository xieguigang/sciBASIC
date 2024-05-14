#Region "Microsoft.VisualBasic::9de5453181d63f23f1623963f4dc474f, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\Program.vb"

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

    '   Total Lines: 28
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 994 B


    ' Module Program
    ' 
    '     Sub: Main, peakFinding
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding

Module Program

    Sub Main()
        Call peakFinding()
        Dim signal As TimeSignal() = TimeSignal.SignalSequence(Source.bumps(10000, 5).AsVector.Log(base:=10) * 100).ToArray

        Call signal.SaveTo("./signals.csv")

        Dim signal2 = New Source.SinusSignal().GetGraphData(10, 10)

        Call signal2.SaveTo("./signals2.csv")

        Pause()
    End Sub

    Sub peakFinding()
        Dim signals = File.Load("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\SignalProcessing\GUCA.csv").Skip(1).Select(Function(r) New TimeSignal With {.time = r(0), .intensity = r(1)}).ToArray
        Dim peaks = New ElevationAlgorithm(3, 0.65).FindAllSignalPeaks(signals).ToArray

        Pause()
    End Sub
End Module
