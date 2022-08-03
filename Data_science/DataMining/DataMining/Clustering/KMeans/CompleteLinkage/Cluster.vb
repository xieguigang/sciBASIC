#Region "Microsoft.VisualBasic::a8eea3aa3fa7f789a22257b2a1726788, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\CompleteLinkage\Cluster.vb"

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
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 646 B


    '     Class Cluster
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace KMeans.CompleteLinkage

    Public Class Cluster(Of T As EntityBase(Of Double))

        Protected Friend ReadOnly _innerList As New List(Of T)

        Public Sub New(points As List(Of T))
            _innerList = points
        End Sub

        Public Sub New()
            _innerList = New List(Of T)
        End Sub

        Public Sub New(p As T)
            _innerList = New List(Of T)
            Call Add(p)
        End Sub

        Public Overridable Sub Add(p As T)
            Call _innerList.Add(p)
        End Sub
    End Class
End Namespace
