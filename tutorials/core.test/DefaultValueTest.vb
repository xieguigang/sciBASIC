#Region "Microsoft.VisualBasic::596014c27216b500b6f6c1bfd0034ffd, sciBASIC#\tutorials\core.test\DefaultValueTest.vb"

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

    '   Total Lines: 101
    '    Code Lines: 30
    ' Comment Lines: 42
    '   Blank Lines: 29
    '     File Size: 2.04 KB


    ' Module DefaultValueTest
    ' 
    '     Function: [ByRef], Draw
    ' 
    '     Sub: Main, syntaxTest
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::85490eaa71ae2164b0ad1f76ee952a79, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module DefaultValueTest
    ' 
    '     Function: [ByRef], Draw
    ' 
    '     Sub: Main, syntaxTest
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::3b041cec7c68c691890dede98b8add7b, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module DefaultValueTest
    ' 
    '     Function: [ByRef], Draw
    ' 
    ' 
    '     Sub: Main, syntaxTest
    ' 
    ' 
    ' 

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module DefaultValueTest

    Sub Main()

        'Dim [new] = [Default](New List(Of String))
        'Dim null As List(Of String) = Nothing

        'Dim x As List(Of String) = null Or [new]

        'Dim notnull As New List(Of String) From {"123"}

        'Dim y = notnull Or [new]

        'println("x:= %s", x.GetJson)
        'println("y:= %s", y.GetJson)

        'Pause()

        Call Draw()  ' using default font
        Call Draw(New Font(FontFace.Cambria, 36, FontStyle.Regular))  ' using user defined font

        Pause()
    End Sub

    Public Function Draw(Optional font As Font = Nothing)
        font = font Or MicrosoftYaHei.Large

        println(font.ToString)
    End Function

    Sub syntaxTest()

        With "88888"
            Console.WriteLine(.ByRef)
        End With

    End Sub

    <Extension>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [ByRef](Of T)(obj As T) As T
        Return obj
    End Function

End Module
