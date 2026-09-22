# XLSX 工作簿读写与旧版 BIFF 写出器

## 引言

`.xlsx` 本质是一个 **ZIP 包**，里面装着一组相互引用的 XML 部件：内容类型声明、关系表、文档属性，以及 `xl/` 目录下的工作表、共享字符串表、样式表、主题与计算链。因此「读写 xlsx」的真实难度并不在 XML 本身，而在于：

1. **OPC 包的组织**：哪些部件必须存在、彼此如何引用；
2. **样式体系**：单元格样式不是内联的，而是通过 `cellXfs` 索引到共享的 `fonts` / `fills` / `borders` / `numberFormats`；
3. **数值与公式**：公式以文本形式存储，数值需要正确的类型标记。

本包把这三层都覆盖，并额外提供**旧版 BIFF 2.1 写出器**，用于生成老系统仍需要的 `.xls`。

## 设计目标

- **读成强类型**：工作表既能读成表格帧（`GetTable`），也能直接映射为强类型记录集合（`LoadDataSet(Of T)`）；
- **写可控且带样式**：从工作簿、工作表、单元格到样式仓库逐层可控；
- **新旧格式兼顾**：xlsx 为主，BIFF 2.1 作为兼容出口。

## 核心特性

- **读**：把 xlsx 包打开为强类型 OPC 目录部件（content types、rels、docProps、xl 下的 worksheets / sharedStrings / styles / theme / calcChain），并支持把工作表读成表格帧或强类型记录。
- **写**：创建工作簿、工作表、单元格、公式、区域、列宽与冻结窗格，然后保存到文件或流。
- **样式**：字体、填充、边框、数字格式、对齐方式，以及可复用的样式仓库，可按单元格或区域应用。
- **旧格式**：BIFF 2.1 记录写出器（生成旧式 `.xls`），外加把预置样式的科研报表写入工作簿的 `ReportHelper`。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.Office.Excel`（根） | `ReportHelper` 报表辅助 |
| `....Excel.XLSX` | 工作簿文件模型：`File`、`IXml`、`Extensions` |
| `....Excel.XLSX.FileIO` | OPC 包 I/O：`ZipPackage`、`LowLevel`、`DocumentPath` |
| `....Excel.XLSX.Model` / `.Model.Directory` | 工作表表格模型与各目录部件 XML 模型 |
| `....Excel.XLSX.Writer`（+ `.Cell` / `.Styling` / `.Worksheet`） | 工作簿、单元格、样式与工作表写出 |
| `....Excel.XLSX.XML.*` | 各 OPC 部件的 XML 对象模型（xl、docProps、_rels、theme、worksheets） |
| `....Excel.XLS` / `.XLS.BIFF` | 旧式 `.xls` 支持与 BIFF 记录写出器 |

## 关键类型与 API

- `...XLSX.File` —— 已打开的 xlsx 包；`Open`、`SheetNames`、`GetTable`、`GetWorksheet`、`LoadDataSet(Of T)`、`IDisposable`；
- `...XLSX.FileIO.ZipPackage` / `LowLevel` / `DocumentPath` —— zip 包处理、XML 写出与 OPC 部件打包；
- `...XLSX.Writer.Workbook` —— 工作簿生成器：`AddWorksheet`、`SetCurrentWorksheet`、`Save` / `SaveAs` / `SaveAsStream`；
- `...XLSX.Writer.Worksheet.Worksheet` —— 单元格网格：`AddCell`、`AddCellFormula`、列宽、合并单元格与当前样式；
- `...XLSX.Writer.Cell.Cell` / `Cell.Address` / `Cell.CellType` —— 强类型单元格与 A1 / RC 地址解析；
- `...XLSX.Writer.Styling.Style`（含 `Font`、`Fill`、`Border`、`NumberFormat`、`CellXf`）—— 由样式管理器消费的样式模型；
- `...XLSX.Model.StoreProcedure` / `SheetTable` —— 工作表与 csv 帧之间的互转；
- `...XLS.BIFF.BiffWriter` —— 写出旧版 Excel BIFF 2.1 记录流（页边距、字体、列宽、页眉页脚、保护）；
- `...Office.Excel.ReportHelper` —— 把预置样式的报表工作表写入工作簿的扩展方法。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

' 读取已有工作簿
Using xlsx As File = File.Open("data.xlsx")
    For Each name As String In xlsx.SheetNames()
        Console.WriteLine(name)
    Next
End Using

' 写出新工作簿
Dim book As New Workbook("out.xlsx", "Sheet1")

Call book.CurrentWorksheet.AddCell("Name", "A1")
Call book.CurrentWorksheet.AddCell(3.14159, "B1")
Call book.Save()
```

## 实现要点

- **为什么样式要集中管理**：OOXML 的样式是共享表结构（`cellXfs` 索引到 `fonts` / `fills` / `borders`），逐单元格重复定义会显著增大文件；本包通过样式仓库去重。
- **共享字符串表**：字符串不是内联的，而是写入 `sharedStrings.xml` 并按索引引用，这是 xlsx 体积小的关键之一。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.Office.Excel`
- TargetFramework：`net10.0`
- Tags：`scibasic;xlsx;excel;openxml;spreadsheet;biff;workbook;report`
- 许可：GPL-3.0-or-later
