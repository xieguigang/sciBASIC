#Region "Microsoft.VisualBasic::951bf83c7d4659006bcc88e52d9a7a9f, Data_science\Graph\Model\GridGraph\Grid2D.vb"

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

    '   Total Lines: 341
    '    Code Lines: 203 (59.53%)
    ' Comment Lines: 97 (28.45%)
    '    - Xml Docs: 96.91%
    ' 
    '   Blank Lines: 41 (12.02%)
    '     File Size: 13.25 KB


    '     Class Grid
    ' 
    '         Properties: height, rectangle, size, width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Blank, Cells, Check, (+4 Overloads) Create, (+2 Overloads) CreateReadOnly
    '                   EnumerateData, GetData, LineScans, (+2 Overloads) Query, ShuffleAll
    ' 
    '         Sub: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

Namespace GridGraph

    ''' <summary>
    ''' a generic grid graph for fast query of the 2D geometry data
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Grid(Of T)

        ''' <summary>
        ''' [x => [y => pixel]]
        ''' </summary>
        ReadOnly matrix2D As Dictionary(Of Long, Dictionary(Of Long, GridCell(Of T)))
        ReadOnly toPoint As Func(Of T, Point)

        Public ReadOnly Property rectangle As Rectangle
            Get
                Dim points As Layout2D() = matrix2D.Values _
                    .Select(Function(a) a.Values) _
                    .IteratesALL _
                    .Select(Function(c) DirectCast(c, Layout2D)) _
                    .ToArray
                Dim rectf = New Polygon2D(points).GetRectangle

                Return New Rectangle(rectf.X, rectf.Y, rectf.Width, rectf.Height)
            End Get
        End Property

        ''' <summary>
        ''' get the max value of x axis
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' this assuming that all x axis value is positive
        ''' </remarks>
        Public ReadOnly Property width As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return matrix2D.Keys.Max
            End Get
        End Property

        ''' <summary>
        ''' get the max value of y axis
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' this assuming that all y axis value is positive
        ''' </remarks>
        Public ReadOnly Property height As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Aggregate d As Dictionary(Of Long, GridCell(Of T))
                   In matrix2D.Values
                   Into Max(d.Keys.Max)
            End Get
        End Property

        ''' <summary>
        ''' get target cell data via a given pixel point
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns>
        ''' nothing will be returns if there is no data on the given ``[x,y]`` pixel point.
        ''' </returns>
        Default Public ReadOnly Property GetPoint(x As Integer, y As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetData(x, y)
            End Get
        End Property

        ''' <summary>
        ''' counts of all non-empty cell.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Aggregate row
                   In matrix2D
                   Let rowCount = row.Value.Count
                   Into Sum(rowCount)
            End Get
        End Property

        ''' <summary>
        ''' this constructor will removed duplicated pixels
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="toPoint">
        ''' just works for the <see cref="Add"/> method, this 
        ''' parameter can be omit is the grid is readonly.
        ''' </param>
        Private Sub New(points As IEnumerable(Of GridCell(Of T)), toPoint As Func(Of T, Point))
            Me.toPoint = toPoint
            Me.matrix2D = points _
                .GroupBy(Function(d) d.index.X) _
                .ToDictionary(Function(d) CLng(d.Key),
                              Function(d)
                                  Return d _
                                      .GroupBy(Function(p) p.index.Y) _
                                      .ToDictionary(Function(p) CLng(p.Key),
                                                    Function(p)
                                                        Return p.First
                                                    End Function)
                              End Function)
        End Sub

        Public Function Cells() As IEnumerable(Of GridCell(Of T))
            Return matrix2D _
                .Select(Function(row) row.Value.Values) _
                .IteratesALL
        End Function

        Public Sub Add(point As T)
            Dim xy As Point = toPoint(point)
            Dim xi As Long = xy.X
            Dim yi As Long = xy.Y

            If Not matrix2D.ContainsKey(xi) Then
                matrix2D.Add(xi, New Dictionary(Of Long, GridCell(Of T)))
            End If

            If Not matrix2D(key:=xi).ContainsKey(yi) Then
                matrix2D(key:=xi).Add(yi, New GridCell(Of T)(xy.X, xy.Y, point))
            Else
                matrix2D(key:=xi)(key:=yi) = New GridCell(Of T)(xy.X, xy.Y, point)
            End If
        End Sub

        ''' <summary>
        ''' populate all of the cell data in current grid graph
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function EnumerateData() As IEnumerable(Of T)
            For Each row In matrix2D
                For Each col In row.Value
                    Yield col.Value.data
                Next
            Next
        End Function

        Public Iterator Function LineScans() As IEnumerable(Of T)
            For Each row In matrix2D.OrderBy(Function(r) r.Key)
                For Each col In row.Value.OrderBy(Function(c) c.Key)
                    Yield col.Value.data
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ShuffleAll() As T()
            Return EnumerateData.Shuffles
        End Function

        ''' <summary>
        ''' get target cell data via a given pixel point
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="hit"></param>
        ''' <returns>
        ''' nothing will be returns if there is no data on the given ``[x,y]`` pixel point.
        ''' </returns>
        Public Function GetData(x As Integer, y As Integer, Optional ByRef hit As Boolean = False) As T
            Dim xkey = CLng(x), ykey = CLng(y)

            If Not matrix2D.ContainsKey(xkey) Then
                hit = False
                Return Nothing
            ElseIf Not matrix2D(xkey).ContainsKey(ykey) Then
                hit = False
                Return Nothing
            Else
                hit = True
            End If

            Return matrix2D(xkey)(ykey).data
        End Function

        ''' <summary>
        ''' check of the given point is existed?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function Check(x As Integer, y As Integer) As Boolean
            Dim xkey = CLng(x), ykey = CLng(y)

            If Not matrix2D.ContainsKey(xkey) Then
                Return False
            ElseIf Not matrix2D(xkey).ContainsKey(ykey) Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' get a range of nearby cell data via a given pixel point data 
        ''' and query size of the cell block rectangle.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="gridSize"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Query(x As Integer, y As Integer, gridSize As Integer) As IEnumerable(Of T)
            Return Query(x, y, New Size(gridSize, gridSize))
        End Function

        ''' <summary>
        ''' Query a block of the data points
        ''' 
        ''' [<paramref name="x"/>, <paramref name="y"/>] is the center point of the target rectangle region.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="gridSize"></param>
        ''' <returns></returns>
        Public Iterator Function Query(x As Integer, y As Integer, gridSize As Size) As IEnumerable(Of T)
            Dim q As T
            Dim hit As Boolean = False

            For i As Integer = x - gridSize.Width To x + gridSize.Width
                For j As Integer = y - gridSize.Height To y + gridSize.Height
                    q = GetData(i, j, hit)

                    If hit Then
                        Yield q
                    End If
                Next
            Next
        End Function

        ''' <summary>
        ''' Create a new readonly spatial graph
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="getSpatial"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateReadOnly(data As IEnumerable(Of T), getSpatial As Func(Of T, Point)) As Grid(Of T)
            Return CreateReadOnly(data.Select(Function(i) New GridCell(Of T)(getSpatial(i), i)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateReadOnly(data As IEnumerable(Of GridCell(Of T))) As Grid(Of T)
            Return New Grid(Of T)(data, toPoint:=Nothing)
        End Function

        Public Shared Function Create(data As IEnumerable(Of (T, x%, y%))) As Grid(Of T)
            Dim cells As GridCell(Of T)() = data _
                .Select(Function(i) New GridCell(Of T)(i.x, i.y, i.Item1)) _
                .ToArray
            Dim grid As New Grid(Of T)(cells, toPoint:=Nothing)

            Return grid
        End Function

        ''' <summary>
        ''' duplicated pixels will be removed from this constructor function
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="getPixel"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(data As IEnumerable(Of T), getPixel As Func(Of T, Point)) As Grid(Of T)
            Return data _
                .SafeQuery _
                .Select(Function(d)
                            Dim pxy As Point = getPixel(d)
                            Dim cell As New GridCell(Of T)(pxy.X, pxy.Y, d)

                            Return cell
                        End Function) _
                .DoCall(Function(vec)
                            Return New Grid(Of T)(vec, toPoint:=getPixel)
                        End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(data As IEnumerable(Of T),
                                      getX As Func(Of T, Integer),
                                      getY As Func(Of T, Integer)) As Grid(Of T)
            Return data _
                .SafeQuery _
                .Select(Function(d)
                            Return New GridCell(Of T)(getX(d), getY(d), d)
                        End Function) _
                .DoCall(Function(vec)
                            Return New Grid(Of T)(vec, toPoint:=Function(t)
                                                                    Dim x As Integer = getX(t)
                                                                    Dim y As Integer = getY(t)

                                                                    Return New Point(x, y)
                                                                End Function)
                        End Function)
        End Function

        ''' <summary>
        ''' a generic grid constructor for <see cref="IPoint2D"/>
        ''' </summary>
        ''' <typeparam name="Point"></typeparam>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(Of Point As IPoint2D)(data As IEnumerable(Of Point)) As Grid(Of Point)
            Return Grid(Of Point).Create(data, Function(p) p.X, Function(p) p.Y)
        End Function

        Public Shared Function Blank(dims As Size,
                                     blankSpot As Func(Of Integer, Integer, T),
                                     getSpot As Func(Of T, Point),
                                     Optional steps As SizeF = Nothing) As Grid(Of T)

            Dim cells As New List(Of T)

            If steps.IsEmpty Then
                steps = New SizeF(1, 1)
            End If

            For x As Double = 0 To dims.Width Step steps.Width
                For y As Double = 0 To dims.Height Step steps.Height
                    cells.Add(blankSpot(x, y))
                Next
            Next

            Return Create(cells, getSpot)
        End Function
    End Class
End Namespace
