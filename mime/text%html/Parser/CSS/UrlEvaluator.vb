#Region "Microsoft.VisualBasic::15a5ac34ac3cb75826162e5411f73468, mime\text%html\Parser\CSS\UrlEvaluator.vb"

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

    '   Total Lines: 55
    '    Code Lines: 33 (60.00%)
    ' Comment Lines: 16 (29.09%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 6 (10.91%)
    '     File Size: 1.89 KB


    '     Module UrlEvaluator
    ' 
    '         Function: EvaluateAsImage, IsURLPattern
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http

#If NET48 Then
Imports Image = System.Drawing.Image
#Else
Imports Image = Microsoft.VisualBasic.Imaging.Image
#End If

Namespace Language.CSS

    Public Module UrlEvaluator

        ''' <summary>
        ''' ``url(xxxxx)``
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsURLPattern(expression As String) As Boolean
            Return Strings.Trim(expression).IsPattern("url\(.+\)", RegexICSng)
        End Function

        ''' <summary>
        ''' 暂时还不支持SVG图像
        ''' </summary>
        ''' <param name="expression">
        ''' + url('filepath') 图片文件路径
        ''' + url('data:xxx') base64编码的data uri图像数据
        ''' </param>
        ''' <returns></returns>
        Public Function EvaluateAsImage(expression As String) As Image
            Dim uri As String = expression.GetStackValue("(", ")").Trim("'"c)

            If DataURI.IsWellFormedUriString(uri) Then
                ' 是data uri base64字符串
                Return DataURI.URIParser(uri).ToStream.LoadImage
            Else
                If uri.IsURLPattern Then
                    ' 是网络文件
                    With TempFileSystem.GetAppSysTempFile
                        uri.DownloadFile(.ByRef)
                        Return .LoadImage
                    End With
                Else
                    ' 是本地文件
                    Return uri.LoadImage
                End If
            End If
        End Function
    End Module
End Namespace
