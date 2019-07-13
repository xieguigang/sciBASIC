#Region "Microsoft.VisualBasic::02a6b956c15a6bc900237956efeeadbb, Microsoft.VisualBasic.Core\My\JavaScript\UnionType.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class UnionType
    ' 
    '         Properties: IsLambda
    ' 
    '         Function: [GetType], ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace My.JavaScript

    ''' <summary>
    ''' Union type of the value and its value generator function.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class UnionType(Of T)

        Public value As T
        Public lambda As Func(Of T)
        Public lambda1 As Func(Of Object, T)
        Public lambda2 As Func(Of Object, Object, T)
        Public lambda3 As Func(Of Object, Object, Object, T)

        Default Public ReadOnly Property Eval(a As Object) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lambda1(a)
            End Get
        End Property

        Default Public ReadOnly Property Eval(a As Object, b As Object) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lambda2(a, b)
            End Get
        End Property

        Default Public ReadOnly Property Eval(a As Object, b As Object, c As Object) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return lambda3(a, b, c)
            End Get
        End Property

        Public ReadOnly Property IsLambda As Boolean
            Get
                With {
                    lambda, lambda1, lambda2, lambda3
                }
                    Return .Any(Function(f) Not f Is Nothing)
                End With
            End Get
        End Property

        Public Overloads Function [GetType]() As Type
            If IsLambda Then
                ' 是一个函数
                Return GetType(Func(Of T))
            Else
                Return GetType(T)
            End If
        End Function

        Public Overrides Function ToString() As String
            If Me.GetType Is GetType(T) Then
                Return $"Dim <Anonymous> As {GetType(T).FullName} = {value.ToString}"
            Else
                Return $"<Anonymous> Function(...) As {GetType(T).FullName}"
            End If
        End Function

        Public Shared Narrowing Operator CType(uv As UnionType(Of T)) As T
            If uv.GetType Is GetType(T) Then
                Return uv.value
            ElseIf Not uv.lambda Is Nothing Then
                Return uv.lambda()
            Else
                Throw New InvalidCastException()
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(val As T) As UnionType(Of T)
            Return New UnionType(Of T) With {.value = val}
        End Operator
    End Class
End Namespace
