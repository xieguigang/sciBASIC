Imports System.Dynamic
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

' http://www.codeproject.com/Articles/1087921/An-Almost-Extension-Property

''' <summary>
''' Two Extension methods which expose the ExtendedProps class through the label's Tag object is all you need.
''' </summary>
Public Module ControlsExtension

    <Extension> Public Function GetMoreProps(ByRef ctrl As Control) As ExtendedProps
        If Not ctrl.Tag Is Nothing Then
            Dim tagType As Type = ctrl.Tag.GetType

            If tagType.Equals(GetType(ExtendedProps)) OrElse tagType.IsInheritsFrom(GetType(ExtendedProps)) Then
                Return DirectCast(ctrl.Tag, ExtendedProps)
            Else
                Return Nothing
            End If
        Else
            Dim tag As New ExtendedProps
            ctrl.Tag = tag
            Return tag
        End If
    End Function

    <Extension> Public Function SetMoreProps(ByRef ctrl As Control, extraProps As ExtendedProps, Optional [overrides] As Boolean = False) As Boolean
        If Not ctrl.Tag Is Nothing AndAlso Not [overrides] Then
            Call VBDebugger.Warning("The control already has a tag data, and not allowed to overrides....")
            Return False
        Else
            ctrl.Tag = extraProps
            Return True
        End If
    End Function
End Module

''' <summary>
''' An Almost Extension Property
''' </summary>
Public Class ExtendedProps : Inherits Dynamic.DynamicObject

    Dim __hash As [Property](Of Object)

    <XmlIgnore> <ScriptIgnore> Public Property DynamicHash As [Property](Of Object)
        Get
            If __hash Is Nothing Then
                __hash = New [Property](Of Object)
            End If
            Return __hash
        End Get
        Set(value As [Property](Of Object))
            __hash = value
        End Set
    End Property

    Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
        Return DynamicHash.Properties.Keys
    End Function

    Dim __nameCache As StringBuilder = New StringBuilder

    Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
        Dim name As String = binder.Name

        If __nameCache.Length > 0 Then
            Call __nameCache.Append("." & name)
            name = __nameCache.ToString
        End If

        If DynamicHash.Properties.ContainsKey(name) Then
            Call __nameCache.Clear()
            Return DynamicHash.Properties.TryGetValue(name, result)
        Else
            result = Me
            Return True
        End If
    End Function

    Public Overrides Function TrySetMember(binder As SetMemberBinder, value As Object) As Boolean
        Dim name As String = binder.Name

        If __nameCache.Length > 0 Then
            Call __nameCache.Append("." & name)
            name = __nameCache.ToString
        End If

        If DynamicHash.Properties.ContainsKey(name) Then
            __nameCache.Clear()
            DynamicHash.Properties(name) = value
        Else

        End If

        Return True
    End Function
End Class
