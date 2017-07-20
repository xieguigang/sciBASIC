Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Module Test2

    Class Foo
        Public Property str As String

        Sub New(s$)
            str = s
        End Sub

        Public Overrides Function ToString() As String
            Return str
        End Function

        Public Overloads Shared Operator Like(str As Foo, s$) As Boolean
            With str
                Return Regex.Match(.str, s).Success
            End With
        End Operator
    End Class

    Sub Main()
        Call Linq()
        Call Vector()
    End Sub

    Sub Linq()
        Dim array = {New Foo("123"), New Foo("ABC"), New Foo("!@#")}

        Dim isNumeric = array.Select(Function(s) s Like "\d+").ToArray
        Dim Allstrings = array.Select(Function(s) s.str).ToArray
        Dim Selects = array _
            .Where(Function(s) s Like "\d+") _
            .Select(Function(s) s.str) _
            .ToArray

        ' and then if we want to update the property value?
        With {"ABC", "CBA", "ZZz"}

            For i As Integer = 0 To .Length - 1
                array(i).str = .ref(i)
            Next
        End With
    End Sub

    Sub Vector()
        ' dynamics shadowing a vector from any .NET collection type
        ' Example from the array object that we declared above
        Dim strings = {New Foo("123"), New Foo("ABC"), New Foo("!@#")}.VectorShadows

        Dim isNumeric = strings Like "\d+"
        Dim Allstrings = strings.str
        Dim Selects = strings(strings Like "\d+").str

        ' and then if we want to update the property value?
        strings.str = {"ABC", "CBA", "ZZz"}
        ' all of the property value was set to "123"
        strings.str = "123"

        Pause()
    End Sub
End Module
