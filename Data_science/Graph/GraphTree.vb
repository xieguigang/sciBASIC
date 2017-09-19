#Region "Microsoft.VisualBasic::9cb1ed5084ea350873e4a50632193081, ..\sciBASIC#\gr\Datavisualization.Network\Graph\GraphTree.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree

Public Class GraphTree : Inherits TreeNodeBase(Of GraphTree)

    Public Overrides ReadOnly Property MySelf As GraphTree
        Get
            Return Me
        End Get
    End Property

    Public ReadOnly Property Vertex As Vertex

    Sub New(v As Vertex)
        Call MyBase.New(v.ID)
        Vertex = v
    End Sub
End Class

