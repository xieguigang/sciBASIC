# nlp/word2vec/word2vec.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.NLP.Word2Vec
- AssemblyName  : Microsoft.VisualBasic.Data.NLP.Word2Vec
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: Word2Vec Embedding Trainer with Skip-Gram and CBOW
- Existing Desc : Trains word embeddings for sciBASIC#: a multi-threaded Word2Vec using skip-gram and CBOW over Huffman trees, a fluent factory for vector size, window and sampling, plus vector models for similarity and analogy queries.
- Existing Tags : scibasic;word2vec;word-embedding;skip-gram;machine-learning

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.NLP.Word2Vec)

## Public types
- Module Extensions (Extensions.vb)
- Class Trainer (Trainer.vb)
- Enum TrainMethod (TrainMethod.vb) - + 目标：CBow和Skip-Gram模型的主要目标都是通过上下文单词来预测目标单词，或者通过目标单词来预测上下文单词，从而学习单词的向量表示。 + 神经网络结构： 两种方法都使用了一个简单的三层神经网络， 包括输入层、隐藏层和输出层。 + 高效训练： CBow和Skip-Gram都采用了负采样或层次化softmax等技巧来提高训练效率。
- Class VectorModel (VectorModel.vb) - the word embedding vector set
- Class Word2Vec (Word2Vec.vb) - Created by fangy on 13-12-19. Word2Vec 算法实现 https://github.com/siegfang/word2vec
- Class Word2VecFactory (Word2VecFactory.vb)
- Class WordNeuron (WordNeuron.vb) - Created by fangy on 13-12-17. 词神经元
- Class WordScore (WordScore.vb) - the word score of the vector model

## Notable public members
- Public Function BuildWord2VecFactory() As Word2VecFactory
- Public Sub New(outerInstance As Word2Vec, corpus As LinkedList(Of String()))
- Public Sub New(outerInstance As Word2Vec, corpusQueue As Queue(Of LinkedList(Of String())))
- Friend Sub computeAlpha()
- Friend Sub training()
- Public Function run() As Integer Implements ITaskDriver.Run
- Public Property wordMap As New Dictionary(Of String, Single())
- Public Property vectorSize As Integer = 200
- Public ReadOnly Property words As Integer
- Public ReadOnly Property tokens As String()
- Public Sub New(wordMap As IDictionary(Of String, Single()), vectorSize As Integer)
- Public Function similar(queryWord As String, Optional topNSize As Integer = 40) As IEnumerable(Of WordScore)
- Public Function similar(center As Single(), Optional topNSize As Integer = 40) As IEnumerable(Of WordScore)
- Public Function analogy(word0 As String, word1 As String, word2 As String, Optional topNSize As Integer = 40) As SortedSet(Of WordScore)
- Public Function getWordVector(word As String) As Single()
- Public Overrides Function ToString() As String
- Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedCollection(Of Single)) Implements Enumeration(Of NamedCollection(Of Single)).Gener…
- Public ReadOnly Property wordSize As Integer
- Friend Sub New(factory As Word2VecFactory)
- Public Sub readTokens(tokenizer As Sentence)
- Public Sub readTokens(tokenizer As ICollection(Of String))
- Public Sub training()
- Friend Sub skipGram(index As Integer, sentence As IList(Of WordNeuron), b As Integer, alpha As Double)
- Friend Sub cbowGram(index As Integer, sentence As IList(Of WordNeuron), b As Integer, alpha As Double)
- Public Function outputVector() As VectorModel
- Public Function setVectorSize(size As Integer) As Word2VecFactory
- Public Function setWindow(size As Integer) As Word2VecFactory
- Public Function setFreqThresold(thresold As Integer) As Word2VecFactory
- Public Function setMethod(method As TrainMethod) As Word2VecFactory
- Public Function setSample(rate As Double) As Word2VecFactory
- Public Function setAlpha(alpha As Double) As Word2VecFactory
- Public Function setAlphaThresold(alpha As Double) As Word2VecFactory
- Public Function setNumOfThread(numOfThread As Integer) As Word2VecFactory
- Public Function build() As Word2Vec
- Public Property name As String
- Public ReadOnly Property pathNeurons As IList(Of HuffmanNode)
- Public Sub New(name As String, freq As Integer, vectorSize As Integer)
- Public Overrides Function ToString() As String
- Public Sub New(name As String, score As Single)
- Public Overrides Function ToString() As String
- Public Function CompareTo(o As WordScore) As Integer Implements IComparable(Of WordScore).CompareTo

## Imports
- Microsoft.VisualBasic.ComponentModel
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.ComponentModel.DataStructures
- Microsoft.VisualBasic.Data.GraphTheory
- Microsoft.VisualBasic.Data.GraphTheory.HuffmanTree
- Microsoft.VisualBasic.Data.NLP.Model
- Microsoft.VisualBasic.Data.Trinity.NLP
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Serialization.JSON
- randf = Microsoft.VisualBasic.Math.RandomExtensions
- std = System.Math
- System.Runtime.CompilerServices

## File tree
- Extensions.vb
- Trainer.vb
- TrainMethod.vb
- VectorModel.vb
- Word2Vec.vb
- Word2VecFactory.vb
- WordNeuron.vb
- WordScore.vb

