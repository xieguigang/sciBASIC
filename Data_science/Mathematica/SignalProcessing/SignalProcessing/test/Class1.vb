#Region "Microsoft.VisualBasic::9d169e36ad388ba26b791510368051fe, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\Class1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Wave
Imports Microsoft.VisualBasic.Data.IO


Module Module1

    Sub Main()

        Dim wav = "E:\VB_GamePads\runtime\sciBASIC#\Data_science\Mathematica\SignalProcessing\wav\M1F1-int16-AFsp.wav".OpenBinaryReader
        Dim header As File = File.Open(wav)

        '    Dim chunks = header.data.data.Split(1024)

        Pause()
    End Sub

End Module
