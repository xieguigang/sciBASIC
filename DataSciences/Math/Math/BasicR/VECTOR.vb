#Region "Microsoft.VisualBasic::6d39267484e7244a085acd98e22465aa, ..\visualbasic_App\Scripting\Math\Math\BasicR\VECTOR.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BasicR

    ''' <summary>
    ''' <see cref="List(Of Double)"/>
    ''' </summary>
    Public Class Vector : Inherits GenericVector(Of Double)

        Public Shared ReadOnly Property NAN As Vector
            Get
                Return New Vector({Double.NaN})
            End Get
        End Property

        Public Shared ReadOnly Property Inf As Vector
            Get
                Return New Vector({Double.PositiveInfinity})
            End Get
        End Property

        Public Shared ReadOnly Property Zero As Vector
            Get
                Return New Vector({0})
            End Get
        End Property

        ''' <summary>
        ''' Creates vector with m element and init value set to zero
        ''' </summary>
        ''' <param name="m"></param>
        Public Sub New(m As Integer)
            Call Me.New(0R, m)
        End Sub

        ''' <summary>
        ''' Creates vector with a specific value sequence.
        ''' </summary>
        ''' <param name="data"></param>
        Sub New(data As IEnumerable(Of Double))
            Call Me.New(0)

            For Each x As Double In data
                Add(x)
            Next
        End Sub

        ''' <summary>
        ''' Creates vector with m element and init value specific by init parameter.
        ''' </summary>
        ''' <param name="init"></param>
        ''' <param name="m"></param>
        Sub New(init As Double, m As Integer)
            Call MyBase.New(capacity:=m)

            For i As Integer = 0 To m - 1
                Add(init)
            Next
        End Sub

        ''' <summary>
        ''' 两个向量加法算符重载，分量分别相加
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator +(v1 As Vector, v2 As Vector) As Vector
            Dim N0 As Integer = v1.[Dim] ' 获取变量维数
            Dim v3 As New Vector(N0)

            For j As Integer = 0 To N0 - 1
                v3(j) = v1(j) + v2(j)
            Next

            Return v3
        End Operator

        ''' <summary>
        ''' 向量减法算符重载，分量分别想减
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator -(v1 As Vector, v2 As Vector) As Vector
            Dim N0 As Integer = v1.[Dim]    ' 获取变量维数
            Dim v3 As New Vector(N0)

            For j As Integer = 0 To N0 - 1
                v3(j) = v1(j) - v2(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量乘法算符重载，分量分别相乘，相当于MATLAB中的  .*算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(v1 As Vector, v2 As Vector) As Vector
            Dim N0 As Integer = v1.[Dim]        '获取变量维数
            Dim v3 As New Vector(N0)

            For j As Integer = 0 To N0 - 1
                v3(j) = v1(j) * v2(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量除法算符重载，分量分别相除，相当于MATLAB中的   ./算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(v1 As Vector, v2 As Vector) As Vector
            Dim N0 As Integer = v1.[Dim]         ' 获取变量维数
            Dim v3 As New Vector(N0)

            For j = 0 To N0 - 1
                v3(j) = v1(j) / v2(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量减加实数，各分量分别加实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator +(v1 As Vector, a As Double) As Vector
            '向量数加算符重载
            Dim N0 As Integer = v1.[Dim]         ' 获取变量维数
            Dim v2 As New Vector(N0)

            For j = 0 To N0 - 1
                v2(j) = v1(j) + a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量减实数，各分量分别减实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator -(v1 As Vector, a As Double) As Vector
            '向量数加算符重载
            Dim N0 As Integer = v1.[Dim]           '获取变量维数
            Dim v2 As New Vector(N0)

            For j = 0 To N0 - 1
                v2(j) = v1(j) - a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量 数乘，各分量分别乘以实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(v1 As Vector, a As Double) As Vector
            Dim N0 As Integer = v1.[Dim]            '获取变量维数
            Dim v2 As New Vector(N0)

            For j As Integer = 0 To N0 - 1
                v2(j) = v1(j) * a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量 数除，各分量分别除以实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(v1 As Vector, a As Double) As Vector
            Dim N0 As Integer = v1.[Dim]         '获取变量维数
            Dim v2 As New Vector(N0)

            For j = 0 To N0 - 1
                v2(j) = v1(j) / a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 实数加向量
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator +(a As Double, v1 As Vector) As Vector
            '向量数加算符重载
            Dim N0 As Integer = v1.[Dim]        '获取变量维数
            Dim v2 As New Vector(N0)

            For j = 0 To N0 - 1
                v2(j) = v1(j) + a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 实数减向量
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Operator -(a As Double, v1 As Vector) As Vector
            '向量数加算符重载
            Dim N0 As Integer = v1.[Dim]         '获取变量维数
            Dim v2 As New Vector(N0)

            For j = 0 To N0 - 1
                v2(j) = v1(j) - a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量 数乘
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(a As Double, v1 As Vector) As Vector
            Dim N0 As Integer = v1.[Dim]        '获取变量维数
            Dim v2 As New Vector(N0)

            For j = 0 To N0 - 1
                v2(j) = v1(j) * a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量内积
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Or(v1 As Vector, v2 As Vector) As Double
            '获取变量维数
            Dim N0 = v1.[Dim]
            Dim M0 = v2.[Dim]

            If N0 <> M0 Then
                Throw New ArgumentException("Inner vector dimensions must agree！")
            End If
            '如果向量维数不匹配，给出告警信息

            Dim sum As Double

            For j = 0 To N0 - 1
                sum = sum + v1(j) * v2(j)
            Next
            Return sum
        End Operator

        ''' <summary>
        ''' 向量外积（相当于列向量，乘以横向量）
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Xor(v1 As Vector, v2 As Vector) As MATRIX
            '获取变量维数
            Dim N0 = v1.[Dim]
            Dim M0 = v2.[Dim]

            If N0 <> M0 Then
                Throw New ArgumentException("Inner vector dimensions must agree！")
            End If
            '如果向量维数不匹配，给出告警信息

            Dim vvmat As New MATRIX(N0, N0)

            For i As Integer = 0 To N0 - 1
                For j As Integer = 0 To N0 - 1
                    vvmat(i, j) = v1(i) * v2(j)
                Next
            Next

            '返回外积矩阵
            Return vvmat
        End Operator

        ''' <summary>
        ''' 向量模的平方
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Not(v1 As Vector) As Double
            '获取变量维数
            Dim N0 = v1.[Dim]
            Dim sum As Double

            For j = 0 To N0 - 1
                sum = sum + v1(j) * v1(j)
            Next
            Return sum
        End Operator

        ''' <summary>
        ''' 负向量 
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(v1 As Vector) As Vector
            Dim N0 As Integer = v1.[Dim]
            Dim v2 As New Vector(N0)

            For i As Integer = 0 To N0 - 1
                v2(i) = -v1(i)
            Next

            Return v2
        End Operator

        ''' <summary>
        ''' Display member's data as json array
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Me.ToArray.GetJson
        End Function

        Public Overloads Shared Operator =(x As Vector, n As Integer) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d = n)
        End Operator

        Public Overloads Shared Operator <>(x As Vector, n As Integer) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d <> n)
        End Operator

        Public Overloads Shared Operator ^(v As Vector, n As Integer) As Vector
            Return New Vector(From d As Double In v Select d ^ n)
        End Operator

        Public Overloads Shared Operator >(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d > n)
        End Operator

        Public Overloads Shared Operator <(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d < n)
        End Operator

        Public Shared Operator >=(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d >= n)
        End Operator

        Public Shared Operator <=(x As Vector, n As Double) As BooleanVector
            Return New BooleanVector(From d As Double In x Select d <= n)
        End Operator

        Public Shared Widening Operator CType(v As Double()) As Vector
            Return New Vector(v)
        End Operator
    End Class
End Namespace
