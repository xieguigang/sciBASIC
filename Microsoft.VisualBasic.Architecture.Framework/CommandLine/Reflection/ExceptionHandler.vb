Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization

Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class ExceptionHelp : Inherits Attribute
        Public Property Documentation As String
        Public Property Debugging As String
        Public Property HelpsLink As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Module ExceptionHandler

    End Module
End Namespace