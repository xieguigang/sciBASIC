#Region "Microsoft.VisualBasic::50c1453e7e3b59d1a833f3ddcfb97482, mime\application%pdf\PdfFileWriter\PDF\PdfTableBorder.vb"

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

    '   Total Lines: 647
    '    Code Lines: 309
    ' Comment Lines: 259
    '   Blank Lines: 79
    '     File Size: 19.76 KB


    ' Class PdfTableBorderStyle
    ' 
    '     Properties: Color, Display, HalfWidth, Width
    ' 
    '     Constructor: (+4 Overloads) Sub New
    '     Sub: [Set], Clear, Copy
    ' 
    ' Class PdfTableBorder
    ' 
    '     Properties: BottomBorder, CellHorBorder, CellVertBorder, CellVertBorderActive, HeaderHorBorder
    '                 HeaderVertBorder, HeaderVertBorderActive, TopBorder
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: HorizontalBordersTotalWidth
    ' 
    '     Sub: BordersInitialization, ClearAllBorders, ClearBottomBorder, ClearCellHorBorder, ClearCellVertBorder
    '          ClearHeaderHorBorder, ClearHeaderVertBorder, ClearTopBorder, (+4 Overloads) SetAllBorders, (+2 Overloads) SetBottomBorder
    '          (+2 Overloads) SetCellHorBorder, (+2 Overloads) SetCellVertBorder, SetDefaultBorders, (+2 Overloads) SetFrame, (+2 Overloads) SetHeaderHorBorder
    '          (+2 Overloads) SetHeaderVertBorder, (+2 Overloads) SetTopBorder, TestInit
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfTableBorder
'	Data table border lines support.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System
Imports System.Drawing
Imports stdNum = System.Math

''' <summary>
''' Border line style class
''' </summary>
Public Class PdfTableBorderStyle

    ''' <summary>
    ''' Gets display border line flag
    ''' </summary>
    Public Property Display As Boolean

    ''' <summary>
    ''' Gets border line width
    ''' </summary>
    Public Property Width As Double

    ''' <summary>
    ''' Gets border line color
    ''' </summary>
    Public Property Color As Color

    ''' <summary>
    ''' Gets border line half width
    ''' </summary>
    ''' <remarks>
    ''' If display flag is false, the returned value is zero
    ''' </remarks>
    Public ReadOnly Property HalfWidth As Double
        Get
            Return If(Display, 0.5 * Width, 0.0)
        End Get
    End Property

    ''' <summary>
    ''' PdfTableBorderStyle default constructor
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' PdfTableBorderStyle constructor
    ''' </summary>
    ''' <param name="Width">Border line width</param>
    Public Sub New(Width As Double)
        Display = True
        Me.Width = Width
        Color = Color.Black
    End Sub

    ''' <summary>
    ''' PdfTableBorderStyle constructor
    ''' </summary>
    ''' <param name="Width">Border line width</param>
    ''' <param name="Color">Border line color</param>
    Public Sub New(Width As Double, Color As Color)
        Display = True
        Me.Width = Width
        Me.Color = Color
    End Sub

    Sub New(pen As Pen)
        Call Me.New(pen.Width, pen.Color)
    End Sub

    ''' <summary>
    ''' Clear border line style
    ''' </summary>
    Friend Sub Clear()
        Display = False
        Width = 0
        Color = Color.Empty
    End Sub

    ''' <summary>
    ''' Set border line
    ''' </summary>
    ''' <param name="Width">Line width in user units</param>
    ''' <param name="Color">Line color</param>
    Friend Sub [Set](Width As Double, Color As Color)
        Display = True
        Me.Width = Width
        Me.Color = Color
    End Sub

    ''' <summary>
    ''' Copy border line style
    ''' </summary>
    ''' <param name="Other">Border line template</param>
    Friend Sub Copy(Other As PdfTableBorderStyle)
        Display = Other.Display
        Width = Other.Width
        Color = Other.Color
    End Sub
End Class

''' <summary>
''' Table's borders control
''' </summary>
Public Class PdfTableBorder

    ''' <summary>
    ''' At least one cell vertical border is active
    ''' </summary>
    Private _TopBorder As PdfTableBorderStyle, _BottomBorder As PdfTableBorderStyle

    ''' <summary>
    ''' Header horizontal border
    ''' </summary>
    ''' <remarks>
    ''' Border between headers and first row of cells.
    ''' </remarks>
    Dim _HeaderHorBorder As PdfTableBorderStyle, _CellHorBorder As PdfTableBorderStyle, _HeaderVertBorder As PdfTableBorderStyle(), _HeaderVertBorderActive As Boolean, _CellVertBorder As PdfTableBorderStyle(), _CellVertBorderActive As Boolean

    ''' <summary>
    ''' Top border line
    ''' </summary>
    Public Property TopBorder As PdfTableBorderStyle
        Get
            Return _TopBorder
        End Get
        Friend Set(value As PdfTableBorderStyle)
            _TopBorder = value
        End Set
    End Property

    ''' <summary>
    ''' Clear top border line
    ''' </summary>
    Public Sub ClearTopBorder()
        TestInit()
        TopBorder.Clear()
        Return
    End Sub

    ''' <summary>
    ''' Set top border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    Public Sub SetTopBorder(Width As Double)
        TestInit()
        TopBorder.Set(Width, Color.Black)
        Return
    End Sub

    ''' <summary>
    ''' Set top border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    ''' <param name="Color">LineColor</param>
    Public Sub SetTopBorder(Width As Double, Color As Color)
        TestInit()
        TopBorder.Set(Width, Color)
        Return
    End Sub

    ''' <summary>
    ''' Bottom border line
    ''' </summary>
    Public Property BottomBorder As PdfTableBorderStyle
        Get
            Return _BottomBorder
        End Get
        Friend Set(value As PdfTableBorderStyle)
            _BottomBorder = value
        End Set
    End Property

    ''' <summary>
    ''' Clear bottom border line
    ''' </summary>
    Public Sub ClearBottomBorder()
        TestInit()
        BottomBorder.Clear()
        Return
    End Sub

    ''' <summary>
    ''' Set bottom border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    Public Sub SetBottomBorder(Width As Double)
        TestInit()
        BottomBorder.Set(Width, Color.Black)
    End Sub

    ''' <summary>
    ''' Set bottom border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    ''' <param name="Color">LineColor</param>
    Public Sub SetBottomBorder(Width As Double, Color As Color)
        TestInit()
        BottomBorder.Set(Width, Color)
    End Sub

    ''' <summary>
    ''' Header horizontal border
    ''' </summary>
    ''' <remarks>
    ''' Border between headers and first row of cells.
    ''' </remarks>
    Public Property HeaderHorBorder As PdfTableBorderStyle
        Get
            Return _HeaderHorBorder
        End Get
        Friend Set(value As PdfTableBorderStyle)
            _HeaderHorBorder = value
        End Set
    End Property

    ''' <summary>
    ''' Clear header horizontal border line
    ''' </summary>
    Public Sub ClearHeaderHorBorder()
        TestInit()
        HeaderHorBorder.Clear()
    End Sub

    ''' <summary>
    ''' Set header horizontal border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    Public Sub SetHeaderHorBorder(Width As Double)
        TestInit()
        HeaderHorBorder.Set(Width, Color.Black)
    End Sub

    ''' <summary>
    ''' Set header horizontal border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    ''' <param name="Color">LineColor</param>
    Public Sub SetHeaderHorBorder(Width As Double, Color As Color)
        TestInit()
        HeaderHorBorder.Set(Width, Color)
    End Sub

    ''' <summary>
    ''' Cell horizontal border line
    ''' </summary>
    ''' <remarks>
    ''' One border style for all horizontal borders between rows of cells.
    ''' </remarks>
    Public Property CellHorBorder As PdfTableBorderStyle
        Get
            Return _CellHorBorder
        End Get
        Friend Set(value As PdfTableBorderStyle)
            _CellHorBorder = value
        End Set
    End Property

    ''' <summary>
    ''' Clear cell horizontal border line
    ''' </summary>
    Public Sub ClearCellHorBorder()
        TestInit()
        CellHorBorder.Clear()
    End Sub

    ''' <summary>
    ''' Set cell horizontal border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    Public Sub SetCellHorBorder(Width As Double)
        TestInit()
        CellHorBorder.Set(Width, Color.Black)
    End Sub

    ''' <summary>
    ''' Set cell horizontal border line
    ''' </summary>
    ''' <param name="Width">Line width</param>
    ''' <param name="Color">LineColor</param>
    Public Sub SetCellHorBorder(Width As Double, Color As Color)
        TestInit()
        CellHorBorder.Set(Width, Color)
    End Sub

    ''' <summary>
    ''' Array of vertical borders between headers
    ''' </summary>
    ''' <remarks>
    ''' Array of vertical borders between all headers.
    ''' Array's size is Columns + 1.
    ''' Array's item [0] is left border.
    ''' Array's item [Columns] is right border.
    ''' </remarks>
    Public Property HeaderVertBorder As PdfTableBorderStyle()
        Get
            Return _HeaderVertBorder
        End Get
        Friend Set(value As PdfTableBorderStyle())
            _HeaderVertBorder = value
        End Set
    End Property

    ''' <summary>
    ''' Clear header vertical border line
    ''' </summary>
    ''' <param name="Index">Border line index</param>
    Public Sub ClearHeaderVertBorder(Index As Integer)
        TestInit()
        HeaderVertBorder(Index).Clear()
    End Sub

    ''' <summary>
    ''' Set header vertical border line
    ''' </summary>
    ''' <param name="Index">Border line index</param>
    ''' <param name="Width">Line width</param>
    Public Sub SetHeaderVertBorder(Index As Integer, Width As Double)
        TestInit()
        HeaderVertBorder(Index).Set(Width, Color.Black)
    End Sub

    ''' <summary>
    ''' Set header horizontal border line
    ''' </summary>
    ''' <param name="Index">Border line index</param>
    ''' <param name="Width">Line width</param>
    ''' <param name="Color">LineColor</param>
    Public Sub SetHeaderVertBorder(Index As Integer, Width As Double, Color As Color)
        TestInit()
        HeaderVertBorder(Index).Set(Width, Color)
    End Sub

    ''' <summary>
    ''' At least one header vertical border is active
    ''' </summary>
    Public Property HeaderVertBorderActive As Boolean
        Get
            Return _HeaderVertBorderActive
        End Get
        Friend Set(value As Boolean)
            _HeaderVertBorderActive = value
        End Set
    End Property

    ''' <summary>
    ''' Array of vertical borders between cells
    ''' </summary>
    ''' <remarks>
    ''' Array of vertical borders between all cells.
    ''' Array's size is Columns + 1.
    ''' Array's item [0] is left border.
    ''' Array's item [Coloumns] is right border.
    ''' </remarks>
    Public Property CellVertBorder As PdfTableBorderStyle()
        Get
            Return _CellVertBorder
        End Get
        Friend Set(value As PdfTableBorderStyle())
            _CellVertBorder = value
        End Set
    End Property

    ''' <summary>
    ''' Clear cell vertical border line
    ''' </summary>
    ''' <param name="Index">Border line index</param>
    Public Sub ClearCellVertBorder(Index As Integer)
        TestInit()
        CellVertBorder(Index).Clear()
        Return
    End Sub

    ''' <summary>
    ''' Set cell vertical border line
    ''' </summary>
    ''' <param name="Index">Border line index</param>
    ''' <param name="Width">Line width</param>
    Public Sub SetCellVertBorder(Index As Integer, Width As Double)
        TestInit()
        CellVertBorder(Index).Set(Width, Color.Black)
        Return
    End Sub

    ''' <summary>
    ''' Set cell horizontal border line
    ''' </summary>
    ''' <param name="Index">Border line index</param>
    ''' <param name="Width">Line width</param>
    ''' <param name="Color">LineColor</param>
    Public Sub SetCellVertBorder(Index As Integer, Width As Double, Color As Color)
        TestInit()
        CellVertBorder(Index).Set(Width, Color)
        Return
    End Sub

    Public Property CellVertBorderActive As Boolean
        Get
            Return _CellVertBorderActive
        End Get
        Friend Set(value As Boolean)
            _CellVertBorderActive = value
        End Set
    End Property

    Friend Parent As PdfTable
    Friend Document As PdfDocument
    Friend Columns As Integer
    Friend VertBorderHalfWidth As Double()

    Friend Sub New(Parent As PdfTable)
        ' save PdfTable parent and document
        Me.Parent = Parent
        Document = Parent.Document
        Return
    End Sub

    Friend Sub BordersInitialization()
        ' save number of columns
        Columns = Parent.Columns

        ' define horizontal borders
        TopBorder = New PdfTableBorderStyle()
        BottomBorder = New PdfTableBorderStyle()
        HeaderHorBorder = New PdfTableBorderStyle()
        CellHorBorder = New PdfTableBorderStyle()

        ' define vertical border lines
        HeaderVertBorder = New PdfTableBorderStyle(Columns + 1 - 1) {}
        CellVertBorder = New PdfTableBorderStyle(Columns + 1 - 1) {}

        For Index = 0 To Columns
            HeaderVertBorder(Index) = New PdfTableBorderStyle()
            CellVertBorder(Index) = New PdfTableBorderStyle()
        Next

        SetDefaultBorders()
        Return
    End Sub

    ''' <summary>
    ''' Clear all borders
    ''' </summary>
    ''' <remarks>
    ''' The table will be displayed with no borders or gris lines.
    ''' </remarks>
    Public Sub ClearAllBorders()
        ' set is not allowed
        If Parent.Active OrElse TopBorder Is Nothing Then Throw New ApplicationException("Set borders after SetColumnWidth and before table is active.")

        ' clear all horizontal borders
        TopBorder.Clear()
        BottomBorder.Clear()
        HeaderHorBorder.Clear()
        CellHorBorder.Clear()

        ' clear all vertical border lines
        For Index = 0 To Columns
            HeaderVertBorder(Index).Clear()
            CellVertBorder(Index).Clear()
        Next

        Return
    End Sub

    ''' <summary>
    ''' Set all borders to default values.
    ''' </summary>
    ''' <remarks>
    ''' All borders will be black.
    ''' Frame line width is set to one point.
    ''' Grids line width are set to 0.2 of one point
    ''' </remarks>
    Public Sub SetDefaultBorders()
        ' one point or 1/72 inch width
        Dim OnePoint = 1.0 / Document.ScaleFactor
        SetAllBorders(OnePoint, Color.Black, 0.2 * OnePoint, Color.Black)
        Return
    End Sub

    ''' <summary>
    ''' Set all borders to the same line width
    ''' </summary>
    ''' <param name="Width">Border line width</param>
    Public Sub SetAllBorders(Width As Double)
        SetAllBorders(Width, Color.Black, Width, Color.Black)
        Return
    End Sub

    ''' <summary>
    ''' Set all borders to the same line width and color
    ''' </summary>
    ''' <param name="Width">Border line width</param>
    ''' <param name="Color">Border line color</param>
    Public Sub SetAllBorders(Width As Double, Color As Color)
        SetAllBorders(Width, Color, Width, Color)
        Return
    End Sub

    ''' <summary>
    ''' Set all borders
    ''' </summary>
    ''' <param name="FrameWidth">Frame border line width</param>
    ''' <param name="GridWidth">Grid borders line width</param>
    Public Sub SetAllBorders(FrameWidth As Double, GridWidth As Double)
        SetAllBorders(FrameWidth, Color.Black, GridWidth, Color.Black)
        Return
    End Sub

    ''' <summary>
    ''' Set all borders
    ''' </summary>
    ''' <param name="FrameWidth">Frame border line width</param>
    ''' <param name="FrameColor">Frame border color</param>
    ''' <param name="GridWidth">Grid borders line width</param>
    ''' <param name="GridColor">Grid line color</param>
    Public Sub SetAllBorders(FrameWidth As Double, FrameColor As Color, GridWidth As Double, GridColor As Color)
        ' set is not allowed
        If Parent.Active OrElse TopBorder Is Nothing Then Throw New ApplicationException("Set borders after SetColumnWidth and before table is active.")

        ' define default horizontal borders
        TopBorder.Set(FrameWidth, FrameColor)
        BottomBorder.Set(FrameWidth, FrameColor)
        HeaderHorBorder.Set(FrameWidth, FrameColor)
        CellHorBorder.Set(GridWidth, GridColor)

        ' vertical border lines
        HeaderVertBorder(0).Set(FrameWidth, FrameColor)
        CellVertBorder(0).Set(FrameWidth, FrameColor)

        For Index = 1 To Columns - 1
            HeaderVertBorder(Index).Set(GridWidth, GridColor)
            CellVertBorder(Index).Set(GridWidth, GridColor)
        Next

        HeaderVertBorder(Columns).Set(FrameWidth, FrameColor)
        CellVertBorder(Columns).Set(FrameWidth, FrameColor)
        Return
    End Sub

    ''' <summary>
    ''' Set frame border lines
    ''' </summary>
    ''' <param name="FrameWidth">Frame line width</param>
    Public Sub SetFrame(FrameWidth As Double)
        SetFrame(FrameWidth, Color.Black)
        Return
    End Sub

    ''' <summary>
    ''' Set frame border lines
    ''' </summary>
    ''' <param name="FrameWidth">Frame line width</param>
    ''' <param name="FrameColor">Frame line color</param>
    Public Sub SetFrame(FrameWidth As Double, FrameColor As Color)
        ' set is not allowed
        If Parent.Active OrElse TopBorder Is Nothing Then Throw New ApplicationException("Set borders after SetColumnWidth and before table is active.")

        ' define default horizontal borders
        TopBorder.Set(FrameWidth, FrameColor)
        BottomBorder.Set(FrameWidth, FrameColor)
        HeaderHorBorder.Set(FrameWidth, FrameColor)
        CellHorBorder.Clear()

        ' vertical border lines
        HeaderVertBorder(0).Set(FrameWidth, FrameColor)
        CellVertBorder(0).Set(FrameWidth, FrameColor)

        For Index = 1 To Columns - 1
            HeaderVertBorder(Index).Clear()
            CellVertBorder(Index).Clear()
        Next

        HeaderVertBorder(Columns).Set(FrameWidth, FrameColor)
        CellVertBorder(Columns).Set(FrameWidth, FrameColor)
        Return
    End Sub

    Friend Function HorizontalBordersTotalWidth() As Double
        ' look for at least one header vertical border
        For Index = 0 To Columns

            If HeaderVertBorder(Index).Display Then
                HeaderVertBorderActive = True
                Exit For
            End If
        Next

        ' look for at least one cell vertical border
        For Index = 0 To Columns

            If CellVertBorder(Index).Display Then
                CellVertBorderActive = True
                Exit For
            End If
        Next

        ' allocate array of overall half width
        VertBorderHalfWidth = New Double(Columns + 1 - 1) {}

        ' we have at least one vertical border
        If HeaderVertBorderActive OrElse CellVertBorderActive Then
            For Index = 0 To Columns
                VertBorderHalfWidth(Index) = stdNum.Max(HeaderVertBorder(Index).HalfWidth, CellVertBorder(Index).HalfWidth)
            Next

            Dim TotalWidth = VertBorderHalfWidth(0) + VertBorderHalfWidth(Columns)

            For Index = 1 To Columns - 1
                TotalWidth += 2.0 * VertBorderHalfWidth(Index)
            Next

            Return TotalWidth
        End If

        ' no borders
        Return 0.0
    End Function

    Friend Sub TestInit()
        ' set is not allowed
        If Parent.Active OrElse TopBorder Is Nothing Then Throw New ApplicationException("Set borders after SetColumnWidth and before table is active.")
        Return
    End Sub
End Class
