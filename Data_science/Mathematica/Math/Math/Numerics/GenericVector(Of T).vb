#Region "Microsoft.VisualBasic::2207e431f94291f702c63743effd8e86, Data_science\Mathematica\Math\Math\Numerics\GenericVector(Of T).vb"

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

    '   Total Lines: 150
    '    Code Lines: 95 (63.33%)
    ' Comment Lines: 36 (24.00%)
    '    - Xml Docs: 61.11%
    ' 
    '   Blank Lines: 19 (12.67%)
    '     File Size: 5.59 KB


    '     Class GenericVector
    ' 
    '         Properties: [Dim], (+2 Overloads) [GET], SelectWhere
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose, Factor
    ' 
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Scripting.Rscript

    ''' <summary>
    ''' 在这个泛型向量之中,仅包含有对元素对象的访问方法的封装,并没有涉及类型解析等反射操作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class GenericVector(Of T) : Inherits Vector(Of T)
        Implements IEnumerable(Of T)
        Implements IDisposable

        ''' <summary>
        ''' 向量维数，就是向量的长度（元素的个数）
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property [Dim] As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Length
            End Get
        End Property

        Sub New()
            MyBase.New
        End Sub

        Sub New(data As IEnumerable(Of T))
            MyBase.New(data)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the generic type vector class that
        ''' is empty and has the specified initial capacity.
        ''' </summary>
        ''' <param name="capacity">
        ''' The number of elements that the new list can initially store.
        ''' </param>
        Sub New(capacity%)
            MyBase.New(capacity)
        End Sub

        Public Overloads Property SelectWhere(conditions As BooleanVector) As T()
            Get
                Dim LQuery As T() = LinqAPI.Exec(Of T) <=
                    From i As SeqValue(Of Boolean)
                    In conditions.SeqIterator
                    Where i.value = True
                    Select MyBase.Item(i.i)

                Return LQuery
            End Get
            Set(value As T())
                For i As Integer = 0 To conditions.Count - 1
                    If conditions(i) Then
                        Me(i) = value(i)
                    End If
                Next
            End Set
        End Property

        Public Sub Factor(value%)
            ' ReDim Preserve _Elements(value - 1)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">只有一个元素的</param>
        ''' <param name="b">只有一个元素的</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Property [GET](a As Vector, b As Vector) As T()
            Get
                Dim x As Integer = a(0), y As Integer = b(0)
                Dim buffer As T() = New T(y - x - 1) {}
                Call System.Array.ConstrainedCopy(Me.ToArray, x, buffer, 0, buffer.Length)
                Return buffer
            End Get
            Set(value As T())
                Dim x As Integer = a(0), y As Integer = b(0)
                Dim idx As Integer = 0
                For i As Integer = x To y
                    Me(i) = value(idx)
                    idx += 1
                Next
            End Set
        End Property

        Public Overloads Property [GET](a As Integer, b As Vector) As T()
            Get
                Return [GET](New Vector(integers:={a}), b)
            End Get
            Set(value As T())
                [GET](New Vector(integers:={a}), b) = value
            End Set
        End Property

        Public Overloads Shared Operator <>(x As GenericVector(Of T), y As GenericVector(Of T)) As BooleanVector
            Dim LQuery = (From i In x.SeqIterator Select Not i.value.Equals(y(i.i))).ToArray
            Return New BooleanVector(LQuery)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(x As GenericVector(Of T), y As GenericVector(Of T)) As BooleanVector
            Return Not (x <> y)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(v As GenericVector(Of T)) As T()
            Return v.ToArray
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose( disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose( disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
