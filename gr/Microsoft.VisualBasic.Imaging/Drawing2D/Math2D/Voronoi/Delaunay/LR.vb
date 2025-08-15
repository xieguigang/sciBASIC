#Region "Microsoft.VisualBasic::d894b493d960035667bdddf34789f692, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\LR.vb"

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

    '   Total Lines: 22
    '    Code Lines: 16 (72.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (27.27%)
    '     File Size: 587 B


    '     Class LR
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Other, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class LR

        Public Shared ReadOnly LEFT As LR = New LR("left")
        Public Shared ReadOnly RIGHT As LR = New LR("right")

        Private name As String

        Public Sub New(name As String)
            Me.name = name
        End Sub

        Public Shared Function Other(leftRight As LR) As LR
            Return If(leftRight Is LEFT, RIGHT, LEFT)
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class
End Namespace

