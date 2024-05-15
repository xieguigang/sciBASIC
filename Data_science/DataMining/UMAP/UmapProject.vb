#Region "Microsoft.VisualBasic::b114a6b524e694af621e2c549ec288d4, Data_science\DataMining\UMAP\UmapProject.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 386 B


    ' Class UmapProject
    ' 
    '     Properties: dimension, labels, projection
    ' 
    '     Function: CreateProjection
    ' 
    ' /********************************************************************************/

#End Region

Public Class UmapProject

    Public Property projection As Umap
    Public Property labels As String()

    Public ReadOnly Property dimension As Integer
        Get
            Return projection.dimension
        End Get
    End Property

    Public Shared Function CreateProjection() As UmapProject
        Throw New NotImplementedException
    End Function

End Class
