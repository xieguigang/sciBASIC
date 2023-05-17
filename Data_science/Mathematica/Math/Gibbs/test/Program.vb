Imports Microsoft.VisualBasic.Math.GibbsSampling
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main(args As String())
        Call gibbsTest()

        Console.WriteLine("Hello World!")
    End Sub

    Public Sub gibbsTest()
        Dim data = New String() {"ABCDAAAABDB", "AAAADCBBCA", "DDBCABAAAACBBD", "AABAAAACCDD", "ABCBDBDDDDDBCBBCBC", "ABAAAACBBDAABAAAACC", "CDAAAABDBAA", "AADCBBCADDB"}
        Dim length = 4
        Dim gibbs As Gibbs = New Gibbs(data, length)

        Call Console.WriteLine(gibbs.sample.GetJson)
    End Sub
End Module
