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