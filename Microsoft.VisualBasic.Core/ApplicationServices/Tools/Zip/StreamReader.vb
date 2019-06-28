Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Zip

    <HideModuleName> Public Module ZipStreamReader

        Public Iterator Function LoadZipArchive(zipFile As String, Optional takes As IEnumerable(Of String) = Nothing) As IEnumerable(Of NamedValue(Of MemoryStream))
            Dim takeIndex As Index(Of String) = takes.SafeQuery.ToArray
            Dim entries As IEnumerable(Of ZipArchiveEntry)

            Using zip As New ZipArchive(zipFile.Open(doClear:=False), ZipArchiveMode.Read)
                If takes Is Nothing Then
                    entries = zip.Entries.OrderBy(Function(e) e.Name)
                Else
                    entries = Iterator Function() As IEnumerable(Of ZipArchiveEntry)
                                  For Each item In zip.Entries
                                      If item.Name Like takeIndex Then
                                          Call takeIndex.Delete(item.Name)
                                          Yield item
                                      End If

                                      If takeIndex.Count = 0 Then
                                          Exit For
                                      End If
                                  Next
                              End Function()
                End If

                For Each entry As ZipArchiveEntry In entries
                    Using ref As Stream = entry.Open
                        Dim ms As New MemoryStream()

                        Call ref.CopyTo(ms)
                        Call ms.Seek(0, SeekOrigin.Begin)

                        Yield New NamedValue(Of MemoryStream) With {
                            .Name = entry.Name,
                            .Value = ms
                        }
                    End Using
                Next
            End Using
        End Function
    End Module
End Namespace