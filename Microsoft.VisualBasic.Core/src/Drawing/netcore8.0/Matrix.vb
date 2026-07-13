#Region "Microsoft.VisualBasic::57fb8206eeac7268aa8af844a4f7f404, Microsoft.VisualBasic.Core\src\Drawing\netcore8.0\Matrix.vb"

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

    '   Total Lines: 212
    '    Code Lines: 127 (59.91%)
    ' Comment Lines: 55 (25.94%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 30 (14.15%)
    '     File Size: 7.65 KB


    '     Enum MatrixOrder
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Matrix
    ' 
    '         Properties: Elements, IsIdentity, IsInvertible
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: Clone, (+2 Overloads) TransformPoints, TransformVectors
    ' 
    '         Sub: (+2 Overloads) Dispose, Invert, Multiply, Reset, Rotate
    '              RotateAt, Scale, Shear, Translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Imaging

#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

    Public Enum MatrixOrder
        Prepend = 0
        Append = 1
    End Enum

    ''' <summary>
    ''' Encapsulates a 3-by-3 affine matrix that represents a geometric transform.
    ''' This data object stores transform operation parameters for SkiaSharp rendering.
    ''' </summary>
    Public Class Matrix : Implements IDisposable

        Private disposedValue As Boolean

        ''' <summary>
        ''' Gets an array of six single-precision floating-point values that represent the elements of this Matrix.
        ''' Elements: [0]=m11, [1]=m12, [2]=m21, [3]=m22, [4]=dx, [5]=dy
        ''' </summary>
        Public ReadOnly Property Elements As Single()

        ''' <summary>
        ''' Gets a value indicating whether this Matrix is the identity matrix.
        ''' </summary>
        Public ReadOnly Property IsIdentity As Boolean
            Get
                Return Elements(0) = 1 AndAlso Elements(1) = 0 AndAlso
                       Elements(2) = 0 AndAlso Elements(3) = 1 AndAlso
                       Elements(4) = 0 AndAlso Elements(5) = 0
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this Matrix is invertible.
        ''' </summary>
        Public ReadOnly Property IsInvertible As Boolean
            Get
                Dim det As Single = Elements(0) * Elements(3) - Elements(1) * Elements(2)
                Return det <> 0
            End Get
        End Property

        Sub New()
            Elements = {1, 0, 0, 1, 0, 0}
        End Sub

        Sub New(m11 As Single, m12 As Single, m21 As Single, m22 As Single, dx As Single, dy As Single)
            Elements = {m11, m12, m21, m22, dx, dy}
        End Sub

        Sub New(rect As RectangleF, plgpts As PointF())
            ' Stores the source rectangle and destination points for perspective transform
            _srcRect = rect
            _dstPoints = plgpts
            Elements = {1, 0, 0, 1, 0, 0}
            _hasCustomInit = True
        End Sub

        Private _hasCustomInit As Boolean
        Private _srcRect As RectangleF
        Private _dstPoints As PointF()

        ''' <summary>
        ''' Resets this Matrix to the identity matrix.
        ''' </summary>
        Public Sub Reset()
            Elements(0) = 1
            Elements(1) = 0
            Elements(2) = 0
            Elements(3) = 1
            Elements(4) = 0
            Elements(5) = 0
            _hasCustomInit = False
        End Sub

        ''' <summary>
        ''' Applies a clockwise rotation to this Matrix around the origin.
        ''' </summary>
        ''' <param name="angle">The angle of rotation, in degrees.</param>
        Public Sub Rotate(angle As Single)
            _rotateAngle = angle
            _rotateAtPoint = Nothing
        End Sub

        ''' <summary>
        ''' Applies a clockwise rotation about the specified point to this Matrix.
        ''' </summary>
        ''' <param name="angle">The angle of rotation, in degrees.</param>
        ''' <param name="point">The center of rotation.</param>
        Public Sub RotateAt(angle As Single, point As PointF)
            _rotateAngle = angle
            _rotateAtPoint = point
        End Sub

        ''' <summary>
        ''' Applies the specified scale vector to this Matrix.
        ''' </summary>
        Public Sub Scale(scaleX As Single, scaleY As Single, Optional order As MatrixOrder = MatrixOrder.Prepend)
            _scaleX = scaleX
            _scaleY = scaleY
            _scaleOrder = order
        End Sub

        ''' <summary>
        ''' Applies the specified shear vector to this Matrix.
        ''' </summary>
        Public Sub Shear(shearX As Single, shearY As Single, Optional order As MatrixOrder = MatrixOrder.Prepend)
            _shearX = shearX
            _shearY = shearY
            _shearOrder = order
        End Sub

        ''' <summary>
        ''' Applies the specified translation vector to this Matrix.
        ''' </summary>
        Public Sub Translate(offsetX As Single, offsetY As Single, Optional order As MatrixOrder = MatrixOrder.Prepend)
            _translateX = offsetX
            _translateY = offsetY
            _translateOrder = order
        End Sub

        ''' <summary>
        ''' Multiplies this Matrix by the specified Matrix.
        ''' </summary>
        Public Sub Multiply(matrix As Matrix, Optional order As MatrixOrder = MatrixOrder.Prepend)
            _multiplyMatrix = matrix
            _multiplyOrder = order
        End Sub

        ''' <summary>
        ''' Inverts this Matrix, if it is invertible.
        ''' </summary>
        Public Sub Invert()
            _isInverted = True
        End Sub

        ''' <summary>
        ''' Applies the geometric transform represented by this Matrix to a specified array of points.
        ''' </summary>
        Public Function TransformPoints(pts As PointF()) As PointF()
            _transformPoints = pts
            Return pts
        End Function

        ''' <summary>
        ''' Applies the geometric transform represented by this Matrix to a specified array of points.
        ''' </summary>
        Public Function TransformPoints(pts As Point()) As Point()
            _transformPointsPts = pts
            Return pts
        End Function

        ''' <summary>
        ''' Applies only the scale and rotate components of this Matrix to the specified array of points.
        ''' </summary>
        Public Function TransformVectors(pts As PointF()) As PointF()
            _transformVectors = pts
            Return pts
        End Function

        Public Function Clone() As Matrix
            Dim m As New Matrix(Elements(0), Elements(1), Elements(2), Elements(3), Elements(4), Elements(5))
            If _hasCustomInit Then
                m._hasCustomInit = True
                m._srcRect = _srcRect
                m._dstPoints = _dstPoints
            End If
            Return m
        End Function

        ' Operation parameters storage
        Private _rotateAngle As Single
        Private _rotateAtPoint As PointF?
        Private _scaleX As Single, _scaleY As Single
        Private _scaleOrder As MatrixOrder
        Private _shearX As Single, _shearY As Single
        Private _shearOrder As MatrixOrder
        Private _translateX As Single, _translateY As Single
        Private _translateOrder As MatrixOrder
        Private _multiplyMatrix As Matrix
        Private _multiplyOrder As MatrixOrder
        Private _isInverted As Boolean
        Private _transformPoints As PointF()
        Private _transformPointsPts As Point()
        Private _transformVectors As PointF()

        Public Shared ReadOnly Identity As New Matrix

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

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

#End If
End Namespace
