Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language

Namespace Serialize.ObjectSchema

    ''' <summary>
    ''' Class object schema
    ''' </summary>
    Public Class [Class] : Implements IEnumerable(Of Field)

        ''' <summary>
        ''' Properties in the class type
        ''' </summary>
        ''' <returns></returns>
        Public Property Fields As Dictionary(Of String, Field)
        ''' <summary>
        ''' raw
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As Type

        Public Overrides Function ToString() As String
            Return "Public Class " & Type.FullName
        End Function

        Public Shared Function GetSchema(Of T)() As [Class]
            Return GetSchema(GetType(T))
        End Function

        ''' <summary>
        ''' Property stack
        ''' </summary>
        ''' <returns></returns>
        Public Property Stack As String

        Friend __writer As Writer

        Public Function GetField(name$) As Field
            If Fields.ContainsKey(name) Then
                Return Fields(name)
            Else
                Return Nothing
            End If
        End Function

        Public Sub Remove(name$)
            If Fields.ContainsKey(name) Then
                Call Fields.Remove(name)
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="stack">Top stack using identifier ``#``</param>
        ''' <returns></returns>
        Public Shared Function GetSchema(type As Type, Optional stack As String = "#") As [Class]
            Dim props As PropertyInfo() =
                type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
            Dim fields As New List(Of Field)

            For Each prop As PropertyInfo In props
                Dim sp = TypeSchemaProvider.GetInterfaces(prop, False, False)
                Dim cls As [Class] = Nothing

                If sp Is Nothing Then  ' 复杂类型，需要建立外部文件的连接
                    Dim pType As Type = prop.PropertyType
                    cls = GetSchema(pType, stack & "::" & prop.Name)
                    sp = Column.CreateObject(New ColumnAttribute(prop.Name), prop)
                Else
                    ' 简单类型，不需要再做额外域的处理
                End If

                fields += New Field With {
                    .Binding = sp,
                    .InnerClass = cls
                }
            Next

            Return New [Class] With {
                .Fields = fields.ToDictionary(Function(k) k.Name),
                .Type = type,
                .Stack = stack
            }
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Field) Implements IEnumerable(Of Field).GetEnumerator
            For Each f As Field In Fields.Values
                Yield f
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace