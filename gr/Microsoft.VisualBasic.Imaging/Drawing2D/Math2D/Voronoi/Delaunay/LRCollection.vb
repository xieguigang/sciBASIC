#Region "Microsoft.VisualBasic::41e72e1c26ddaf359700e89d920f3bc4, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\LRCollection.vb"

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

    '   Total Lines: 23
    '    Code Lines: 22 (95.65%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 1 (4.35%)
    '     File Size: 649 B


    '     Class LRCollection
    ' 
    '         Sub: Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Class LRCollection(Of T)
        Private left As T
        Private right As T
        Default Public Property Item(index As LR) As T
            Get
                Return If(index Is LR.LEFT, left, right)
            End Get
            Set(value As T)
                If index Is LR.LEFT Then
                    left = value
                Else
                    right = value
                End If
            End Set
        End Property

        Public Sub Clear()
            left = Nothing
            right = Nothing
        End Sub
    End Class
End Namespace
