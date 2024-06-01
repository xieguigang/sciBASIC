#Region "Microsoft.VisualBasic::fb5af354a84253b94fa6152ee679a41f, mime\text%html\CSS\Elements\Size.vb"

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

    '   Total Lines: 32
    '    Code Lines: 21 (65.62%)
    ' Comment Lines: 3 (9.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (25.00%)
    '     File Size: 680 B


    '     Class CSSsize
    ' 
    '         Properties: height, width
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace CSS

    ''' <summary>
    ''' parse the width and height data from the css style string
    ''' </summary>
    Public Class CSSsize

        Public Property width As String
        Public Property height As String

        Sub New()
        End Sub

        Sub New(size As CSSsize)
            width = size.width
            height = size.height
        End Sub

        Sub New(size As Size)
            width = size.Width
            height = size.Height
        End Sub

        Sub New(size As SizeF)
            width = size.Width
            height = size.Height
        End Sub

    End Class
End Namespace
