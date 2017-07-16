Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Module VectorTest

    Structure fdddd
        Public Shared Operator &(a As fdddd, b$) As String

        End Operator

        Public Shared Operator *(a As fdddd, b$) As String

        End Operator
        Public Shared Operator ^(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Mod(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Like(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Xor(a As fdddd, b$) As String

        End Operator
        Public Shared Operator And(a As fdddd, b$) As String

        End Operator
        Public Shared Operator Or(a As fdddd, b$) As String

        End Operator
        Public Shared Operator \(a As fdddd, b$) As String

        End Operator

        Public Shared Operator -(a As fdddd) As Boolean

        End Operator

    End Structure

    Sub Main()

        'Dim strings = {"", "sdafa", "sssssss"}.VectorShadows

        'Dim newStrings$() = strings & strings

        'Call newStrings.GetJson.__DEBUG_ECHO


        '  Pause()



        Dim vector = {
           New fdddd
        }.VectorShadows


        Dim dddd = vector Like 1234
        Dim fffff = vector \ 33333

        Pause()

        Dim textArray As Integer() = vector.value 'As Integer()

        Call textArray.GetJson.__DEBUG_ECHO

        Dim newText%() = {4, 5, 6, 7}

        vector.value = newText

        Dim gt = vector > 3

        Pause()
    End Sub
End Module
