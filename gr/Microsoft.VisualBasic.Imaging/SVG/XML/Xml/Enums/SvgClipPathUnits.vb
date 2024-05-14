#Region "Microsoft.VisualBasic::6a617773d14eae6f08f19b89a1d17caa, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Enums\SvgClipPathUnits.vb"

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

    '   Total Lines: 13
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 450 B


    '     Class SvgClipPathUnits
    ' 
    '         Properties: ObjectBoundingBox, UserSpaceOnUse
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.XML.Enums

    Public Class SvgClipPathUnits
        Inherits SvgEnum
        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property UserSpaceOnUse As SvgClipPathUnits = New SvgClipPathUnits("userSpaceOnUse")

        Public Shared ReadOnly Property ObjectBoundingBox As SvgClipPathUnits = New SvgClipPathUnits("objectBoundingBox")
    End Class
End Namespace
