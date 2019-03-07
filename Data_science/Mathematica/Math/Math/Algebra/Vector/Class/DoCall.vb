Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace LinearAlgebra

    Partial Class Vector

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

            For i As Integer = 0 To length - 1
                inputs = arguments.Select(Function(a) a.Populate).ToArray
                out = info.Invoke(Nothing, inputs)

                Yield DirectCast(out, TOut)
            Next
        End Function
    End Class

    ''' <summary>
    ''' Numeric argument for <see cref="Vector.Call(Of TOut)([Delegate], Argument())"/>
    ''' </summary>
    Public Class Argument

        Dim value As Object
        Dim type As Type
        Dim i As VBInteger

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

        Public Shared Widening Operator CType(x As List(Of Double)) As Argument
            Return New Argument(x)
        End Operator
    End Class
End Namespace