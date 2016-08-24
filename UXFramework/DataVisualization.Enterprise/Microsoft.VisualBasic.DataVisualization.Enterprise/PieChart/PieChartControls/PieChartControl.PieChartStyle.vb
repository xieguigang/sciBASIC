#Region "Microsoft.VisualBasic::ff4c29496a7ba99565d7bc0ff563c45f, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.PieChartStyle.vb"

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

Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Windows.Forms.Nexus

    Partial Public Class PieChart
        Inherits Control
        ''' <summary>
        ''' Represents the possible text display modes for the PieChart.
        ''' </summary>
        Public Enum TextDisplayTypes
            Always
            FitOnly
            Never
        End Enum

        ''' <summary>
        ''' Represents the possible styles corresponding to a PieChart.
        ''' </summary>
        Public Class PieChartStyle
#Region "Constructor"
            ''' <summary>
            ''' Constructs a new instance of PieChartItemStyle.
            ''' </summary>
            ''' <param name="container">The control that contains the style.</param>
            Friend Sub New(container As PieChart)
                Me.container = container
            End Sub
#End Region

#Region "Fields"
            ''' <summary>
            ''' The control that contains the style.
            ''' </summary>
            Private container As PieChart

            ''' <summary>
            ''' Whether or not the pie should be sized to automatically fit the control.
            ''' </summary>
            Private autoSize As Boolean = False

            ''' <summary>
            ''' The radius of the pie.
            ''' </summary>
            Private m_radius As Single = 200.0F

            ''' <summary>
            ''' The thickness of the pie.
            ''' </summary>
            Private m_thickness As Single = 10.0F

            ''' <summary>
            ''' The rotation of the pie.
            ''' </summary>
            Private m_rotation As Single = 0F

            ''' <summary>
            ''' The inclination at which the pie is viewed.
            ''' </summary>
            Private m_inclination As Single = CSng(Math.PI / 6)

            ''' <summary>
            ''' Whether or not edges should be drawn.
            ''' </summary>
            Private m_showEdges As Boolean = True

            ''' <summary>
            ''' The text display mode of the control.
            ''' </summary>
            Private m_textDisplayMode As TextDisplayTypes = TextDisplayTypes.FitOnly

            ''' <summary>
            ''' Whether or not tool tips should be shown on the control.
            ''' </summary>
            Private m_showToolTips As Boolean = True

            ''' <summary>
            ''' The minimum radius when the control is auto-sized.
            ''' </summary>
            Friend Const AutoSizeMinimumRadius As Single = 10.0F
#End Region

#Region "Properties"
            ''' <summary>
            ''' Gets or sets if the pie should be sized to fit the control.  If this property is true,
            ''' the Radius property is ignored.
            ''' </summary>
            Public Property AutoSizePie() As Boolean
                Get
                    Return autoSize
                End Get
                Set
                    If autoSize <> value Then
                        autoSize = value
                        container.MarkStructuralChange()
                        container.FireAutoSizePieChanged()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets radius of the control, in pixels.  If AutoSizePie is set to true, this value will be ignored.
            ''' </summary>
            Public Property Radius() As Single
                Get
                    Return m_radius
                End Get
                Set
                    If Not AutoSizePie Then
                        RadiusInternal = value
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the radius of the control, ignoring the AutoSizePie property.
            ''' </summary>
            Friend Property RadiusInternal() As Single
                Get
                    Return m_radius
                End Get
                Set
                    If m_radius <> value Then
                        If value <= 0 Then
                            Throw New ArgumentOutOfRangeException("Radius", value, "Radius must be a positive value.")
                        End If

                        m_radius = value
                        container.MarkStructuralChange()
                        container.FireRadiusChanged()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets thickness of the pie, in pixels.
            ''' </summary>
            ''' <remarks>This represents the three-dimensional thickness of the control.
            ''' The actual visual thickness of the control depends on the inclination.  To determine what the apparent
            ''' thickness of the control is, use the Style.VisualHeight property.  The thickness must be greater than or equal to 0.</remarks>
            Public Property Thickness() As Single
                Get
                    Return m_thickness
                End Get
                Set
                    If m_thickness <> value Then
                        If value < 0 Then
                            Throw New ArgumentOutOfRangeException("Thickness", value, "Thickness must be greater than or equal to 0.")
                        End If

                        m_thickness = value
                        container.MarkStructuralChange()
                        container.FireThicknessChanged()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets the visual thickness of the pie, after the inclination has been taken into account.
            ''' </summary>
            Public ReadOnly Property VisualThickness() As Single
                Get
                    Return CSng(Thickness * Math.Cos(Inclination))
                End Get
            End Property

            ''' <summary>
            ''' Gets or sets the rotation of the pie chart.  This is represented in radians, with positive values indicating
            ''' a rotation in the clockwise direction.
            ''' </summary>
            Public Property Rotation() As Single
                Get
                    Return m_rotation
                End Get
                Set
                    If m_rotation <> value Then
                        m_rotation = CSng(value Mod (Math.PI * 2))
                        If m_rotation < 0 Then
                            m_rotation += CSng(Math.PI * 2)
                        End If

                        container.MarkStructuralChange()
                        container.FireRotationChanged()
                    End If
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
            Public Property Inclination() As Single
                Get
                    Return m_inclination
                End Get
                Set
                    If m_inclination <> value Then
                        If value <= 0 OrElse value > Math.PI / 2 Then
                            Throw New ArgumentOutOfRangeException("Inclination", value, "The inclination must be a radian angle greater than 0 and less than or equal to PI / 2.")
                        End If

                        m_inclination = value
                        container.MarkStructuralChange()
                        container.FireInclinationChanged()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets if edges should be drawn on pie slices.  If false, edges are not drawn.
            ''' </summary>
            Public Property ShowEdges() As Boolean
                Get
                    Return m_showEdges
                End Get
                Set
                    If m_showEdges <> value Then
                        m_showEdges = value
                        container.MarkVisualChange()
                        container.FireShowEdgesChanged()
                    End If
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
            Public Property TextDisplayMode() As TextDisplayTypes
                Get
                    Return m_textDisplayMode
                End Get
                Set
                    If m_textDisplayMode <> value Then
                        m_textDisplayMode = value
                        container.MarkVisualChange()
                        container.FireTextDisplayModeChanged()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets if tool tips should be shown when the mouse hovers over pie slices.  If false, tool tips are not shown.
            ''' </summary>
            Public Property ShowToolTips() As Boolean
                Get
                    Return m_showToolTips
                End Get
                Set
                    If m_showToolTips <> value Then
                        m_showToolTips = value
                        container.FireShowToolTipsChanged()
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Represents the brightness factor used in determining shadow colors.
            ''' </summary>
            Friend ReadOnly Property ShadowBrightnessFactor() As Single
                Get
                    Return -0.3F
                End Get
            End Property

            ''' <summary>
            ''' Gets the width of the major axis of the ellipse.  This is half the total
            ''' width of the ellipse.
            ''' </summary>
            Friend ReadOnly Property EllipseWidth() As Single
                Get
                    Return Radius
                End Get
            End Property

            ''' <summary>
            ''' Gets the height of the minor axis of the ellipse.  This is half the total
            ''' height of the ellipse.
            ''' </summary>
            Friend ReadOnly Property EllipseHeight() As Single
                Get
                    Return Radius * HeightWidthRatio
                End Get
            End Property

            ''' <summary>
            ''' Gets the height-width ratio for the ellipse.
            ''' </summary>
            Friend ReadOnly Property HeightWidthRatio() As Single
                Get
                    Return CSng(Math.Sin(Inclination))
                End Get
            End Property
#End Region
        End Class
    End Class
End Namespace
