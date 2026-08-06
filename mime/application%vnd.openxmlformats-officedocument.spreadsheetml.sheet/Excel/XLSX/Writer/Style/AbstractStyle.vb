Imports System.Reflection
Imports System.Text

Namespace XLSX.Writer.Styling

    ''' <summary>
    ''' Class represents an abstract style component
    ''' </summary>
    Public MustInherit Class AbstractStyle : Implements IComparable(Of AbstractStyle)

        ''' <summary>
        ''' Gets or sets the internal ID for sorting purpose in the Excel style document (nullable)
        ''' </summary>
        <Append(Ignore:=True)>
        Public Property InternalID As Integer?

        ''' <summary>
        ''' Abstract method to copy a component (dereferencing)
        ''' </summary>
        ''' <returns>Returns a copied component.</returns>
        Public MustOverride Function Copy() As AbstractStyle

        ''' <summary>
        ''' Internal method to copy altered properties from a source object. The decision whether a property is copied is dependent on a untouched reference object
        ''' </summary>
        ''' <typeparam name="T">Style or sub-class of Style that extends AbstractStyle.</typeparam>
        ''' <param name="source">Source object with properties to copy.</param>
        ''' <param name="reference">Reference object to decide whether the properties from the source objects are altered or not.</param>
        Friend Sub CopyProperties(Of T As AbstractStyle)(source As T, reference As T)
            If source Is Nothing OrElse [GetType]() IsNot source.GetType() AndAlso [GetType]() IsNot reference.GetType() Then
                Throw New StyleException("CopyPropertyException", "The objects of the source, target and reference for style appending are not of the same type")
            End If
            Dim infos As PropertyInfo() = [GetType]().GetProperties()
            Dim sourceInfo As PropertyInfo
            Dim referenceInfo As PropertyInfo
            Dim attributes As IEnumerable(Of AppendAttribute)
            For Each info As PropertyInfo In infos
                attributes = CType(info.GetCustomAttributes(GetType(AppendAttribute)), IEnumerable(Of AppendAttribute))
                If attributes.Any() AndAlso Not HandleProperties(attributes) Then
                    Continue For
                End If
                sourceInfo = source.GetType().GetProperty(info.Name)
                referenceInfo = reference.GetType().GetProperty(info.Name)
                If Not sourceInfo.GetValue(source).Equals(referenceInfo.GetValue(reference)) Then
                    info.SetValue(Me, sourceInfo.GetValue(source))
                End If
            Next
        End Sub

        ''' <summary>
        ''' Method to check whether a property is considered or skipped
        ''' </summary>
        ''' <param name="attributes">Collection of attributes to check.</param>
        ''' <returns>Returns false as soon a property of the collection is marked as ignored or nested.</returns>
        Private Shared Function HandleProperties(attributes As IEnumerable(Of AppendAttribute)) As Boolean
            For Each attribute In attributes
                If attribute.Ignore OrElse attribute.NestedProperty Then
                    Return False ' skip property
                End If
            Next
            Return True
        End Function

        ''' <summary>
        ''' Method to compare two objects for sorting purpose
        ''' </summary>
        ''' <param name="other">Other object to compare with this object.</param>
        ''' <returns>-1 if the other object is bigger. 0 if both objects are equal. 1 if the other object is smaller.</returns>
        Public Function CompareTo(other As AbstractStyle) As Integer Implements IComparable(Of AbstractStyle).CompareTo
            If Not InternalID.HasValue Then
                Return -1
            ElseIf other Is Nothing OrElse Not other.InternalID.HasValue Then
                Return 1
            Else
                Return InternalID.Value.CompareTo(other.InternalID.Value)
            End If
        End Function

        ''' <summary>
        ''' Method to compare two objects for sorting purpose
        ''' </summary>
        ''' <param name="other">Other object to compare with this object.</param>
        ''' <returns>True if both objects are equal, otherwise false.</returns>
        Public Overloads Function Equals(other As AbstractStyle) As Boolean
            Return GetHashCode() = other.GetHashCode()
        End Function

        ''' <summary>
        ''' Append a JSON property for debug purpose (used in the ToString methods) to the passed string builder
        ''' </summary>
        ''' <param name="sb">String builder.</param>
        ''' <param name="name">Property name.</param>
        ''' <param name="value">Property value.</param>
        ''' <param name="terminate">If true, no comma and newline will be appended.</param>
        Friend Shared Sub AddPropertyAsJson(sb As StringBuilder, name As String, value As Object, Optional terminate As Boolean = False)
            sb.Append("""").Append(name).Append(""": ")
            If value Is Nothing Then
                sb.Append("""""")
            Else
                sb.Append("""").Append(value.ToString().Replace("""", "\""")).Append("""")
            End If
            If Not terminate Then
                sb.Append("," & vbLf)
            End If
        End Sub

        ''' <summary>
        ''' Attribute designated to control the copying of style properties
        ''' </summary>
        Public Class AppendAttribute
            Inherits Attribute
            ''' <summary>
            ''' Gets or sets a value indicating whether Ignore
            ''' Indicates whether the property annotated with the attribute is ignored during the copying of properties
            ''' </summary>
            Public Property Ignore As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether NestedProperty
            ''' Indicates whether the property annotated with the attribute is a nested property. Nested properties are ignored during the copying of properties but can be broken down to its sub-properties
            ''' </summary>
            Public Property NestedProperty As Boolean

            ''' <summary>
            ''' Initializes a new instance of the <see cref="AppendAttribute"/> class
            ''' </summary>
            Public Sub New()
                Ignore = False
                NestedProperty = False
            End Sub
        End Class
    End Class
End Namespace