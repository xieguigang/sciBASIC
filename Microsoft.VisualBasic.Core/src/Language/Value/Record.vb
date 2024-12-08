Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Language

    ''' <summary>
    ''' Data record type in VisualBasic
    ''' </summary>
    Public MustInherit Class Record

        ''' <summary>
        ''' Make check of the value equals?
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(a As Record, b As Record) As Boolean
            If a Is Nothing AndAlso b Is Nothing Then
                Return True
            ElseIf a Is Nothing OrElse b Is Nothing Then
                Return False
            End If

            Static schema As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

            Dim t1 = schema.ComputeIfAbsent(a.GetType, lazyValue:=Function(t) DataFramework.Schema(t, PropertyAccess.Readable, nonIndex:=True))
            Dim t2 = schema.ComputeIfAbsent(b.GetType, lazyValue:=Function(t) DataFramework.Schema(t, PropertyAccess.Readable, nonIndex:=True))

            ' is not identical type, andalso property name is not equals
            ' assert that value is not equals directly
            If a.GetType IsNot b.GetType AndAlso t1.Keys.Union(t2.Keys).Count <> t1.Count Then
                Return False
            End If

            ' has the same identical keys
            For Each name As String In t1.Keys
                If t1(name).PropertyType IsNot t2(name).PropertyType Then
                    If t1(name).PropertyType.IsInheritsFrom(GetType(Record)) AndAlso
                        t2(name).PropertyType.IsInheritsFrom(GetType(Record)) Then

                        If DirectCast(t1(name).GetValue(a), Record) <> DirectCast(t2(name).GetValue(b), Record) Then
                            Return False
                        End If
                    Else
                        Return False
                    End If
                Else
                    Dim val1 As Object = t1(name).GetValue(a)
                    Dim val2 As Object = t2(name).GetValue(b)

                    If Not Object.Equals(val1, val2) Then
                        Return False
                    End If
                End If
            Next

            Return True
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As Record, b As Record) As Boolean
            Return Not a = b
        End Operator

    End Class
End Namespace