Module SCStest

    Sub Main()
        Dim path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input.txt"
        Dim SCS = Microsoft.VisualBasic.DataMining.SCS.shortest_common_superstring(path.ReadAllLines)

        Call SCS.SaveTo("G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output.txt")
    End Sub
End Module
