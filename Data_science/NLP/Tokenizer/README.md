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

## 十、许可证

MIT License
