Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Serialization

    Public Module JsonSerialization

        <Extension> Public Function Json(Of T)(obj As T) As String
            Dim typeDef As Type = GetType(T)
            Dim jbr As StringBuilder = New StringBuilder("{")
            Call jbr.AppendLine(__json(obj, typeDef))
            Call jbr.AppendLine("}")
            Return jbr.ToString
        End Function

        <Extension> Private Function __json(obj As Object, typeDef As Type) As String
            Dim lstProp As PropertyInfo() = typeDef.__getReadProp
            Dim jbr As StringBuilder = New StringBuilder(1024)

            For Each prop As PropertyInfo In lstProp
                If prop.PropertyType.__isValueType Then
                    Dim value As String = Scripting.ToString(prop.GetValue(obj))
                    Call jbr.AppendLine($"""{prop.Name}"" : ""{value}"",")
                ElseIf prop.PropertyType.__isCollection Then
                    Dim source As Object = prop.GetValue(obj, Nothing)
                    Call jbr.AppendLine($"""{prop.Name}"" : [{"{"}{__jsonCollection(source, prop.PropertyType)}{"}"}],")
                Else
                    Dim value As Object = prop.GetValue(obj, Nothing)
                    If Not value Is Nothing Then
                        Dim innerJSON As String = __json(value, prop.PropertyType)
                        Call jbr.AppendLine($"""{prop.Name}"" : {"{"}{innerJSON}{"}"},")
                    End If
                End If
            Next

            If jbr.Length > 2 Then
                Call jbr.Remove(jbr.Length - 2, 2)  ' 去除最后面的两个字符VbCrLf
            End If
            If jbr.Length > 1 AndAlso jbr.ToString.Last = ","c Then
                jbr = jbr.Remove(jbr.Length - 1, 1)
            End If

            Return jbr.ToString
        End Function

        Private Function __jsonCollection(source As Object, type As Type) As String
            Dim array As Object() = LINQ.ToArray(DirectCast(source, IEnumerable))
            Dim elementType As Type = type.GetElementType
            Throw New NotImplementedException
        End Function

        <Extension> Private Function __getReadProp(typeDef As Type) As PropertyInfo()
            Dim lstProp As PropertyInfo() = typeDef.GetProperties(BindingFlags.Public + BindingFlags.Instance)
            lstProp = (From prop As PropertyInfo In lstProp Where prop.CanRead Select prop).ToArray
            Return lstProp
        End Function

        <Extension> Private Function __isCollection(type As Type) As Boolean
            If type.IsInheritsFrom(GetType(Array)) Then
                Return True
            End If
            If type.BaseType.IsInheritsFrom(GetType(List(Of ))) Then
                Return True
            End If
            If type.BaseType.IsInheritsFrom(GetType(Dictionary(Of ,))) Then
                Return True
            End If

            Return False
        End Function

        <Extension> Private Function __isValueType(type As Type) As Boolean
            Return type.IsValueType OrElse String.Equals(type, GetType(String))
        End Function
    End Module
End Namespace