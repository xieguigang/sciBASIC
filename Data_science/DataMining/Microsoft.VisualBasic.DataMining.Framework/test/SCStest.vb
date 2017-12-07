Imports System.IO
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.Text

Module SCStest

    Sub Main()


        SAMTest()

        Dim path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input.txt"
        Dim SCS = ShortestCommonSuperString(path.ReadAllLines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output.txt".OpenWriter
            Call path.ReadAllLines.TableView(SCS, txt)
        End Using

        path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input2.txt"
        SCS = ShortestCommonSuperString(path.ReadAllLines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output2.txt".OpenWriter
            Call path.ReadAllLines.TableView(SCS, txt)
        End Using

        Pause()
    End Sub

    Sub SAMTest()
        Dim lines = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\EF4.sam" _
            .ReadAllLines _
            .Select(Function(s) s.Split(ASCII.TAB)(9)) _
            .ToArray

        Dim SCS = ShortestCommonSuperString(lines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\SAM_output.txt".OpenWriter
            Call lines.TableView(SCS, txt)
        End Using

        'Dim assembly = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\SAM_output.txt".ReadAllLines
        'Dim coverageValue = Coverage(assembly)


        Pause()
    End Sub
End Module
