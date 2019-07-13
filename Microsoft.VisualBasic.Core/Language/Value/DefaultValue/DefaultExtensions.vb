#Region "Microsoft.VisualBasic::ffe3c76b2706ef78a83bb46534112e17, Microsoft.VisualBasic.Core\Language\Value\DefaultValue\DefaultExtensions.vb"

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

    '     Module DefaultExtensions
    ' 
    '         Function: BaseName, NormalizePathString, Replace, Split, ToLower
    '                   TrimSuffix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Namespace Language.Default

    Public Module DefaultExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToLower(str As [Default](Of String)) As String
            Return Strings.LCase(str.value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Split(str As DefaultString, deli$, Optional ignoreCase As Boolean = False) As String()
            Return Splitter.Split(str.DefaultValue, deli, True, compare:=StringHelpers.IgnoreCase(flag:=ignoreCase))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Replace(str As DefaultString, find$, replaceAs$) As String
            Return str.DefaultValue.Replace(find, replaceAs)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BaseName(path As DefaultString) As String
            Return path.DefaultValue.BaseName
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TrimSuffix(path As DefaultString) As String
            Return path.DefaultValue.TrimSuffix
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NormalizePathString(path As DefaultString, Optional alphabetOnly As Boolean = True) As String
            Return path.DefaultValue.NormalizePathString(alphabetOnly)
        End Function
    End Module
End Namespace
