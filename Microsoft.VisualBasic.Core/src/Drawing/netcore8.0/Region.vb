#Region "Microsoft.VisualBasic::aa55ad14b75cd346b56fb476d4efe0ba, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Region.vb"

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

    '   Total Lines: 389
    '    Code Lines: 212 (54.50%)
    ' Comment Lines: 119 (30.59%)
    '    - Xml Docs: 96.64%
    ' 
    '   Blank Lines: 58 (14.91%)
    '     File Size: 14.27 KB


    '     Class Region
    ' 
    '         Properties: IsEmpty, IsInfinite, RegionData
    ' 
    '         Constructor: (+6 Overloads) Sub New
    ' 
    '         Function: Clone, GetBounds, GetRegionData, (+3 Overloads) IsVisible
    ' 
    '         Sub: (+3 Overloads) [Xor], (+3 Overloads) Complement, (+2 Overloads) Dispose, (+3 Overloads) Exclude, (+3 Overloads) Intersect
    '              MakeEmpty, MakeInfinite, ReleaseHrgn, Transform, (+2 Overloads) Translate
    '              (+3 Overloads) Union
    '         Class region_op
    ' 
    ' 
    ' 
    '         Class region_op_Complement
    ' 
    '             Properties: path, rect, region
    ' 
    '         Class region_op_Exclude
    ' 
    '             Properties: path, rect, region
    ' 
    '         Class region_op_Intersect
    ' 
    '             Properties: path, rect, region
    ' 
    '         Class region_op_Union
    ' 
    '             Properties: path, rect, region
    ' 
    '         Class region_op_Xor
    ' 
    '             Properties: path, rect, region
    ' 
    '         Class region_op_Translate
    ' 
    '             Properties: dx, dy
    ' 
    '         Class region_op_Transform
    ' 
    '             Properties: matrix
    ' 
    '         Class region_op_MakeEmpty
    ' 
    ' 
    ' 
    '         Class region_op_MakeInfinite
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    ''' <summary>
    ''' Describes the interior of a graphics shape composed of rectangles and paths.
    ''' This data object stores clipping region parameters for SkiaSharp rendering.
    ''' </summary>
    Public Class Region : Implements IDisposable

        Private disposedValue As Boolean

        ''' <summary>
        ''' Gets a value indicating whether this Region is empty on the drawing surface.
        ''' </summary>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return _isEmpty
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this Region is infinite on the drawing surface.
        ''' </summary>
        Public ReadOnly Property IsInfinite As Boolean
            Get
                Return _isInfinite
            End Get
        End Property

        Private _isEmpty As Boolean = True
        Private _isInfinite As Boolean = False

        ''' <summary>
        ''' Gets or sets the underlying GraphicsPath data for this Region.
        ''' </summary>
        Public ReadOnly Property RegionData As GraphicsPath

        Dim regionOps As New List(Of region_op)

        ''' <summary>
        ''' Initializes a new Region with an infinite interior.
        ''' </summary>
        Public Sub New()
            _isInfinite = True
            _isEmpty = False
        End Sub

        ''' <summary>
        ''' Initializes a new Region from the specified GraphicsPath.
        ''' </summary>
        Sub New(path As GraphicsPath)
            RegionData = path
            _isEmpty = False
        End Sub

        ''' <summary>
        ''' Initializes a new Region from the specified Rectangle structure.
        ''' </summary>
        Sub New(rect As Rectangle)
            Dim gp As New GraphicsPath
            Call gp.AddRectangle(rect)
            RegionData = gp
            _isEmpty = False
        End Sub

        ''' <summary>
        ''' Initializes a new Region from the specified RectangleF structure.
        ''' </summary>
        Sub New(rect As RectangleF)
            Dim gp As New GraphicsPath
            Call gp.AddRectangle(rect)
            RegionData = gp
            _isEmpty = False
        End Sub

        ''' <summary>
        ''' Initializes a new Region from the specified GDI region handle.
        ''' </summary>
        Sub New(hrgn As IntPtr)
            ' Stores the handle reference
            _hRgn = hrgn
            _isEmpty = False
        End Sub

        Private _hRgn As IntPtr

        ''' <summary>
        ''' Initializes a new Region from the specified data.
        ''' </summary>
        Sub New(rgnData() As Byte)
            _rgnData = rgnData
            _isEmpty = False
        End Sub

        Private _rgnData As Byte()

        ''' <summary>
        ''' Creates an exact copy of this Region.
        ''' </summary>
        Public Function Clone() As Region
            Dim r As New Region
            r._isEmpty = _isEmpty
            r._isInfinite = _isInfinite
            r._RegionData = RegionData
            r._hRgn = _hRgn
            If _rgnData IsNot Nothing Then
                r._rgnData = DirectCast(_rgnData.Clone(), Byte())
            End If
            For Each op As region_op In regionOps
                r.regionOps.Add(op)
            Next
            Return r
        End Function

        ''' <summary>
        ''' Initializes this Region to an empty interior.
        ''' </summary>
        Public Sub MakeEmpty()
            _isEmpty = True
            _isInfinite = False
            _RegionData = Nothing
            Call regionOps.Add(New region_op_MakeEmpty)
        End Sub

        ''' <summary>
        ''' Initializes this Region to an infinite interior.
        ''' </summary>
        Public Sub MakeInfinite()
            _isInfinite = True
            _isEmpty = False
            Call regionOps.Add(New region_op_MakeInfinite)
        End Sub

        ''' <summary>
        ''' Updates this Region to the union minus the intersection of itself with the specified GraphicsPath.
        ''' </summary>
        Public Sub Complement(path As GraphicsPath)
            Call regionOps.Add(New region_op_Complement With {.path = path})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union minus the intersection of itself with the specified RectangleF.
        ''' </summary>
        Public Sub Complement(rect As RectangleF)
            Call regionOps.Add(New region_op_Complement With {.rect = rect})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union minus the intersection of itself with the specified Region.
        ''' </summary>
        Public Sub Complement(region As Region)
            Call regionOps.Add(New region_op_Complement With {.region = region})
        End Sub

        ''' <summary>
        ''' Updates this Region to the portion of itself that does not intersect the specified GraphicsPath.
        ''' </summary>
        Public Sub Exclude(path As GraphicsPath)
            Call regionOps.Add(New region_op_Exclude With {.path = path})
        End Sub

        ''' <summary>
        ''' Updates this Region to the portion of itself that does not intersect the specified RectangleF.
        ''' </summary>
        Public Sub Exclude(rect As RectangleF)
            Call regionOps.Add(New region_op_Exclude With {.rect = rect})
        End Sub

        ''' <summary>
        ''' Updates this Region to the portion of itself that does not intersect the specified Region.
        ''' </summary>
        Public Sub Exclude(region As Region)
            Call regionOps.Add(New region_op_Exclude With {.region = region})
        End Sub

        ''' <summary>
        ''' Updates this Region to the intersection of itself with the specified GraphicsPath.
        ''' </summary>
        Public Sub Intersect(path As GraphicsPath)
            Call regionOps.Add(New region_op_Intersect With {.path = path})
        End Sub

        ''' <summary>
        ''' Updates this Region to the intersection of itself with the specified RectangleF.
        ''' </summary>
        Public Sub Intersect(rect As RectangleF)
            Call regionOps.Add(New region_op_Intersect With {.rect = rect})
        End Sub

        ''' <summary>
        ''' Updates this Region to the intersection of itself with the specified Region.
        ''' </summary>
        Public Sub Intersect(region As Region)
            Call regionOps.Add(New region_op_Intersect With {.region = region})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union of itself and the specified GraphicsPath.
        ''' </summary>
        Public Sub Union(path As GraphicsPath)
            Call regionOps.Add(New region_op_Union With {.path = path})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union of itself and the specified RectangleF.
        ''' </summary>
        Public Sub Union(rect As RectangleF)
            Call regionOps.Add(New region_op_Union With {.rect = rect})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union of itself and the specified Region.
        ''' </summary>
        Public Sub Union(region As Region)
            Call regionOps.Add(New region_op_Union With {.region = region})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union minus the intersection of itself with the specified GraphicsPath.
        ''' </summary>
        Public Sub [Xor](path As GraphicsPath)
            Call regionOps.Add(New region_op_Xor With {.path = path})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union minus the intersection of itself with the specified RectangleF.
        ''' </summary>
        Public Sub [Xor](rect As RectangleF)
            Call regionOps.Add(New region_op_Xor With {.rect = rect})
        End Sub

        ''' <summary>
        ''' Updates this Region to the union minus the intersection of itself with the specified Region.
        ''' </summary>
        Public Sub [Xor](region As Region)
            Call regionOps.Add(New region_op_Xor With {.region = region})
        End Sub

        ''' <summary>
        ''' Offsets the coordinates of this Region by the specified amount.
        ''' </summary>
        Public Sub Translate(dx As Single, dy As Single)
            Call regionOps.Add(New region_op_Translate With {.dx = dx, .dy = dy})
        End Sub

        ''' <summary>
        ''' Offsets the coordinates of this Region by the specified amount (integer version).
        ''' </summary>
        Public Sub Translate(dx As Integer, dy As Integer)
            Call regionOps.Add(New region_op_Translate With {.dx = CSng(dx), .dy = CSng(dy)})
        End Sub

        ''' <summary>
        ''' Transforms this Region by the specified Matrix.
        ''' </summary>
        Public Sub Transform(matrix As Matrix)
            Call regionOps.Add(New region_op_Transform With {.matrix = matrix})
        End Sub

        ''' <summary>
        ''' Gets a RectangleF structure that represents a rectangle that bounds this Region.
        ''' </summary>
        Public Function GetBounds(g As IGraphics) As RectangleF
            If RegionData IsNot Nothing Then
                Return RegionData.GetBounds()
            End If
            If _isInfinite Then
                Return New RectangleF(Single.MinValue / 2, Single.MinValue / 2, Single.MaxValue, Single.MaxValue)
            End If
            Return New RectangleF(0, 0, 0, 0)
        End Function

        ''' <summary>
        ''' Returns a RegionDataInfo that describes this Region.
        ''' </summary>
        Public Function GetRegionData() As Byte()
            Return _rgnData
        End Function

        ''' <summary>
        ''' Tests whether the specified point is contained within this Region when drawn using the specified Graphics.
        ''' </summary>
        Public Function IsVisible(x As Single, y As Single, Optional g As IGraphics = Nothing) As Boolean
            Return IsVisible(New PointF(x, y), g)
        End Function

        ''' <summary>
        ''' Tests whether the specified PointF structure is contained within this Region.
        ''' </summary>
        Public Function IsVisible(point As PointF, Optional g As IGraphics = Nothing) As Boolean
            If _isEmpty Then Return False
            If _isInfinite Then Return True
            If RegionData IsNot Nothing Then
                Return RegionData.IsVisible(point, g)
            End If
            Return False
        End Function

        ''' <summary>
        ''' Tests whether the specified rectangle is contained within this Region.
        ''' </summary>
        Public Function IsVisible(rect As RectangleF, Optional g As IGraphics = Nothing) As Boolean
            If _isEmpty Then Return False
            If _isInfinite Then Return True
            Return IsVisible(New PointF(rect.Left, rect.Top), g) AndAlso
                   IsVisible(New PointF(rect.Right, rect.Bottom), g)
        End Function

        ''' <summary>
        ''' Returns a value indicating whether this Region is equal to the specified object.
        ''' </summary>
        Public Sub ReleaseHrgn(Optional regionHandle As IntPtr = Nothing)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    RegionData?.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Internal operation record for region transforms, similar to GraphicsPath's opSet pattern.
        ''' </summary>
        Private MustInherit Class region_op
        End Class

        Private Class region_op_Complement : Inherits region_op
            Public Property path As GraphicsPath
            Public Property rect As RectangleF?
            Public Property region As Region
        End Class

        Private Class region_op_Exclude : Inherits region_op
            Public Property path As GraphicsPath
            Public Property rect As RectangleF?
            Public Property region As Region
        End Class

        Private Class region_op_Intersect : Inherits region_op
            Public Property path As GraphicsPath
            Public Property rect As RectangleF?
            Public Property region As Region
        End Class

        Private Class region_op_Union : Inherits region_op
            Public Property path As GraphicsPath
            Public Property rect As RectangleF?
            Public Property region As Region
        End Class

        Private Class region_op_Xor : Inherits region_op
            Public Property path As GraphicsPath
            Public Property rect As RectangleF?
            Public Property region As Region
        End Class

        Private Class region_op_Translate : Inherits region_op
            Public Property dx As Single
            Public Property dy As Single
        End Class

        Private Class region_op_Transform : Inherits region_op
            Public Property matrix As Matrix
        End Class

        Private Class region_op_MakeEmpty : Inherits region_op
        End Class

        Private Class region_op_MakeInfinite : Inherits region_op
        End Class
    End Class

#End If
End Namespace
