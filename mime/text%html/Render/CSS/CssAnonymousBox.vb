#Region "Microsoft.VisualBasic::be68a17ec5962cf24108e6470a4d85cb, mime\text%html\Render\CSS\CssAnonymousBox.vb"

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

    '   Total Lines: 29
    '    Code Lines: 16 (55.17%)
    ' Comment Lines: 10 (34.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (10.34%)
    '     File Size: 800 B


    '     Class CssAnonymousBox
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class CssAnonymousSpaceBox
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Render.CSS
#If NET48 Then
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
#End If
End Namespace
