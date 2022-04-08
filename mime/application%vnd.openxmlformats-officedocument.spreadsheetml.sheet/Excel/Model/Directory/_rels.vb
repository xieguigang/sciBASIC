#Region "Microsoft.VisualBasic::48430c7195ffb4cb159cc525f73e1c86, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Model\Directory\_rels.vb"

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

'   Total Lines: 38
'    Code Lines: 23
' Comment Lines: 8
'   Blank Lines: 7
'     File Size: 1.08 KB


'     Class _rels
' 
'         Properties: rels, workbook
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: _name
' 
'         Sub: _loadContents
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML._rels
Imports Microsoft.VisualBasic.Text.Xml

Namespace Model.Directory

    Public Class _rels : Inherits Directory

        ''' <summary>
        ''' ``.rels``
        ''' </summary>
        ''' <returns></returns>
        Public Property rels As rels
        ''' <summary>
        ''' ``workbook.xml.rels``
        ''' </summary>
        ''' <returns></returns>
        Public Property workbook As rels

        Sub New(ROOT$)
            Call MyBase.New(ROOT)
        End Sub

        Protected Overrides Sub _loadContents()
            Dim path As Value(Of String) = ""

            If (path = Folder & "/.rels").FileExists Then
                rels = New rels With {.document = (+path).LoadXml(Of OpenXml.rels)}
            End If
            If (path = Folder & "/workbook.xml.rels").FileExists Then
                workbook = New rels With {.document = (+path).LoadXml(Of OpenXml.rels)}
            End If
        End Sub

        Protected Overrides Function _name() As String
            Return NameOf(_rels)
        End Function
    End Class
End Namespace
