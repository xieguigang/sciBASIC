Imports System
Imports System.Globalization
Imports System.Text

Namespace GgplotTheme

    '==========================================================================
    ' UnitType Enumeration
    '==========================================================================
    ''' <summary>
    ''' Supported unit types for theme measurements, mirroring R's grid unit() system.
    ''' These correspond to the units used in ggplot2 theme specifications.
    ''' </summary>
    Public Enum UnitType
        ''' <summary>Centimeters - absolute physical unit (1cm = 1/2.54 inch)</summary>
        Cm
        ''' <summary>Millimeters - absolute physical unit (1mm = 1/25.4 inch)</summary>
        Mm
        ''' <summary>Points - absolute unit (1pt = 1/72 inch)</summary>
        Pt
        ''' <summary>Lines of text - relative to current font size and line height</summary>
        Lines
        ''' <summary>Character width - relative to current font size (width of '0')</summary>
        [Char]
        ''' <summary>Null unit - represents no value / not set</summary>
        [Null]
        ''' <summary>NPC - Normalized Parent Coordinates (0.0 to 1.0 relative to parent viewport)</summary>
        Npc
        ''' <summary>Inches - absolute physical unit</summary>
        Inch
        ''' <summary>Pixels - device units</summary>
        Px
        ''' <summary>Relative unit - multiplier of the inherited value (ggplot's rel())</summary>
        Rel
    End Enum

    '==========================================================================
    ' Unit Class
    '==========================================================================
    ''' <summary>
    ''' Represents a measurement with a unit, equivalent to R's grid::unit().
    ''' Each Unit holds a numeric value and a UnitType.
    ''' Units are converted to pixels by the UnitConverter based on canvas context.
    ''' </summary>
    <Serializable>
    Public Class Unit
        Implements ICloneable
        Implements IComparable(Of Unit)

        Private _value As Double
        Private _type As UnitType

        ''' <summary>Creates a null unit (value=0, type=Null).</summary>
        Public Sub New()
            _value = 0.0
            _type = UnitType.Null
        End Sub

        ''' <summary>Creates a unit with the specified value and type.</summary>
        Public Sub New(value As Double, type As UnitType)
            _value = value
            _type = type
        End Sub

        ''' <summary>The numeric magnitude of the unit.</summary>
        Public Property Value As Double
            Get
                Return _value
            End Get
            Set(value As Double)
                _value = value
            End Set
        End Property

        ''' <summary>The unit type (cm, mm, pt, lines, char, null, npc, inch, px, rel).</summary>
        Public Property Type As UnitType
            Get
                Return _type
            End Get
            Set(value As UnitType)
                _type = value
            End Set
        End Property

        ''' <summary>Returns True if this unit is a null (unset) unit.</summary>
        Public Function IsNull() As Boolean
            Return _type = UnitType.Null
        End Function

        ''' <summary>Returns True if this unit is a relative (rel) unit.</summary>
        Public Function IsRelative() As Boolean
            Return _type = UnitType.Rel
        End Function

        ''' <summary>
        ''' Resolves a relative (rel) unit against a parent unit value.
        ''' If this unit is rel, the result is (this.Value * parent.Value) with parent's type.
        ''' Otherwise, returns this unit unchanged.
        ''' </summary>
        Public Function ResolveRelative(parent As Unit) As Unit
            If Me.IsRelative() AndAlso parent IsNot Nothing AndAlso Not parent.IsNull() Then
                Return New Unit(_value * parent.Value, parent.Type)
            End If
            Return Me
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Unit(_value, _type)
        End Function

        Public Function CompareTo(other As Unit) As Integer Implements IComparable(Of Unit).CompareTo
            If other Is Nothing Then Return 1
            Dim c As Integer = _type.CompareTo(other._type)
            If c <> 0 Then Return c
            Return _value.CompareTo(other._value)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim u As Unit = TryCast(obj, Unit)
            If u Is Nothing Then Return False
            Return _type = u._type AndAlso _value = u._value
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (_type.ToString() & ":" & _value.ToString(CultureInfo.InvariantCulture)).GetHashCode()
        End Function

        Public Overrides Function ToString() As String
            If _type = UnitType.Null Then Return "null"
            Return _value.ToString("G", CultureInfo.InvariantCulture) & UnitTypeToString(_type)
        End Function

        ''' <summary>Converts a UnitType to its string representation.</summary>
        Public Shared Function UnitTypeToString(type As UnitType) As String
            Select Case type
                Case UnitType.Cm : Return "cm"
                Case UnitType.Mm : Return "mm"
                Case UnitType.Pt : Return "pt"
                Case UnitType.Lines : Return "lines"
                Case UnitType.Char : Return "char"
                Case UnitType.Null : Return "null"
                Case UnitType.Npc : Return "npc"
                Case UnitType.Inch : Return "inch"
                Case UnitType.Px : Return "px"
                Case UnitType.Rel : Return "rel"
                Case Else : Return "null"
            End Select
        End Function

        ''' <summary>
        ''' Parses a unit string (e.g., "0.25cm", "11pt", "0.5npc", "null") into a Unit.
        ''' Supports all ggplot unit types. Bare numbers default to npc.
        ''' </summary>
        Public Shared Function Parse(s As String) As Unit
            If String.IsNullOrWhiteSpace(s) Then Return New Unit(0, UnitType.Null)

            s = s.Trim().ToLowerInvariant()
            If s = "null" OrElse s = "na" OrElse s = "none" Then
                Return New Unit(0, UnitType.Null)
            End If

            ' Handle rel() syntax: rel(0.5)
            If s.StartsWith("rel(") AndAlso s.EndsWith(")") Then
                Dim inner As String = s.Substring(4, s.Length - 5).Trim()
                Dim relVal As Double
                If Double.TryParse(inner, NumberStyles.Float, CultureInfo.InvariantCulture, relVal) Then
                    Return New Unit(relVal, UnitType.Rel)
                End If
            End If

            ' Handle unit() syntax: unit(0.25, "cm")
            If s.StartsWith("unit(") AndAlso s.EndsWith(")") Then
                Dim inner As String = s.Substring(5, s.Length - 6).Trim()
                Dim parts() As String = inner.Split(","c)
                If parts.Length = 2 Then
                    Dim valu As Double
                    Dim unitStrq As String = parts(1).Trim().Trim(""""c, "'"c)
                    If Double.TryParse(parts(0).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, valu) Then
                        Dim ut As UnitType = StringToUnitType(unitStrq)
                        Return New Unit(valu, ut)
                    End If
                End If
            End If

            ' Extract numeric part and unit part
            Dim numEnd As Integer = 0
            Dim negSign As Boolean = False
            If numEnd < s.Length AndAlso (s(numEnd) = "-"c OrElse s(numEnd) = "+"c) Then
                numEnd += 1
            End If
            While numEnd < s.Length
                Dim ch As Char = s(numEnd)
                If Char.IsDigit(ch) OrElse ch = "."c OrElse ch = "e"c OrElse ch = "E"c Then
                    numEnd += 1
                ElseIf (ch = "-"c OrElse ch = "+"c) AndAlso numEnd > 0 AndAlso (s(numEnd - 1) = "e"c OrElse s(numEnd - 1) = "E"c) Then
                    numEnd += 1
                Else
                    Exit While
                End If
            End While

            Dim numStr As String = s.Substring(0, numEnd).Trim()
            Dim unitStr As String = s.Substring(numEnd).Trim()

            If String.IsNullOrEmpty(numStr) Then
                Throw New FormatException($"Cannot parse unit value from: '{s}'")
            End If

            Dim val As Double
            If Not Double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, val) Then
                Throw New FormatException($"Cannot parse numeric value from: '{numStr}' in '{s}'")
            End If

            Dim ut2 As UnitType
            If String.IsNullOrEmpty(unitStr) Then
                ut2 = UnitType.Npc ' bare numbers default to npc
            Else
                ut2 = StringToUnitType(unitStr)
            End If

            Return New Unit(val, ut2)
        End Function

        ''' <summary>Converts a unit string to UnitType.</summary>
        Public Shared Function StringToUnitType(s As String) As UnitType
            If String.IsNullOrEmpty(s) Then Return UnitType.Npc
            s = s.Trim().ToLowerInvariant()
            Select Case s
                Case "cm" : Return UnitType.Cm
                Case "mm" : Return UnitType.Mm
                Case "pt", "point", "points" : Return UnitType.Pt
                Case "lines", "line" : Return UnitType.Lines
                Case "char", "chars" : Return UnitType.Char
                Case "null", "na" : Return UnitType.Null
                Case "npc" : Return UnitType.Npc
                Case "inch", "in", "inches" : Return UnitType.Inch
                Case "px", "pixel", "pixels" : Return UnitType.Px
                Case "rel" : Return UnitType.Rel
                Case Else : Throw New FormatException($"Unknown unit type: '{s}'")
            End Select
        End Function

        '--- Factory methods for convenient construction ---
        Public Shared Function Cm(value As Double) As Unit
            Return New Unit(value, UnitType.Cm)
        End Function

        Public Shared Function Mm(value As Double) As Unit
            Return New Unit(value, UnitType.Mm)
        End Function

        Public Shared Function Pt(value As Double) As Unit
            Return New Unit(value, UnitType.Pt)
        End Function

        Public Shared Function Lines(value As Double) As Unit
            Return New Unit(value, UnitType.Lines)
        End Function

        Public Shared Function [Char](value As Double) As Unit
            Return New Unit(value, UnitType.Char)
        End Function

        Public Shared Function Null() As Unit
            Return New Unit(0, UnitType.Null)
        End Function

        Public Shared Function Npc(value As Double) As Unit
            Return New Unit(value, UnitType.Npc)
        End Function

        Public Shared Function Inch(value As Double) As Unit
            Return New Unit(value, UnitType.Inch)
        End Function

        Public Shared Function Px(value As Double) As Unit
            Return New Unit(value, UnitType.Px)
        End Function

        Public Shared Function Rel(value As Double) As Unit
            Return New Unit(value, UnitType.Rel)
        End Function
    End Class

    '==========================================================================
    ' Margin Class
    '==========================================================================
    ''' <summary>
    ''' Represents a four-sided margin (top, right, bottom, left).
    ''' Equivalent to ggplot2's margin() function.
    ''' Each side is a Unit, allowing mixed units.
    ''' </summary>
    <Serializable>
    Public Class Margin
        Implements ICloneable

        Private _top As Unit
        Private _right As Unit
        Private _bottom As Unit
        Private _left As Unit

        Public Sub New()
            _top = New Unit(0, UnitType.Pt)
            _right = New Unit(0, UnitType.Pt)
            _bottom = New Unit(0, UnitType.Pt)
            _left = New Unit(0, UnitType.Pt)
        End Sub

        Public Sub New(t As Double, r As Double, b As Double, l As Double, type As UnitType)
            _top = New Unit(t, type)
            _right = New Unit(r, type)
            _bottom = New Unit(b, type)
            _left = New Unit(l, type)
        End Sub

        Public Sub New(all As Unit)
            If all Is Nothing Then all = New Unit(0, UnitType.Pt)
            _top = CType(all.Clone(), Unit)
            _right = CType(all.Clone(), Unit)
            _bottom = CType(all.Clone(), Unit)
            _left = CType(all.Clone(), Unit)
        End Sub

        Public Property Top As Unit
            Get
                Return _top
            End Get
            Set(value As Unit)
                _top = value
            End Set
        End Property

        Public Property Right As Unit
            Get
                Return _right
            End Get
            Set(value As Unit)
                _right = value
            End Set
        End Property

        Public Property Bottom As Unit
            Get
                Return _bottom
            End Get
            Set(value As Unit)
                _bottom = value
            End Set
        End Property

        Public Property Left As Unit
            Get
                Return _left
            End Get
            Set(value As Unit)
                _left = value
            End Set
        End Property

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim m As New Margin()
            m._top = If(_top?.Clone(), Nothing)
            m._right = If(_right?.Clone(), Nothing)
            m._bottom = If(_bottom?.Clone(), Nothing)
            m._left = If(_left?.Clone(), Nothing)
            Return m
        End Function

        Public Overrides Function ToString() As String
            Return $"{_top} {_right} {_bottom} {_left}"
        End Function

        ''' <summary>
        ''' Parses a margin from a string like "5pt 5pt 5pt 5pt" (top right bottom left)
        ''' or "5pt" (all sides) or "5pt 10pt" (top/bottom left/right).
        ''' </summary>
        Public Shared Function Parse(s As String) As Margin
            If String.IsNullOrWhiteSpace(s) Then Return New Margin()
            s = s.Trim()
            If s.StartsWith("margin(", StringComparison.OrdinalIgnoreCase) Then
                s = s.Substring(7)
                If s.EndsWith(")") Then s = s.Substring(0, s.Length - 1)
            End If

            Dim parts() As String = s.Split(New Char() {" "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)
            Dim margin As New Margin()

            Select Case parts.Length
                Case 1
                    Dim u As Unit = Unit.Parse(parts(0))
                    margin._top = CType(u.Clone(), Unit)
                    margin._right = CType(u.Clone(), Unit)
                    margin._bottom = CType(u.Clone(), Unit)
                    margin._left = CType(u.Clone(), Unit)
                Case 2
                    Dim tb As Unit = Unit.Parse(parts(0))
                    Dim rl As Unit = Unit.Parse(parts(1))
                    margin._top = CType(tb.Clone(), Unit)
                    margin._bottom = CType(tb.Clone(), Unit)
                    margin._right = CType(rl.Clone(), Unit)
                    margin._left = CType(rl.Clone(), Unit)
                Case 3
                    margin._top = Unit.Parse(parts(0))
                    margin._right = Unit.Parse(parts(1))
                    margin._bottom = Unit.Parse(parts(2))
                    margin._left = CType(margin._right.Clone(), Unit)
                Case 4
                    margin._top = Unit.Parse(parts(0))
                    margin._right = Unit.Parse(parts(1))
                    margin._bottom = Unit.Parse(parts(2))
                    margin._left = Unit.Parse(parts(3))
                Case Else
                    Throw New FormatException($"Cannot parse margin from: '{s}'")
            End Select

            Return margin
        End Function
    End Class

    '==========================================================================
    ' CanvasContext Class
    '==========================================================================
    ''' <summary>
    ''' Provides canvas environment information needed for unit-to-pixel conversion.
    ''' Contains canvas dimensions, PPI (pixels per inch), and base font metrics.
    ''' </summary>
    <Serializable>
    Public Class CanvasContext
        Implements ICloneable

        Private _widthPx As Double = 800.0
        Private _heightPx As Double = 600.0
        Private _ppi As Double = 96.0
        Private _baseFontSizePt As Double = 11.0
        Private _lineHeight As Double = 1.2

        ''' <summary>Canvas width in pixels.</summary>
        Public Property WidthPx As Double
            Get
                Return _widthPx
            End Get
            Set(value As Double)
                _widthPx = value
            End Set
        End Property

        ''' <summary>Canvas height in pixels.</summary>
        Public Property HeightPx As Double
            Get
                Return _heightPx
            End Get
            Set(value As Double)
                _heightPx = value
            End Set
        End Property

        ''' <summary>Pixels per inch (DPI). Default is 96 (standard screen DPI).</summary>
        Public Property Ppi As Double
            Get
                Return _ppi
            End Get
            Set(value As Double)
                _ppi = value
            End Set
        End Property

        ''' <summary>Base font size in points, used for 'lines' and 'char' unit conversion.</summary>
        Public Property BaseFontSizePt As Double
            Get
                Return _baseFontSizePt
            End Get
            Set(value As Double)
                _baseFontSizePt = value
            End Set
        End Property

        ''' <summary>Line height multiplier, used for 'lines' unit conversion.</summary>
        Public Property LineHeight As Double
            Get
                Return _lineHeight
            End Get
            Set(value As Double)
                _lineHeight = value
            End Set
        End Property

        Public Sub New()
        End Sub

        Public Sub New(widthPx As Double, heightPx As Double, ppi As Double, Optional baseFontSize As Double = 11, Optional lineHeight As Double = 1.2)
            _widthPx = widthPx
            _heightPx = heightPx
            _ppi = ppi
            _baseFontSizePt = baseFontSize
            _lineHeight = lineHeight
        End Sub

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New CanvasContext() With {
                .WidthPx = _widthPx,
                .HeightPx = _heightPx,
                .Ppi = _ppi,
                .BaseFontSizePt = _baseFontSizePt,
                .LineHeight = _lineHeight
            }
        End Function
    End Class

    '==========================================================================
    ' UnitConverter Class
    '==========================================================================
    ''' <summary>
    ''' Converts Unit values to pixels based on a CanvasContext.
    ''' Handles all ggplot unit types: cm, mm, pt, lines, char, null, npc, inch, px.
    ''' For relative units (rel), resolve against parent value before calling ToPixels.
    ''' </summary>
    Public Class UnitConverter

        ' Standard physical conversion constants
        Public Const PtPerInch As Double = 72.0
        Public Const CmPerInch As Double = 2.54
        Public Const MmPerInch As Double = 25.4

        Private _canvas As CanvasContext

        Public Sub New(canvas As CanvasContext)
            If canvas Is Nothing Then Throw New ArgumentNullException(NameOf(canvas))
            _canvas = canvas
        End Sub

        Public ReadOnly Property Canvas As CanvasContext
            Get
                Return _canvas
            End Get
        End Property

        ''' <summary>
        ''' Converts a Unit to pixels. For NPC units, uses the larger of parent width/height.
        ''' Use ToPixelsX or ToPixelsY for axis-specific NPC conversion.
        ''' </summary>
        ''' <param name="unit">The unit to convert. If Nothing or Null, returns 0.</param>
        ''' <param name="fontSizePt">Font size in points for 'lines'/'char' conversion. If <= 0, uses canvas base font size.</param>
        ''' <param name="parentWidthPx">Parent width in pixels for NPC. If <= 0, uses canvas width.</param>
        ''' <param name="parentHeightPx">Parent height in pixels for NPC. If <= 0, uses canvas height.</param>
        Public Function ToPixels(unit As Unit,
                                 Optional fontSizePt As Double = -1,
                                 Optional parentWidthPx As Double = -1,
                                 Optional parentHeightPx As Double = -1) As Double
            If unit Is Nothing OrElse unit.IsNull() Then Return 0.0

            Dim fs As Double = If(fontSizePt > 0, fontSizePt, _canvas.BaseFontSizePt)
            Dim pw As Double = If(parentWidthPx > 0, parentWidthPx, _canvas.WidthPx)
            Dim ph As Double = If(parentHeightPx > 0, parentHeightPx, _canvas.HeightPx)

            Select Case unit.Type
                Case UnitType.Px
                    Return unit.Value
                Case UnitType.Inch
                    Return unit.Value * _canvas.Ppi
                Case UnitType.Cm
                    Return unit.Value * _canvas.Ppi / CmPerInch
                Case UnitType.Mm
                    Return unit.Value * _canvas.Ppi / MmPerInch
                Case UnitType.Pt
                    Return unit.Value * _canvas.Ppi / PtPerInch
                Case UnitType.Npc
                    ' NPC is relative to parent viewport. Use max dimension as default.
                    Return unit.Value * Math.Max(pw, ph)
                Case UnitType.Lines
                    ' 1 line = font size (in points) * line height, converted to pixels
                    Dim lineSizePx As Double = fs * _canvas.Ppi / PtPerInch
                    Return unit.Value * lineSizePx * _canvas.LineHeight
                Case UnitType.Char
                    ' 1 char ≈ 0.5 * font size (width of '0' character), converted to pixels
                    Dim charSizePx As Double = fs * 0.5 * _canvas.Ppi / PtPerInch
                    Return unit.Value * charSizePx
                Case UnitType.Rel
                    ' Relative units should be resolved against parent before calling this.
                    ' If we get here, treat the value as a direct pixel multiplier.
                    Return unit.Value
                Case Else
                    Return 0.0
            End Select
        End Function

        ''' <summary>
        ''' Converts a Unit to pixels using width context (for horizontal measurements).
        ''' NPC units are resolved against parent width.
        ''' </summary>
        Public Function ToPixelsX(unit As Unit,
                                  Optional fontSizePt As Double = -1,
                                  Optional parentWidthPx As Double = -1) As Double
            If unit Is Nothing OrElse unit.IsNull() Then Return 0.0
            If unit.Type = UnitType.Npc Then
                Dim pw As Double = If(parentWidthPx > 0, parentWidthPx, _canvas.WidthPx)
                Return unit.Value * pw
            End If
            Return ToPixels(unit, fontSizePt, parentWidthPx, parentWidthPx)
        End Function

        ''' <summary>
        ''' Converts a Unit to pixels using height context (for vertical measurements).
        ''' NPC units are resolved against parent height.
        ''' </summary>
        Public Function ToPixelsY(unit As Unit,
                                  Optional fontSizePt As Double = -1,
                                  Optional parentHeightPx As Double = -1) As Double
            If unit Is Nothing OrElse unit.IsNull() Then Return 0.0
            If unit.Type = UnitType.Npc Then
                Dim ph As Double = If(parentHeightPx > 0, parentHeightPx, _canvas.HeightPx)
                Return unit.Value * ph
            End If
            Return ToPixels(unit, fontSizePt, parentHeightPx, parentHeightPx)
        End Function

        ''' <summary>Converts a Margin to pixel values (top, right, bottom, left).</summary>
        Public Function MarginToPixels(margin As Margin,
                                       Optional fontSizePt As Double = -1,
                                       Optional parentWidthPx As Double = -1,
                                       Optional parentHeightPx As Double = -1) As (Top As Double, Right As Double, Bottom As Double, Left As Double)
            If margin Is Nothing Then
                Return (0, 0, 0, 0)
            End If
            Dim t As Double = ToPixelsY(margin.Top, fontSizePt, parentHeightPx)
            Dim r As Double = ToPixelsX(margin.Right, fontSizePt, parentWidthPx)
            Dim b As Double = ToPixelsY(margin.Bottom, fontSizePt, parentHeightPx)
            Dim l As Double = ToPixelsX(margin.Left, fontSizePt, parentWidthPx)
            Return (t, r, b, l)
        End Function

        ''' <summary>Converts points to pixels.</summary>
        Public Function PtToPx(pt As Double) As Double
            Return pt * _canvas.Ppi / PtPerInch
        End Function

        ''' <summary>Converts pixels to points.</summary>
        Public Function PxToPt(px As Double) As Double
            Return px * PtPerInch / _canvas.Ppi
        End Function
    End Class

End Namespace
