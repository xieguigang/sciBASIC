#Region "Microsoft.VisualBasic::90f1bbed548325b960638f3c0b352970, Data\word2vec\TrainMethod.vb"

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

'   Total Lines: 4
'    Code Lines: 4 (100.00%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 0 (0.00%)
'     File Size: 58 B


' Enum TrainMethod
' 
'     CBow, Skip_Gram
' 
'  
' 
' 
' 
' /********************************************************************************/

#End Region

''' <summary>
''' 
''' </summary>
''' <remarks>
''' + 目标：CBow和Skip-Gram模型的主要目标都是通过上下文单词来预测目标单词，或者通过目标单词来预测上下文单词，从而学习单词的向量表示。
''' + 神经网络结构： 两种方法都使用了一个简单的三层神经网络， 包括输入层、隐藏层和输出层。
''' + 高效训练： CBow和Skip-Gram都采用了负采样或层次化softmax等技巧来提高训练效率。
''' + 词向量： 两种方法最终都会为词汇表中的每个单词生成一个固定大小的向量表示。
''' </remarks>
Public Enum TrainMethod
    ''' <summary>
    ''' Continuous Bag-of-Words
    ''' </summary>
    ''' <remarks>
    ''' 输入是目标单词周围的上下文单词（通常是目标单词前后各n个单词），输出是目标单词。
    ''' 模型试图根据上下文中的多个单词预测一个单词，即多对一的关系。由于其输入是多个单词的向量求和，
    ''' 计算复杂度相对较低，训练速度通常比Skip-Gram快。由于其基于上下文单词的求和，
    ''' 可能不太适合处理罕见词或低频词，因为它们的上下文可能不足以提供有效的信息。
    ''' 通常使用较小的上下文窗口，因为每个输入已经聚合了多个单词的信息。
    ''' 在文档分类或情感分析等任务中表现较好，因为它更关注上下文的整体信息。
    ''' </remarks>
    CBow
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' 输入是目标单词，输出是目标单词周围的上下文单词。模型试图从一个单词预测多个上下文单词，即一对多的关系。
    ''' 需要为每个上下文单词计算输出层，因此计算复杂度较高。由于其预测多个上下文单词，可能对罕见词的处理效果更好。
    ''' 可以使用较大的上下文窗口，因为它试图直接预测每个上下文单词。在单词相似度任务或单词
    ''' 类比任务中表现较好，因为它更关注单词之间的关系。
    ''' </remarks>
    Skip_Gram
End Enum
