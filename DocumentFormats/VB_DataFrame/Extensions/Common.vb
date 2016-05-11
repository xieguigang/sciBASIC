Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("System.Extensions")>
Public Module Common

    <ExportAPI("File.SaveOpen", Info:="Prompts the user to select a location for saving a file.")>
    Public Function SaveFileDialog(Optional Filter As String = "All Files(*.*)|*.*", <Parameter("Dir.Init")> Optional InitDir As String = "") As String
        Dim Dialog As New SaveFileDialog

        Dialog.Filter = Filter
        Dialog.InitialDirectory = InitDir

        If Dialog.ShowDialog = DialogResult.OK Then
            Return Dialog.FileName
        Else
            Return String.Empty
        End If
    End Function
End Module
