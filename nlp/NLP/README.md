# 自然语言处理工具箱：分词、词干、BM25 与 LDA

## 引言

一个完整的文本处理流水线通常要经过四道关口：

1. **切分** —— 英文按空格即可，中文没这么幸运，需要词典或统计模型；
2. **规范化** —— 大小写、变形词、停用词需要处理，英文常用 Porter 词干；
3. **计量** —— TF-IDF 把词频转换成权重；
4. **检索与主题** —— 给定查询返回排序结果（BM25），或从语料中抽出隐含主题（LDA）。

`Microsoft.VisualBasic.Data.NLP` 把这四道关口放在同一个包里，并额外提供一条**与 HuggingFace 生态互通**的分词通道：只要给出预训练目录（`tokenizer.json`），就能复用 Python 侧训练好的 BPE / WordPiece / Unigram 词表。

## 设计目标

- **中英兼顾**：中文用最大匹配 + HMM，英文用 Porter 词干；
- **生态互通**：`tokenizer.json` 直接读，避免分词器与模型训练环境耦合；
- **增量可查**：BM25 索引支持逐篇追加，适合流式语料。

## 核心特性

- **中文分词**：基于 `WordDictionary` 的最大匹配（前向 / 后向 / 双向），以及基于 HMM 的分词器；
- **HuggingFace 兼容流水线**：normalizer → pre-tokenizer → BPE / WordPiece / Unigram 模型 → post-processor → decoder，并包含 byte-level alphabet；
- **排序检索与主题模型**：可选 IDF 变体、带逐词贡献明细的 BM25；带语料 / 词表加载器的 Gibbs 采样 LDA；
- **文本工具**：Porter 词干、停用词、TF-IDF 加权、双词组合提取（bigram），以及句子 / 段落 / 词模型与 token 计数器。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `...Data.NLP`（根） | `Extensions`、`Stemmer`、`Text`、`Bigram`、`StopWords`、`DocumentTFIDF` / `TFIDF` |
| `...Data.NLP.ChineseTokenizer` | 中文分词：`Tokenizer`、`MaxMatchTokenizer`、`HmmModel`、`WordDictionary` |
| `...Data.NLP.ChineseTokenizer.HuggingFace` | `tokenizer.json` 兼容流水线：模型、normalizer、pre-tokenizer、post-processor、decoder |
| `...Data.NLP.BM25` | Okapi BM25 排序检索 |
| `...Data.NLP.LDA` | Gibbs 采样 LDA 主题模型 |
| `...Data.NLP.Model` | 句子 / 段落 / 字符遍历 / token 计数器等共享文本模型 |

## 关键类型与 API

- `...ChineseTokenizer.Tokenizer` —— 中文分词器（`Segment`、`SegmentToString`、`TrainHmm`、`CreateDefault`）；
- `...ChineseTokenizer.HuggingFace.HuggingFaceTokenizer` —— 加载预训练分词器目录，并对文本或 id 编解码；
- `...BM25.BM25Engine` —— 增量 BM25 索引（`AddDocument`、`BuildIndex`、`Search`）；
- `...LDA.LdaGibbsSampler` —— 为词与文档估计最优主题分配的 Gibbs 采样器；
- `...LDA.Corpus` / `Vocabulary` —— LDA 使用的文档集合与词索引映射；
- `...Stemmer` —— 把英文词还原为词干的 Porter 词干器；
- `...Model.Sentence` / `Paragraph` —— NLP 栈共享的有序词 / 句模型。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.NLP.BM25
Imports Microsoft.VisualBasic.Data.NLP.ChineseTokenizer

Dim engine As New BM25Engine(k1:=1.2, b:=0.75)
Dim tokenizer As Tokenizer = Tokenizer.CreateDefault()

Call engine.AddDocument(1, tokenizer.Segment("蛋白质组学数据分析").ToArray())
Call engine.AddDocument(2, tokenizer.Segment("代谢组学数据分析").ToArray())
Call engine.BuildIndex()

For Each hit In engine.Search(tokenizer.Segment("数据分析").ToArray(), 10)
    Console.WriteLine($"{hit.DocId}  {hit.Score}")
Next
```

## 实现要点

- **最大匹配 vs HMM**：最大匹配依赖词典、速度极快，但无法处理未收录词；HMM 通过状态转移概率处理未登录词。实现中两条路径并存，可按语料特点选择。
- **BPE / WordPiece / Unigram 共存**：三者共享同一套 normalizer / pre-tokenizer / post-processor 抽象，因此切换模型只需要替换模型实现，流水线其余部分保持不变。
- **BM25 的可解释性**：除了得分，引擎还能给出每个词的贡献明细，这在调试检索质量时非常有用。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.NLP`
- TargetFramework：`net10.0`
- Tags：`scibasic;nlp;chinese-tokenizer;huggingface;tokenizer;bm25;lda;topic-model;tf-idf;stemmer;text-mining`
- 许可：GPL-3.0-or-later
