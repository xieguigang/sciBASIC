Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace Text.Xml

    Public Module XmlElementExtensions

        <Extension()>
        Public Sub SetAttribute(element As XmlElement, name As String, value As Double)
            element.SetAttribute(name, value.ToString(CultureInfo.InvariantCulture))
        End Sub

        <Extension()>
        Public Sub SetAttribute(element As XmlElement, name As String, value As Integer?)
            If value.HasValue Then
                element.SetAttribute(name, value.ToString())
            Else
                element.RemoveAttribute(name)
            End If
        End Sub

        <Extension()>
        Public Function GetAttribute(Of T)(element As XmlElement, name As String, defaultValue As T) As T
            If Not element.HasAttribute(name) Then Return defaultValue
            Try
                Return Convert.ChangeType(element.GetAttribute(name), GetType(T), CultureInfo.InvariantCulture)
            Catch
                Return defaultValue
            End Try
        End Function
    End Module
End Namespace
