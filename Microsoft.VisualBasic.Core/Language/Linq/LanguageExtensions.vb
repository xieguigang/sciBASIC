#Region "Microsoft.VisualBasic::b09890ca24056e526d08a965c88bf3e6, Microsoft.VisualBasic.Core\Language\Linq\LanguageExtensions.vb"

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

    '     Module LanguageExtensions
    ' 
    '         Sub: (+5 Overloads) Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Language

    ''' <summary>
    ''' <see cref="List(Of T)"/> initizlize syntax supports
    ''' </summary>
    Public Module LanguageExtensions

        ''' <summary>
        ''' New List From syntax supports
        ''' 
        ''' ```
        ''' {Name, value, Description?}
        ''' ```
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub Add(Of T)(list As List(Of NamedValue(Of T)), name$, value As T, Optional descript$ = Nothing)
            list += New NamedValue(Of T) With {
                .Name = name,
                .Value = value,
                .Description = descript
            }
        End Sub

        ''' <summary>
        ''' From {"1,100", "100,1000", "200,500"}
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="range$"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub Add(list As List(Of IntRange), range$)
            list += range
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub Add(list As List(Of DoubleRange), range$)
            list += range
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Add(list As List(Of NamedValue), name$, value$)
            list += New NamedValue With {
                .name = name,
                .text = value
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Add(list As List(Of [Property]), name$, value$, Optional comment$ = Nothing)
            list += New [Property] With {
                .name = name,
                .value = value,
                .Comment = comment
            }
        End Sub
    End Module
End Namespace
