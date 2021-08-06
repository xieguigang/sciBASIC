Imports Microsoft.VisualBasic.Math.DataFrame.MatrixMarket
Imports Microsoft.VisualBasic.My.FrameworkInternal

Module matrixReaderTest

    Sub Main()
        DoConfiguration.ConfigMemory(MemoryLoads.Heavy)

        Dim mtx = MTXFormat.ReadMatrix("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\DataFrame\MatrixMarket\west0655.mtx".Open)

        Pause()
    End Sub
End Module
