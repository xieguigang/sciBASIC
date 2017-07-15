Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Module VectorTest

    Structure fdddd
        Public Shared Operator &(a As fdddd, b$) As String

        End Operator
    End Structure

    Sub Main()

        Dim strings = {"", "sdafa", "sssssss"}.VectorShadows

        Dim newStrings$() = strings & strings

        Call newStrings.GetJson.__DEBUG_ECHO


        '  Pause()



        Dim vector = {
            New int(1),
            New int(2),
            New int(3),
            New int(4)
        }.VectorShadows

        Dim textArray As Integer() = vector.value

        Call textArray.GetJson.__DEBUG_ECHO

        Dim newText%() = {4, 5, 6, 7}

        vector.value = newText

        Dim gt = vector > 3

        Pause()
    End Sub
End Module
