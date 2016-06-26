Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    Public Module Actives

        <Extension> Public Function DisplType(type As Type) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"**Decalre**:  _{type.FullName}_")
            Call sb.AppendLine("Example: ")
            Call sb.AppendLine(Active(type))

            Return sb.ToString
        End Function

        Public Function Active(type As Type) As String
            Dim obj As Object = type.__active
            Return GetJson(obj, type)
        End Function

        <Extension> Private Function __active(type As Type) As Object
            If type.Equals(GetType(String)) Then
                Return type.FullName
            End If
            If type.Equals(GetType(Char)) Then
                Return "c"c
            End If
            If type.Equals(GetType(Date)) OrElse type.Equals(GetType(DateTime)) Then
                Return Now
            End If
            If ToStrings.ContainsKey(type) Then
                Return Nothing
            End If
            If type.IsInheritsFrom(GetType(Array)) Then
                Dim e As Object = type.GetElementType.__active
                Dim array As Array = System.Array.CreateInstance(type.GetElementType, 1)
                array.SetValue(e, Scan0)
                Return array
            End If

            Dim obj As Object = Activator.CreateInstance(type)

            For Each prop As PropertyInfo In type.GetProperties.Where(
                Function(x) x.CanWrite AndAlso
                x.GetIndexParameters.IsNullOrEmpty)

                Dim value As Object = prop.PropertyType.__active

                Call prop.SetValue(obj, value)
            Next

            Return obj
        End Function
    End Module
End Namespace