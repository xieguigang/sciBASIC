#Region "Microsoft.VisualBasic::f5c1678926436a712cf0bdac01b72af1, vs_solutions\dev\VisualStudio\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main, sourceMapDecodeTest, vlqtest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.SourceMap
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()
        Call sourceMapDecodeTest()
        Call vlqtest()
    End Sub

    Sub sourceMapDecodeTest()
        Dim map As sourceMap = "D:\biodeep\biodeep_v2\biodeep\cdn.biodeep.cn\typescripts\build\linq.js.map".LoadJsonFile(Of sourceMap)
        Dim ref = map.decodeMappings.ToArray

        For Each n In ref
            Call Console.WriteLine(n.GetStackFrame(map).ToString)
        Next

        Pause()
    End Sub

    Sub vlqtest()
        Console.WriteLine(base64VLQ.base64VLQ_encode(16))
        Console.WriteLine(base64VLQ.base64VLQ_decode("gB"))
        Console.WriteLine(base64VLQ.getIntegers("AAAAA").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("BBBBB").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("CCCCC").ToArray.GetJson)

        Console.WriteLine(base64VLQ.getIntegers("AAgBC").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("SAAQ").ToArray.GetJson)
        Console.WriteLine(base64VLQ.getIntegers("CAAEA").ToArray.GetJson)

        Pause()
    End Sub
End Module

