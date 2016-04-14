Imports System.Runtime.InteropServices
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization

Namespace Language

    ''' <summary>
    ''' The base class object in VisualBasic
    ''' </summary>
    Public Class ClassObject

        ''' <summary>
        ''' The extension property
        ''' </summary>
        ''' <returns></returns>
        <XmlIgnore> <ScriptIgnore> Public Overridable Property Extension As ExtendedProps

        ''' <summary>
        ''' Default is display the json value of the object class
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Serialization.GetJson(Me, [GetType])
        End Function

        ''' <summary>
        ''' String source for operator <see cref="ClassObject.Operator &(ClassObject, String)"/>
        ''' </summary>
        ''' <returns>Default is using <see cref="ToString"/> method as provider</returns>
        Protected Friend Overridable Function __toString() As String
            Return ToString()
        End Function

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Operator &(x As ClassObject, s As String) As String
            Return x.__toString & s
        End Operator

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator &(s As String, x As ClassObject) As String
            Return s & x.__toString
        End Operator

        Protected Sub Copy(ByRef x As ClassObject)
            x = Me
        End Sub

        ''' <summary>
        ''' Example as: Do While Not (target = stream.read(source)) Is Nothing
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="target"></param>
        ''' <returns></returns>
        Public Shared Operator =(<Out> target As ClassObject, source As ClassObject) As ClassObject
            If source Is Nothing Then
                target = Nothing
            Else
                Call source.Copy(target)
            End If
            Return target
        End Operator

        Public Shared Operator <>(source As ClassObject, target As ClassObject) As ClassObject
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace