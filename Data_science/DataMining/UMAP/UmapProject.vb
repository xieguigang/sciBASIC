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
'    Code Lines: 12 (75.00%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 4 (25.00%)
'     File Size: 386 B


' Class UmapProject
' 
'     Properties: dimension, labels, projection
' 
'     Function: CreateProjection
' 
' /********************************************************************************/

#End Region

''' <summary>
''' binary file data model for save the UMAP embedding result
''' </summary>
Public Class UMAPProject

    ''' <summary>
    ''' KNN graph 
    ''' </summary>
    ''' <returns></returns>
    Public Property graph As Double()()
    ''' <summary>
    ''' the UMAP embedding result
    ''' </summary>
    ''' <returns></returns>
    Public Property embedding As Double()()
    Public Property labels As String()
    ''' <summary>
    ''' width of the vector inside the <see cref="embedding"/> result.
    ''' </summary>
    ''' <returns></returns>
    Public Property dimension As Integer
    Public Property clusters As String()

    Public Function SetLabels(labels As IEnumerable(Of String), Optional clusters As IEnumerable(Of String) = Nothing) As UMAPProject
        _labels = labels.ToArray

        If clusters IsNot Nothing Then
            _clusters = clusters.ToArray
        End If

        Return Me
    End Function

    ''' <summary>
    ''' extract matrix data from umap result
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <returns></returns>
    Public Shared Function CreateProjection(umap As Umap) As UMAPProject
        Return New UMAPProject With {
            .dimension = umap.dimension,
            .embedding = umap.GetEmbedding.ToArray,
            .graph = umap.GetGraph.ToArray
        }
    End Function



End Class
