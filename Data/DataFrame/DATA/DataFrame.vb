Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace DATA

    Public Class DataFrame : Implements IEnumerable(Of EntityObject)

        Dim entityList As Dictionary(Of EntityObject)

        Sub New(list As IEnumerable(Of EntityObject))
            entityList = list.ToDictionary
        End Sub

        Public Function [As](Of T As Class)() As T()
            Return entityList.Values _
                .ToCsvDoc _
                .AsDataSource(Of T)
        End Function

        Public Overrides Function ToString() As String
            Return entityList.Keys.ToArray.GetJson
        End Function

        Public Function SaveTable(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean
            Return entityList.Values.SaveTo(path, encoding:=encoding.CodePage, strict:=False)
        End Function

        Public Shared Function Load(path$, Optional encoding As Encodings = Encodings.Default) As DataFrame
            Return New DataFrame(EntityObject.LoadDataSet(path))
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of EntityObject) Implements IEnumerable(Of EntityObject).GetEnumerator
            For Each x In entityList.Values
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Shared Operator +(data As DataFrame, appends As IEnumerable(Of EntityObject)) As DataFrame
            For Each x As EntityObject In appends
                If data.entityList.ContainsKey(x.ID) Then
                    With data.entityList(x.ID)
                        For Each [property] In x.Properties
                            .ItemValue([property].Key) = [property].Value
                        Next
                    End With
                Else
                    data.entityList += x
                End If
            Next

            Return data
        End Operator
    End Class
End Namespace