#Region "Microsoft.VisualBasic::4c2533b06f085fac445cdeb88c40c91c, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Font.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 202
    '    Code Lines: 130 (64.36%)
    ' Comment Lines: 39 (19.31%)
    '    - Xml Docs: 53.85%
    ' 
    '   Blank Lines: 33 (16.34%)
    '     File Size: 6.58 KB


    '     Class Font
    ' 
    '         Properties: Bold, Height, Italic, Name, Size
    '                     SizeInPoints, Strikeout, Style, Underline, Unit
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Clone, (+2 Overloads) GetHeight, ToHfont
    ' 
    '     Enum GraphicsUnit
    ' 
    '         Display, Document, Inch, Millimeter, Pixel
    '         Point, World
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum FontStyle
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class StringFormat
    ' 
    '         Properties: Alignment, GenericTypographic, LineAlignment
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    '     Class FontFamily
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetCellAscent, GetCellDescent, GetEmHeight, GetLineSpacing, IsStyleAvailable
    ' 
    '     Enum StringAlignment
    ' 
    '         Center, Far, Near
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Driver

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then
    Public Class Font

        Public ReadOnly Property Name As String
        Public ReadOnly Property Size As Single
        Public ReadOnly Property SizeInPoints As Single
            Get
                Return Size * 0.75F
            End Get
        End Property
        Public ReadOnly Property Style As FontStyle
        Public ReadOnly Property Unit As GraphicsUnit

        ''' <summary>
        ''' The line spacing, in pixels, of this font: it is derived from the
        ''' design grid of <see cref="Imaging.FontFamily"/> instead of being a
        ''' constant, so that it stays consistent with
        ''' <see cref="FontFamily.GetLineSpacing(FontStyle)"/>.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Height As Single
            Get
                Dim family As FontFamily = Me.FontFamily

                Return Size * family.GetLineSpacing(Style) / family.GetEmHeight(Style)
            End Get
        End Property

        ''' <summary>
        ''' The font family that this font belongs to (the GDI+ code of the html
        ''' layout engine reads the typographic metrics through this property).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FontFamily As FontFamily
            Get
                If _fontFamily Is Nothing Then
                    _fontFamily = New FontFamily(Name)
                End If

                Return _fontFamily
            End Get
        End Property

        ''' <summary>
        ''' the cached font family object of this font
        ''' </summary>
        Dim _fontFamily As FontFamily = Nothing

        ''' <summary>
        ''' Gets a value indicating whether this font is bold.
        ''' </summary>
        Public ReadOnly Property Bold As Boolean
            Get
                Return (Style And FontStyle.Bold) <> 0
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this font is italic.
        ''' </summary>
        Public ReadOnly Property Italic As Boolean
            Get
                Return (Style And FontStyle.Italic) <> 0
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this font is underlined.
        ''' </summary>
        Public ReadOnly Property Underline As Boolean
            Get
                Return (Style And FontStyle.Underline) <> 0
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this font is struck out.
        ''' </summary>
        Public ReadOnly Property Strikeout As Boolean
            Get
                Return (Style And FontStyle.Strikeout) <> 0
            End Get
        End Property

        Sub New(familyName As String, emSize As Single, Optional style As FontStyle = FontStyle.Regular, Optional unit As GraphicsUnit = GraphicsUnit.Pixel)
            Me.Name = familyName
            Me.Size = emSize
            Me.Style = style
            Me.Unit = unit
        End Sub

        Sub New(font As FontFamily, designHeight As Integer, fontStyle As FontStyle, unit As GraphicsUnit)
            Me.Name = font.Name
            Me.Size = designHeight
            Me.Style = fontStyle
            Me.Unit = unit
            Me._fontFamily = font
        End Sub

        Sub New(baseFont As Font, style As FontStyle)
            _Name = baseFont.Name
            _Size = baseFont.Size
            _Style = style
            _Unit = baseFont.Unit
            _fontFamily = baseFont._fontFamily
        End Sub

        Public Function Clone() As Object
            Return New Font(Name, Size, Style, Unit)
        End Function

        Public Function GetHeight(g As IGraphics) As Single
            Return DriverLoad.MeasureTextSize("A", Me).Height
        End Function

        ''' <summary>
        ''' Returns the line spacing, in pixels, of this font (uses default 1.0 graphics unit factor).
        ''' </summary>
        Public Function GetHeight() As Single
            Return Size * 1.3F
        End Function

        Public Function ToHfont() As IntPtr
            Throw New NotImplementedException()
        End Function
    End Class

    Public Enum GraphicsUnit
        '     Specifies the world coordinate system unit as the unit of measure.
        World
        '     Specifies the unit of measure of the display device. Typically pixels for video
        '     displays, And 1/100 inch for printers.
        Display
        '     Specifies a device pixel as the unit of measure.
        Pixel
        '     Specifies a printer's point (1/72 inch) as the unit of measure.
        Point
        '     Specifies the inch as the unit of measure.
        Inch
        '     Specifies the document unit (1/300 inch) as the unit of measure.
        Document
        ' Specifies the millimeter as the unit of measure.
        Millimeter
    End Enum

    <Flags>
    Public Enum FontStyle
        ''' <summary>
        ''' css normal
        ''' </summary>
        Regular = 0
        ''' <summary>
        ''' css strongs
        ''' </summary>
        Bold = 1
        Italic = 2
        Underline = 4
        Strikeout = 8
    End Enum

    Public Class StringFormat : Implements IDisposable

        Private disposedValue As Boolean

        Public Property Alignment As StringAlignment
        Public Property LineAlignment As StringAlignment

        Public Shared ReadOnly Property GenericTypographic As StringFormat
            Get
                Return New StringFormat
            End Get
        End Property

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

    ''' <summary>
    ''' A cross platform replacement of the GDI+ <c>System.Drawing.FontFamily</c>
    ''' </summary>
    ''' <remarks>
    ''' The typographic metrics that are exposed here are expressed in the design
    ''' units of a typical TrueType font: the em square is split into 2048 units,
    ''' the ascender takes 1854 of them and the descender 434, the line gap is 67
    ''' units so that the line spacing becomes ``1854 + 434 + 67 = 2355`` units.
    ''' 
    ''' Because all of the values share the very same design grid, the classic
    ''' formula that is used by the html layout engine
    ''' (<c>font.Size * GetCellAscent(style) / GetEmHeight(style)</c>) keeps working
    ''' on every platform without any change of the caller code.
    ''' </remarks>
    Public Class FontFamily

        ''' <summary>
        ''' the units per em square of a typical TrueType font
        ''' </summary>
        Public Const UnitsPerEm As Integer = 2048
        ''' <summary>
        ''' the ascender of a typical TrueType font, in design units
        ''' </summary>
        Public Const DesignAscender As Integer = 1854
        ''' <summary>
        ''' the descender of a typical TrueType font, in design units (a positive value)
        ''' </summary>
        Public Const DesignDescender As Integer = 434
        ''' <summary>
        ''' the line gap of a typical TrueType font, in design units
        ''' </summary>
        Public Const DesignLineGap As Integer = 67

        Public Property Name As String

        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Function IsStyleAvailable(fontStyle As FontStyle) As Boolean
            Return True
        End Function

        ''' <summary>
        ''' Gets the height, in design units, of the em square for the specified style.
        ''' </summary>
        Public Function GetEmHeight(fontStyle As FontStyle) As Integer
            Return UnitsPerEm
        End Function

        ''' <summary>
        ''' Gets the cell ascent, in design units, of this font family.
        ''' </summary>
        Public Function GetCellAscent(fontStyle As FontStyle) As Integer
            Return DesignAscender
        End Function

        ''' <summary>
        ''' Gets the cell descent, in design units, of this font family.
        ''' </summary>
        Public Function GetCellDescent(fontStyle As FontStyle) As Integer
            Return DesignDescender
        End Function

        ''' <summary>
        ''' Gets the distance, in design units, between two consecutive baselines.
        ''' </summary>
        Public Function GetLineSpacing(fontStyle As FontStyle) As Integer
            Return DesignAscender + DesignDescender + DesignLineGap
        End Function
    End Class

    Public Enum StringAlignment
        Center
        Far
        Near
    End Enum
#End If
End Namespace
