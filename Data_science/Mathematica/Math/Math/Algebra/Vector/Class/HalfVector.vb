#Region "Microsoft.VisualBasic::21e597e0e1508e892c6dcf61d74efea0, Data_science\Mathematica\Math\Math\Algebra\Vector\Class\HalfVector.vb"

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

    '   Total Lines: 132
    '    Code Lines: 103
    ' Comment Lines: 3
    '   Blank Lines: 26
    '     File Size: 5.24 KB


    '     Class HalfVector
    ' 
    '         Properties: Mean, Sum
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: AsVector, ToString
    '         Operators: (+2 Overloads) -, (+4 Overloads) *, (+2 Overloads) /, (+3 Overloads) ^, (+3 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Numerics
Imports Microsoft.VisualBasic.Math.Scripting.Rscript
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' 精度比较低的半精度向量
    ''' </summary>
    Public Class HalfVector : Inherits GenericVector(Of Half)

        Public ReadOnly Property Sum As Double
            Get
                Dim result As Double

                For Each value As Half In buffer
                    result += CSng(value)
                Next

                Return result
            End Get
        End Property

        Public ReadOnly Property Mean As Double
            Get
                Return Sum / [Dim]
            End Get
        End Property

        Sub New(data As IEnumerable(Of Half))
            Call MyBase.New(data)
        End Sub

        Sub New(values As IEnumerable(Of Double))
            Call MyBase.New(From x As Double In values Select CType(CSng(x), Half))
        End Sub

        Sub New(values As IEnumerable(Of Single))
            Call MyBase.New(From x As Single In values Select CType(x, Half))
        End Sub

        Sub New(value As Single, n As Integer)
            Call MyBase.New(CType(value, Half).Replicate(n))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsVector() As Vector
            Return New Vector(buffer.Select(Function(x) CSng(x)))
        End Function

        Public Overrides Function ToString() As String
            Return buffer.Select(Function(n) CSng(n).ToString).GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x + CSng(add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x + add)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v As HalfVector, add As Integer) As HalfVector
            Return New HalfVector(From x As Half In v Select x + CSng(add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x - CSng(add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x - add)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x * CSng(add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(multiply As Double, v As HalfVector) As HalfVector
            Return v * multiply
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x * add)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As HalfVector, multiple As Vector) As Vector
            If v.Dim <> multiple.Dim Then
                Throw New InvalidConstraintException
            Else
                Return New Vector(From i As Integer In v.Sequence Select CSng(v(i)) * multiple(i))
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(v As HalfVector, div As Double) As HalfVector
            Dim divVal! = div
            Return New HalfVector(From x As Half In v Select x / divVal)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x / add)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(v As HalfVector, pow As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x ^ pow)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(base#, v As HalfVector) As HalfVector
            Dim baseVal! = base
            Return New HalfVector(From x As Half In v Select baseVal ^ CSng(x))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x ^ add)
        End Operator
    End Class
End Namespace
