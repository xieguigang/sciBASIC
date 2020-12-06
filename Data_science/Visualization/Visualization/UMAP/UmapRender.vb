#Region "Microsoft.VisualBasic::7898971dbc71f1b96e14e29cd4d61b6d, Data_science\Visualization\Visualization\UMAP\UmapRender.vb"

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

    ' Module UmapRender
    ' 
    '     Function: DrawUmap2D, GetPoint2D
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module UmapRender

    <Extension>
    Public Function GetPoint2D(umap As Umap) As PointF()
        If umap.dimension <> 2 Then
            Throw New InvalidProgramException($"the given umap projection result(dimension={umap.dimension}) is not a 2D data!")
        Else
            Return umap.GetEmbedding() _
                .Select(Function(vec)
                            Return New PointF With {.X = vec(0), .Y = vec(1)}
                        End Function) _
                .ToArray()
        End If
    End Function

    <Extension>
    Public Function DrawUmap2D(umap As Umap, Optional labels As IEnumerable(Of String) = Nothing) As GraphicsData
        Dim labelList As String() = Nothing

        If Not labels Is Nothing Then
            labelList = labels.ToArray
        End If

        Dim embeddings As PointF() = umap.GetPoint2D


    End Function
End Module

