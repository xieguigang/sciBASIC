# Microsoft Bing 网页 / 学术 / 翻译与 DOI 服务客户端

## 引言

构建知识库时，「知识从哪来」往往比「知识怎么存」更麻烦。真实的科研工作流需要同时访问好几类外部服务：

- 通用**网页检索**（找线索）；
- **学术检索**（拿题录、作者、期刊、DOI、引用）；
- **术语翻译**（跨语言对齐概念）；
- **DOI 解析**（把标识符变成可访问的元数据）。

本包把这几件事统一到 `sciBASIC#` 的服务客户端里，并提供从检索结果直接构建文献知识库的扩展方法。

## 设计目标

- **面向流水线**：每一步都返回强类型模型（`SearchResult` / `WebResult` / `ArticleProfile` / `WordTranslation` / `Response`），而不是原始 HTML；
- **分页友好**：网页检索显式暴露「下一页」语义，并提供一次遍历全部结果的迭代器；
- **零配置建模**：DOI 服务响应被建模为强类型对象，避免调用方自行处理 JSON 结构。

## 核心特性

- **网页检索**：构造 Bing 查询 URL、下载并解析结果页为 `WebResult` 条目，支持 `NextPage` / `HaveNext` 分页与全量结果迭代；
- **学术检索**：按关键词检索返回候选文章链接，抓取文章详情为 `ArticleProfile` 元数据（作者、期刊、DOI、引用、关键词），并提供从关键词构建知识库的扩展；
- **翻译**：单词翻译查询，返回读音与目标词列表；
- **DOI**：DOI handle 服务的强类型响应模型（handle、values、`HS_ADMIN`）。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing` | 网页检索：`SearchEngineProvider`、`SearchResult`、`WebResult`、`BingImage` |
| `...Web.Bing.Academic` | 学术检索：`AcademicSearch`、`ArticleProfile`、`ProfileResult`、`Extensions` |
| `...Web.Bing.Translation` | 翻译：`Translation`、`WordTranslation` |
| `...Web.DOI` | DOI handle 服务响应模型：`Response` |

## 关键类型与 API

- `...Bing.SearchEngineProvider` —— Bing 网页检索入口（`URLProvider`、`Search`、`DownloadResult`、`GetAllResults`）；
- `...Bing.SearchResult` —— 一页结果：条目数量、当前条目与下一页导航；
- `...Bing.WebResult` —— 一条抓取到的结果（标题、URL、摘要、更新时间）；
- `...Bing.Academic.AcademicSearch` —— 学术门户检索与文章详情获取；
- `...Bing.Academic.ArticleProfile` —— 一篇文献解析后的元数据；
- `...Bing.Academic.Extensions` —— 由关键词构建文献知识库并对文章集合做汇总；
- `...Bing.Translation.WordTranslation` —— 单个词的翻译结果；
- `...Web.DOI.Response` —— 解码后的 DOI handle 服务响应。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing
Imports Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Academic

Dim page As SearchResult = SearchEngineProvider.Search("gcmodeller")

For Each item As WebResult In page.CurrentPage
    Console.WriteLine($"{item.Title} -> {item.URL}")
Next

For Each article In AcademicSearch.Search("gcmodeller")
    Console.WriteLine(article.Name)
Next
```

## 实现要点

- **分页语义显式化**：`SearchResult` 同时携带当前页与「是否有下一页」，因此调用方可以边翻页边处理，而不必一次性把全部结果载入内存。
- **知识库视角**：`Academic.Extensions` 把「检索 → 抓取详情 → 组装知识库」这条链路固化下来，是构建文献知识图谱最短的路径。

## 与 sciBASIC# 生态的关系

本包属于 `www` 服务客户端层：抓取到的文献元数据可以交给 `nlp/KnowledgeGraph` 做实体消歧与本体推断，也可以配合 `mime` 家族解析下载到的 PDF / XML 全文。

## 包信息

- Assembly：`Microsoft.VisualBasic.Web.KnowledgeBase`
- TargetFramework：`net10.0`
- Tags：`scibasic;bing;search-engine;web-scraping;academic-search;translation;doi;knowledge-base`
- 许可：GPL-3.0-or-later
