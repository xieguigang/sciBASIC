Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq

Public Module Debugger

    <Extension>
    Public Function ListFiles(hds As StreamPack) As IEnumerable(Of StreamObject)
        Return New StreamObject() {hds.superBlock}.JoinIterates(hds.superBlock.ListFiles)
    End Function

    <Extension>
    Public Iterator Function ListFiles(dir As StreamGroup) As IEnumerable(Of StreamObject)
        For Each file As StreamObject In dir.files
            If TypeOf file Is StreamBlock Then
                Yield file
            Else
                Yield file

                For Each child As StreamObject In DirectCast(file, StreamGroup).ListFiles
                    Yield child
                Next
            End If
        Next
    End Function
End Module
