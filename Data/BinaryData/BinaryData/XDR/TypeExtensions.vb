Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Xdr
    Public Module TypeExtensions
        <Extension()>
        Public Function GetAttr(Of T As Attribute)(mi As MemberInfo) As T
            Return TryCast(mi.GetCustomAttributes(GetType(T), True).FirstOrDefault(), T)
        End Function

        <Extension()>
        Public Function GetAttrs(Of T As Attribute)(mi As MemberInfo) As IEnumerable(Of T)
            Return mi.GetCustomAttributes(GetType(T), True).Cast(Of T)()
        End Function

        <Extension()>
        Public Function NullableSubType(type As Type) As Type
            If Not type.IsGenericType Then Return Nothing
            If type.GetGenericTypeDefinition() IsNot GetType(Nullable(Of)) Then Return Nothing
            Return type.GetGenericArguments()(0)
        End Function

        <Extension()>
        Public Function ArraySubType(type As Type) As Type
            If Not type.HasElementType Then Return Nothing
            Dim itemType As Type = type.GetElementType()
            If itemType Is Nothing OrElse itemType.MakeArrayType() IsNot type Then Return Nothing
            Return itemType
        End Function

        <Extension()>
        Public Function ListSubType(type As Type) As Type
            If Not type.IsGenericType Then Return Nothing
            Dim genericType As Type = type.GetGenericTypeDefinition()
            If genericType IsNot GetType(List(Of)) Then Return Nothing
            Return type.GetGenericArguments()(0)
        End Function
    End Module
End Namespace
