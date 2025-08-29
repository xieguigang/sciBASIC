Imports Microsoft.VisualBasic.Linq

Namespace Scripting.BasicR

    Public Module base

        Public Function c(Of T)(ParamArray v As IEnumerable(Of T)()) As T()
            Return v.IteratesALL.ToArray
        End Function

    End Module
End Namespace