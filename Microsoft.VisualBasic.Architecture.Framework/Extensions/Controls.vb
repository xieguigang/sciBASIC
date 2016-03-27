Imports System.Runtime.CompilerServices
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
Public Class ExtendedProps : Inherits DynamicPropertyBase(Of Object)
End Class
