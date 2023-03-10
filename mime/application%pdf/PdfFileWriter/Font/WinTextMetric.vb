
''' <summary>
''' TextMetric class
''' </summary>
''' <remarks>
''' The TEXTMETRIC structure contains basic information about a
''' physical font. All sizes are specified in logical units;
''' that is, they depend on the current mapping mode of the
''' display context.
''' </remarks>

Public Class WinTextMetric
    ''' <summary>
    ''' TextMetric height
    ''' </summary>

    ''' <summary>
    ''' TextMetric ascent
    ''' </summary>

    ''' <summary>
    ''' TextMetric descent
    ''' </summary>

    ''' <summary>
    ''' TextMetric internal leading
    ''' </summary>

    ''' <summary>
    ''' TextMetric external leading
    ''' </summary>

    ''' <summary>
    ''' TextMetric average character width
    ''' </summary>

    ''' <summary>
    ''' TextMetric maximum character width
    ''' </summary>

    ''' <summary>
    ''' TextMetric height
    ''' </summary>

    ''' <summary>
    ''' TextMetric overhang
    ''' </summary>

    ''' <summary>
    ''' TextMetric digitize aspect X
    ''' </summary>

    ''' <summary>
    ''' TextMetric digitize aspect Y
    ''' </summary>

    ''' <summary>
    ''' TextMetric first character
    ''' </summary>

    ''' <summary>
    ''' TextMetric last character
    ''' </summary>

    ''' <summary>
    ''' TextMetric default character
    ''' </summary>

    ''' <summary>
    ''' TextMetric break character
    ''' </summary>

    ''' <summary>
    ''' TextMetric italic
    ''' </summary>

    ''' <summary>
    ''' TextMetric underlined
    ''' </summary>

    ''' <summary>
    ''' TextMetric struck out
    ''' </summary>

    ''' <summary>
    ''' TextMetric pitch and family
    ''' </summary>

    ''' <summary>
    ''' TextMetric character set
    ''' </summary>
    Private _tmHeight As Integer, _tmAscent As Integer, _tmDescent As Integer, _tmInternalLeading As Integer, _tmExternalLeading As Integer, _tmAveCharWidth As Integer, _tmMaxCharWidth As Integer, _tmWeight As Integer, _tmOverhang As Integer, _tmDigitizedAspectX As Integer, _tmDigitizedAspectY As Integer, _tmFirstChar As UShort, _tmLastChar As UShort, _tmDefaultChar As UShort, _tmBreakChar As UShort, _tmItalic As Byte, _tmUnderlined As Byte, _tmStruckOut As Byte, _tmPitchAndFamily As Byte, _tmCharSet As Byte

    Public Property tmHeight As Integer
        Get
            Return _tmHeight
        End Get
        Private Set(value As Integer)
            _tmHeight = value
        End Set
    End Property

    Public Property tmAscent As Integer
        Get
            Return _tmAscent
        End Get
        Private Set(value As Integer)
            _tmAscent = value
        End Set
    End Property

    Public Property tmDescent As Integer
        Get
            Return _tmDescent
        End Get
        Private Set(value As Integer)
            _tmDescent = value
        End Set
    End Property

    Public Property tmInternalLeading As Integer
        Get
            Return _tmInternalLeading
        End Get
        Private Set(value As Integer)
            _tmInternalLeading = value
        End Set
    End Property

    Public Property tmExternalLeading As Integer
        Get
            Return _tmExternalLeading
        End Get
        Private Set(value As Integer)
            _tmExternalLeading = value
        End Set
    End Property

    Public Property tmAveCharWidth As Integer
        Get
            Return _tmAveCharWidth
        End Get
        Private Set(value As Integer)
            _tmAveCharWidth = value
        End Set
    End Property

    Public Property tmMaxCharWidth As Integer
        Get
            Return _tmMaxCharWidth
        End Get
        Private Set(value As Integer)
            _tmMaxCharWidth = value
        End Set
    End Property

    Public Property tmWeight As Integer
        Get
            Return _tmWeight
        End Get
        Private Set(value As Integer)
            _tmWeight = value
        End Set
    End Property

    Public Property tmOverhang As Integer
        Get
            Return _tmOverhang
        End Get
        Private Set(value As Integer)
            _tmOverhang = value
        End Set
    End Property

    Public Property tmDigitizedAspectX As Integer
        Get
            Return _tmDigitizedAspectX
        End Get
        Private Set(value As Integer)
            _tmDigitizedAspectX = value
        End Set
    End Property

    Public Property tmDigitizedAspectY As Integer
        Get
            Return _tmDigitizedAspectY
        End Get
        Private Set(value As Integer)
            _tmDigitizedAspectY = value
        End Set
    End Property

    Public Property tmFirstChar As UShort
        Get
            Return _tmFirstChar
        End Get
        Private Set(value As UShort)
            _tmFirstChar = value
        End Set
    End Property

    Public Property tmLastChar As UShort
        Get
            Return _tmLastChar
        End Get
        Private Set(value As UShort)
            _tmLastChar = value
        End Set
    End Property

    Public Property tmDefaultChar As UShort
        Get
            Return _tmDefaultChar
        End Get
        Private Set(value As UShort)
            _tmDefaultChar = value
        End Set
    End Property

    Public Property tmBreakChar As UShort
        Get
            Return _tmBreakChar
        End Get
        Private Set(value As UShort)
            _tmBreakChar = value
        End Set
    End Property

    Public Property tmItalic As Byte
        Get
            Return _tmItalic
        End Get
        Private Set(value As Byte)
            _tmItalic = value
        End Set
    End Property

    Public Property tmUnderlined As Byte
        Get
            Return _tmUnderlined
        End Get
        Private Set(value As Byte)
            _tmUnderlined = value
        End Set
    End Property

    Public Property tmStruckOut As Byte
        Get
            Return _tmStruckOut
        End Get
        Private Set(value As Byte)
            _tmStruckOut = value
        End Set
    End Property

    Public Property tmPitchAndFamily As Byte
        Get
            Return _tmPitchAndFamily
        End Get
        Private Set(value As Byte)
            _tmPitchAndFamily = value
        End Set
    End Property

    Public Property tmCharSet As Byte
        Get
            Return _tmCharSet
        End Get
        Private Set(value As Byte)
            _tmCharSet = value
        End Set
    End Property

    Friend Sub New(DC As FontApi)
        tmHeight = DC.ReadInt32()
        tmAscent = DC.ReadInt32()
        tmDescent = DC.ReadInt32()
        tmInternalLeading = DC.ReadInt32()
        tmExternalLeading = DC.ReadInt32()
        tmAveCharWidth = DC.ReadInt32()
        tmMaxCharWidth = DC.ReadInt32()
        tmWeight = DC.ReadInt32()
        tmOverhang = DC.ReadInt32()
        tmDigitizedAspectX = DC.ReadInt32()
        tmDigitizedAspectY = DC.ReadInt32()
        tmFirstChar = DC.ReadUInt16()
        tmLastChar = DC.ReadUInt16()
        tmDefaultChar = DC.ReadUInt16()
        tmBreakChar = DC.ReadUInt16()
        tmItalic = DC.ReadByte()
        tmUnderlined = DC.ReadByte()
        tmStruckOut = DC.ReadByte()
        tmPitchAndFamily = DC.ReadByte()
        tmCharSet = DC.ReadByte()
        Return
    End Sub
End Class

