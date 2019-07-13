#Region "Microsoft.VisualBasic::ab00a99e8d49a425b35339f455296714, Data\DataFrame\Extensions\ProfileStreams.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module ProfileStreams
    ' 
    '     Function: GenerateProfiles, LoadProfiles, WriteProfiles
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Provides the ``*.ini`` file like config data function
''' </summary>
Public Module ProfileStreams

    ''' <summary>
    ''' Save target configuration model as the csv table
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteProfiles(Of T As {New, IProfile})(x As T, path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim buf As ProfileTable() = x.GenerateProfiles
        Return buf.SaveTo(path, encoding:=encoding.CodePage)
    End Function

    <Extension>
    Public Function GenerateProfiles(Of T As {New, IProfile})(x As T) As ProfileTable()
        Dim settings As New Settings(Of T)(x)
        Dim buf As ProfileTable() =
            LinqAPI.Exec(Of ProfileTable) <= From config As BindMapping
                                             In settings.AllItems
                                             Select New ProfileTable(config)
        Return buf
    End Function

    ''' <summary>
    ''' Load config data from the csv table into the configuration model
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadProfiles(Of T As {New, IProfile})(path As String, Optional encoding As Encodings = Encodings.UTF8) As T
        Dim buf As ProfileTable() = path.LoadCsv(Of ProfileTable)(encoding:=encoding.CodePage).ToArray
        Dim config As Settings(Of T) = Settings(Of T).CreateEmpty

        For Each x As ProfileTable In buf
            Call x.SetValue(config)
        Next

        Return config.SettingsData
    End Function
End Module
