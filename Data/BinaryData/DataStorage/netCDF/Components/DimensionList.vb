Imports System.Xml.Serialization

Namespace netCDF.Components

    Public Class DimensionList

        <XmlAttribute> Public Property recordId As Integer?
        <XmlAttribute> Public Property recordName As String

        Public ReadOnly Property HaveRecordDimension As Boolean
            Get
                Return Not (recordId Is Nothing AndAlso recordName = "NA")
            End Get
        End Property

        Public Property dimensions As Dimension()

        Public Overrides Function ToString() As String
            Return $"[{recordId}] {recordName}"
        End Function
    End Class

    ''' <summary>
    ''' Metadata for the record dimension
    ''' </summary>
    Public Class recordDimension

        ''' <summary>
        ''' Number of elements in the record dimension
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property length As Integer
        ''' <summary>
        ''' Id number In the list Of dimensions For the record dimension
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property id As Integer
        ''' <summary>
        ''' String with the name of the record dimension
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        ''' <summary>
        ''' Number with the record variables step size
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property recordStep As Integer

        Public Overrides Function ToString() As String
            Return $"[{id}] {name} ({recordStep}x{length})"
        End Function
    End Class
End Namespace