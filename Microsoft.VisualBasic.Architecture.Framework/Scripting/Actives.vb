Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Serialization

Namespace Scripting

    Public Module Actives

        <Extension>
        Public Function DisplType(type As Type) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"**Decalre**:  _{type.FullName}_")
            Call sb.AppendLine("Example: ")
            Call sb.AppendLine(Active(type))

            Return sb.ToString
        End Function

        Public Function Active(type As Type) As String
            Dim obj As Object = Activator.CreateInstance(type)

            For Each prop As PropertyInfo In type.GetProperties.Where(Function(x) x.CanWrite)
                type = prop.PropertyType

                If type.Equals(GetType(String)) Then
                    Call prop.SetValue(obj, "null")
                ElseIf type.IsValueType Then
                    Dim o As Object = Activator.CreateInstance(type)
                    Call prop.SetValue(obj, o)
                Else
                    Call prop.SetValue(obj, Active(type))
                End If
            Next

            Return GetJson(obj, type)
        End Function
    End Module
End Namespace