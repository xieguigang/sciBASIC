Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly.XmlDocs
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.Linq

Module ObjectParser

    <Extension>
    Private Function ParseSchemaWithIgnores(schema As Type, opt As JSONSerializerOptions) As KeyValuePair(Of String, PropertyInfo)()
        Dim key As String = $"{schema.FullName}+{opt.createUniqueKey}"

        Static cache As New Dictionary(Of String, KeyValuePair(Of String, PropertyInfo)())

        If Not cache.ContainsKey(key) Then
            cache(key) = schema _
                .Schema(PropertyAccess.Readable, nonIndex:=True) _
                .Where(Function(p)
                           If opt.maskReadonly AndAlso Not p.Value.CanWrite Then
                               Return False
                           End If

                           Dim reader = p.Value
                           Dim ignores1 = reader.GetAttribute(Of ScriptIgnoreAttribute) Is Nothing
                           Dim ignores2 = reader.GetAttribute(Of DataIgnoredAttribute) Is Nothing
                           Dim ignores3 = reader.GetAttribute(Of IgnoreDataMemberAttribute) Is Nothing

                           Return ignores1 AndAlso ignores2 AndAlso ignores3
                       End Function) _
                .ToArray
        End If

        Return cache(key)
    End Function

    <Extension>
    Private Function LoadXmlDocs(schema As Type) As ProjectType
        Dim asmFile = schema.Assembly.Location
        Dim xmlfile As String

        If asmFile.StringEmpty(, True) Then
            ' is runtime in-memory generated clr assembly
            Return Nothing
        Else
            xmlfile = asmFile.ChangeSuffix("xml")
        End If

        If xmlfile.FileExists Then
            Dim proj As Project = ProjectSpace.CreateDocProject(xmlfile)
            Dim ns = proj.GetNamespace(schema.Namespace)
            Dim schemaDoc = ns.GetType(schema.Name)

            Return schemaDoc
        Else
            ' xml docs file is missing from the filesystem
            ' so no data for get comment text
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Friend Function populateObjectJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As JsonElement
        ' 会需要忽略掉有<ScriptIgnore>标记的属性
        Dim memberReaders As KeyValuePair(Of String, PropertyInfo)() = schema.ParseSchemaWithIgnores(opt)
        Dim [property] As PropertyInfo
        Dim valueType As Type
        Dim json As New JsonObject
        Dim valObj As Object
        Dim jsonVal As JsonElement
        Dim isNullable As Boolean = False

        If memberReaders.Any(Function(a) a.Value.Name = "HasValue") AndAlso
            memberReaders.Any(Function(a) a.Value.Name = "Value") Then

            isNullable = True
        End If

        If isNullable Then
            Dim elementType As Type = schema.GenericTypeArguments.FirstOrDefault

            obj = memberReaders _
                .First(Function(a) a.Value.Name = "Value").Value _
                .GetValue(obj, Nothing)
            schema = obj.GetType

            If Not elementType Is Nothing Then
                If DataFramework.IsPrimitive(elementType) Then
                    Return New JsonValue(obj)
                End If
            End If

            Return populateObjectJson(schema, obj, opt)
        End If

        Dim metadata As PropertyInfo = DynamicMetadataAttribute.GetMetadata(memberReaders.Select(Function(p) p.Value))
        Dim comment As String = Nothing
        Dim docs As ProjectType = If(opt.comment, schema.LoadXmlDocs, Nothing)

        For Each reader As KeyValuePair(Of String, PropertyInfo) In memberReaders
            If opt.comment Then
                comment = docs.GetProperties(reader.Value.Name) _
                    .SafeQuery _
                    .Select(Function(p) p.Summary) _
                    .JoinBy(vbLf) _
                    .Trim(" "c, vbLf, vbCr, vbTab)
            End If

            [property] = reader.Value
            valueType = [property].PropertyType
            valObj = [property].GetValue(obj)

            If metadata IsNot Nothing AndAlso [property] Is metadata Then
                Dim js As IDictionary = TryCast(valObj, IDictionary)

                ' expends current dictionary property value as 
                ' object properties
                ' handling of the dynamics metadata property
                If Not js Is Nothing Then
                    For Each key As Object In js.Keys
                        Dim keystr As String = key.ToString
                        Dim value As Object = js(key)

                        If json.HasObjectKey(keystr) Then
                            keystr = "metadata:" & keystr
                        End If

                        If value Is Nothing Then
                            Call json.Add(keystr, JsonValue.NULL)
                        Else
                            Call json.Add(keystr, value.GetType.GetJsonElement(value, opt))
                        End If
                    Next

                    If opt.comment AndAlso Not comment.StringEmpty Then
                        Call $"can not comment on the dynamics metadata property '{schema.Name}::{reader.Value.Name}' while required generates hjson style document.".warning
                    End If
                End If
            Else
                If valueType.IsInterface OrElse
                    valueType Is GetType(Object) OrElse
                    valueType.IsAbstract Then

                    If valObj Is Nothing Then
                        valueType = GetType(Object)
                    Else
                        valueType = valObj.GetType
                    End If
                End If

                If valObj Is Nothing Then
                    jsonVal = JsonValue.NULL
                ElseIf Not (valueType Is GetType(Type) OrElse
                    valueType Is GetType(TypeInfo) OrElse
                    valueType.FullName = "System.RuntimeType" OrElse
                    valueType.FullName = "System.Reflection.RuntimeAssembly") Then

                    jsonVal = valueType.GetJsonElement(valObj, opt)
                Else
                    jsonVal = Nothing
                End If

                If Not jsonVal Is Nothing Then
                    Call json.Add(reader.Key, jsonVal, comment)
                End If
            End If
        Next

        Return json
    End Function

End Module
