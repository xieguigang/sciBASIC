#Region "Microsoft.VisualBasic::8ae3c7b5310b2519bd94e37439e5f69e, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\ProjectType.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 226
    '    Code Lines: 141 (62.39%)
    ' Comment Lines: 42 (18.58%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 43 (19.03%)
    '     File Size: 8.71 KB


    '     Class XmlDocs
    ' 
    '         Properties: Keywords, Name, Remarks, Summary
    ' 
    '         Function: readFieldText
    ' 
    '         Sub: LoadFromNode
    ' 
    '     Class ProjectType
    ' 
    '         Properties: [Namespace]
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: EnsureEvent, EnsureField, EnsureMethod, EnsureProperty, GetEvent
    '                   GetField, getInternal, GetMethods, GetProperties, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Assembly

    Public MustInherit Class XmlDocs

        ''' <summary>
        ''' the member name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        ''' <summary>
        ''' Specifies the summary of the member. Use the &lt;summary> tag to describe a type or 
        ''' a type member. Use &lt;remarks> to add supplemental information to a type description.
        ''' 
        ''' The text For the &lt;summary> tag Is the only source Of information about the type 
        ''' In IntelliSense, And Is also displayed In the Object Browser. 
        ''' </summary>
        ''' <returns></returns>
        Public Property Summary As String
        ''' <summary>
        ''' Use the &lt;remarks> tag to add information about a type, supplementing the information specified with &lt;summary>.
        ''' </summary>
        ''' <returns></returns>
        Public Property Remarks As String
        Public Property Keywords As String

        ''' <summary>
        ''' the xml document text for the generic type parameters of the current
        ''' generic type or generic method
        ''' </summary>
        ''' <returns></returns>
        Public Property TypeParams As typeparam()

        ''' <summary>
        ''' a unify method for read content data from xml
        ''' </summary>
        ''' <param name="xn"></param>
        ''' <remarks>
        ''' summary, remarks, keywords
        ''' </remarks>
        Friend Overridable Sub LoadFromNode(xn As XmlNode)
            ' Name = readFieldText(xn, "name")
            Summary = readFieldText(xn, "summary")
            Remarks = readFieldText(xn, "remarks")
            Keywords = readFieldText(xn, "keywords")
            TypeParams = readTypeParams(xn)
        End Sub

        ''' <summary>
        ''' read the ``typeparam`` nodes of a generic type or a generic method
        ''' </summary>
        ''' <param name="xn"></param>
        ''' <returns></returns>
        Protected Shared Function readTypeParams(xn As XmlNode) As typeparam()
            Dim ns = xn.SelectNodes("typeparam")

            If ns Is Nothing Then
                Return Nothing
            End If

            Dim list As New List(Of typeparam)

            For Each node As XmlNode In ns
                Dim name$ = node.Attributes _
                    .GetNamedItem("name") _
                    .InnerText _
                    .Trim(ASCII.CR, ASCII.LF, " ")

                list.Add(New typeparam With {
                    .name = name,
                    .text = node.InnerText
                })
            Next

            Return list.ToArray
        End Function

        Protected Shared Function readFieldText(xn As XmlNode, nodeKey As String) As String
            Dim textNode As XmlNode = xn.SelectSingleNode(nodeKey)

            If textNode IsNot Nothing Then
                Return textNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            Else
                Return Nothing
            End If
        End Function
    End Class

    ''' <summary>
    ''' A type within a project namespace.
    ''' </summary>
    ''' <remarks>
    ''' Fields和Events都不允许重载，但是属性和函数都可以重载
    ''' </remarks>
    Public Class ProjectType : Inherits XmlDocs

        Protected projectNamespace As ProjectNamespace

        Protected fields As Dictionary(Of String, ProjectMember)
        Protected events As Dictionary(Of String, ProjectMember)

        ''' <summary>
        ''' 因为属性存在参数，所以可能会出现重载的情况
        ''' </summary>
        Protected Friend properties As Dictionary(Of String, List(Of ProjectMember))
        ''' <summary>
        ''' 会出现重载函数，所以这里也应该是一个list
        ''' </summary>
        Protected Friend methods As Dictionary(Of String, List(Of ProjectMember))

        ''' <summary>
        ''' The namespace container of this type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [Namespace]() As ProjectNamespace
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.projectNamespace
            End Get
        End Property

        Sub New()
            properties = New Dictionary(Of String, List(Of ProjectMember))
            methods = New Dictionary(Of String, List(Of ProjectMember))
            fields = New Dictionary(Of String, ProjectMember)
            events = New Dictionary(Of String, ProjectMember)
        End Sub

        Public Sub New(projectNamespace As ProjectNamespace)
            Me.projectNamespace = projectNamespace

            Me.fields = New Dictionary(Of String, ProjectMember)()
            Me.properties = New Dictionary(Of String, List(Of ProjectMember))()
            Me.methods = New Dictionary(Of String, List(Of ProjectMember))()
            Me.events = New Dictionary(Of String, ProjectMember)
        End Sub

        Protected Sub New(type As ProjectType)
            projectNamespace = type.projectNamespace
            events = type.events
            fields = type.fields
            properties = type.properties
            methods = type.methods
            Name = type.Name
            Summary = type.Summary
            Remarks = type.Remarks
        End Sub

        Friend Sub New(t1 As ProjectType, t2 As ProjectType)
            projectNamespace = t1.projectNamespace
            Call t1.ensureTables()
            Call t2.ensureTables()
            fields = (t1.fields.Values.AsList + t2.fields.Values).GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.Sum(Me))
            events = (t1.events.Values.AsList + t2.events.Values).GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.Sum(Me))
            properties = (t1.properties.Values.AsList + t2.properties.Values).IteratesALL.GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.ToList)
            methods = (t1.methods.Values.AsList + t2.methods.Values).IteratesALL.GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.ToList)
        End Sub

        ''' <summary>
        ''' 保证四个成员表都已经初始化，避免默认构造函数或者合并路径下出现空引用异常
        ''' </summary>
        Private Sub ensureTables()
            If fields Is Nothing Then fields = New Dictionary(Of String, ProjectMember)
            If events Is Nothing Then events = New Dictionary(Of String, ProjectMember)
            If properties Is Nothing Then properties = New Dictionary(Of String, List(Of ProjectMember))
            If methods Is Nothing Then methods = New Dictionary(Of String, List(Of ProjectMember))
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMethods(methodName As String) As List(Of ProjectMember)
            Return getInternal(methods, methodName.ToLower)
        End Function

        Friend Function EnsureMethod(methodName As String) As ProjectMember
            Dim pmlist As List(Of ProjectMember) = Me.GetMethods(methodName)
            Dim pm As New ProjectMember(Me) With {
                .Name = methodName
            }

            Call pmlist.Add(pm)

            Return pm
        End Function

        Private Shared Function getInternal(ByRef table As Dictionary(Of String, List(Of ProjectMember)), name$) As List(Of ProjectMember)
            If table Is Nothing Then
                table = New Dictionary(Of String, List(Of ProjectMember))
            End If

            If table.ContainsKey(name) Then
                Return table(name)
            Else
                Dim list As New List(Of ProjectMember)
                table.Add(name, list)
                Return list
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetProperties(propertyName As String) As List(Of ProjectMember)
            Return getInternal(properties, propertyName.ToLower)
        End Function

        ''' <summary>
        ''' all of the methods that is declared in this type, the overloads are flattened
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AllMethods As IEnumerable(Of ProjectMember)
            Get
                Call ensureTables()
                Return methods.Values.IteratesALL
            End Get
        End Property

        ''' <summary>
        ''' all of the properties that is declared in this type, the overloads are flattened
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AllProperties As IEnumerable(Of ProjectMember)
            Get
                Call ensureTables()
                Return properties.Values.IteratesALL
            End Get
        End Property

        ''' <summary>
        ''' all of the fields that is declared in this type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AllFields As IEnumerable(Of ProjectMember)
            Get
                Call ensureTables()
                Return fields.Values
            End Get
        End Property

        ''' <summary>
        ''' all of the events that is declared in this type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AllEvents As IEnumerable(Of ProjectMember)
            Get
                Call ensureTables()
                Return events.Values
            End Get
        End Property

        Friend Function EnsureProperty(propertyName As String) As ProjectMember
            Dim pmlist As List(Of ProjectMember) = Me.GetProperties(propertyName)
            Dim pm As New ProjectMember(Me) With {
                .Name = propertyName
            }

            Call pmlist.Add(pm)

            Return pm
        End Function

        Public Function GetField(fieldName As String) As ProjectMember
            Call Me.ensureTables()

            If Me.fields.ContainsKey(fieldName.ToLower()) Then
                Return Me.fields(fieldName.ToLower())
            End If

            Return Nothing
        End Function

        Friend Function EnsureField(fieldName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetField(fieldName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = fieldName
                }

                Me.fields.Add(fieldName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Function GetEvent(eventName As String) As ProjectMember
            Call Me.ensureTables()

            If Me.events.ContainsKey(eventName.ToLower()) Then
                Return Me.events(eventName.ToLower())
            End If

            Return Nothing
        End Function

        Friend Function EnsureEvent(eventName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetEvent(eventName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = eventName
                }

                Me.events.Add(eventName.ToLower(), pm)
            End If

            Return pm
        End Function
    End Class
End Namespace
