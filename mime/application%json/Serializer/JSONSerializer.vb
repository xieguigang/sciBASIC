Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Module JSONSerializer

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetJson(Of T)(obj As T) As String
        Return obj.GetType.GetJson(obj)
    End Function

    <Extension>
    Private Function populateArrayJson(schema As Type, obj As Object) As IEnumerable(Of String)
        Dim elementSchema As Type
        Dim populator As IEnumerable(Of String)

        If schema.IsArray Then
            elementSchema = schema.GetElementType
            populator = From element As Object
                        In DirectCast(obj, Array)
                        Select elementSchema.GetJson(element)
        Else
            ' list of type
            elementSchema = schema.GenericTypeArguments(Scan0)
            populator = From element As Object
                        In DirectCast(obj, IList)
                        Select elementSchema.GetJson(element)
        End If

        Return populator
    End Function

    <Extension>
    Private Function populateObjectJson(schema As Type, obj As Object) As String
        Dim members As New List(Of String)
        ' 会需要忽略掉有<ScriptIgnore>标记的属性
        Dim memberReaders = schema _
            .Schema(PropertyAccess.Readable, nonIndex:=True) _
            .Where(Function(p)
                       Return p.Value.GetAttribute(Of ScriptIgnoreAttribute) Is Nothing
                   End Function)
        Dim [property] As PropertyInfo
        Dim valueType As Type

        For Each reader As KeyValuePair(Of String, PropertyInfo) In memberReaders
            [property] = reader.Value
            valueType = [property].PropertyType
            members += $"""{reader.Key}"": {valueType.GetJson([property].GetValue(obj, Nothing))}"
        Next

        Return $"{{
            {members.JoinBy("," & ASCII.LF)}
        }}"
    End Function

    ''' <summary>
    ''' 所有的字典键都会被强制转换为字符串类型
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="valueSchema"></param>
    ''' <returns></returns>
    <Extension>
    Private Function populateTableJson(obj As IDictionary, valueSchema As Type) As String
        Dim members As New List(Of String)
        Dim key As String
        Dim value As Object

        For Each member In obj
            key = Scripting.ToString(member.Key)
            value = member.Value
            members += $"""{key}"": {valueSchema.GetJson(value)}"
        Next

        Return $"{{
            {members.JoinBy("," & ASCII.LF)}
        }}"
    End Function

    <Extension>
    Public Function GetJson(schema As Type, obj As Object) As String
        If schema.IsArray OrElse schema.IsInheritsFrom(GetType(List(Of )), strict:=False) Then
            Dim elementJSON = schema.populateArrayJson(obj).ToArray

            Return $"[
                {elementJSON.JoinBy(", " & ASCII.LF)}
            ]"
        ElseIf DataFramework.IsPrimitive(schema) Then
            Return JsonContract.GetObjectJson(schema, obj)
        ElseIf schema.IsInheritsFrom(GetType(Dictionary(Of, )), strict:=False) Then
            Dim valueType As Type = schema _
                .GenericTypeArguments _
                .ElementAtOrDefault(
                    index:=1,
                    [default]:=schema.GenericTypeArguments(Scan0)
                )

            Return DirectCast(obj, IDictionary).populateTableJson(valueType)
        Else
            ' isObject
            Return schema.populateObjectJson(obj)
        End If
    End Function
End Module
