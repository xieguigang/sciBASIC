#Region "Microsoft.VisualBasic::5723dc4e4812a32ed1a2cb43dabb24e9, Microsoft.VisualBasic.Core\src\Data\Trinity\WordClass.vb"

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

    '   Total Lines: 61
    '    Code Lines: 18 (29.51%)
    ' Comment Lines: 39 (63.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (6.56%)
    '     File Size: 1.55 KB


    '     Enum WordClass
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Data.Trinity.NLP

    ''' <summary>
    ''' 单词的词性分类
    ''' </summary>
    Public Enum WordClass As Integer

        ''' <summary>
        ''' 未知的词性
        ''' </summary>
        NA = 0

        ''' <summary>
        ''' 名词：to describe a person or thing
        ''' </summary>
        <Description("n.")> noun
        <Description("na.")> name
        ''' <summary>
        ''' 可数名词
        ''' </summary>
        <Description("c.")> countable_noun
        ''' <summary>
        ''' 动词
        ''' </summary>
        <Description("v.")> verb
        ''' <summary>
        ''' 形容词
        ''' </summary>
        <Description("adj.")> adjective
        ''' <summary>
        ''' 副词 
        ''' </summary>
        <Description("adv.")> adverb
        ''' <summary>
        ''' 代词 
        ''' </summary>
        <Description("pron.")> pronoun
        ''' <summary>
        ''' 介词
        ''' </summary>
        <Description("prep.")> preposition
        ''' <summary>
        ''' 数词
        ''' </summary>
        <Description("num.")> numeral
        ''' <summary>
        ''' 连词
        ''' </summary>
        <Description("conj.")> conjunction
        ''' <summary>
        ''' 冠词 
        ''' </summary>
        <Description("art.")> article
        ''' <summary>
        ''' 感叹词
        ''' </summary>
        <Description("int.")> interjection
    End Enum
End Namespace
