#Region "Microsoft.VisualBasic::a604b5e7f5d8a8d530cbb50170cfd8d6, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\ColorBrewer.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    Public Class ColorBrewer

        Public Property c3 As String()
        Public Property c4 As String()
        Public Property c5 As String()
        Public Property c6 As String()
        Public Property c7 As String()
        Public Property c8 As String()
        Public Property c9 As String()
        Public Property c10 As String()
        Public Property c11 As String()
        Public Property c12 As String()

        Public Property type As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function GetColors(name$) As Color()
            Select Case LCase(name)
                Case NameOf(c3)
                    Return c3.ToArray(AddressOf ToColor)
                Case NameOf(c4)
                    Return c4.ToArray(AddressOf ToColor)
                Case NameOf(c5)
                    Return c5.ToArray(AddressOf ToColor)
                Case NameOf(c6)
                    Return c6.ToArray(AddressOf ToColor)
                Case NameOf(c7)
                    Return c7.ToArray(AddressOf ToColor)
                Case NameOf(c8)
                    Return c8.ToArray(AddressOf ToColor)
                Case NameOf(c9)
                    Return c9.ToArray(AddressOf ToColor)
                Case NameOf(c10)
                    Return c10.ToArray(AddressOf ToColor)
                Case NameOf(c11)
                    Return c11.ToArray(AddressOf ToColor)
                Case NameOf(c12)
                    Return c12.ToArray(AddressOf ToColor)
                Case Else
                    Return c3.ToArray(AddressOf ToColor)
            End Select
        End Function

        ''' <summary>
        ''' example: ``Accent:c6``
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <returns></returns>
        Public Shared Function ParseName(term$) As NamedValue(Of String)
            Return term.GetTagValue(":")
        End Function
    End Class
End Namespace
