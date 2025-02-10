#Region "Microsoft.VisualBasic::7a88cabcb12e3acc6a6aa9ef1a9121c4, Data_science\Visualization\Visualization\Embedding\SOMEmbedding.vb"

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

    '   Total Lines: 63
    '    Code Lines: 50 (79.37%)
    ' Comment Lines: 4 (6.35%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 9 (14.29%)
    '     File Size: 2.44 KB


    ' Class SOMEmbedding
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: plot2D, PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Plots
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' plot SOM embedding
''' </summary>
Public Class SOMEmbedding : Inherits Plot

    ReadOnly som As SelfOrganizingMap
    ReadOnly dims As Integer

    Public Sub New(som As SelfOrganizingMap, dims As Integer, theme As Theme)
        MyBase.New(theme)

        ' check of the embedding data
        If som.numberOfNeurons <> som.class_id.Count Then
            Throw New InvalidProgramException($"the dimension size of embedding neuron number should be equals to the number of input dataset!")
        ElseIf som.depth < dims Then
            Throw New InvalidProgramException($"dimension data size is greater than the som depth!")
        ElseIf dims <> 2 AndAlso dims <> 3 Then
            Throw New InvalidProgramException("Only supports 2d or 3d embedding scatter plot!")
        Else
            Me.som = som
            Me.dims = dims
        End If
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        If dims = 2 Then
            Call plot2D(g, canvas)
        Else
            Throw New NotImplementedException
        End If
    End Sub

    Private Sub plot2D(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim points As New List(Of PointData)
        Dim class_id As String() = som.class_id.AsCharacter.ToArray
        Dim colors = New CategoryColorProfile(class_id, Designer.GetColors(theme.colorSet, class_id.Distinct.Count))
        Dim embedding = som.embeddings

        For i As Integer = 0 To embedding.Length - 1
            Dim xy = embedding(i)

            Call points.Add(New PointData With {
                .color = colors.GetColor(class_id(i)).ToHtmlColor,
                .pt = New PointF(xy(0), xy(1))
            })
        Next

        Dim scatter As New Scatter2D(points, theme)
        Call scatter.Plot(g, canvas)
    End Sub
End Class
