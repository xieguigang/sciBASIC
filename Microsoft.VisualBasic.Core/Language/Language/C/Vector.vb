#Region "Microsoft.VisualBasic::5f4f9cddb13ef03d682594b7938ebda9, Microsoft.VisualBasic.Core\Language\Language\C\Vector.vb"

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

    '     Module Vector
    ' 
    '         Sub: Resize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.C

    Public Module Vector

        <Extension>
        Public Sub Resize(Of T)(ByRef list As List(Of T), len%, Optional fill As T = Nothing)
            Call list.Clear()

            For i As Integer = 0 To len - 1
                Call list.Add(fill)
            Next
        End Sub
    End Module
End Namespace
