#Region "Microsoft.VisualBasic::579fa48d582987d3f031c394ca6f24fb, Data_science\DataMining\UMAP\KNN\KDTree\KDPoint.vb"

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

    '   Total Lines: 15
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 321 B


    '     Class KDPoint
    ' 
    '         Properties: id, size, vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KNN.KDTreeMethod

    Public Class KDPoint

        Public Property vector As Double()
        Public Property id As Integer

        Public ReadOnly Property size As Integer
            Get
                Return vector.Length
            End Get
        End Property

    End Class
End Namespace
