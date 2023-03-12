
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

    Private _tmHeight As Integer, _tmAscent As Integer, _tmDescent As Integer, _tmInternalLeading As Integer, _tmExternalLeading As Integer, _tmAveCharWidth As Integer, _tmMaxCharWidth As Integer, _tmWeight As Integer, _tmOverhang As Integer, _tmDigitizedAspectX As Integer, _tmDigitizedAspectY As Integer, _tmFirstChar As UShort, _tmLastChar As UShort, _tmDefaultChar As UShort, _tmBreakChar As UShort, _tmItalic As Byte, _tmUnderlined As Byte, _tmStruckOut As Byte, _tmPitchAndFamily As Byte, _tmCharSet As Byte

    ''' <summary>
    ''' TextMetric height
    ''' </summary>
    Public Property tmHeight As Integer
        Get
            Return _tmHeight
        End Get
        Private Set(value As Integer)
            _tmHeight = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric ascent
    ''' </summary>
    Public Property tmAscent As Integer
        Get
            Return _tmAscent
        End Get
        Private Set(value As Integer)
            _tmAscent = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric descent
    ''' </summary>
    Public Property tmDescent As Integer
        Get
            Return _tmDescent
        End Get
        Private Set(value As Integer)
            _tmDescent = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric internal leading
    ''' </summary>
    Public Property tmInternalLeading As Integer
        Get
            Return _tmInternalLeading
        End Get
        Private Set(value As Integer)
            _tmInternalLeading = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric external leading
    ''' </summary>
    Public Property tmExternalLeading As Integer
        Get
            Return _tmExternalLeading
        End Get
        Private Set(value As Integer)
            _tmExternalLeading = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric average character width
    ''' </summary>
    Public Property tmAveCharWidth As Integer
        Get
            Return _tmAveCharWidth
        End Get
        Private Set(value As Integer)
            _tmAveCharWidth = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric maximum character width
    ''' </summary>
    Public Property tmMaxCharWidth As Integer
        Get
            Return _tmMaxCharWidth
        End Get
        Private Set(value As Integer)
            _tmMaxCharWidth = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric height
    ''' </summary>
    Public Property tmWeight As Integer
        Get
            Return _tmWeight
        End Get
        Private Set(value As Integer)
            _tmWeight = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric overhang
    ''' </summary>
    Public Property tmOverhang As Integer
        Get
            Return _tmOverhang
        End Get
        Private Set(value As Integer)
            _tmOverhang = value
        End Set
    End Property

    ''' <summary>
    ''' TextMetric digitize aspect X
    ''' </summary>
    Public Property tmDigitizedAspectX As Integer
        Get
            Return _tmDigitizedAspectX
        End Get
        Private Set(value As Integer)
            _tmDigitizedAspectX = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric digitize aspect Y
    ''' </summary>

    Public Property tmDigitizedAspectY As Integer
        Get
            Return _tmDigitizedAspectY
        End Get
        Private Set(value As Integer)
            _tmDigitizedAspectY = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric first character
    ''' </summary>
    Public Property tmFirstChar As UShort
        Get
            Return _tmFirstChar
        End Get
        Private Set(value As UShort)
            _tmFirstChar = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric last character
    ''' </summary>
    Public Property tmLastChar As UShort
        Get
            Return _tmLastChar
        End Get
        Private Set(value As UShort)
            _tmLastChar = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric default character
    ''' </summary>
    Public Property tmDefaultChar As UShort
        Get
            Return _tmDefaultChar
        End Get
        Private Set(value As UShort)
            _tmDefaultChar = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric break character
    ''' </summary>
    Public Property tmBreakChar As UShort
        Get
            Return _tmBreakChar
        End Get
        Private Set(value As UShort)
            _tmBreakChar = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric italic
    ''' </summary>
    Public Property tmItalic As Byte
        Get
            Return _tmItalic
        End Get
        Private Set(value As Byte)
            _tmItalic = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric underlined
    ''' </summary>
    Public Property tmUnderlined As Byte
        Get
            Return _tmUnderlined
        End Get
        Private Set(value As Byte)
            _tmUnderlined = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric struck out
    ''' </summary>
    Public Property tmStruckOut As Byte
        Get
            Return _tmStruckOut
        End Get
        Private Set(value As Byte)
            _tmStruckOut = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric pitch and family
    ''' </summary>
    Public Property tmPitchAndFamily As Byte
        Get
            Return _tmPitchAndFamily
        End Get
        Private Set(value As Byte)
            _tmPitchAndFamily = value
        End Set
    End Property
    ''' <summary>
    ''' TextMetric character set
    ''' </summary>
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

