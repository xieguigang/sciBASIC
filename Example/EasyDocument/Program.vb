Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping.StructFormatter

Module Program

    Sub Main()

        Dim a As TestBin = TestBin.inst  ' Init test data

        Call a.Serialize("./test.dat")   ' test on the binary serialization  
        a = Nothing
        a = "./test.dat".Load(Of TestBin)

        Dim json As String = a.GetJson   ' JSON serialization test
        a = Nothing
        a = json.LoadObject(Of TestBin)
        Call json.__DEBUG_ECHO

        Call New Profiles With {.Test = a}.WriteProfile  ' Write profile file data
        Call a.WriteClass("./test2.ini")                 ' Write ini section data.
        a = Nothing
        a = "./test2.ini".LoadIni(Of TestBin)                        ' Load ini section data
        Dim pp As Profiles = "./test2.ini".LoadProfile(Of Profiles)  ' Load entire ini file
        Call a.GetJson.__DEBUG_ECHO
        Call pp.GetJson.__DEBUG_ECHO

        ' XML test
        Dim xml As String = a.GetXml   ' Convert object into Xml
        Call xml.__DEBUG_ECHO
        Call a.SaveAsXml("./testssss.Xml")   ' Save Object to Xml
        a = Nothing
        a = "./testssss.Xml".LoadXml(Of TestBin)  ' Load Object from Xml
        Call a.GetXml.__DEBUG_ECHO

        Dim array As TestBin() = {a, a, a, a, a, a, a, a, a, a}   ' We have a collection of object
        Call array.SaveTo("./test.Csv")    ' then wen can save this collection into Csv file 
        array = Nothing
        array = "./test.Csv".LoadCsv(Of TestBin)  ' test on load csv data
        Call array.GetJson.__DEBUG_ECHO

        Dim s As String = array.GetJson
        Call s.SaveTo("./tesssss.txt")

        Dim lines As String() = "./tesssss.txt".ReadAllLines()
        s = "./tesssss.txt".ReadAllText

        Pause()
    End Sub
End Module

<IniMapIO("#/test.ini")>
Public Class Profiles
    Public Property Test As TestBin
End Class

<ClassName("JSON")>
<Serializable> Public Class TestBin
    <DataFrameColumn> Public Property Property1 As String
    <DataFrameColumn> Public Property D As Date
    <DataFrameColumn> Public Property n As Integer
    <DataFrameColumn> Public Property f As Double

    Public Shared Function inst() As TestBin
        Return New TestBin With {
            .D = Now,
            .f = RandomDouble(),
            .n = RandomDouble() * 1000,
            .Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
        }
    End Function
End Class