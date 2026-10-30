#Region "Microsoft.VisualBasic::e8ed6bcfa05daee7e9649676c5500245, www\Microsoft.VisualBasic.Webservices.Bing\MicrosoftBing\Translation\WordTranslation.vb"

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

    '   Total Lines: 27
    '    Code Lines: 12 (44.44%)
    ' Comment Lines: 11 (40.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (14.81%)
    '     File Size: 787 B


    '     Class WordTranslation
    ' 
    '         Properties: Pronunciation, Translations, Word
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Bing.Translation

    ''' <summary>
    ''' 单词翻译的结果
    ''' </summary>
    Public Class WordTranslation

        ''' <summary>
        ''' 输入的目标单词
        ''' </summary>
        ''' <returns></returns>
        Public Property Word As String
        ''' <summary>
        ''' 该单词所产生的翻译结果列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Translations As Word()
        Public Property Pronunciation As String()

        Public Overrides Function ToString() As String
            Return $"{Word} -> {Translations.GetJson}"
        End Function
    End Class
End Namespace
