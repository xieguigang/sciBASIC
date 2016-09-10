Imports System.Linq
Imports System.Runtime.CompilerServices

Namespace Emit.Delegates

    Public Module TypeExtensions

        <Extension>
        Public Function CanBeAssignedFrom(destination As Type, source As Type) As Boolean
            If source Is Nothing OrElse destination Is Nothing Then
                Return False
            End If

            If destination Is source OrElse source.IsSubclassOf(destination) Then
                Return True
            End If

            If destination.IsInterface Then
                Return source.ImplementsInterface(destination)
            End If

            If Not destination.IsGenericParameter Then
                Return False
            End If

            Dim constraints = destination.GetGenericParameterConstraints()
            Return constraints.All(Function(t1) t1.CanBeAssignedFrom(source))
        End Function

        <Extension> Public Function ImplementsInterface(source As Type, interfaceType As Type) As Boolean
            While source IsNot Nothing
                Dim interfaces = source.GetInterfaces()
                If interfaces.Any(Function(i) i Is interfaceType OrElse i.ImplementsInterface(interfaceType)) Then
                    Return True
                End If

                source = source.BaseType
            End While
            Return False
        End Function
    End Module
End Namespace
