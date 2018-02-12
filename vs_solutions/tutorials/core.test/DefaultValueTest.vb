#Region "Microsoft.VisualBasic::3b041cec7c68c691890dede98b8add7b, ..\core.test\DefaultValueTest.vb"

    ' Author:
    ' 
    '       asuka ()
    '       xieguigang ()
    '       xie ()
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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
    ' M
    ' o
    ' d
    ' u
    ' l
    ' e
    '  
    ' D
    ' e
    ' f
    ' a
    ' u
    ' l
    ' t
    ' V
    ' a
    ' l
    ' u
    ' e
    ' T
    ' e
    ' s
    ' t
    ' 
    ' 

    ' 
    ' 

    '  
    '  
    '  
    '  
    ' F
    ' u
    ' n
    ' c
    ' t
    ' i
    ' o
    ' n
    ' :
    '  
    ' [
    ' B
    ' y
    ' R
    ' e
    ' f
    ' ]
    ' ,
    '  
    ' D
    ' r
    ' a
    ' w
    ' 

    ' 

    ' 

    '  
    '  
    '  
    '  
    ' S
    ' u
    ' b
    ' :
    '  
    ' M
    ' a
    ' i
    ' n
    ' ,
    '  
    ' s
    ' y
    ' n
    ' t
    ' a
    ' x
    ' T
    ' e
    ' s
    ' t
    ' 

    ' 

    ' 
    ' 

    ' 
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
