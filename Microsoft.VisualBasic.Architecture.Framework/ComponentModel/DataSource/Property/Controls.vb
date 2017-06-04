#Region "Microsoft.VisualBasic::653e0433987e989ce48bac0f2a3d83b6, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\Controls.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Dynamic
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization

Namespace ComponentModel.DataSourceModel

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

        <DataIgnored, XmlIgnore, ScriptIgnore>
        Public Property DynamicHashTable As [Property](Of Object)
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

        Sub New()
        End Sub

        Sub New(init As [Property](Of Object))
            __hash = init
        End Sub

        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return DynamicHashTable.Properties.Keys
        End Function

        Dim __nameCache As StringBuilder = New StringBuilder

        Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
            Dim name As String = binder.Name

            If __nameCache.Length > 0 Then
                Call __nameCache.Append("." & name)
                name = __nameCache.ToString
            End If

            If DynamicHashTable.Properties.ContainsKey(name) Then
                Call __nameCache.Clear()
                Return DynamicHashTable.Properties.TryGetValue(name, result)
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

            If DynamicHashTable.Properties.ContainsKey(name) Then
                __nameCache.Clear()
                DynamicHashTable.Properties(name) = value
            Else

            End If

            Return True
        End Function
    End Class
End Namespace
