Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ApplicationServices.Zip

    <HideModuleName> Public Module ZipStreamReader

        Public Iterator Function LoadZipArchive(zipFile As String) As IEnumerable(Of NamedValue(Of MemoryStream))
            Using zip As New ZipArchive(zipFile.Open(doClear:=False), ZipArchiveMode.Read)
                For Each entry As ZipArchiveEntry In zip.Entries.OrderBy(Function(e) e.Name)
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