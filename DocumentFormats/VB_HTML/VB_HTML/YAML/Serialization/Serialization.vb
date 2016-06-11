Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.MarkupLanguage.YAML.Grammar
Imports Microsoft.VisualBasic.MarkupLanguage.YAML.Syntax
Imports Microsoft.VisualBasic.Scripting

Namespace YAML

    Public Module Serialization

        <Extension>
        Public Function LoadYAML(Of T)(path As String) As T
            Dim input As New TextInput(path.GET)
            Dim success As Boolean
            Dim parser As New YamlParser()
            Dim yamlStream As YamlStream = parser.ParseYamlStream(input, success)

            If success Then
                Return yamlStream.Load(Of T)
            Else
                Dim ex As New Exception(parser.GetEorrorMessages())
                Throw New Exception(path.ToFileURL, ex)
            End If
        End Function

        <Extension>
        Public Function Load(Of T)(yaml As YamlStream) As T
            Dim type As Type = GetType(T)
            Dim maps As Dictionary(Of MappingEntry) = yaml.Enumerative.FirstOrDefault

            If maps Is Nothing Then
                Return Nothing
            Else
                Return DirectCast(maps.__setMaps(type), T)
            End If
        End Function

        <Extension>
        Private Function __setMaps(maps As Dictionary(Of MappingEntry), type As Type) As Object
            Dim obj As Object = Activator.CreateInstance(type)
            Dim schema As BindProperty(Of DataFrameColumnAttribute)() =
                DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True) _
                                        .Values _
                                        .ToArray
            Dim value As Object
            Dim s As String

            For Each prop As BindProperty(Of DataFrameColumnAttribute) In schema
                If Not maps.ContainsKey(prop.Identity) Then
                    Continue For
                End If

                If prop.IsPrimitive Then
                    s = maps(prop.Identity).Value.ToString
                    value = Scripting.CTypeDynamic(s, prop.Type)
                Else
                    Try
                        Dim subMaps As Dictionary(Of MappingEntry)
                        value = maps(prop.Identity).Value
                        subMaps = New Dictionary(Of MappingEntry)(DirectCast(value, Mapping).Enties)
                        value = subMaps.__setMaps(prop.Type)
                    Catch ex As Exception
                        ex = New Exception($"Dim {prop.Identity} As {prop.Type.FullName}", ex)
                        ex = New Exception(type.FullName, ex)
#If DEBUG Then
                        Call ex.PrintException
#End If
                        Throw ex
                    End Try
                End If

                Call prop.SetValue(obj, value)
            Next

            Return obj
        End Function

        <Extension>
        Public Function WriteYAML(Of T)(
                        obj As T,
                        path As String,
               Optional encoding As Encodings = Encodings.Unicode) _
                                 As Boolean

            Dim sb As New StringBuilder(1024)
            Dim schema = DataFrameColumnAttribute.LoadMapping(Of T)(mapsAll:=True)

            Throw New NotImplementedException
        End Function
    End Module
End Namespace