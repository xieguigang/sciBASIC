#Region "Microsoft.VisualBasic::5e8d1f4cbf48d04d3aa303a224d63a8a, tutorials\core.test\Module2.vb"

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

    ' Module Module2
    ' 
    '     Function: populateNothing
    ' 
    '     Sub: Main
    '     Class List
    ' 
    '         Properties: data
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Module Module2

    Sub Main()

        Dim nnn = TryCast(Nothing, String()).SafeQuery

        Dim list = populateNothing()?.data

        For Each s In list.SafeQuery
        Next
        For Each s In populateNothing()?.data.SafeQuery
        Next

        Pause()
    End Sub

    Public Class List
        Public Property data As String()
    End Class

    Public Function populateNothing() As List
        Return Nothing
    End Function
End Module
