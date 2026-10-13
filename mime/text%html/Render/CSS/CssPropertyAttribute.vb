#Region "Microsoft.VisualBasic::dc03f34a575b40b9c8f7182d82292ab7, mime\text%html\Render\CSS\CssPropertyAttribute.vb"

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

    '   Total Lines: 28
    '    Code Lines: 12 (42.86%)
    ' Comment Lines: 11 (39.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (17.86%)
    '     File Size: 823 B


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

Imports Microsoft.VisualBasic.Imaging

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
