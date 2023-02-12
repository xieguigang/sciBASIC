#Region "Microsoft.VisualBasic::050e16ba84ef1932bc43eda21a2e551e, sciBASIC#\gr\Landscape\PLY\Header.vb"

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

'   Total Lines: 94
'    Code Lines: 76
' Comment Lines: 0
'   Blank Lines: 18
'     File Size: 3.52 KB


'     Class Header
' 
'         Properties: comment, element_face, element_vertex, properties
' 
'         Sub: WriteAsciiText
' 
'     Module SimplePlyWriter
' 
'         Function: WriteAsciiText
' 
'     Class PointCloud
' 
'         Properties: color, intensity, x, y, z
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Ply

    Public Class Header

        Public Property comment As String
        Public Property element_vertex As Integer
        Public Property element_face As Integer
        Public Property properties As NamedValue(Of String)()

        Friend Sub WriteAsciiText(file As StreamWriter)
            Call file.WriteLine($"ply")
            Call file.WriteLine($"format ascii 1.0")
            Call file.WriteLine($"comment {comment}")
            Call file.WriteLine($"element vertex {element_vertex}")
            Call file.WriteLine($"element face {element_face}")

            For Each field As NamedValue(Of String) In properties
                Call file.WriteLine($"property {field.Value} {field.Name}")
            Next

            Call file.WriteLine($"end_header")
        End Sub
    End Class
End Namespace
