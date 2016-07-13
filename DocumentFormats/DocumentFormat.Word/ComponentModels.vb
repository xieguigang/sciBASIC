#Region "Microsoft.VisualBasic::120415fe25d454d93cfba047220ef8ca, ..\VisualBasic_AppFramework\DocumentFormats\DocumentFormat.Word\ComponentModels.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace ComponentModels

    Public MustInherit Class Font

        Public Property FontSize As Integer
        Public Property FontBold As Boolean
        Public Property FontFamilyName As String
        Public Property FontItalic As Boolean
        Public Property FontColor As System.Drawing.Color
        Public Property FontUnderline As Boolean

    End Class
End Namespace

