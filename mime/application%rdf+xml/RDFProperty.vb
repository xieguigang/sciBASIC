#Region "Microsoft.VisualBasic::e597fae89faa802ff51afc46f797d496, mime\application%rdf+xml\RDFProperty.vb"

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

    '   Total Lines: 107
    '    Code Lines: 65 (60.75%)
    ' Comment Lines: 24 (22.43%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 18 (16.82%)
    '     File Size: 3.38 KB


    ' Class RDFProperty
    ' 
    ' 
    ' 
    ' Class EntityProperty
    ' 
    '     Properties: dataType, resource, value
    ' 
    '     Constructor: (+4 Overloads) Sub New
    '     Function: ParseValue, ToString
    '     Operators: <>, =
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Public Class RDFProperty : Inherits EntityProperty

End Class

''' <summary>
''' rdf:type
''' </summary>
''' 
<XmlType("type", [Namespace]:=RDFEntity.XmlnsNamespace)>
Public Class RDFType : Inherits Resource

    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Overrides Property resource As String

    Sub New()
        Call MyBase.New()
    End Sub

    Public Function GetTypeName() As String
        If resource.StringEmpty Then
            Return ""
        Else
            Return resource.Split("#"c).Last
        End If
    End Function

End Class

''' <summary>
''' RDF DataValue
''' </summary>
Public Class EntityProperty

    ''' <summary>
    ''' rdf:datatype
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("datatype", [Namespace]:=RDFEntity.XmlnsNamespace)> Public Property dataType As String
    ''' <summary>
    ''' rdf:resource
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)> Public Property resource As String

    ''' <summary>
    ''' the rdf xml text value
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' ###### 20191102
    ''' 
    ''' Base type '<see cref="RDFProperty"/>' has simpleContent and can only be extended 
    ''' by adding <see cref="XmlAttributeAttribute"/> elements. 
    ''' 
    ''' Please consider changing <see cref="XmlTextAttribute"/> member of the base class to 
    ''' string array.
    ''' </remarks>
    <XmlText>
    Public Property value As String

    Sub New()
    End Sub

    Sub New(value As Object, Optional res As String = Nothing)
        If value Is Nothing Then
            Me.value = ""
            Me.dataType = DataTypes.dtString
        Else
            Me.value = Scripting.ToString(value)
            Me.dataType = DataTypes.SchemaDataType(value.GetType)
        End If

        Me.resource = res
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Sub New(dt As String)
        dataType = dt
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Sub New(type As Type)
        Call Me.New(type.SchemaDataType)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ParseValue() As Object
        Return Scripting.CTypeDynamic(value, Me.SchemaDataType)
    End Function

    Public Overrides Function ToString() As String
        Return $"[resource: {resource}] ({Me.SchemaDataType.FullName}) '{value}'"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As String
        Return res?.value
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As Boolean
        If res Is Nothing Then
            Return False
        Else
            Return Boolean.Parse(res.value)
        End If
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As Double
        Return Double.Parse(res.value)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As Integer
        Return Integer.Parse(res.value)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator =(res As EntityProperty, str As String) As Boolean
        Return res.value = str
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator <>(res As EntityProperty, str As String) As Boolean
        Return res.value <> str
    End Operator
End Class
