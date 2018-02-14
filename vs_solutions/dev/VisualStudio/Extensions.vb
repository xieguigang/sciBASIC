Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function EnumerateSourceFiles(vbproj As String) As IEnumerable(Of String)
        Return vbproj _
            .LoadXml(Of Project) _
            .ItemGroups _
            .Where(Function(items) Not items.Compiles.IsNullOrEmpty) _
            .Select(Function(items)
                        Return items.Compiles _
                            .Where(Function(vb) Not vb.AutoGen) _
                            .Select(Function(vb)
                                        Return vb.Include
                                    End Function)
                    End Function) _
            .IteratesALL
    End Function
End Module
