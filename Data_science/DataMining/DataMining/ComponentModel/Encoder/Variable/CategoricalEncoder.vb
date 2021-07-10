Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel.Encoder.Variable

    Public Class CategoricalEncoder

        Public Property properties As Dictionary(Of String, String())
            Get
                Return propertyNames _
                    .ToDictionary(Function(d) d.Name,
                                  Function(d)
                                      Return d.Value _
                                          .Select(Function(x) x.value) _
                                          .ToArray
                                  End Function)
            End Get
            Set(value As Dictionary(Of String, String()))
                propertyNames = value _
                    .Select(Function(t)
                                Return New NamedValue(Of (String, String)()) With {
                                    .Name = t.Key,
                                    .Value = t.Value _
                                        .Select(Function(v) ($"{t.Key}.{v}", v)) _
                                        .ToArray
                                }
                            End Function) _
                    .ToArray
            End Set
        End Property

        Dim propertyNames As NamedValue(Of (propertyName$, value$)())()

        Public Shared Function EncodeBinary(content As IEnumerable(Of Categorical), Optional ByRef encoder As CategoricalEncoder = Nothing) As IEnumerable(Of Binary)
            Dim raw As Categorical() = content.ToArray

            If encoder Is Nothing Then
                encoder = CreateEncoder(raw)
            End If

            Return encoder.EncodeBinary(content)
        End Function

        Private Shared Function CreateEncoder(raw As Categorical()) As CategoricalEncoder
            Dim propList As New Dictionary(Of String, String())
            Dim nlen As Integer = raw(Scan0).Length

            For i As Integer = 0 To nlen - 1
#Disable Warning
                propList($"#{i + 1}") = raw _
                    .Select(Function(r) r(i)) _
                    .Distinct _
                    .OrderBy(Function(str) str) _
                    .ToArray
#Enable Warning
            Next

            Return New CategoricalEncoder With {
                .properties = propList
            }
        End Function

        Public Iterator Function EncodeBinary(contents As IEnumerable(Of Categorical)) As IEnumerable(Of Binary)
            For Each item As Categorical In contents
                Dim bin As New List(Of Boolean)

                For i As Integer = 0 To propertyNames.Length - 1
                    Dim dimension = propertyNames(i)
                    Dim value As String = item(i)

                    For j As Integer = 0 To dimension.Value.Length - 1
                        bin.Add(dimension.Value(i).value = value)
                    Next
                Next

                Yield New Binary With {
                    .entityVector = bin.ToArray,
                    .id = item.id
                }
            Next
        End Function
    End Class
End Namespace