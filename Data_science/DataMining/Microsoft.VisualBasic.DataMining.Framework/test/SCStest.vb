Imports System.IO
Imports Microsoft.VisualBasic.DataMining

Module SCStest

    Sub Main()
        Dim path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input.txt"
        Dim SCS = ShortestCommonSuperString(path.ReadAllLines.AsList)

        Call SCS.SaveTo("G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output.txt")

        Call path.ReadAllLines.TableView(SCS, New StreamWriter(Console.OpenStandardOutput))

        Pause()
    End Sub
End Module
