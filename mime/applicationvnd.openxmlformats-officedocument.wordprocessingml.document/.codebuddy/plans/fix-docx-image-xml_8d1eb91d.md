---
name: fix-docx-image-xml
overview: 修复 WordDocument.vb 中图片 XML 生成缺少必需元素导致 Word 无法打开 docx 的问题。
todos:
  - id: fix-image-xml
    content: 修复 Image 方法中图片 XML 结构，在 WordDocument.vb 中补充缺失的 wp:effectExtent、wp:cNvGraphicFramePr、pic:nvPicPr 三个必需 OOXML 元素
    status: completed
  - id: verify-docx
    content: 编译运行测试，验证生成的 docx 文件能被 Word 正常打开
    status: completed
    dependencies:
      - fix-image-xml
---

## 问题描述

通过 `WordDocument.Image()` 方法生成的 docx 文件无法被 Microsoft Word 打开，根因是生成的图片 DrawingML XML 结构不符合 OOXML 规范，缺少 3 个必需子元素。

## 核心修复

在 `WordDocument.vb` 的 `Image` 方法（第583-594行）中，向图片 XML 模板补充以下 OOXML 规范要求的缺失元素：

1. **`<wp:effectExtent>`**：在 `<wp:extent>` 之后插入，声明绘图效果扩展边界为零
2. **`<wp:cNvGraphicFramePr/>`**：在 `<wp:docPr>` 之后插入，非可视图形框架属性
3. **`<pic:nvPicPr>`**：在 `<pic:pic>` 之后、`<pic:blipFill>` 之前插入，包含 `<pic:cNvPr>` 和 `<pic:cNvPicPr/>` 子元素，是非可视图片属性（OOXML 要求 `<pic:pic>` 的第一个子元素）

## 技术方案

### 修改文件

- `docx/WordDocument.vb`：第 583-594 行的 `Image` 方法

### 修改详情

**位置 1**：第 585 行之后插入 `wp:effectExtent`

```xml
<wp:effectExtent l="0" t="0" r="0" b="0"/>
```

**位置 2**：第 586 行之后插入 `wp:cNvGraphicFramePr`

```xml
<wp:cNvGraphicFramePr/>
```

**位置 3**：第 588 行，`<pic:pic>` 之后插入 `pic:nvPicPr` 块（作为 `<pic:pic>` 的第一个子元素）

```xml
<pic:nvPicPr>
  <pic:cNvPr id="{imgId}" name="Picture {imgId}"/>
  <pic:cNvPicPr/>
</pic:nvPicPr>
```

### 正确的 OOXML 图片结构

```
<w:drawing>
  <wp:inline distT="0" distB="0" distL="0" distR="0">
    <wp:extent cx="..." cy="..."/>
    <wp:effectExtent l="0" t="0" r="0" b="0"/>        ← 补充
    <wp:docPr id="..." name="..."/>
    <wp:cNvGraphicFramePr/>                              ← 补充
    <a:graphic>
      <a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture">
        <pic:pic>
          <pic:nvPicPr>                                  ← 补充
            <pic:cNvPr id="..." name="..."/>
            <pic:cNvPicPr/>
          </pic:nvPicPr>
          <pic:blipFill>...</pic:blipFill>
          <pic:spPr>...</pic:spPr>
        </pic:pic>
      </a:graphicData>
    </a:graphic>
  </wp:inline>
</w:drawing>
```

### 验证方式

- 编译项目
- 运行 `dotnet run --project test/test.vbproj` 生成 docx
- 用 Microsoft Word 打开 `demo_full_report.docx` 和 `demo_blocks.docx`，确认可正常打开