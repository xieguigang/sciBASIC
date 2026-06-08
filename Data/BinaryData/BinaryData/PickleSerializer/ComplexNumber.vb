#Region "Microsoft.VisualBasic::2f3e7b652340098e0f648c88249428f6, Data\BinaryData\BinaryData\PickleSerializer\ComplexNumber.vb"

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

    '   Total Lines: 39
    '    Code Lines: 25 (64.10%)
    ' Comment Lines: 7 (17.95%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (17.95%)
    '     File Size: 1.34 KB


    '     Class ComplexNumber
    ' 
    '         Properties: Imaginary, Real
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Equals, GetHashCode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Pickle

    ''' <summary>
    ''' 表示 Python 的复数类型 (complex)。
    ''' Python 原生支持复数运算，.NET 没有内置复数类型（System.Numerics.Complex 
    ''' 需要额外引用），因此提供此轻量级实现。
    ''' </summary>
    Public Class ComplexNumber
        ''' <summary>实部</summary>
        Public ReadOnly Property Real As Double

        ''' <summary>虚部</summary>
        Public ReadOnly Property Imaginary As Double

        Public Sub New(real As Double, imaginary As Double)
            Me.Real = real
            Me.Imaginary = imaginary
        End Sub

        Public Overrides Function ToString() As String
            If Imaginary >= 0 Then
                Return $"{Real}+{Imaginary}j"
            Else
                Return $"{Real}{Imaginary}j"
            End If
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, ComplexNumber)
            If other Is Nothing Then Return False
            Return Real = other.Real AndAlso Imaginary = other.Imaginary
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Real.GetHashCode() Xor Imaginary.GetHashCode()
        End Function
    End Class

End Namespace
