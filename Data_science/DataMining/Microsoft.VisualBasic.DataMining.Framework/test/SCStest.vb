Imports System.IO
Imports Microsoft.VisualBasic.DataMining

Module SCStest

    Sub Main()
        Dim path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input.txt"
        Dim SCS = ShortestCommonSuperString(path.ReadAllLines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output.txt".OpenWriter
            Call path.ReadAllLines.TableView(SCS, txt)
        End Using

        Pause()
    End Sub
End Module
