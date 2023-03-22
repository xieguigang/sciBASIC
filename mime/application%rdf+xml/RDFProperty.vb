
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Public Class RDFProperty : Inherits EntityProperty

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
