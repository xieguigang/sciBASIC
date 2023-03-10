
''' <summary>
''' Outline text metric class
''' </summary>
''' <remarks>
''' The OUTLINETEXTMETRIC structure contains metrics describing
''' a TrueType font.
''' </remarks>
Public Class WinOutlineTextMetric


    ''' <summary>
    ''' Outline text metric full name
    ''' </summary>
    Private _otmSize As UInteger, _otmTextMetric As WinTextMetric, _otmPanoseNumber As WinPanose, _otmfsSelection As UInteger, _otmfsType As UInteger, _otmsCharSlopeRise As Integer, _otmsCharSlopeRun As Integer, _otmItalicAngle As Integer, _otmEMSquare As UInteger, _otmAscent As Integer, _otmDescent As Integer, _otmLineGap As UInteger, _otmsCapEmHeight As UInteger, _otmsXHeight As UInteger, _otmrcFontBox As FontBox, _otmMacAscent As Integer, _otmMacDescent As Integer, _otmMacLineGap As UInteger, _otmusMinimumPPEM As UInteger, _otmptSubscriptSize As System.Drawing.Point, _otmptSubscriptOffset As System.Drawing.Point, _otmptSuperscriptSize As System.Drawing.Point, _otmptSuperscriptOffset As System.Drawing.Point, _otmsStrikeoutSize As UInteger, _otmsStrikeoutPosition As Integer, _otmsUnderscoreSize As Integer, _otmsUnderscorePosition As Integer, _otmpFamilyName As String, _otmpFaceName As String, _otmpStyleName As String, _otmpFullName As String

    ''' <summary>
    ''' Outline text metric size
    ''' </summary>
    Public Property otmSize As UInteger
        Get
            Return _otmSize
        End Get
        Private Set(value As UInteger)
            _otmSize = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric TextMetric
    ''' </summary>
    Public Property otmTextMetric As WinTextMetric
        Get
            Return _otmTextMetric
        End Get
        Private Set(value As WinTextMetric)
            _otmTextMetric = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric panose number
    ''' </summary>
    Public Property otmPanoseNumber As WinPanose
        Get
            Return _otmPanoseNumber
        End Get
        Private Set(value As WinPanose)
            _otmPanoseNumber = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric FS selection
    ''' </summary>
    Public Property otmfsSelection As UInteger
        Get
            Return _otmfsSelection
        End Get
        Private Set(value As UInteger)
            _otmfsSelection = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric ascent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric descent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric line gap
    ''' </summary>

    ''' <summary>
    ''' Outline text metric capital M height
    ''' </summary>

    ''' <summary>
    ''' Outline text metric X height
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Font box class
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Mac ascent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Mac descent
    ''' </summary>

    ''' <summary>
    ''' Outline text metric Mac line gap
    ''' </summary>

    ''' <summary>
    ''' Outline text metric minimum PPEM
    ''' </summary>

    ''' <summary>
    ''' Outline text metric subscript size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric subscript offset
    ''' </summary>

    ''' <summary>
    ''' Outline text metric superscript size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric superscript offset
    ''' </summary>

    ''' <summary>
    ''' Outline text metric strikeout size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric strikeout position
    ''' </summary>

    ''' <summary>
    ''' Outline text metric underscore size
    ''' </summary>

    ''' <summary>
    ''' Outline text metric underscore position
    ''' </summary>

    ''' <summary>
    ''' Outline text metric family name
    ''' </summary>

    ''' <summary>
    ''' Outline text metric face name
    ''' </summary>

    ''' <summary>
    ''' Outline text metric style name
    ''' </summary>

    ''' <summary>
    ''' Outline text metric FS type
    ''' </summary>
    Public Property otmfsType As UInteger
        Get
            Return _otmfsType
        End Get
        Private Set(value As UInteger)
            _otmfsType = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric char slope rise
    ''' </summary>
    Public Property otmsCharSlopeRise As Integer
        Get
            Return _otmsCharSlopeRise
        End Get
        Private Set(value As Integer)
            _otmsCharSlopeRise = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric char slope run
    ''' </summary>
    Public Property otmsCharSlopeRun As Integer
        Get
            Return _otmsCharSlopeRun
        End Get
        Private Set(value As Integer)
            _otmsCharSlopeRun = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric italic angle
    ''' </summary>
    Public Property otmItalicAngle As Integer
        Get
            Return _otmItalicAngle
        End Get
        Private Set(value As Integer)
            _otmItalicAngle = value
        End Set
    End Property

    ''' <summary>
    ''' Outline text metric EM square
    ''' </summary>
    Public Property otmEMSquare As UInteger
        Get
            Return _otmEMSquare
        End Get
        Private Set(value As UInteger)
            _otmEMSquare = value
        End Set
    End Property

    Public Property otmAscent As Integer
        Get
            Return _otmAscent
        End Get
        Private Set(value As Integer)
            _otmAscent = value
        End Set
    End Property

    Public Property otmDescent As Integer
        Get
            Return _otmDescent
        End Get
        Private Set(value As Integer)
            _otmDescent = value
        End Set
    End Property

    Public Property otmLineGap As UInteger
        Get
            Return _otmLineGap
        End Get
        Private Set(value As UInteger)
            _otmLineGap = value
        End Set
    End Property

    Public Property otmsCapEmHeight As UInteger
        Get
            Return _otmsCapEmHeight
        End Get
        Private Set(value As UInteger)
            _otmsCapEmHeight = value
        End Set
    End Property

    Public Property otmsXHeight As UInteger
        Get
            Return _otmsXHeight
        End Get
        Private Set(value As UInteger)
            _otmsXHeight = value
        End Set
    End Property

    Public Property otmrcFontBox As FontBox
        Get
            Return _otmrcFontBox
        End Get
        Private Set(value As FontBox)
            _otmrcFontBox = value
        End Set
    End Property

    Public Property otmMacAscent As Integer
        Get
            Return _otmMacAscent
        End Get
        Private Set(value As Integer)
            _otmMacAscent = value
        End Set
    End Property

    Public Property otmMacDescent As Integer
        Get
            Return _otmMacDescent
        End Get
        Private Set(value As Integer)
            _otmMacDescent = value
        End Set
    End Property

    Public Property otmMacLineGap As UInteger
        Get
            Return _otmMacLineGap
        End Get
        Private Set(value As UInteger)
            _otmMacLineGap = value
        End Set
    End Property

    Public Property otmusMinimumPPEM As UInteger
        Get
            Return _otmusMinimumPPEM
        End Get
        Private Set(value As UInteger)
            _otmusMinimumPPEM = value
        End Set
    End Property

    Public Property otmptSubscriptSize As Point
        Get
            Return _otmptSubscriptSize
        End Get
        Private Set(value As Point)
            _otmptSubscriptSize = value
        End Set
    End Property

    Public Property otmptSubscriptOffset As Point
        Get
            Return _otmptSubscriptOffset
        End Get
        Private Set(value As Point)
            _otmptSubscriptOffset = value
        End Set
    End Property

    Public Property otmptSuperscriptSize As Point
        Get
            Return _otmptSuperscriptSize
        End Get
        Private Set(value As Point)
            _otmptSuperscriptSize = value
        End Set
    End Property

    Public Property otmptSuperscriptOffset As Point
        Get
            Return _otmptSuperscriptOffset
        End Get
        Private Set(value As Point)
            _otmptSuperscriptOffset = value
        End Set
    End Property

    Public Property otmsStrikeoutSize As UInteger
        Get
            Return _otmsStrikeoutSize
        End Get
        Private Set(value As UInteger)
            _otmsStrikeoutSize = value
        End Set
    End Property

    Public Property otmsStrikeoutPosition As Integer
        Get
            Return _otmsStrikeoutPosition
        End Get
        Private Set(value As Integer)
            _otmsStrikeoutPosition = value
        End Set
    End Property

    Public Property otmsUnderscoreSize As Integer
        Get
            Return _otmsUnderscoreSize
        End Get
        Private Set(value As Integer)
            _otmsUnderscoreSize = value
        End Set
    End Property

    Public Property otmsUnderscorePosition As Integer
        Get
            Return _otmsUnderscorePosition
        End Get
        Private Set(value As Integer)
            _otmsUnderscorePosition = value
        End Set
    End Property

    Public Property otmpFamilyName As String
        Get
            Return _otmpFamilyName
        End Get
        Private Set(value As String)
            _otmpFamilyName = value
        End Set
    End Property

    Public Property otmpFaceName As String
        Get
            Return _otmpFaceName
        End Get
        Private Set(value As String)
            _otmpFaceName = value
        End Set
    End Property

    Public Property otmpStyleName As String
        Get
            Return _otmpStyleName
        End Get
        Private Set(value As String)
            _otmpStyleName = value
        End Set
    End Property

    Public Property otmpFullName As String
        Get
            Return _otmpFullName
        End Get
        Private Set(value As String)
            _otmpFullName = value
        End Set
    End Property

    Friend Sub New(DC As FontApi)
        otmSize = DC.ReadUInt32()
        otmTextMetric = New WinTextMetric(DC)
        DC.Align4()
        otmPanoseNumber = New WinPanose(DC)
        DC.Align4()
        otmfsSelection = DC.ReadUInt32()
        otmfsType = DC.ReadUInt32()
        otmsCharSlopeRise = DC.ReadInt32()
        otmsCharSlopeRun = DC.ReadInt32()
        otmItalicAngle = DC.ReadInt32()
        otmEMSquare = DC.ReadUInt32()
        otmAscent = DC.ReadInt32()
        otmDescent = DC.ReadInt32()
        otmLineGap = DC.ReadUInt32()
        otmsCapEmHeight = DC.ReadUInt32()
        otmsXHeight = DC.ReadUInt32()
        otmrcFontBox = New FontBox(DC)
        otmMacAscent = DC.ReadInt32()
        otmMacDescent = DC.ReadInt32()
        otmMacLineGap = DC.ReadUInt32()
        otmusMinimumPPEM = DC.ReadUInt32()
        otmptSubscriptSize = DC.ReadWinPoint()
        otmptSubscriptOffset = DC.ReadWinPoint()
        otmptSuperscriptSize = DC.ReadWinPoint()
        otmptSuperscriptOffset = DC.ReadWinPoint()
        otmsStrikeoutSize = DC.ReadUInt32()
        otmsStrikeoutPosition = DC.ReadInt32()
        otmsUnderscoreSize = DC.ReadInt32()
        otmsUnderscorePosition = DC.ReadInt32()
        otmpFamilyName = DC.ReadString()
        otmpFaceName = DC.ReadString()
        otmpStyleName = DC.ReadString()
        otmpFullName = DC.ReadString()
        Return
    End Sub
End Class
