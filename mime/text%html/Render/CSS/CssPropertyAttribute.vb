#Region "Microsoft.VisualBasic::47ce05ab7d1f0d93d6141c042f8d2245, mime\text%html\Render\CSS\CssPropertyAttribute.vb"

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

    '   Total Lines: 26
    '    Code Lines: 11 (42.31%)
    ' Comment Lines: 11 (42.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (15.38%)
    '     File Size: 782 B


    '     Class CssPropertyAttribute
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Render.CSS

    ''' <summary>
    ''' Used to mark a property as a Css property.
    ''' The <see cref="Name"/> property is used to specify the oficial CSS name
    ''' </summary>
    Public Class CssPropertyAttribute : Inherits Attribute

        ''' <summary>
        ''' Gets or sets the name of the CSS property
        ''' </summary>
        Public Property Name() As String

        ''' <summary>
        ''' Creates a new CssPropertyAttribute
        ''' </summary>
        ''' <param name="name">Name of the Css property</param>
        Public Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Namespace
