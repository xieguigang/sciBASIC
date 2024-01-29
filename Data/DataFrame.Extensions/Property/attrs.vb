Imports System.Text

Namespace IO.Properties

    ''' <summary>
    ''' This attribute for separate the configuration data into another section region using the comment lines.
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class SectionRegion : Inherits Attribute
        Public Property Name As String
    End Class

    ''' <summary>
    ''' Comment data string about this configuration item.(当前的配置数据项的在数据文件之中的注释字符串)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class Comment : Inherits Attribute

        Public ReadOnly Property Value As String
        Public ReadOnly Property Order As Integer

        Sub New(s As String, order As Integer)
            Value = s
            order = order
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Const LIMITED_LENGHT As Integer = 128

        Public Sub WriteComment(sBuilder As StringBuilder)
            Dim Tokens As String() = Strings.Split(Me.Value, vbCrLf)

            For Each strLine As String In Tokens
                If Len(strLine) > 100 Then
                    For i As Integer = 1 To Len(strLine) Step LIMITED_LENGHT
                        Call sBuilder.AppendLine("# " & Mid(strLine, i, LIMITED_LENGHT))
                    Next
                Else
                    Call sBuilder.AppendLine("# " & strLine)
                End If
            Next
        End Sub

        Public Shared ReadOnly Property TypeInfo As Global.System.Type = GetType(Comment)
    End Class
End Namespace