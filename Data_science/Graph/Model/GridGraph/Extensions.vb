Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace GridGraph

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CreateReadOnlyGrid(points As IEnumerable(Of Point)) As Grid(Of Point)
            Return Grid(Of Point).CreateReadOnly(points.Select(Function(pt) New GridCell(Of Point)(pt, pt)))
        End Function
    End Module
End Namespace