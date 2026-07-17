Imports System
Imports Microsoft.VisualBasic.Data.IO.HDF5

Module Program
    Sub Main(args As String())
        Dim file = HDF5File.Open("G:\pixelArtist\src\framework\Data\BinaryData\data\EP388069_K40_BS1D.otu_table.biom")

        Pause()
    End Sub
End Module
