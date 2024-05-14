#Region "Microsoft.VisualBasic::8fac665f08cf07ef00b39f868e60845f, mime\application%pdf\PdfReader\Document\PdfRectangle.vb"

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

    '   Total Lines: 92
    '    Code Lines: 76
    ' Comment Lines: 3
    '   Blank Lines: 13
    '     File Size: 3.03 KB


    '     Class PdfRectangle
    ' 
    '         Properties: Height, LowerLeftX, LowerLeftY, UpperRightX, UpperRightY
    '                     Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ObjectToFloat, ToString
    ' 
    '         Sub: Visit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports stdNum = System.Math

Namespace PdfReader
    Public Class PdfRectangle
        Inherits PdfObject

        Private _LowerLeftX As Single, _LowerLeftY As Single, _UpperRightX As Single, _UpperRightY As Single

        Public Sub New(parent As PdfObject, array As ParseArray)
            MyBase.New(parent, array)
            ' Extract raw values
            Dim lx = ObjectToFloat(array.Objects(0))
            Dim ly = ObjectToFloat(array.Objects(1))
            Dim ux = ObjectToFloat(array.Objects(2))
            Dim uy = ObjectToFloat(array.Objects(3))

            ' Normalize so the lower-left and upper-right are actually those values
            LowerLeftX = stdNum.Min(lx, ux)
            LowerLeftY = stdNum.Min(ly, uy)
            UpperRightX = stdNum.Max(lx, ux)
            UpperRightY = stdNum.Max(ly, uy)
        End Sub

        Public Overrides Function ToString() As String
            Return $"({LowerLeftX},{LowerLeftY}) -> ({UpperRightX},{UpperRightY})"
        End Function

        Public Overrides Sub Visit(visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property LowerLeftX As Single
            Get
                Return _LowerLeftX
            End Get
            Private Set(value As Single)
                _LowerLeftX = value
            End Set
        End Property

        Public Property LowerLeftY As Single
            Get
                Return _LowerLeftY
            End Get
            Private Set(value As Single)
                _LowerLeftY = value
            End Set
        End Property

        Public Property UpperRightX As Single
            Get
                Return _UpperRightX
            End Get
            Private Set(value As Single)
                _UpperRightX = value
            End Set
        End Property

        Public Property UpperRightY As Single
            Get
                Return _UpperRightY
            End Get
            Private Set(value As Single)
                _UpperRightY = value
            End Set
        End Property

        Public ReadOnly Property Width As Single
            Get
                Return UpperRightX - LowerLeftX
            End Get
        End Property

        Public ReadOnly Property Height As Single
            Get
                Return UpperRightY - LowerLeftY
            End Get
        End Property

        Private Function ObjectToFloat(obj As ParseObjectBase) As Single
            ' Might be an integer if the value has no fractional part
            If TypeOf obj Is ParseInteger Then
                Return TryCast(obj, ParseInteger).Value
            ElseIf TypeOf obj Is ParseReal Then
                Return TryCast(obj, ParseReal).Value
            Else
                Throw New ApplicationException($"Array does not contain numbers that can be converted to a rectangle.")
            End If
        End Function
    End Class
End Namespace
