#Region "Microsoft.VisualBasic::5e33dcee84cfcca520af8f94defc3c8b, ..\sciBASIC#\mime\text%html\HTML\CSS\Render\CssAnonymousBox.vb"

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

Namespace HTML.CSS.Render

    ''' <summary>
    ''' Represents an anonymous inline box
    ''' </summary>
    ''' <remarks>
    ''' To learn more about anonymous inline boxes visit:
    ''' http://www.w3.org/TR/CSS21/visuren.html#anonymous
    ''' </remarks>
    Public Class CssAnonymousBox : Inherits CssBox

#Region "Ctor"
        Public Sub New(parentBox As CssBox)
            MyBase.New(parentBox)
        End Sub
#End Region
    End Class

    ''' <summary>
    ''' Represents an anonymous inline box which contains nothing but blank spaces
    ''' </summary>
    Public Class CssAnonymousSpaceBox : Inherits CssAnonymousBox

        Public Sub New(parentBox As CssBox)
            MyBase.New(parentBox)
        End Sub
    End Class
End Namespace
