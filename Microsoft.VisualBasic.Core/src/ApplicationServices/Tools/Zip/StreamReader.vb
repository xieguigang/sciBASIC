#Region "Microsoft.VisualBasic::b7df676ea198508f698deedff360a1a1, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Zip\StreamReader.vb"

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

    '   Total Lines: 76
    '    Code Lines: 50
    ' Comment Lines: 16
    '   Blank Lines: 10
    '     File Size: 3.05 KB


    '     Module ZipStreamReader
    ' 
    '         Function: Decompress, GetZipSubStream, LoadZipArchive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Zip

    ''' <summary>
    ''' zip file stream helper
    ''' </summary>
    Public Module ZipStreamReader

        ''' <summary>
        ''' 将zip文件中的所有数据包一次性的返回给上层调用代码
        ''' </summary>
        ''' <param name="zipFile"></param>
        ''' <param name="takes"></param>
        ''' <returns></returns>
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
                    Yield New NamedValue(Of MemoryStream) With {
                        .Name = entry.Name,
                        .Value = Decompress(file:=entry)
                    }
                Next
            End Using
        End Function

        <Extension>
        Public Function Decompress(file As ZipArchiveEntry) As MemoryStream
            Using ref As Stream = file.Open
                Dim ms As New MemoryStream()

                Call ref.CopyTo(ms)
                Call ms.Seek(0, SeekOrigin.Begin)

                Return ms
            End Using
        End Function

        ''' <summary>
        ''' 如果这个函数返回的是空值，说明zip压缩文件中没有该目标
        ''' </summary>
        ''' <param name="zipFile$"></param>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetZipSubStream(zipFile$, name$) As MemoryStream
            Return LoadZipArchive(zipFile, {name}).FirstOrDefault
        End Function
    End Module
End Namespace
