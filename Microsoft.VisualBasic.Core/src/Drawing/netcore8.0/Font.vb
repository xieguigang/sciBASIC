#Region "Microsoft.VisualBasic::7544dc3a0fd002abfcec2d3a07bab867, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Font.vb"

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

    '   Total Lines: 173
    '    Code Lines: 107 (61.85%)
    ' Comment Lines: 39 (22.54%)
    '    - Xml Docs: 53.85%
    ' 
    '   Blank Lines: 27 (15.61%)
    '     File Size: 5.55 KB


    '     Class Font
    ' 
    '         Properties: Bold, Height, Italic, Name, Size
    '                     SizeInPoints, Strikeout, Style, Underline, Unit
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Clone, (+2 Overloads) GetHeight
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
        Public ReadOnly Property Height As Single

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

        Sub New(baseFont As Font, style As FontStyle)
            _Name = baseFont.Name
            _Size = baseFont.Size
            _Style = style
            _Unit = baseFont.Unit
        End Sub

        Public Function Clone() As Object
            Return New Font(Name, Size, Style)
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

    Public Class FontFamily

        Public Property Name As String

        Sub New(name As String)
            Me.Name = name
        End Sub
    End Class

    Public Enum StringAlignment
        Center
        Far
        Near
    End Enum
#End If
End Namespace
