#Region "Microsoft.VisualBasic::794b4edb08ff634d8c164504a01c6ff0, Microsoft.VisualBasic.Core\Text\Xml\Models\ValueTuples\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: ToDictionary, ToProperties
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Text.Xml.Models

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToDictionary(Of T)(properties As IEnumerable(Of [Property]), parser As Func(Of String, T)) As Dictionary(Of String, T)
            Return properties.ToDictionary(Function(p) p.name, Function(p) parser(p.value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToProperties(Of T)(table As Dictionary(Of String, T), toString As Func(Of T, String)) As IEnumerable(Of [Property])
            Return table.Select(Function(p) New [Property] With {.name = p.Key, .value = toString(p.Value)})
        End Function
    End Module
End Namespace
