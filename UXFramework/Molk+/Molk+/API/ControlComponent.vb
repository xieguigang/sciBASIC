Imports System.Runtime.CompilerServices

Public Module ControlComponent

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Hook"></param>
    ''' <param name="InputHandle"></param>
    ''' <param name="Location"></param>
    ''' <param name="Width"></param>
    ''' <param name="DefaultValue">默认值，当用户没有输入任何字符串直接点击确定的时候，返回这个预设值</param>
    ''' <remarks></remarks>
    <Extension> Public Sub InputBoxShowDialog(Hook As Control, InputHandle As Action(Of String), Location As Point, Optional Width As Integer = -1, Optional DefaultValue As String = "")
        Call MolkPlusTheme.UserInputControl.Input(Hook, InputHandle, Location, Width, DefaultValue)
    End Sub

    <Extension> Public Sub DrawBorder(ByRef Control As Control, Color As Color)
        Using Gr As Graphics = Graphics.FromHwnd(Control.Handle)
            Dim Rect = New Rectangle(New Point(1, 1), New Size(Control.Width - 2, Control.Height - 2))
            Call Gr.DrawRectangle(New Pen(Color, 1), Rect)
        End Using
    End Sub
End Module

Public Delegate Sub InvokeHandle()
