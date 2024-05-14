#Region "Microsoft.VisualBasic::ba4b0cdb8500ef891eab9b8b03368ac8, Microsoft.VisualBasic.Core\src\Language\Language\UnixBash\Cowsay.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 105
    '    Code Lines: 74
    ' Comment Lines: 20
    '   Blank Lines: 11
    '     File Size: 4.09 KB


    '     Module CowsayTricks
    ' 
    '         Function: __msgbox, RunCowsay
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Language.UnixBash

    ''' <summary>
    ''' Cowsay tricks for showing your message more friendly.
    ''' </summary>
    <Package("Cowsay",
                  Description:="cowsay is a program which generates ASCII pictures of a cow with a message.[2] 
                  It can also generate pictures using pre-made images of other animals, such as Tux the Penguin, the Linux mascot. 
                  It is written in Perl. There is also a related program called cowthink, with cows with thought bubbles rather than speech bubbles. .
                  cow files for cowsay exist which are able to produce different variants of ""cows"", with different kinds of ""eyes"", 
                  and so forth.[3] It is sometimes used on IRC, desktop screenshots, and in software documentation. 
                  It is more or less a joke within hacker culture, but has been around long enough that its use is rather widespread. 
                  In 2007 it was highlighted as a Debian package of the day.
                  Cowsay tricks for showing your message more friendly. https://en.wikipedia.org/wiki/Cowsay",
                  Revision:=21,
                  Publisher:="<a href=""mailto://xie.guigang@live.com"">xie.guigang@live.com</a>",
                  Url:="http://gcmodeller.org",
                  Category:=APICategories.UtilityTools)>
    Public Module CowsayTricks

        ''' <summary>
        ''' Normal cow
        ''' </summary>
        Public Const NormalCow As String =
"          |
          |    ^__^
           --  (oo)\_______
               (__)\       )\/\
                   ||----W |
                   ||     ||
"

        ''' <summary>
        ''' The cow in dead face
        ''' </summary>
        Public Const DeadCow As String =
"          |
          |    ^__^
           --  (XX)\_______
               (__)\       )\/\
                   ||----W |
                   ||     ||
"

        Public Const tux As String = " __________________
< This is my text. >
 ------------------
   \
    \
        .--.
       |o_o |
       |:_/ |
      //   \ \
     (|     | )
    /'\_   _/`\
    \___)=(___/"

        Const HelloWorld As String =
"
H   H EEEEE L     L      OOO       W   W  OOO  RRRR  L     DDDD  !!
H   H E     L     L     O   O      W W W O   O R   R L     D   D !! 
HHHHH EEEEE L     L     O   O      W W W O   O RRRR  L     D   D !! 
H   H E     L     L     O   O  ,,   W W  O   O R   R L     D   D    
H   H EEEEE LLLLL LLLLL  OOO  ,,    W W   OOO  R   R LLLLL DDDD  !!
"

        ''' <summary>
        ''' Show cowsay with a specific input message on your console screen. you can using /dead to change its face.
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <returns></returns>
        ''' 
        Public Function RunCowsay(msg As String, Optional isDead As Boolean = False) As String
            If isDead Then
                msg = __msgbox(msg) & DeadCow
            Else
                msg = __msgbox(msg) & NormalCow
            End If

            Call Console.WriteLine(msg)

            Return msg
        End Function

        ''' <summary>
        ''' Creates the message box to display the message for the cow on the console.
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <returns></returns>
        Private Function __msgbox(msg As String) As String
            Dim l = Len(msg)
            Dim offset As String = New String(" ", 8)
            Dim sBuilder As StringBuilder = New StringBuilder(vbCrLf, 1024)
            Call sBuilder.AppendLine(offset & " " & New String("_", l + 4) & " ")
            Call sBuilder.AppendLine(offset & String.Format("<  {0}  >", msg))
            Call sBuilder.AppendLine(offset & " " & New String("-", l + 4) & " ")

            Return sBuilder.ToString
        End Function
    End Module
End Namespace
