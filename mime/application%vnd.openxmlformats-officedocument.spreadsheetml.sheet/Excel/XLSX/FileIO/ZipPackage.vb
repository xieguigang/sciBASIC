#Region "Microsoft.VisualBasic::63f037224ccc13c9d7ea132eb8227b94, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\FileIO\ZipPackage.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 75
    '    Code Lines: 48 (64.00%)
    ' Comment Lines: 16 (21.33%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 11 (14.67%)
    '     File Size: 2.52 KB


    '     Class ZipPackage
    ' 
    '         Properties: Err, Retry, ROOT, xlsx
    ' 
    '         Function: ExtractZip
    ' 
    '         Sub: ExtractZipInternal, WriteZip
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.Net.Http
Imports Directory = Microsoft.VisualBasic.FileIO.Directory

Namespace XLSX.FileIO

    ''' <summary>
    ''' a helper function for unzip of the xlsx data file. 
    ''' </summary>
    Friend Class ZipPackage

        ''' <summary>
        ''' the file uri of the target xlsx data file
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' could be 
        ''' 
        ''' 1. a local xlsx zip file path
        ''' 2. a network location
        ''' 3. a local xlsx unzip directory folder path
        ''' 4. base64 data uri of the zip data
        ''' </remarks>
        Public Property xlsx As String
        Public Property Err As Exception
        Public Property Retry As Integer = 3
        Public Property data As IFileSystemEnvironment

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Function ExtractZip() As Boolean
            If Not xlsx.FileExists Then
                Err = New Exception($"The specific local file is missing: {xlsx}!")
                Return False
            End If

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
                Call requestHttpFile()
            ElseIf DataURI.IsWellFormedUriString(xlsx) Then
                Call requestDataURI()
            ElseIf xlsx.DirectoryExists Then
                ' is a local unzip directory output
                ' for debug test used only
                data = Directory.FromLocalFileSystem(xlsx)
            ElseIf xlsx.FileExists Then
                data = New ZipStream(xlsx, is_readonly:=False)
            Else
                ' file is not existsed
            End If
        End Sub

        Private Sub requestDataURI()
            Dim s As Stream = DataURI.URIParser(xlsx).ToStream
            s.Flush()
            s.Seek(Scan0, SeekOrigin.Begin)
            data = New ZipStream(s, is_readonly:=True)
        End Sub

        Private Sub requestHttpFile()
            Using buffer As New MemoryStream, file As Stream = xlsx.GetRequestRaw
                Call file.CopyTo(buffer)
                Call buffer.Flush()
                Call buffer.Seek(Scan0, SeekOrigin.Begin)

                data = New ZipStream(buffer, is_readonly:=True)
            End Using
        End Sub
    End Class
End Namespace
