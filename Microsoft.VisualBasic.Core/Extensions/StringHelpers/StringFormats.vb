#Region "Microsoft.VisualBasic::4cf9869cd1aaf27d3d627f9a4d4885ed, Microsoft.VisualBasic.Core\Extensions\StringHelpers\StringFormats.vb"

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

    ' Module StringFormats
    ' 
    '     Function: Lanudry
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.C
Imports stdNum = System.Math

Public Module StringFormats

    ''' <summary>
    ''' 对bytes数值进行格式自动优化显示
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <returns>经过自动格式优化过后的大小显示字符串</returns>
    Public Function Lanudry(bytes As Double) As String
        If bytes <= 0 Then
            Return "0 B"
        End If

        Dim symbols = {"B", "KB", "MB", "GB", "TB"}
        Dim exp = stdNum.Floor(stdNum.Log(bytes) / stdNum.Log(1000))
        Dim symbol = symbols(exp)
        Dim val = (bytes / (1000 ^ stdNum.Floor(exp)))

        Return sprintf($"%.2f %s", val, symbol)
    End Function
End Module
