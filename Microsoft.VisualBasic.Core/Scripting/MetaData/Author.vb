#Region "Microsoft.VisualBasic::789706b42533a1753c01bc35b32bdc75, Microsoft.VisualBasic.Core\Scripting\MetaData\Author.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Author
    ' 
    '         Properties: value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: EMail
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting.MetaData

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=True, Inherited:=True)>
    Public Class Author : Inherits Attribute

        Public ReadOnly Property value As NamedValue(Of String)

        Sub New(name As String, email As String)
            value = New NamedValue(Of String)(name, email)
        End Sub

        Public Sub EMail()
            Call Diagnostics.Process.Start($"mailto://{value.Value}")
        End Sub

        Public Overrides Function ToString() As String
            Return value.GetJson
        End Function
    End Class
End Namespace
