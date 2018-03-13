
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