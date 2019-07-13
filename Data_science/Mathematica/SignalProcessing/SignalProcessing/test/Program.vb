#Region "Microsoft.VisualBasic::684d9839d4e920a32079ca28adb4c250, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\Program.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.SignalProcessing

Module Program

    Sub Main()

        Dim signal As TimeSignal() = TimeSignal.SignalSequence(Source.bumps(10000, 5).AsVector.Log(base:=10) * 100).ToArray

        Call signal.SaveTo("./signals.csv")

        Dim signal2 = New Source.SinusSignal().GetGraphData(10, 10)

        Call signal2.SaveTo("./signals2.csv")

        Pause()
    End Sub
End Module
