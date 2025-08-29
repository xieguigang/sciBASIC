#Region "Microsoft.VisualBasic::873e1965e63335164cb2a33b2f76dc37, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\BooleanVector.vb"

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

    '   Total Lines: 171
    '    Code Lines: 86 (50.29%)
    ' Comment Lines: 65 (38.01%)
    '    - Xml Docs: 83.08%
    ' 
    '   Blank Lines: 20 (11.70%)
    '     File Size: 6.04 KB


    '     Class BooleanVector
    ' 
    '         Properties: [False], [True], IsLogical
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Sum, ToString
    '         Operators: (+2 Overloads) IsFalse, (+2 Overloads) IsTrue, (+2 Overloads) Not, (+4 Overloads) Or
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Language.Vectorization

    ''' <summary>
    ''' <see cref="System.Boolean"/> Array
    ''' </summary>
    Public Class BooleanVector : Inherits Vector(Of Boolean)

        ''' <summary>
        ''' Only one boolean value ``True`` in the array list
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property [True] As BooleanVector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New BooleanVector({True})
            End Get
        End Property

        ''' <summary>
        ''' Only one boolean value ``False`` in the array list
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property [False] As BooleanVector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New BooleanVector({False})
            End Get
        End Property

        Public ReadOnly Property IsLogical As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Length = 1
            End Get
        End Property

        Public ReadOnly Property Any As Boolean
            Get
                Return buffer.Any(Function(a) a)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(b As IEnumerable(Of Boolean))
            MyBase.New(b)
        End Sub

        Public Function Sum() As Integer
            Return BooleanVector.Sum(Me)
        End Function

        Public Overrides Function ToString() As String
            Dim countTrue% = Linq.which(buffer).Count
            Dim countFalse% = Linq.which(Not Me).Count

            Return $"all({Length}) = {countTrue}:true + {countFalse}:false"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Sum(b As BooleanVector) As Integer
            ' 因为 Aggregate 表达式中，当前的函数名和Linq拓展函数Sum冲突
            ' 所以在这里就只能使用拓展函数链来进行累加
            Return b.Select(Function(x) If(x, 1, 0)).Sum
        End Function

        ''' <summary>
        ''' And
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator &(x As Boolean, y As BooleanVector) As BooleanVector
            Return New BooleanVector(From b As Boolean In y Select b AndAlso x)
        End Operator

        ''' <summary>
        ''' X AndAlso Y
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        Public Overloads Shared Operator &(x As BooleanVector, y As BooleanVector) As BooleanVector
            Dim vy = y.buffer
            Return New BooleanVector(x.Select(Function(xi, i) xi AndAlso vy(i)))
        End Operator

        ''' <summary>
        ''' 将逻辑向量之中的每一个逻辑值都进行翻转
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Not(x As BooleanVector) As BooleanVector
            Return New BooleanVector(From b As Boolean In x Select Not b)
        End Operator

        ''' <summary>
        ''' x(0)
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(x As BooleanVector) As Boolean
            Return x(0)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(b As Boolean()) As BooleanVector
            Return New BooleanVector(b)
        End Operator

        ''' <summary>
        ''' x Or Y
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(x As BooleanVector, y As Boolean()) As BooleanVector
            Return New BooleanVector(x.Select(Function(b, i) b OrElse y(i)))
        End Operator

        ''' <summary>
        ''' x Or Y
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Or(x As BooleanVector, y As BooleanVector) As BooleanVector
            Return x Or y.ToArray
        End Operator

        ''' <summary>
        ''' <see cref="ToArray"/>
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(x As BooleanVector) As Boolean()
            Return x.ToArray
        End Operator

        Public Shared Operator IsTrue(b As BooleanVector) As Boolean
            If b.IsNullOrEmpty Then
                Return False
            Else
                Return Not Enumerable.Any(b, Function(x) x = False)
            End If
        End Operator

        Public Shared Operator IsFalse(b As BooleanVector) As Boolean
            If b Then
                ' b是True，则不是False，在这里返回False，表明IsFalse不成立
                Return False
            Else
                Return True
            End If
        End Operator
    End Class
End Namespace
