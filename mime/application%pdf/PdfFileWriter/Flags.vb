
''' <summary>
''' PDF font style flags enumeration
''' </summary>
Public Enum DrawStyle
    ''' <summary>
    ''' Normal
    ''' </summary>
    Normal = 0

    ''' <summary>
    ''' Underline
    ''' </summary>
    Underline = 4

    ''' <summary>
    ''' Strikeout
    ''' </summary>
    Strikeout = 8

    ''' <summary>
    ''' Subscript
    ''' </summary>
    Subscript = 16

    ''' <summary>
    ''' Superscript
    ''' </summary>
    Superscript = 32
End Enum

''' <summary>
''' Path painting and clipping operators enumeration
''' </summary>
''' <remarks>
''' <para>
''' Note Special path paining considerations in section 4.4
''' of the PDF specifications. EOR is even odd rule. Otherwise
''' it is nonzero winding number rule.
''' </para>
''' </remarks>
Public Enum PaintOp
    ''' <summary>
    ''' No operator
    ''' </summary>
    NoOperator

    ''' <summary>
    ''' No paint
    ''' </summary>
    NoPaint         ' n

    ''' <summary>
    ''' Stoke
    ''' </summary>
    Stroke              ' S

    ''' <summary>
    ''' Close and stroke
    ''' </summary>
    CloseStroke     ' s

    ''' <summary>
    ''' close and Fill
    ''' </summary>
    Fill                ' f

    ''' <summary>
    ''' close and fill EOR
    ''' </summary>
    FillEor         ' f*

    ''' <summary>
    ''' Fill and stoke
    ''' </summary>
    FillStroke          ' B

    ''' <summary>
    ''' Fill and stroke EOR
    ''' </summary>
    FillStrokeEor       ' B*

    ''' <summary>
    ''' Close, Fill and stroke
    ''' </summary>
    CloseFillStroke ' b

    ''' <summary>
    ''' Close, Fill and Stroke EOR
    ''' </summary>
    CloseFillStrokeEor  ' b*

    ''' <summary>
    ''' Clip path
    ''' </summary>
    ClipPathWnr     ' h W n

    ''' <summary>
    ''' Clip path EOR
    ''' </summary>
    ClipPathEor     ' h W* n

    ''' <summary>
    ''' Close sub-path
    ''' </summary>
    CloseSubPath        ' h
End Enum

''' <summary>
''' PDF line cap enumeration
''' </summary>
Public Enum PdfLineCap
    ''' <summary>
    ''' Butt
    ''' </summary>
    Butt

    ''' <summary>
    ''' Round
    ''' </summary>
    Round

    ''' <summary>
    ''' Square
    ''' </summary>
    Square
End Enum

''' <summary>
''' PDF line join enumeration
''' </summary>
Public Enum PdfLineJoin
    ''' <summary>
    ''' Miter
    ''' </summary>
    Miter

    ''' <summary>
    ''' Round
    ''' </summary>
    Round

    ''' <summary>
    ''' Bevel
    ''' </summary>
    Bevel
End Enum

''' <summary>
''' Text rendering enumeration
''' </summary>
Public Enum TextRendering
    ''' <summary>
    ''' Fill
    ''' </summary>
    Fill

    ''' <summary>
    ''' Stroke
    ''' </summary>
    Stroke

    ''' <summary>
    ''' Fill and stroke
    ''' </summary>
    FillStroke

    ''' <summary>
    ''' Invisible
    ''' </summary>
    Invisible

    ''' <summary>
    ''' Fill and clip
    ''' </summary>
    FillClip

    ''' <summary>
    ''' Stroke and clip
    ''' </summary>
    StrokeClip

    ''' <summary>
    ''' Fill, stroke and clip
    ''' </summary>
    FillStrokeClip

    ''' <summary>
    ''' Clip
    ''' </summary>
    Clip
End Enum

''' <summary>
''' Text justify enumeration
''' </summary>
Public Enum TextJustify
    ''' <summary>
    ''' Left
    ''' </summary>
    Left

    ''' <summary>
    ''' Center
    ''' </summary>
    Center

    ''' <summary>
    ''' Right
    ''' </summary>
    Right
End Enum

' text justify
''' <summary>
''' TextBox justify enumeration
''' </summary>
''' <remarks>The first three must be the same as TextJustify
''' </remarks>
Public Enum TextBoxJustify
    ''' <summary>
    ''' Left
    ''' </summary>
    Left

    ''' <summary>
    ''' Center
    ''' </summary>
    Center

    ''' <summary>
    ''' Right
    ''' </summary>
    Right

    ''' <summary>
    ''' Fit to width
    ''' </summary>
    FitToWidth
End Enum

''' <summary>
''' Draw Bezier point one control enumeration
''' </summary>
Public Enum BezierPointOne
    ''' <summary>
    ''' Ignore
    ''' </summary>
    Ignore

    ''' <summary>
    ''' Move to
    ''' </summary>
    MoveTo

    ''' <summary>
    ''' Line to
    ''' </summary>
    LineTo
End Enum

''' <summary>
''' Blend mode enumeration
''' </summary>
''' <remarks>See Blend Mode section of the PDF specifications menual.</remarks>
Public Enum BlendMode
    ''' <summary>
    ''' Normal (no blend)
    ''' </summary>
    Normal
    ''' <summary>
    ''' Multiply
    ''' </summary>
    Multiply
    ''' <summary>
    ''' Screen
    ''' </summary>
    Screen
    ''' <summary>
    ''' Overlay
    ''' </summary>
    Overlay
    ''' <summary>
    ''' Darken
    ''' </summary>
    Darken
    ''' <summary>
    ''' Lighten
    ''' </summary>
    Lighten
    ''' <summary>
    ''' Color Dodge
    ''' </summary>
    ColorDodge
    ''' <summary>
    ''' Color burn
    ''' </summary>
    ColorBurn
    ''' <summary>
    ''' Hard light
    ''' </summary>
    HardLight
    ''' <summary>
    ''' Soft light
    ''' </summary>
    SoftLight
    ''' <summary>
    ''' Difference
    ''' </summary>
    Difference
    ''' <summary>
    ''' Exclusion
    ''' </summary>
    Exclusion
End Enum