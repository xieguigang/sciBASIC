#Region "Microsoft.VisualBasic::aeb106bf96344a4acb3e0a6e9cc02065, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\File.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO.Compression
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``*.xlsx`` document file
''' </summary>
Public Class File

    Public Function WriteXlsx(path$) As Boolean

    End Function

    Public Shared Function CreatePackage(tmp$, xlsx$) As Boolean
        Try
            Call GZip.DirectoryArchive(tmp, xlsx, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)
        Catch ex As Exception
            Dim debug$ = New Dictionary(Of String, String) From {
                {NameOf(tmp), tmp},
                {NameOf(xlsx), xlsx}
            }.GetJson
            ex = New Exception(debug, ex)
            Return False
        End Try

        Return True
    End Function
End Class
