
Imports Microsoft.VisualBasic.Data.Wave
Imports Microsoft.VisualBasic.Data.IO


Module Module1

    Sub Main()

        Dim wav = "E:\VB_GamePads\runtime\sciBASIC#\Data_science\Mathematica\SignalProcessing\wav\SND_FISH_TROPICAL_03.wav".OpenBinaryReader
        Dim header As File = File.ParseHeader(wav)



        Pause()
    End Sub

End Module