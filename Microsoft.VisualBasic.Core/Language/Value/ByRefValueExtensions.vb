#Region "Microsoft.VisualBasic::bb1ee2643f8913577fc3056a8b97d978, Microsoft.VisualBasic.Core\Language\Value\ByRefValueExtensions.vb"

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

    '     Module ByRefValueExtensions
    ' 
    '         Function: (+2 Overloads) First, Split, ToLower
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports ByRefString = Microsoft.VisualBasic.Language.Value(Of String)

Namespace Language.Values

    Public Module ByRefValueExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Split(s As ByRefString, ParamArray delimiter As Char()) As String()
            Return s.Value.Split(delimiter)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToLower(str As ByRefString) As String
            Return Strings.LCase(str.Value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function First(Of T)(list As Value(Of IEnumerable(Of T))) As T
            Return list.Value.First
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function First(str As ByRefString) As Char
            Return str.Value.First
        End Function
    End Module
End Namespace
