Imports System.Runtime.CompilerServices

Namespace ComponentModel.StoreProcedure

    <HideModuleName>
    Public Module SampleHelper

        ''' <summary>
        ''' get feature dimension
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="[dim]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function dimension(samples As IEnumerable(Of SampleData), [dim] As Integer) As IEnumerable(Of Double)
            Return samples.Select(Function(si) si.features([dim]))
        End Function

    End Module
End Namespace