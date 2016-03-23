Imports System.Text

''' <summary>
''' Collection IO extensions
''' </summary>
Public Module CollectionIO

    Public Delegate Function ISave(obj As IEnumerable, path As String, encoding As Encoding) As Boolean

    Public ReadOnly Property DefaultHandle As ISave = AddressOf SaveJSON

    Public Sub SetHandle(handle As ISave)
        _DefaultHandle = handle
    End Sub

    Public Function SaveJSON(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
        Return Serialization.GetJson(obj, obj.GetType).SaveTo(path, encoding)
    End Function

    Public Function SaveXml(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
        Return GetXml(obj, obj.GetType).SaveTo(path, encoding)
    End Function
End Module
