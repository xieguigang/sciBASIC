Imports System.IO
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.Net.Http

Namespace XLSX.FileIO

    ''' <summary>
    ''' a helper function for unzip of the xlsx data file. 
    ''' </summary>
    Friend Class ZipPackage

        ''' <summary>
        ''' the file uri of the target xlsx data file
        ''' </summary>
        ''' <returns></returns>
        Public Property xlsx As String
        ''' <summary>
        ''' a temp directory for unzip file
        ''' </summary>
        ''' <returns></returns>
        Public Property ROOT As String
        Public Property Err As Exception
        Public Property Retry As Integer = 3

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Function ExtractZip() As Boolean
            ' 20190606 会随机性的出现本地文件头已损坏的错误？？
            For i As Integer = 1 To Retry
                Try
                    Call ExtractZipInternal()
                    Err = Nothing
                    Return True
                Catch ex As Exception
                    Err = ex
                End Try

                Call Thread.Sleep(10)
            Next

            Return False
        End Function

        Private Sub ExtractZipInternal()
            If xlsx.IsURLPattern Then
                Using buffer As New MemoryStream, file As Stream = xlsx.GetRequestRaw
                    Dim rootDir As String = Nothing

                    Call file.CopyTo(buffer)
                    Call buffer.Flush()
                    Call buffer.Seek(Scan0, SeekOrigin.Begin)

                    Call buffer.IsSourceFolderZip(folder:=rootDir, reset:=True)
                    Call buffer.ImprovedExtractToDirectory(
                        destinationDirectoryName:=ROOT,
                        overwriteMethod:=Overwrite.Always,
                        extractToFlat:=False,
                        rootDir:=rootDir
                    )
                End Using
            ElseIf DataURI.IsWellFormedUriString(xlsx) Then
                UnZip.ImprovedExtractToDirectory(DataURI.URIParser(xlsx), destinationDirectoryName:=ROOT, Overwrite.Always)
            Else
                UnZip.ImprovedExtractToDirectory(xlsx, ROOT, Overwrite.Always)
            End If
        End Sub

        Public Shared Sub WriteZip()

        End Sub
    End Class
End Namespace