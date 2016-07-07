#Region "Microsoft.VisualBasic::7f818e6580e0ad727a61a5300660d6a4, ..\VisualBasic_AppFramework\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.ComponentModel
Imports System.Drawing.Design
Imports System.ComponentModel.Design
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports Microsoft.VisualBasic.Imaging

Namespace Windows.Forms.Nexus

    ''' <summary>
    ''' A control for displaying pie charts.
    ''' </summary>
    <ToolboxBitmap(GetType(PieChart))>
    Partial Public Class PieChart
        Inherits Control
#Region "Constructor"
        ''' <summary>
        ''' Constructs a new instance of a PieChart.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint, True)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.DoubleBuffer, True)
            Me.SetStyle(ControlStyles.ResizeRedraw, True)

            Me._Items = New ItemCollection(Me)
            Me._Style = New PieChartStyle(Me)
            Me._ItemStyle = New PieChartItemStyle(Me)
            Me._FocusedItemStyle = New PieChartItemStyle(Me)
            Me.toolTip = New ToolTip()
        End Sub
#End Region

#Region "Fields"
        ''' <summary>
        ''' The collection which holds PieChartItems
        ''' </summary>
        Private _Items As ItemCollection

        ''' <summary>
        ''' The collection of styles that apply to this PieChart.
        ''' </summary>
        Private _Style As PieChartStyle

        ''' <summary>
        ''' The style for default (non-focused) items.
        ''' </summary>
        Private _ItemStyle As PieChartItemStyle

        ''' <summary>
        ''' The style for focused items.
        ''' </summary>
        Private _FocusedItemStyle As PieChartItemStyle

        ''' <summary>
        ''' The PieChartItem that has mouse focus.
        ''' </summary>
        Private _FocusedItem As PieChartItem

        ''' <summary>
        ''' True if the structure of the pie has changed and the layout needs to be recalculated.
        ''' </summary>
        Private isStructureChanged As Boolean = True

        ''' <summary>
        ''' True if the pie needs to be redrawn.
        ''' </summary>
        Private isVisualChanged As Boolean = True

        ''' <summary>
        ''' True if the underlying pens and brushes need to be recreated when the control is redrawn.
        ''' </summary>
        Private recreateGraphics As Boolean = True

        ''' <summary>
        ''' A reference counter for the number of change transactions that have been begun and not ended.
        ''' </summary>
        Private transactionRef As Integer = 0

        ''' <summary>
        ''' A list of DrawingMetrics objects that store calculated drawing data about each pie slice.
        ''' </summary>
        Private drawingMetrics As New List(Of DrawingMetrics2)()

        ''' <summary>
        ''' The ToolTip control that is used when hovering over pie slices.
        ''' </summary>
        Private toolTip As ToolTip

        ''' <summary>
        ''' The default ToolTip delay, which is stored when the delay is overwritten by this control.
        ''' </summary>
        Private toolTipDefaultDelay As Integer
#End Region

#Region "Properties"
        ''' <summary>
        ''' The collection which holds PieChartItems
        ''' </summary>
        <Browsable(True)> <Category("Pie Chart")> <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
        Public ReadOnly Property Items() As ItemCollection
            Get
                Return _Items
            End Get
        End Property

        ''' <summary>
        ''' The collection of styles that apply to this PieChart.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property Style() As PieChartStyle
            Get
                Return _Style
            End Get
        End Property

        ''' <summary>
        ''' The collection of styles that apply to this PieChart.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property ItemStyle() As PieChartItemStyle
            Get
                Return _ItemStyle
            End Get
        End Property

        ''' <summary>
        ''' The collection of styles that apply to this PieChart.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property FocusedItemStyle() As PieChartItemStyle
            Get
                Return _FocusedItemStyle
            End Get
        End Property

        ''' <summary>
        ''' The PieChartItem that has mouse focus.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property FocusedItem() As PieChartItem
            Get
                Return _FocusedItem
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the rotation of the pie chart.  This is represented in radians, with positive values indicating
        ''' a rotation in the clockwise direction.
        ''' </summary>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(0.0F)> <Description("The rotation around the center of the control, in radians.")>
        Public Property Rotation() As Single
            Get
                Return Style.Rotation
            End Get
            Set(value As Single)
                Style.Rotation = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the inclination of the control.  This is represented in radians, where an angle of 0
        ''' represents looking at the edge of the control and an angle of pi represents looking
        ''' straight down at the top of the pie.
        ''' </summary>
        ''' <remarks>
        ''' The angle must be greater than 0 and less than or equal to pi radians.
        ''' </remarks>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(CSng(Math.PI / 6))> <Description("The inclination of the control, in radians.")>
        Public Property Inclination() As Single
            Get
                Return Style.Inclination
            End Get
            Set(value As Single)
                Style.Inclination = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets thickness of the pie, in pixels.
        ''' </summary>
        ''' <remarks>This represents the three-dimensional thickness of the control.
        ''' The actual visual thickness of the control depends on the inclination.  To determine what the apparent
        ''' thickness of the control is, use the Style.VisualHeight property.  The thickness must be greater than or equal to 0.</remarks>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(10)> <Description("The thickness of the pie, in pixels.")>
        Public Property Thickness() As Single
            Get
                Return Style.Thickness
            End Get
            Set(value As Single)
                Style.Thickness = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets radius of the control, in pixels.  If AutoSizePie is set to true, this value will be ignored.
        ''' </summary>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(200)> <Description("The radius of the pie, in pixels.")>
        Public Property Radius() As Single
            Get
                Return Style.Radius
            End Get
            Set(value As Single)
                Style.Radius = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets if the pie should be sized to fit the control.  If this property is true,
        ''' the Radius property is ignored.
        ''' </summary>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(False)> <Description("True if the control should size the pie to fit the control.")>
        Public Property AutoSizePie() As Boolean
            Get
                Return Style.AutoSizePie
            End Get
            Set(value As Boolean)
                Style.AutoSizePie = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets if edges should be drawn on pie slices.  If false, edges are not drawn.
        ''' </summary>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(True)> <Description("True if the edges of pie slices should be drawn.")>
        Public Property ShowEdges() As Boolean
            Get
                Return Style.ShowEdges
            End Get
            Set(value As Boolean)
                Style.ShowEdges = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets if text should be drawn on pie slices.
        ''' </summary>
        ''' <remarks>
        ''' This can have one of three values.  If TextDisplayTypes.Always, the text is always drawn.
        ''' If TextDisplayTypes.FitOnly, the text is drawn only if it fits in the wedge.  If TextDisplayTypes.Never,
        ''' the text is never drawn.
        ''' </remarks>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(PieChart.TextDisplayTypes.FitOnly)> <Description("TextDisplayModeTypes.Always if text should always be drawn, TextDisplayModeTypes.Never if text should never be drawn, or TextDisplayModeTypes.FitOnly if text should be drawn only when it fits in the pie slice.")>
        Public Property TextDisplayMode() As TextDisplayTypes
            Get
                Return Style.TextDisplayMode
            End Get
            Set(value As TextDisplayTypes)
                Style.TextDisplayMode = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets if tool tips should be shown when the mouse hovers over pie slices.  If false, tool tips are not shown.
        ''' </summary>
        <Browsable(True)> <Category("Pie Chart")> <DefaultValue(True)> <Description("True if tool tips for pie slices should be drawn.")>
        Public Property ShowToolTips() As Boolean
            Get
                Return Style.ShowToolTips
            End Get
            Set(value As Boolean)
                Style.ShowToolTips = value
            End Set
        End Property
#End Region

#Region "Methods"
        Public Function GetChartSize(padding As Padding) As Size
            Dim maxOffset As Single = GetMaximumItemOffset()
            Dim width As Integer = CInt(Math.Truncate(2 * (Radius + maxOffset) + padding.Horizontal))
            Dim height As Integer = CInt(Math.Truncate(2 * (Radius * Style.HeightWidthRatio + maxOffset) + Style.VisualThickness + padding.Vertical))
            Return New Size(width, height)
        End Function

        ''' <summary>
        ''' Gets the maximum offset of all PieChartItems in the Items collection.
        ''' </summary>
        ''' <returns>The maximum offset of all items.</returns>
        Private Function GetMaximumItemOffset() As Single
            Dim max As Single = 0
            For Each item As PieChartItem In Items
                max = Math.Max(max, item.Offset)
            Next
            Return max
        End Function

        ''' <summary>
        ''' Calculates the radius that will be used for autosizing the pie to fit the control.
        ''' </summary>
        ''' <returns>The radius that will fit the pie in the control bounds.</returns>
        Private Function GetAutoSizeRadius(bounds As Rectangle, padding As Padding) As Single
            Dim maxOffset As Single = GetMaximumItemOffset()
            Dim widthHeightRatio As Single = Style.HeightWidthRatio
            Dim width As Single = (bounds.Width - padding.Horizontal) \ 2
            Dim height As Single = (bounds.Height - padding.Vertical - Style.VisualThickness) / 2

            Dim radius As Single = Math.Max(PieChartStyle.AutoSizeMinimumRadius, Math.Min(width - maxOffset, (height - maxOffset) / widthHeightRatio))

            Return radius
        End Function

        ''' <summary>
        ''' Constructs the array of DrawingMetrics, which store drawing information about each pie slice.
        ''' </summary>
        Private Function ConstructDrawingMetrics(bounds As Rectangle, padding As Padding) As List(Of DrawingMetrics2)
            Dim results As New List(Of DrawingMetrics2)()
            Try
                ' increment the transaction reference counter so that any modifications in this method don't lead to a recursive redrawing of the control.
                transactionRef += 1

                If Items.TotalItemWeight = 0 Then
                    Return results
                End If

                If Style.AutoSizePie Then
                    Style.RadiusInternal = GetAutoSizeRadius(bounds, padding)
                End If

                Dim angleUsed As Single = Style.Rotation
                For i As Integer = 0 To Items.Count - 1
                    Dim dm As New DrawingMetrics2(Me, Items(i), bounds, angleUsed, CSng(Items(i).Percent * Math.PI * 2))
                    results.Add(dm)
                    angleUsed += CSng(dm.SweepAngle)
                Next

                ' sort the drawing metrics in the order they should be drawn
                results.Sort()
            Finally
                ' end our transaction
                transactionRef -= 1
            End Try

            Return results
        End Function

        ''' <summary>
        ''' Recreates all of the pens and brushes used by the DrawingMetrics that have been constructed.
        ''' </summary>
        Private Overloads Sub RecreateGraphics2(drawingMetrics As List(Of DrawingMetrics2), bounds As Rectangle)
            For i As Integer = 0 To drawingMetrics.Count - 1
                drawingMetrics(i).DrawingBounds = bounds
                drawingMetrics(i).RecreateGraphics()
            Next
        End Sub

        ''' <summary>
        ''' Destroys all of the DrawingMetrics currently in the array by releasing all of their resources.
        ''' </summary>
        Private Sub DestructDrawingMetrics(drawingMetrics As List(Of DrawingMetrics2))
            For Each metric As DrawingMetrics2 In drawingMetrics
                metric.DestroyResources()
            Next

            drawingMetrics.Clear()
        End Sub

        ''' <summary>
        ''' Set the currently focused PieChartItem.
        ''' </summary>
        ''' <param name="item">The item that currently has mouse focus.</param>
        Private Sub SetFocusedItem(item As PieChartItem)
            If item IsNot Me.FocusedItem Then
                FireItemFocusChanging(Me.FocusedItem, item)
                Me._FocusedItem = item
                FireItemFocusChanged()

                MarkVisualChange(True)
            End If

            ' check to see if the item has a tool tip and if it should be displayed
            If Me.FocusedItem IsNot Nothing AndAlso Me.FocusedItem.ToolTipText IsNot Nothing AndAlso Me.Style.ShowToolTips Then
                toolTip.SetToolTip(Me, Me.FocusedItem.ToolTipText)
            Else
                toolTip.RemoveAll()
            End If
        End Sub

        ''' <summary>
        ''' Performs a hit test to see which PieChartItem is under the current mouse position.
        ''' </summary>
        ''' <param name="controlPoint">The untranslated point given by the mouse move notification.</param>
        ''' <returns>The DrawingMetrics of the item under the point, or null if no item is there.</returns>
        Private Function HitTest(controlPoint As PointF) As DrawingMetrics2
            If drawingMetrics.Count = 0 Then
                Return Nothing
            End If

            ' translated the point so the origin is at the center of the pie
            Dim transPoint As New PointF(controlPoint.X - Width \ 2, controlPoint.Y - (Height + Style.VisualThickness) / 2)

            ' if a single item is both the frontmost (bottom) and rearmost (top) item in the display, special hit testing is needed
            Dim itemBottomTop As Boolean = drawingMetrics(0).IsBottomItem AndAlso drawingMetrics(0).IsTopItem
            If itemBottomTop Then
                ' check to see if the top surface or exterior surface of the control is hit, but not the interior surface
                If drawingMetrics(0).TopRegion.IsVisible(transPoint) OrElse drawingMetrics(0).ExteriorRegion.IsVisible(transPoint) Then
                    Return drawingMetrics(0)
                End If
            End If

            ' check surfaces of all controls in order, returning the first hit
            For i As Integer = drawingMetrics.Count - 1 To 0 Step -1
                If drawingMetrics(i).VisibleRegion.IsVisible(transPoint) Then
                    Return drawingMetrics(i)
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Handles the MouseEnter event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            MyBase.OnMouseEnter(e)

            toolTipDefaultDelay = toolTip.AutoPopDelay
            toolTip.AutoPopDelay = Integer.MaxValue
        End Sub

        ''' <summary>
        ''' Handles the MouseLeave event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)

            toolTip.AutoPopDelay = toolTipDefaultDelay
            SetFocusedItem(Nothing)
        End Sub

        ''' <summary>
        ''' Handles the MouseMove event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)

            Dim m As DrawingMetrics2 = HitTest(New PointF(e.X, e.Y))
            SetFocusedItem(If(m Is Nothing, Nothing, m.Item))
        End Sub

        ''' <summary>
        ''' Handles the MouseClick event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
            MyBase.OnMouseClick(e)

            If Me.FocusedItem IsNot Nothing Then
                FireItemClicked(Me.FocusedItem)
            End If
        End Sub

        ''' <summary>
        ''' Handles the DoubleClick event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnDoubleClick(e As EventArgs)
            MyBase.OnDoubleClick(e)

            If Me.FocusedItem IsNot Nothing Then
                FireItemDoubleClicked(Me.FocusedItem)
            End If
        End Sub

        ''' <summary>
        ''' Handles the SizeChanged event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnSizeChanged(e As EventArgs)
            MyBase.OnSizeChanged(e)

            If Me.AutoSizePie Then
                MarkStructuralChange()
            End If
        End Sub

        ''' <summary>
        ''' Handles the PaddingChanged event.
        ''' </summary>
        ''' <param name="e">The event arguments.</param>
        Protected Overrides Sub OnPaddingChanged(e As EventArgs)
            MyBase.OnPaddingChanged(e)

            If Me.AutoSizePie Then
                MarkStructuralChange()
            End If
        End Sub

        ''' <summary>
        ''' Renders the given DrawingMetrics, which are calculated using ConstructDrawingMetrics.
        ''' </summary>
        ''' <param name="g">The graphics surface on which the chart is being rendered.</param>
        ''' <param name="drawingMetrics">The drawing metrics to render.</param>
        Private Sub Render(g As Graphics, drawingMetrics As List(Of DrawingMetrics2), bounds As Rectangle, padding As Padding)
            ' use a high-quality smoothing mode
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TranslateTransform(bounds.Left + padding.Left + (bounds.Width - padding.Horizontal) \ 2, bounds.Top + padding.Top + (bounds.Height - padding.Vertical + Style.VisualThickness) / 2)

            ' don't draw anything if there's nothing to draw
            If drawingMetrics.Count = 0 Then
                Return
            End If

            ' if there is an item that is both at the bottom and top of the pie, special drawing considerations
            ' are needed
            Dim itemBottomTop As Boolean = drawingMetrics(0).IsBottomItem AndAlso drawingMetrics(0).IsTopItem
            If itemBottomTop Then
                drawingMetrics(0).RenderBottom(g)
                drawingMetrics(0).RenderInterior(g)
            Else
                drawingMetrics(0).RenderBottom(g)
                drawingMetrics(0).RenderInterior(g)
                drawingMetrics(0).RenderExterior(g)
            End If

            For i As Integer = 1 To drawingMetrics.Count - 1
                drawingMetrics(i).RenderBottom(g)
                drawingMetrics(i).RenderInterior(g)
                drawingMetrics(i).RenderExterior(g)
                drawingMetrics(i).RenderTop(g)
            Next

            If itemBottomTop Then
                drawingMetrics(0).RenderExterior(g)
            End If
            drawingMetrics(0).RenderTop(g)

            For Each metric As DrawingMetrics2 In Me.drawingMetrics
                metric.RenderText(g)
            Next
        End Sub

        ''' <summary>
        ''' Save the chart as an image.
        ''' </summary>
        ''' <param name="fileName">The path to the file where the image will be saved.</param>
        ''' <param name="format">The format to save the image in.</param>
        ''' <param name="sizeInPixels">The size of the image, in pixels.</param>
        Public Sub SaveAs(fileName As String, format As ImageFormats, sizeInPixels As Size)
            SaveAs(fileName, format, sizeInPixels, Padding.Empty)
        End Sub

        ''' <summary>
        ''' Saves the chart as an image.
        ''' </summary>
        ''' <param name="fileName">The path to the file where the image will be saved.</param>
        ''' <param name="format">The format to save the image in.</param>
        ''' <param name="sizeInPixels">The size of the image, in pixels.</param>
        ''' <param name="padding">The padding which defines the border of the image.</param>
        Public Sub SaveAs(fileName As String, format As ImageFormats, sizeInPixels As Size, padding As Padding)
            Dim metrics As List(Of DrawingMetrics2) = ConstructDrawingMetrics(New Rectangle(Point.Empty, sizeInPixels), padding)
            Using bitmap As New Bitmap(sizeInPixels.Width, sizeInPixels.Height)
                Using g As Graphics = Graphics.FromImage(bitmap)
                    ' fill in the background
                    g.FillRectangle(Brushes.White, 0, 0, sizeInPixels.Width, sizeInPixels.Height)

                    ' render the chart
                    Render(g, metrics, New Rectangle(Point.Empty, sizeInPixels), padding)

                    ' save the image
                    bitmap.Save(fileName, GetFormat(format))
                End Using
            End Using

            DestructDrawingMetrics(metrics)
        End Sub

        ''' <summary>
        ''' Registers a PrintDocument to print this pie chart.
        ''' </summary>
        ''' <param name="doc">The PrintDocument to register.</param>
        Public Sub AttachPrintDocument(doc As PrintDocument)
            AddHandler doc.PrintPage, New PrintPageEventHandler(AddressOf OnPrintPage)
        End Sub

        ''' <summary>
        ''' Called by a registered PrintDocument to control printing of the chart.
        ''' </summary>
        ''' <param name="sender">The sender.</param>
        ''' <param name="e">The event arguments.</param>
        Private Sub OnPrintPage(sender As Object, e As PrintPageEventArgs)
            Dim metrics As List(Of DrawingMetrics2) = ConstructDrawingMetrics(e.MarginBounds, Padding.Empty)

            e.Graphics.SetClip(e.MarginBounds)
            Render(e.Graphics, metrics, e.MarginBounds, Padding.Empty)
            e.HasMorePages = False

            DestructDrawingMetrics(metrics)
        End Sub

        ''' <summary>
        ''' Handles the painting of the control.
        ''' </summary>
        ''' <param name="pe">The paint event arguments.</param>
        Protected Overrides Sub OnPaint(pe As PaintEventArgs)
            ' check to see if the structure has changed and if we're not in the middle of a transaction
            If isStructureChanged AndAlso transactionRef = 0 Then
                DestructDrawingMetrics(Me.drawingMetrics)
                Me.drawingMetrics = ConstructDrawingMetrics(Me.ClientRectangle, Me.Padding)
            ElseIf isVisualChanged AndAlso recreateGraphics AndAlso transactionRef = 0 Then
                RecreateGraphics2(Me.drawingMetrics, Me.ClientRectangle)
            End If

            ' clear any change markings
            isStructureChanged = False
            isVisualChanged = False
            recreateGraphics = False

            Render(pe.Graphics, Me.drawingMetrics, ClientRectangle, Padding)
        End Sub
#End Region

#Region "Transaction Methods"
        ''' <summary>
        ''' Starts a modification transaction.  As long as any modification trasactions are open,
        ''' the changes made to the control will not be reflected.  It is necessary to call
        ''' EndModification for each call to BeginModification; otherwise, the control will
        ''' never redraw.
        ''' </summary>
        Public Sub BeginModification()
            Me.transactionRef += 1
        End Sub

        ''' <summary>
        ''' Ends a modification transaction.  As long as any modification trasactions are open,
        ''' the changes made to the control will not be reflected.  It is necessary to call
        ''' EndModification for each call to BeginModification; otherwise, the control will
        ''' never redraw.
        ''' </summary>
        Public Sub EndModification()
            Me.transactionRef = Math.Max(0, Me.transactionRef - 1)
            If Me.transactionRef = 0 AndAlso Me.isVisualChanged Then
                Me.Invalidate()
            End If
        End Sub

        ''' <summary>
        ''' Sets a flag that indicates the control has changed structurally, and that DrawingMetrics
        ''' will need to be completely recreated.
        ''' </summary>
        Friend Sub MarkStructuralChange()
            Me.isStructureChanged = True
            Me.isVisualChanged = True
            If Me.transactionRef = 0 Then
                Me.Invalidate()
            Else
                Console.WriteLine("Pie chart not invalidated")
            End If
        End Sub

        ''' <summary>
        ''' Sets a flag that indicates that the control needs to be refreshed, but that no structural
        ''' or resource (pen/brush) altering changes were made.
        ''' </summary>
        Friend Sub MarkVisualChange()
            MarkVisualChange(False)
        End Sub

        ''' <summary>
        ''' Sets a flag that indicates the control needs to be refreshed.  If recreateGraphics is true,
        ''' then pens and brushes will be recreated.
        ''' </summary>
        ''' <param name="recreateGraphics">True if pens and brushes should be recreated.</param>
        Friend Sub MarkVisualChange(recreateGraphics As Boolean)
            Me.isVisualChanged = True
            Me.recreateGraphics = Me.recreateGraphics OrElse recreateGraphics
            If Me.transactionRef = 0 Then
                Me.Invalidate()
            End If
        End Sub
#End Region
    End Class
End Namespace
