#Region "Microsoft.VisualBasic::31277089733791dd128b81f1bc9fecfd, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\PieChart\PieChartControls\PieChartControl.PieChartItemStyle.vb"

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
Imports System.ComponentModel

Namespace Windows.Forms.Nexus

    Partial Public Class PieChart
        ''' <summary>
        ''' Represents the possible styles corresponding to a PieChartItem.
        ''' </summary>
        Public Class PieChartItemStyle
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
            ''' The factor by which edge brightness will be affected.
            ''' </summary>
            Private m_edgeBrightnessFactor As Single = -0.3F

            ''' <summary>
            ''' The surface alpha transparency factor.
            ''' </summary>
            Private m_surfaceAlphaTransparency As Single = 1.0F

            ''' <summary>
            ''' The factor by which surface brightness will be affected.
            ''' </summary>
            Private m_surfaceBrightnessFactor As Single = 0F
#End Region

#Region "Properties"
            ''' <summary>
            ''' Gets or sets the surface alpha transparency factor.
            ''' </summary>
            ''' <remarks>
            ''' This value must be between 0 and 1, and represents the multiplier that is applied to the 
            ''' alpha value of the color for pie slices that use this style.
            ''' </remarks>
            Public Property SurfaceAlphaTransparency() As Single
                Get
                    Return m_surfaceAlphaTransparency
                End Get
                Set
                    If m_surfaceAlphaTransparency <> value Then
                        If value < 0 OrElse value > 1 Then
                            Throw New ArgumentOutOfRangeException("SurfaceAlphaTransparenty", value, "The SurfaceAlphaTransparency must be between 0 and 1 inclusive.")
                        End If

                        m_surfaceAlphaTransparency = value
                        container.MarkVisualChange(True)
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the factor by which edge brightness will be affected.
            ''' </summary>
            Public Property EdgeBrightnessFactor() As Single
                Get
                    Return m_edgeBrightnessFactor
                End Get
                Set
                    If m_edgeBrightnessFactor <> Value Then
                        If Value < -1 OrElse Value > 1 Then
                            Throw New ArgumentOutOfRangeException("EdgeBrightnessFactor", Value, "The EdgeBrightnessFactor must be between -1 and 1 inclusive.")
                        End If

                        m_edgeBrightnessFactor = Value
                        container.MarkVisualChange(True)
                    End If
                End Set
            End Property

            ''' <summary>
            ''' Gets or sets the factor by which surface brightness will be affected.
            ''' </summary>
            Public Property SurfaceBrightnessFactor() As Single
                Get
                    Return m_surfaceBrightnessFactor
                End Get
                Set
                    If m_surfaceBrightnessFactor <> value Then
                        If value < -1 OrElse value > 1 Then
                            Throw New ArgumentOutOfRangeException("SurfaceBrightnessFactor", value, "The SurfaceBrightnessFactor must be between -1 and 1 inclusive.")
                        End If

                        m_surfaceBrightnessFactor = value
                        container.MarkVisualChange(True)
                    End If
                End Set
            End Property
#End Region
        End Class
    End Class
End Namespace
