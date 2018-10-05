Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Serialization.JSON

    ''' <summary>
    ''' Extension helpers for deal with the anonymous type
    ''' </summary>
    Public Module AnonymousJSONExtensions

        <Extension>
        Public Function GetJson(obj As String(,)) As String
            With New Dictionary(Of String, String)
                For Each prop As String() In obj.RowIterator
                    Call .Add(prop(0), prop(1))
                Next

                Return .GetJson
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetJson(array As IEnumerable(Of String), Optional indent As Boolean = False) As String
            Return array.ToArray.GetJson(indent:=indent)
        End Function

        <Extension>
        Public Function AnonymousJSON(Of T As Class)(obj As T) As String
            Dim keys = obj.GetType.GetProperties(PublicProperty)

            With New Dictionary(Of String, String)
                For Each key As PropertyInfo In keys
                    Call .Add(key.Name, key.GetValue(obj).ToString)
                Next

                Return .GetJson
            End With
        End Function
    End Module
End Namespace