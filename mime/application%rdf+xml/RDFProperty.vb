#Region "Microsoft.VisualBasic::0dca58c11282e1b9b981a08528e1228e, mime\application%rdf+xml\RDFProperty.vb"

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

    '   Total Lines: 142
    '    Code Lines: 87 (61.27%)
    ' Comment Lines: 31 (21.83%)
    '    - Xml Docs: 90.32%
    ' 
    '   Blank Lines: 24 (16.90%)
    '     File Size: 4.07 KB


    ' Class RDFProperty
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class RDFType
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetTypeName
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
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' property value with data type
''' </summary>
Public Class RDFProperty : Inherits EntityProperty

    Sub New()
        Call MyBase.New()
    End Sub
End Class

''' <summary>
''' rdf:type
''' </summary>
''' 
<XmlType("type", [Namespace]:=RDFEntity.xmlns_nil)>
Public Class RDFType : Inherits Resource

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
    Public Property value As String()

    Sub New()
    End Sub

    Sub New(value As Object, Optional res As String = Nothing)
        If value Is Nothing Then
            Me.value = {}
            Me.dataType = DataTypes.dtString
        ElseIf value.GetType.IsArray Then
            Me.value = DirectCast(value, Array).AsEnumerable _
                .Select(Function(o)
                            Return Scripting.ToString(o)
                        End Function) _
                .ToArray
            Me.dataType = DataTypes.SchemaDataType(value.GetType)
        Else
            Me.value = {Scripting.ToString(value)}
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
        Return res?.value.JoinBy(vbCrLf)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As Boolean
        If res Is Nothing Then
            Return False
        Else
            Return Boolean.Parse(res.value(0))
        End If
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As Double
        If res Is Nothing Then
            Return 0
        End If

        Return Double.Parse(res.value(0))
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(res As EntityProperty) As Integer
        If res Is Nothing Then
            Return 0
        End If

        Return Integer.Parse(res.value(0))
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator =(res As EntityProperty, str As String) As Boolean
        Return res.value(0) = str
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator <>(res As EntityProperty, str As String) As Boolean
        Return res.value(0) <> str
    End Operator
End Class
