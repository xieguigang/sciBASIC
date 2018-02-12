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

