Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Language

Public Module ProfileStreams

    <Extension>
    Public Function WriteProfiles(Of T As IProfile)(x As T, path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim buf As ProfileTable() = x.GenerateProfiles
        Return buf.SaveTo(path, encoding.GetEncodings)
    End Function

    <Extension>
    Public Function GenerateProfiles(Of T As IProfile)(x As T) As ProfileTable()
        Dim settings As New Settings(Of T)(x)
        Dim buf As ProfileTable() =
            LinqAPI.Exec(Of ProfileTable) <= From config As BindMapping
                                             In settings.AllItems
                                             Select New ProfileTable(config)
        Return buf
    End Function

    <Extension>
    Public Function LoadProfiles(Of T As IProfile)(path As String, Optional encoding As Encodings = Encodings.UTF8) As T
        Dim buf As ProfileTable() = path.LoadCsv(Of ProfileTable)(encoding:=encoding.GetEncodings).ToArray
        Dim config As Settings(Of T) = Settings(Of T).CreateEmpty

        For Each x As ProfileTable In buf
            Call x.SetValue(config)
        Next

        Return config.SettingsData
    End Function
End Module