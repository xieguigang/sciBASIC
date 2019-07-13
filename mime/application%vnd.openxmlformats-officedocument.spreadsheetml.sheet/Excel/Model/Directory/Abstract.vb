#Region "Microsoft.VisualBasic::59bf99ac8b302584aeaf6579450981b0, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Model\Directory\Abstract.vb"

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

    '     Class Directory
    ' 
    '         Properties: Folder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Model.Directory

    Public MustInherit Class Directory

        Public ReadOnly Property Folder As String

        Sub New(ROOT$)
            Folder = $"{ROOT}/{_name()}"
            Call _loadContents()
        End Sub

        Protected MustOverride Function _name() As String
        Protected MustOverride Sub _loadContents()

        Public Overrides Function ToString() As String
            Return Folder
        End Function
    End Class
End Namespace
