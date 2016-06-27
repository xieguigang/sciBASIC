Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Collection IO extensions
''' </summary>
Public Module CollectionIO

    Public Delegate Function ISave(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
    Public Delegate Function IRead(type As Type, path As String, encoding As Encoding) As IEnumerable

    Public ReadOnly Property DefaultHandle As ISave = AddressOf SaveJSON
    Public ReadOnly Property DefaultLoadHandle As IRead = AddressOf ReadJSON

    Public Sub SetHandle(handle As ISave)
        _DefaultHandle = handle
    End Sub

    Public Function ReadJSON(type As Type, path As String, encoding As Encoding) As IEnumerable
        Dim text As String = path.ReadAllText(encoding)
        type = type.MakeArrayType
        Return DirectCast(JsonContract.LoadObject(text, type), IEnumerable)
    End Function

    Public Function SaveJSON(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
        Return GetJson(obj, obj.GetType).SaveTo(path, encoding)
    End Function

    Public Function SaveXml(obj As IEnumerable, path As String, encoding As Encoding) As Boolean
        Return GetXml(obj, obj.GetType).SaveTo(path, encoding)
    End Function

    Public Function [TypeOf](Of T)() As [Class](Of T)
        Dim cls As New [Class](Of T)
        Return cls
    End Function
End Module
