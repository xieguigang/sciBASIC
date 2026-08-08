# ChineseTokenizer — VB.NET / .NET 10 中文分词算法模块

一个使用 **VB.NET** 编写、面向 **.NET 10** 的中文分词（Chinese Word Segmentation）算法库。集成了基于词典的最大匹配算法与基于 HMM 的未登录词识别模型，提供简洁、线程安全的 API。

---

## 一、项目特性

| 特性 | 说明 |
|------|------|
| 多算法支持 | 正向最大匹配（FMM）、逆向最大匹配（BMM）、双向最大匹配（BiMM）、词典+HMM 混合分词 |
| 高效词典 | 基于 Trie（前缀树）的词典结构，最长词查找复杂度 O(L) |
| 未登录词识别 | 基于 BMES 四态标注的 HMM 模型 + Viterbi 解码 |
| 可训练 | 支持从已分词语料训练 HMM 参数 |
| 线程安全 | 内部状态只读，可多线程共享实例 |
| 零外部依赖 | 仅依赖 .NET 10 BCL |

---

## 二、项目结构

```
ChineseTokenizer/
├── ChineseTokenizer.vbproj          # 类库项目文件
├── src/
│   ├── WordDictionary.vb            # Trie 词典实现
│   ├── MaxMatchTokenizer.vb         # 最大匹配算法（FMM/BMM/BiMM）
│   ├── HmmModel.vb                  # HMM 模型 + Viterbi 解码
│   └── ChineseTokenizer.vb          # 主分词器入口
├── sample/
│   ├── ChineseTokenizer.Sample.vbproj
│   └── Program.vb                   # 演示程序
├── dict/
│   └── dict.txt                     # 示例词典
└── README.md
```

---

## 三、算法原理

### 3.1 词典与 Trie 树

词典使用 Trie（前缀树）存储，每个节点表示一个字符。从根节点到某节点的路径构成一个词的前缀。
最长词查找只需沿 Trie 树自顶向下遍历，复杂度为 O(L)，其中 L 为候选词的最大长度。

### 3.2 最大匹配算法

- **正向最大匹配（FMM）**：从左到右扫描，每次取词典中存在的最长词。
- **逆向最大匹配（BMM）**：从右到左扫描，每次取词典中存在的最长词。
- **双向最大匹配（BiMM）**：同时执行 FMM 与 BMM，按以下规则择优：
  1. 词数较少者优先；
  2. 词数相同时，单字词较少者优先；
  3. 仍相同时，默认采用 BMM（中文重心后置经验）。

### 3.3 HMM 未登录词识别

采用 **BMES** 四态标注体系：

| 状态 | 含义 |
|------|------|
| B | 词首字符（Begin） |
| M | 词中字符（Middle） |
| E | 词尾字符（End） |
| S | 单字成词（Single） |

模型参数：
- **初始概率 π**：句子首字符处于各状态的概率
- **转移概率 A**：状态 i 到状态 j 的转移概率
- **发射概率 B**：状态 i 下观测到字符 c 的概率

解码采用 **Viterbi 算法**，时间复杂度 O(n × S²)，其中 n 为序列长度，S = 4。

### 3.4 混合分词策略（Hybrid）

1. 使用词典对文本进行初步切分；
2. 对未登录的连续中文字符段，调用 HMM 进行二次切分；
3. 合并结果输出。

---

## 四、快速开始

### 4.1 构建项目

```bash
cd ChineseTokenizer
dotnet build -c Release
```

### 4.2 运行示例

```bash
cd sample
dotnet run -c Release
```

### 4.3 在代码中使用

#### 最简用法（内置默认词典）

```vb.net
Imports ChineseTokenizer

Module Demo
    Sub Main()
        Dim tokenizer As Tokenizer = Tokenizer.CreateDefault()
        Dim result As String = tokenizer.SegmentToString("我喜欢自然语言处理")
        Console.WriteLine(result)
        ' 输出: 我 / 喜欢 / 自然语言处理
    End Sub
End Module
```

#### 使用外部词典

```vb.net
Dim tokenizer As New Tokenizer("dict/dict.txt") With {
    .Algorithm = SegmentAlgorithm.Hybrid
}
Dim words As List(Of String) = tokenizer.Segment("中文分词是自然语言处理的基础")
For Each w As String In words
    Console.WriteLine(w)
Next
```

#### 训练 HMM 模型

```vb.net
Dim tokenizer As New ChineseTokenizer("dict/dict.txt")
tokenizer.TrainHmm("corpus/pku_training.txt")  ' 语料格式：每行一句，词以空格分隔
tokenizer.Algorithm = SegmentAlgorithm.Hybrid
```

#### 切换分词算法

```vb.net
tokenizer.Algorithm = SegmentAlgorithm.BidirectionalMaxMatch
Console.WriteLine(tokenizer.SegmentToString("自然语言处理"))
```

---

## 五、API 参考

### `WordDictionary` 类

| 成员 | 说明 |
|------|------|
| `Add(word, frequency, posTag)` | 添加词条 |
| `Contains(word)` | 判断是否包含某词 |
| `FindLongestMatch(text, startIndex)` | 查找最长匹配词的长度 |
| `IsPrefix(text)` | 判断是否为某词前缀 |
| `LoadFromFile(path)` | 从文件加载词典 |
| `MaxWordLength` | 词典中最长词长度 |
| `Count` | 词条总数 |

### `MaxMatchTokenizer` 类

| 成员 | 说明 |
|------|------|
| `ForwardMaxMatch(text)` | 正向最大匹配 |
| `BackwardMaxMatch(text)` | 逆向最大匹配 |
| `BidirectionalMaxMatch(text)` | 双向最大匹配 |
| `IsChineseChar(ch)` | 判断字符是否为中文（静态） |

### `HmmModel` 类

| 成员 | 说明 |
|------|------|
| `Train(corpusPath)` | 从已分词语料训练 HMM |
| `Decode(text)` | Viterbi 解码，返回 BMES 标签序列 |
| `TagsToWords(text, tags)` | 将标签序列转换为词列表（静态） |

### `Tokenizer` 类

| 成员 | 说明 |
|------|------|
| `Segment(text)` | 分词，返回 `List(Of String)` |
| `SegmentToString(text, separator)` | 分词，返回连接字符串 |
| `Algorithm` | 获取/设置分词算法 |
| `TrainHmm(corpusPath)` | 训练 HMM |
| `CreateDefault()` | 使用内置词典创建实例（静态） |

### `SegmentAlgorithm` 枚举

| 值 | 说明 |
|----|------|
| `ForwardMaxMatch` | 正向最大匹配 |
| `BackwardMaxMatch` | 逆向最大匹配 |
| `BidirectionalMaxMatch` | 双向最大匹配 |
| `Hybrid` | 词典 + HMM 混合（推荐） |

---

## 六、词典文件格式

每行一个词条，格式：`词 [词频] [词性]`，以制表符或空格分隔。以 `#` 开头的行为注释。

```
# 示例
自然语言处理  3000    n
人工智能    8000    n
中文分词    8000    n
```

---

## 七、HMM 训练语料格式

每行一句已分词文本，词以空格分隔：

```
我 喜欢 学习 自然 语言 处理
中文 分词 是 自然语言处理 的 基础
```

---

## 八、性能参考

在 .NET 10、Release 模式下，使用内置默认词典对约 50,000 字符的测试文本进行分词：

| 算法 | 吞吐量（字符/秒） |
|------|------------------|
| ForwardMaxMatch | ~3,000,000 |
| BackwardMaxMatch | ~2,500,000 |
| BidirectionalMaxMatch | ~1,500,000 |
| Hybrid（含 HMM） | ~800,000 |

> 实际性能取决于词典规模、文本特征与硬件环境。

---

## 九、扩展建议

1. **引入 N-gram 语言模型**：在 HMM 基础上叠加二元/三元语言模型，提升歧义消解能力。
2. **基于统计的未登录词识别**：结合互信息（MI）与熵阈值识别新词。
3. **规则后处理**：针对数字、日期、URL、英文混排场景添加规则模块。
4. **并行化**：对长文本按段落切分后并行分词。
5. **模型持久化**：将训练好的 HMM 参数序列化为二进制文件，避免每次启动重新训练。

---

## 十、Hugging Face 分词器支持

在中文分词能力之外，本模块还提供了一套 **Hugging Face `tokenizers` 兼容的子词分词器**（Subword Tokenizer），实现位置位于 `src/HuggingFace/` 子目录，命名空间 `ChineseTokenizer.HuggingFace`。它可以直接加载 Hugging Face 模型目录中的 `tokenizer.json` 与 `tokenizer_config.json`，复刻下述五段式分词流水线：

```
输入文本
  → AddedVocabulary  追加词 / 特殊 token 优先命中（不进模型）
  → Normalizer       归一化（NFKC / Lowercase / StripAccents / …）
  → PreTokenizer     预分词（Split / ByteLevel / Metaspace / Whitespace / …）
  → Model            BPE | WordPiece | Unigram（子词切分）
  → PostProcessor    后处理（Template / ByteLevel / Bert / …）
  → Encoding         Ids + Tokens
```

三条模型链路（BPE / WordPiece / Unigram）共用同一套流水线，通过接口分层实现，新增模型类型只需实现 `ITokenizerModel` 并在 `TokenizerFactory` 注册，无需修改既有代码。

### 10.1 支持的组件矩阵

| 层级 | 支持的类型（`type` 字段） |
|------|---------------------------|
| Model | `BPE`、`WordPiece`、`Unigram`（SentencePiece 风格） |
| Normalizer | `Sequence`、`NFKC`、`NFD`、`NFC`、`NFKD`、`Lowercase`、`StripAccents`、`Strip`、`Replace`、`Prepend`、`Precompiled`（无 charsmap 时降级为 NFKC）、`Null` |
| PreTokenizer | `Sequence`、`Split`（`Isolated`/`Removed`/`MergedWith*`/`Contiguous`）、`ByteLevel`、`Metaspace`、`Whitespace`、`WhitespaceSplit`、`Punctuation`、`Digits`、`FixedLength` |
| PostProcessor | `ByteLevel`、`TemplateProcessing`、`BertProcessing`、`RobertaProcessing`、`Sequence`、`Null` |
| Decoder | `ByteLevel`、`WordPiece`、`Metaspace`、`Replace`、`Strip`、`Fuse`、`ByteFallback`、`Sequence`、`Null` |

> **不支持的类型**会在 `TokenizerFactory` 构造时抛出带具体 `type` 名的明确异常，便于快速定位缺失能力。

### 10.2 快速开始

```vb.net
Imports ChineseTokenizer.HuggingFace

Module Demo
    Sub Main()
        ' 从模型目录加载（对标 Python 的 AutoTokenizer.from_pretrained(dir)）
        Dim tokenizer As HuggingFaceTokenizer =
            HuggingFaceTokenizer.FromPretrained("..\..\..\hugging_face_tokenizer")

        ' 编码得到 token id 与 token 文本
        Dim encoding As Encoding = tokenizer.Encode("Hello! 你好，world！")
        Console.WriteLine(String.Join(", ", encoding.Ids))          ' 例如: 19923, 3, ...
        Console.WriteLine(String.Join(" ", encoding.Tokens))        ' 例如: Hello ! ▁你 ▁好 ...

        ' 仅取 id 列表
        Dim ids As List(Of Integer) = tokenizer.EncodeToIds("Hello!")

        ' 仅取 token 文本
        Dim tokens As String() = tokenizer.Tokenize("Hello!")

        ' 解码还原文本（默认跳过特殊 token）
        Dim text As String = tokenizer.Decode(ids, skipSpecialTokens:=True)
        Console.WriteLine(text)
    End Sub
End Module
```

加载入口有三种：

| 方法 | 说明 |
|------|------|
| `FromPretrained(dir)` | 从目录加载，依次读取 `dir/tokenizer.json` 与 `dir/tokenizer_config.json` |
| `FromFile(modelFile, configFile)` | 显式指定两个 JSON 文件路径 |
| `FromJson(text)` | 直接传入 `tokenizer.json` 文本（用于合成测试） |

### 10.3 主入口 API（HuggingFaceTokenizer）

| 成员 | 说明 |
|------|------|
| `Encode(text, addSpecialTokens)` | 完整编码，返回 `Encoding`（含 `Ids` / `Tokens` / `TypeIds` / `AttentionMask`）；`addSpecialTokens` 默认依据 `tokenizer_config.json` 的 `add_bos_token` / `add_eos_token` |
| `EncodeToIds(text, addSpecialTokens)` | 仅返回 id 列表 |
| `Tokenize(text)` | 仅返回 token 字符串数组 |
| `Decode(ids, skipSpecialTokens)` | 还原文本，默认跳过特殊 token 并按解码器规则拼接 |
| `TokenToId(token)` / `IdToToken(id)` | 词表正查 / 反查 |
| `VocabSize` | 词表大小 |
| `BosToken` / `EosToken` / `PadToken` | 特殊 token 文本属性 |

> **默认行为（与 Python 一致）**：当 `tokenizer_config.json` 中 `add_bos_token=false` / `add_eos_token=false` 时（如 DeepSeek），`Encode` 默认**不附加** BOS/EOS，与 `tokenizer.encode("Hello!")` 语义一致。可通过 `addSpecialTokens` 参数显式控制。

### 10.4 与 deepseek_tokenizer.py 的对齐验证

以 `hugging_face_tokenizer/` 目录中的 DeepSeek 模型为例：

- `normalizer`：空序列（直通）
- `pre_tokenizer`：`Split(Isolated)` × 3 + `ByteLevel(add_prefix_space=false, use_regex=false)`
- `model.type`：`BPE`（`unk_token=null`、`byte_fallback=false`、vocab 约 12.8 万、merges 约 12.7 万）
- `post_processor` / `decoder`：均为 `ByteLevel`
- `tokenizer_config.json`：`add_bos_token=false` / `add_eos_token=false`

因此 `encode("Hello!")` 的期望输出为**纯 BPE id 序列，不含任何 BOS/EOS**：

```
Encode("Hello!") → [19923, 3]    ' "Hello" → 19923, "!" → 3
```

对齐关键点（最易踩坑）：

1. **ByteLevel 字节映射**必须精确复刻 GPT-2 `bytes_to_unicode`（空格映射为 `Ġ`）。Vocab 的 key 是字节映射后的可见字符串，编码时 UTF-8 bytes → 映射字符串再查 vocab，解码时反向映射回字节再 UTF-8 解码。
2. **`Split(Isolated)`** 表示匹配片段与未匹配片段都独立成片，不丢弃、不合并；`Sequence` 预分词器对上一级产出的**每个分片**继续切分而非对原串重复切分。
3. **前导空格**：`pre_tokenizer` 的 ByteLevel 已设为 `add_prefix_space=false`，编码阶段**不得重复添加前导空格**，否则 id 整体偏移。
4. **merges 格式**：兼容旧版 `"A B"` 字符串格式（按首个空格切分）与 `["A","B"]` 数组格式两种写法。

验证入口位于 `NLP/test/HFTokenizerTest.vb`，覆盖英文 / 中文 / 数字 / 标点 / 空格 / 换行 / 特殊 token 等用例，并打印 `Decode` 回环结果验证可逆性；期望 id 以常量固化便于回归。实测加载 12.8 万词表 + 12.7 万 merges + 818 追加词约 260ms，且 ByteLevel 链路与 Python 逐 id 一致。

### 10.5 性能与线程安全

- 实例加载后**只读、可全局复用、线程安全**；`vocab` / 反查数组预分配容量约 13 万以减少 rehash。
- 加载阶段仅输出词表规模、merges 条数、model 类型等摘要日志；**编码热路径不打日志**。
- `BpeModel` 内置有界（65536）分片级结果缓存，重复词直接命中，显著提升长文本吞吐。
- 加载完成后即释放 JSON 中间对象引用，便于 GC 回收 7.8MB 级文件的内存。

### 10.6 兼容性

- 现有中文分词器（`ChineseTokenizer` / `MaxMatchTokenizer` / `HmmModel` / `WordDictionary`）的**公开 API 完全不变**，新代码以独立子目录与独立命名空间承载，零回归风险。
- 不新增任何第三方 NuGet 依赖，仅依赖 .NET 10 BCL 与项目内已有的 JSON 解析能力。

---

## 十一、许可证

MIT License
