#Region "Microsoft.VisualBasic::14ab68a159339eca4da9b94e67cc896d, Microsoft.VisualBasic.Core\CommandLine\Reflection\Attributes\Attribute.vb"

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

    '     Class UsageAttribute
    ' 
    '         Properties: UsageInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ExampleAttribute
    ' 
    '         Properties: ExampleInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class UsageAttribute : Inherits Attribute

        Public ReadOnly Property UsageInfo As String

        Sub New(usage$)
            UsageInfo = usage
        End Sub

        Public Overrides Function ToString() As String
            Return UsageInfo
        End Function
    End Class

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class ExampleAttribute : Inherits Attribute

        Public ReadOnly Property ExampleInfo As String

        Sub New(note$)
            ExampleInfo = note
        End Sub

        Public Overrides Function ToString() As String
            Return ExampleInfo
        End Function
    End Class
End Namespace
