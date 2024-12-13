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
