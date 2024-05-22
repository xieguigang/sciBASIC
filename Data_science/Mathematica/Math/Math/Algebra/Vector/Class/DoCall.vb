#Region "Microsoft.VisualBasic::2be909ef2f2b250086f753b22bff18ab, Data_science\Mathematica\Math\Math\Algebra\Vector\Class\DoCall.vb"

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

    '   Total Lines: 199
    '    Code Lines: 145 (72.86%)
    ' Comment Lines: 20 (10.05%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 34 (17.09%)
    '     File Size: 7.61 KB


    '     Class Vector
    ' 
    '         Function: (+6 Overloads) [Call]
    '         Class Argument
    ' 
    '             Properties: IsArray, IsCollection, IsPrimitive, IsVector, Length
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: [GetType], Populate
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports ArgumentAttribute = Microsoft.VisualBasic.CommandLine.Reflection.ArgumentAttribute

Namespace LinearAlgebra

    Partial Class Vector

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Call](method As Func(Of Double, Double), x As Argument) As Vector
            Return Vector.Call(Of Double)(method, x).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Call](method As Func(Of Double, Double, Double), x As Argument, y As Argument) As Vector
            Return Vector.Call(Of Double)(method, x, y).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Call](method As Func(Of Double, Double, Double, Double), x As Argument, y As Argument, z As Argument) As Vector
            Return Vector.Call(Of Double)(method, x, y, z).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Call](method As Func(Of Double, Double, Double, Double, Double), ParamArray args As Argument()) As Vector
            Return Vector.Call(Of Double)(method, args).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Call](method As Func(Of Double, Double, Double, Double, Double, Double), ParamArray args As Argument()) As Vector
            Return Vector.Call(Of Double)(method, args).AsVector
        End Function

        ''' <summary>
        ''' Try to call target <paramref name="method"/> in vector mode.
        ''' </summary>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="method">只可以是静态的共享方法</param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Shared Iterator Function [Call](Of TOut)(method As [Delegate], ParamArray args As Argument()) As IEnumerable(Of TOut)
            Dim info As MethodInfo = method.Method
            Dim arguments As New List(Of Argument)
            Dim argValue = Value(Of Argument).Default
            Dim params As ParameterInfo() = info.GetParameters

            For i As Integer = 0 To params.Length - 1
                If (argValue = args.ElementAtOrDefault(i)) Is Nothing Then
                    If params(i).IsOptional Then
                        arguments += New Argument(params(i).DefaultValue)
                    Else
                        arguments += New Argument(Nothing)
                    End If
                Else
                    arguments += argValue
                End If
            Next

            Dim length%
            Dim inputs As Object()
            Dim out As Object

            With arguments.Where(Function(a) a.Length > 1) _
                          .Select(Function(a) a.Length) _
                          .ToArray

                If .Length > 1 AndAlso .Any(Function(n) n <> .First) Then
                    Throw New Exception("Element length is not agree!")
                Else
                    length = .First
                End If
            End With

            Dim target As Object = Nothing

            If info.Attributes.HasFlag(MethodAttributes.SpecialName) Then
                target = Activator.CreateInstance(info.DeclaringType)
            End If

            For i As Integer = 0 To length - 1
                inputs = arguments _
                    .Select(Function(a) a.Populate) _
                    .ToArray
                out = info.Invoke(target, inputs)

                Yield DirectCast(out, TOut)
            Next
        End Function

        ''' <summary>
        ''' Numeric argument for <see cref="Vector.Call(Of TOut)([Delegate], Argument())"/>
        ''' </summary>
        ''' <remarks>
        ''' ###### 2019-03-07 
        ''' 因为这个类的名称会与<see cref="ArgumentAttribute"/>产生冲突，所以将这个对象移到内部了
        ''' </remarks>
        Public Class Argument

            Dim value As Object
            Dim type As Type
            Dim i As i32

            Public ReadOnly Property Length As Integer

            Public ReadOnly Property IsPrimitive As Boolean
                Get
                    Return DataFramework.IsPrimitive(type)
                End Get
            End Property

            Public ReadOnly Property IsVector As Boolean
                Get
                    Return type Is GetType(Vector)
                End Get
            End Property

            Public ReadOnly Property IsCollection As Boolean
                Get
                    Return Not IsVector AndAlso Not IsArray AndAlso Not IsPrimitive
                End Get
            End Property

            Public ReadOnly Property IsArray As Boolean
                Get
                    Return type Is GetType(Double())
                End Get
            End Property

            Friend Sub New(value As Object)
                Me.value = value

                If value Is Nothing Then
                    Me.type = Nothing
                    Me.Length = 1
                Else
                    Me.type = value.GetType
                End If

                If Not IsPrimitive AndAlso Not type Is Nothing Then
                    ' is array/vector/collection
                    If IsArray Then
                        Me.Length = DirectCast(value, Array).Length
                    ElseIf IsVector Then
                        Me.Length = DirectCast(value, Vector).Length
                    Else
                        Me.Length = DirectCast(value, IList).Count
                    End If

                    i = 0
                Else
                    ' is a number value
                    Me.Length = 1
                End If
            End Sub

            ''' <summary>
            ''' Iterator function
            ''' </summary>
            ''' <returns></returns>
            Public Function Populate() As Object
                If value Is Nothing Then
                    Return Nothing
                End If

                If IsPrimitive Then
                    Return value
                ElseIf IsVector Then
                    Return DirectCast(value, Vector)(++i)
                ElseIf IsArray Then
                    Return DirectCast(value, Array).GetValue(++i)
                Else
                    Return DirectCast(value, IList).Item(++i)
                End If
            End Function

            Public Overloads Function [GetType]() As Type
                Return type
            End Function

            Public Shared Widening Operator CType(x As Double) As Argument
                Return New Argument(x)
            End Operator

            Public Shared Widening Operator CType(x As Vector) As Argument
                Return New Argument(x)
            End Operator

            Public Shared Widening Operator CType(x As Double()) As Argument
                Return New Argument(x)
            End Operator

            Public Shared Widening Operator CType(x As System.Collections.Generic.List(Of Double)) As Argument
                Return New Argument(x)
            End Operator
        End Class
    End Class
End Namespace
