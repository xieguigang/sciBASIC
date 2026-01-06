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
        ReadOnly keys As String()

        Sub New(template As String, Optional defaults As Dictionary(Of String, String) = Nothing)
            Me.template = template
            Me.defaults = If(defaults, New Dictionary(Of String, String))
            Me.keys = ParseKeys(template).ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function ParseKeys(template As String) As IEnumerable(Of String)
            Return template.Matches("[<].*?[>]", RegexICSng)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetDefaultKey(key As String, data As String)
            defaults(key) = data
        End Sub

        ''' <summary>
        ''' Create a string based on the given template
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function CreateString(Of T As IKeyDataReader)(i As T) As String
            Dim str As New StringBuilder(template)
            Dim value As String

            For Each key As String In keys
                value = i.GetData(key)

                If value.StringEmpty Then
                    value = defaults.TryGetValue(key, default:="unknown")
                End If

                Call str.Replace(key, value)
            Next

            Return str.ToString
        End Function

    End Class
End Namespace