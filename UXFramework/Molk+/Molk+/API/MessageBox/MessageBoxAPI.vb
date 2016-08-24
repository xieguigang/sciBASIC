#Region "Microsoft.VisualBasic::2d64c24da60b21dfce33365510b9e71c, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\MessageBox\MessageBoxAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Public Module MessageBox

    Public Enum CYIcon
        [Error]
        Explorer
        Find
        Information
        Mail
        Media
        Print
        Question
        RecycleBinEmpty
        RecycleBinFull
        [Stop]
        User
        Warning
    End Enum

    Public Enum CYButtons
        AbortRetryIgnore
        OK
        OKCancel
        RetryCancel
        YesNo
        YesNoCancel
    End Enum

    ''' <summary>
    ''' Message: Text to display in the message box.
    ''' </summary>
    Public Function Show(Message As String) As DialogResult
        Return CYMessageBox.Show(Message)
    End Function

    ''' <summary>
    ''' Title: Text to display in the title bar of the messagebox.
    ''' </summary>
    Public Function Show(Message As String, Title As String) As DialogResult
        Return CYMessageBox.Show(Message, Title)
    End Function

    ''' <summary>
    ''' MButtons: Display CYButtons on the message box.
    ''' </summary>
    Public Function Show(Message As String, Title As String, MButtons As CYButtons) As DialogResult
        Return CYMessageBox.Show(Message, Title, MButtons)
    End Function

    ''' <summary>
    ''' MIcon: Display CYIcon on the message box.
    ''' </summary>
    Public Function Show(Message As String, Title As String, MButtons As CYButtons, MIcon As CYIcon) As DialogResult
        Return CYMessageBox.Show(Message, Title, MButtons, MIcon)
    End Function

End Module
