#Region "Microsoft.VisualBasic::184be0541f37da381b3437ea1bc1d0c4, Data_science\Mathematica\Math\DataFrame\DataFrame\FeatureVector.vb"

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

    '   Total Lines: 231
    '    Code Lines: 164 (71.00%)
    ' Comment Lines: 29 (12.55%)
    '    - Xml Docs: 96.55%
    ' 
    '   Blank Lines: 38 (16.45%)
    '     File Size: 7.89 KB


    ' Class FeatureVector
    ' 
    '     Properties: isScalar, name, size, type, vector
    ' 
    '     Constructor: (+10 Overloads) Sub New
    '     Function: [TryCast], CastTo, CheckSupports, FromGeneral, GetScalarValue
    '               Getter, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.ValueTypes

''' <summary>
''' the feature column vector
''' </summary>
Public Class FeatureVector : Implements IReadOnlyId

    ''' <summary>
    ''' a generic data vector
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property vector As Array

    ''' <summary>
    ''' the vector element scalar type, example as <see cref="Integer"/>, <see cref="Double"/>, etc...
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property type As Type

    ''' <summary>
    ''' the feature name
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property name As String Implements IReadOnlyId.Identity

    ''' <summary>
    ''' does current vector has no data or just a single value?
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property isScalar As Boolean
        Get
            Return vector Is Nothing OrElse vector.Length = 0 OrElse vector.Length = 1
        End Get
    End Property

    ''' <summary>
    ''' the vector length
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property size As Integer
        Get
            Return vector.Length
        End Get
    End Property

    ''' <summary>
    ''' get element value by index
    ''' </summary>
    ''' <param name="i"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property GetValue(i As Integer) As Object
        Get
            If isScalar Then
                Return vector.GetValue(0)
            Else
                Return vector.GetValue(i)
            End If
        End Get
    End Property

    Private Sub New(name As String)
        Me.name = name
    End Sub

    Sub New(name As String, ints As IEnumerable(Of Integer))
        Call Me.New(name)

        vector = ints.ToArray
        type = GetType(Integer)
    End Sub

    Sub New(name As String, factors As IEnumerable(Of String))
        Call Me.New(name)

        vector = factors.ToArray
        type = GetType(String)
    End Sub

    Sub New(name As String, floats As IEnumerable(Of Single))
        Call Me.New(name)

        vector = floats.ToArray
        type = GetType(Single)
    End Sub

    Sub New(name As String, doubles As IEnumerable(Of Double))
        Call Me.New(name)

        vector = doubles.ToArray
        type = GetType(Double)
    End Sub

    Sub New(name As String, int16 As IEnumerable(Of Short))
        Call Me.New(name)

        vector = int16.ToArray
        type = GetType(Short)
    End Sub

    Sub New(name As String, int64 As IEnumerable(Of Long))
        Call Me.New(name)

        vector = int64.ToArray
        type = GetType(Long)
    End Sub

    Sub New(name As String, logicals As IEnumerable(Of Boolean))
        Call Me.New(name)

        vector = logicals.ToArray
        type = GetType(Boolean)
    End Sub

    Sub New(name As String, times As IEnumerable(Of DateTime))
        Call Me.New(name)

        vector = times.ToArray
        type = GetType(DateTime)
    End Sub

    Sub New(name As String, spans As IEnumerable(Of TimeSpan))
        Call Me.New(name)

        vector = spans.ToArray
        type = GetType(TimeSpan)
    End Sub

    Public Function GetScalarValue() As Object
        If vector Is Nothing OrElse vector.Length = 0 Then
            Return Nothing
        Else
            Return vector(0)
        End If
    End Function

    Public Function Getter() As Func(Of Integer, Object)
        If vector Is Nothing OrElse vector.Length = 0 Then
            Return Function() Nothing
        ElseIf vector.Length = 1 Then
            Dim [single] As Object = GetScalarValue()
            Return Function() [single]
        Else
            Return Function(i) _vector(i)
        End If
    End Function

    Public Function [TryCast](Of T)() As T()
        If GetType(T) Is type Then
            Return DirectCast(vector, T())
        Else
            Select Case GetType(T)
                Case GetType(Double)
                    ' cast value to double [target]
                    Select Case type
                        Case GetType(Integer), GetType(Short), GetType(Long), GetType(Single)
                            Return CastTo(Of Object, Double)(Function(o) CDbl(o))
                        Case GetType(Boolean)
                            Return CastTo(Of Boolean, Double)(Function(b) If(b, 1.0, 0.0))
                        Case GetType(Date)
                            Return CastTo(Of Date, Double)(Function(d) d.UnixTimeStamp)
                        Case GetType(TimeSpan)
                            Return CastTo(Of TimeSpan, Double)(Function(d) d.TotalMilliseconds)
                        Case GetType(String)
                            Return CastTo(Of String, Double)(AddressOf Conversion.Val)
                    End Select
            End Select

            Throw New NotImplementedException(GetType(T).FullName)
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function CastTo(Of T, TOut)(cast As Func(Of T, TOut)) As Object
        Return vector _
            .AsObjectEnumerator _
            .Select(Function(o) cast(o)) _
            .ToArray
    End Function

    Public Overrides Function ToString() As String
        Return $"[{type.Name}] {vector.Length} elements"
    End Function

    Public Shared Function CheckSupports(type As Type) As Boolean
        Static primitiveSupports As Index(Of Type) = {
            GetType(Integer), GetType(Short), GetType(Long),
            GetType(Single), GetType(Double), GetType(Boolean),
            GetType(String)
        }

        Return type Like primitiveSupports
    End Function

    Public Shared Function FromGeneral(name As String, vec As Array) As FeatureVector
        Select Case vec.GetType.GetElementType
            Case GetType(Integer) : Return New FeatureVector(name, DirectCast(vec, Integer()))
            Case GetType(Short) : Return New FeatureVector(name, DirectCast(vec, Short()))
            Case GetType(Long) : Return New FeatureVector(name, DirectCast(vec, Long()))
            Case GetType(Single) : Return New FeatureVector(name, DirectCast(vec, Single()))
            Case GetType(Double) : Return New FeatureVector(name, DirectCast(vec, Double()))
            Case GetType(Boolean) : Return New FeatureVector(name, DirectCast(vec, Boolean()))
            Case GetType(String) : Return New FeatureVector(name, DirectCast(vec, String()))
            Case Else
                Throw New NotImplementedException(vec.GetType.FullName)
        End Select
    End Function

    Public Shared Narrowing Operator CType(col As FeatureVector) As Vector
        If DataFramework.IsNumericType(col.type) Then
            Return New Vector(From xi As Object In col.vector Select CDbl(xi))
        Else
            Throw New InvalidCastException($"{col.type.Name} could not be cast to a number directly!")
        End If
    End Operator

    Public Shared Narrowing Operator CType(col As FeatureVector) As BooleanVector
        If col.type Is GetType(Boolean) Then
            Return New BooleanVector(From xi As Object In col.vector Select CBool(xi))
        Else
            Throw New InvalidCastException($"{col.type.Name} could not be cast to a logical value directly!")
        End If
    End Operator

End Class
