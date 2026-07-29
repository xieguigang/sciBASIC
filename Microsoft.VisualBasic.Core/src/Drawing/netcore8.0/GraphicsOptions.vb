
Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    Public Class PathData

        ''' <summary>
        ''' Gets or sets an array of PointF structures that represent the points through which the path is constructed.
        ''' </summary>
        Public Property Points As PointF()

        ''' <summary>
        ''' Gets or sets the types of the corresponding points in the path. 
        ''' 0=Start, 1=Line, 3=Bezier/Bezier3, 0x80=CloseSubpath flag
        ''' </summary>
        Public Property Types As Byte()

    End Class

    Public Enum FillMode
        ''' <summary>
        ''' Specifies the alternate fill mode.
        ''' </summary>
        Alternate = 0
        ''' <summary>
        ''' Specifies the winding fill mode.
        ''' </summary>
        Winding = 1
    End Enum


    ''' <summary>
    ''' Specifies the warping mode.
    ''' </summary>
    Public Enum WarpMode
        ''' <summary>
        ''' Specifies a perspective warp.
        ''' </summary>
        Perspective = 0
        ''' <summary>
        ''' Specifies a bilinear warp.
        ''' </summary>
        Bilinear = 1
    End Enum

    ''' <summary>
    ''' Specifies the smoothing/antialiasing quality applied to lines, curves and edges.
    ''' </summary>
    Public Enum SmoothingMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default mode (no antialiasing).
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies low speed, high quality (antialiased).
        ''' </summary>
        HighSpeed = 1

        ''' <summary>
        ''' Specifies high speed, low quality (no antialiasing).
        ''' </summary>
        HighQuality = 2

        ''' <summary>
        ''' Specifies no antialiasing.
        ''' </summary>
        None = 3

        ''' <summary>
        ''' Specifies antialiased rendering.
        ''' </summary>
        AntiAlias = 4
    End Enum

    ''' <summary>
    ''' Specifies how intermediate values between two endpoints are calculated during scaling or rotation.
    ''' </summary>
    Public Enum InterpolationMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default interpolation mode.
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies low quality interpolation (equivalent to NearestNeighbor).
        ''' </summary>
        Low = 1

        ''' <summary>
        ''' Specifies high quality interpolation (equivalent to HighQualityBicubic).
        ''' </summary>
        High = 2

        ''' <summary>
        ''' Specifies bilinear interpolation. No prefiltering is done.
        ''' </summary>
        Bilinear = 3

        ''' <summary>
        ''' Specifies bicubic interpolation. No prefiltering is done.
        ''' </summary>
        Bicubic = 4

        ''' <summary>
        ''' Specifies nearest-neighbor interpolation.
        ''' </summary>
        NearestNeighbor = 5

        ''' <summary>
        ''' Specifies high-quality, bilinear interpolation. Prefiltering ensures high-quality shrinking.
        ''' </summary>
        HighQualityBilinear = 6

        ''' <summary>
        ''' Specifies high-quality, bicubic interpolation. Prefiltering ensures high-quality shrinking.
        ''' </summary>
        HighQualityBicubic = 7
    End Enum

    ''' <summary>
    ''' Specifies how the source colors are combined with the background colors during rendering.
    ''' </summary>
    Public Enum CompositingMode

        ''' <summary>
        ''' Specifies that the color being rendered overwrites the background color.
        ''' </summary>
        SourceOver = 0

        ''' <summary>
        ''' Specifies that the color being rendered is blended with the background color. 
        ''' The blend is determined by the alpha component of the color being rendered.
        ''' </summary>
        SourceCopy = 1
    End Enum

    ''' <summary>
    ''' Specifies how pixels are offset during rendering.
    ''' </summary>
    Public Enum PixelOffsetMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default mode.
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies high speed, low quality rendering.
        ''' </summary>
        HighSpeed = 1

        ''' <summary>
        ''' Specifies high quality, low speed rendering.
        ''' </summary>
        HighQuality = 2

        ''' <summary>
        ''' Specifies no pixel offset.
        ''' </summary>
        None = 3

        ''' <summary>
        ''' Specifies that pixels are offset by -.5 units both horizontally and vertically 
        ''' for high speed antialiasing.
        ''' </summary>
        Half = 4
    End Enum

    ''' <summary>
    ''' Specifies the overall quality when rendering GDI+ objects.
    ''' </summary>
    Public Enum QualityMode

        ''' <summary>
        ''' Specifies an invalid mode.
        ''' </summary>
        Invalid = -1

        ''' <summary>
        ''' Specifies the default mode.
        ''' </summary>
        [Default] = 0

        ''' <summary>
        ''' Specifies low quality, high speed rendering.
        ''' </summary>
        Low = 1

        ''' <summary>
        ''' Specifies high quality, low speed rendering.
        ''' </summary>
        High = 2
    End Enum

    ''' <summary>
    ''' Represents the state of a Graphics object. Returned by Save() and passed to Restore().
    ''' </summary>
    Public Class GraphicsState

        Private ReadOnly _stateIndex As Integer

        Sub New(stateIndex As Integer)
            _stateIndex = stateIndex
        End Sub

        ''' <summary>
        ''' Gets the index of this state in the state stack.
        ''' </summary>
        Public ReadOnly Property StateIndex As Integer
            Get
                Return _stateIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"GraphicsState[{_stateIndex}]"
        End Function
    End Class
#End If
End Namespace