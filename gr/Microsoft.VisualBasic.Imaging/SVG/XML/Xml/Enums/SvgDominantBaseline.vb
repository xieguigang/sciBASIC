#Region "Microsoft.VisualBasic::fa5b6987d268ec40c78c195a22221ad8, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\Enums\SvgDominantBaseline.vb"

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
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 1.60 KB


    '     Class SvgDominantBaseline
    ' 
    '         Properties: Alphabetic, Auto, Central, Hanging, Ideographic
    '                     Mathematical, Middle, NoChange, ResetSize, TextAfterEdge
    '                     TextBeforeEdge, UseScript
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVG.XML.Enums
    Public Class SvgDominantBaseline
        Inherits SvgEnum
        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property Auto As SvgDominantBaseline = New SvgDominantBaseline("auto")

        Public Shared ReadOnly Property UseScript As SvgDominantBaseline = New SvgDominantBaseline("use-script")

        Public Shared ReadOnly Property NoChange As SvgDominantBaseline = New SvgDominantBaseline("no-change")

        Public Shared ReadOnly Property ResetSize As SvgDominantBaseline = New SvgDominantBaseline("reset-size")

        Public Shared ReadOnly Property Ideographic As SvgDominantBaseline = New SvgDominantBaseline("ideagraphic")

        Public Shared ReadOnly Property Alphabetic As SvgDominantBaseline = New SvgDominantBaseline("alphabetic")

        Public Shared ReadOnly Property Hanging As SvgDominantBaseline = New SvgDominantBaseline("hanging")

        Public Shared ReadOnly Property Mathematical As SvgDominantBaseline = New SvgDominantBaseline("mathematical")

        Public Shared ReadOnly Property Central As SvgDominantBaseline = New SvgDominantBaseline("central")

        Public Shared ReadOnly Property Middle As SvgDominantBaseline = New SvgDominantBaseline("middle")

        Public Shared ReadOnly Property TextAfterEdge As SvgDominantBaseline = New SvgDominantBaseline("text-after-edge")

        Public Shared ReadOnly Property TextBeforeEdge As SvgDominantBaseline = New SvgDominantBaseline("text-before-edge")
    End Class
End Namespace
