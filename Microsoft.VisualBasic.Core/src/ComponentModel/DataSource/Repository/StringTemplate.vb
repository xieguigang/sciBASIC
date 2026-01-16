Imports System.Runtime.CompilerServices
Imports System.Text

Namespace ComponentModel.DataSourceModel.Repository

    Public Interface IKeyDataReader

        Function GetData(key As String) As String

    End Interface

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' template syntax example:
    ''' 
    ''' &lt;locus_tag>|taxid|&lt;product>
    ''' </remarks>
    Public Class StringTemplate

        ReadOnly template As String
        ReadOnly defaults As Dictionary(Of String, String)
        ''' <summary>
        ''' a collection mapping of [key -> &lt;key>] for get value and make string template interpolation
        ''' </summary>
        ReadOnly keys As NamedValue(Of String)()
        ReadOnly missing As New List(Of String)

        Sub New(template As String, Optional defaults As Dictionary(Of String, String) = Nothing)
            Me.template = template
            Me.defaults = If(defaults, New Dictionary(Of String, String))
            Me.keys = ParseKeys(template).ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Iterator Function ParseKeys(template As String) As IEnumerable(Of NamedValue(Of String))
            For Each placeholder As String In template.Matches("[<].*?[>]", RegexICSng)
                Yield New NamedValue(Of String)(placeholder.GetStackValue("<", ">"), placeholder)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetDefaultKey(key As String, data As String)
            defaults(key) = data
        End Sub

        Public Function GetLastMissingKeys() As IEnumerable(Of String)
            Return missing
        End Function

        ''' <summary>
        ''' Create a string based on the given template
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function CreateString(Of T As IKeyDataReader)(i As T) As String
            Dim str As New StringBuilder(template)
            Dim value As String

            Call missing.Clear()
            Const unknown As String = "unknown"

            For Each placeholder As NamedValue(Of String) In keys
                value = i.GetData(placeholder.Name)

                If value.StringEmpty Then
                    value = defaults.TryGetValue(placeholder.Name, default:=unknown)

                    If value = unknown Then
                        Call missing.Add(placeholder.Name)
                    End If
                End If

                Call str.Replace(placeholder.Value, value)
            Next

            Return str.ToString
        End Function

    End Class
End Namespace