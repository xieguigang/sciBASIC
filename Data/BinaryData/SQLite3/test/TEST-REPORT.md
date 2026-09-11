# Managed SQLite3 读取模块测试报告

- 生成时间: 2026-09-11 17:12:12
- 测试模块: ``Microsoft.VisualBasic.Data.IO.SQLite3``
- 目标数据库: ``G:\compounds_2-copy.sqlite``
- 运行形态: 结构 + 抽样(大表 ``compounds`` 前 5,000 行, 其余表全量, 上限 200,000 行)
- 用例总数: 16, 通过 16, 失败 0, 累计耗时 20,583 ms
- 总体结论: **全部用例通过**

## 1. 测试环境

- 操作系统: Microsoft Windows NT 10.0.20348.0
- 运行框架: 10.0.12 / .NET 10.0.12
- 计算机: XIEGUIGANG-PC
- 处理器数量: 24
- 工作目录: ``G:\pixelArtist\src\framework\Data\BinaryData\SQLite3\test``

## 2. 数据库文件信息

- 文件路径: ``G:\compounds_2-copy.sqlite``
- 文件大小: 1,337,335,808 字节
- 页面大小 PageSize: 4096 字节
- 文本编码 TextEncoding: UTF8
- 预留空间 ReservedSpace: 0 字节
- Schema 格式: 4
- 文件头记录页数 DatabaseSizeInPages: 326,498
- 实际页数(文件大小/页大小): 326,498
- ChangeCounter: 935, ApplicationId: 0, UserVersion: 0

## 3. sqlite_master 条目

| type | name | tableName | rootPage |
|---|---|---|---|
| table | registries | registries | 2 |
| index | ix_registries_namespace | registries | 3 |
| table | compounds | compounds | 4 |
| index | ix_compounds_inchi | compounds | 191387 |
| index | ix_compounds_inchi_key | compounds | 191388 |
| index | ix_compounds_smiles | compounds | 191390 |
| table | compound_identifiers | compound_identifiers | 191391 |
| index | ix_compound_identifiers_accession | compound_identifiers | 191393 |
| table | compound_microspecies | compound_microspecies | 10 |
| table | magnesium_dissociation_constant | magnesium_dissociation_constant | 191394 |

## 4. 表结构解析结果

### compound_identifiers

```sql
CREATE TABLE compound_identifiers (
	created_on DATETIME NOT NULL, 
	updated_on DATETIME, 
	id INTEGER NOT NULL, 
	compound_id INTEGER NOT NULL, 
	registry_id INTEGER NOT NULL, 
	accession VARCHAR NOT NULL, 
	PRIMARY KEY (id), 
	FOREIGN KEY(compound_id) REFERENCES compounds (id), 
	FOREIGN KEY(registry_id) REFERENCES registries (id)
)
```

| # | 列名 | 声明类型 | 亲和性 |
|---|---|---|---|
| 0 | created_on | DATETIME | NUMERIC |
| 1 | updated_on | DATETIME | NUMERIC |
| 2 | id | INTEGER | INTEGER |
| 3 | compound_id | INTEGER | INTEGER |
| 4 | registry_id | INTEGER | INTEGER |
| 5 | accession | VARCHAR | TEXT |

### compound_microspecies

```sql
CREATE TABLE compound_microspecies (
	created_on DATETIME NOT NULL, 
	updated_on DATETIME, 
	id INTEGER NOT NULL, 
	compound_id INTEGER NOT NULL, 
	charge INTEGER NOT NULL, 
	number_protons INTEGER NOT NULL, 
	number_magnesiums INTEGER NOT NULL, 
	is_major BOOLEAN NOT NULL, 
	ddg_over_rt FLOAT, 
	PRIMARY KEY (id), 
	FOREIGN KEY(compound_id) REFERENCES compounds (id), 
	CHECK (is_major IN (0, 1))
)
```

| # | 列名 | 声明类型 | 亲和性 |
|---|---|---|---|
| 0 | created_on | DATETIME | NUMERIC |
| 1 | updated_on | DATETIME | NUMERIC |
| 2 | id | INTEGER | INTEGER |
| 3 | compound_id | INTEGER | INTEGER |
| 4 | charge | INTEGER | INTEGER |
| 5 | number_protons | INTEGER | INTEGER |
| 6 | number_magnesiums | INTEGER | INTEGER |
| 7 | is_major | BOOLEAN | BOOLEAN |
| 8 | ddg_over_rt | FLOAT | REAL |

### compounds

```sql
CREATE TABLE compounds (
	created_on DATETIME NOT NULL, 
	updated_on DATETIME, 
	id INTEGER NOT NULL, 
	inchi_key VARCHAR, 
	inchi VARCHAR, 
	smiles VARCHAR, 
	mass FLOAT, 
	atom_bag BLOB, 
	dissociation_constants BLOB, 
	group_vector BLOB, 
	PRIMARY KEY (id)
)
```

| # | 列名 | 声明类型 | 亲和性 |
|---|---|---|---|
| 0 | created_on | DATETIME | NUMERIC |
| 1 | updated_on | DATETIME | NUMERIC |
| 2 | id | INTEGER | INTEGER |
| 3 | inchi_key | VARCHAR | TEXT |
| 4 | inchi | VARCHAR | TEXT |
| 5 | smiles | VARCHAR | TEXT |
| 6 | mass | FLOAT | REAL |
| 7 | atom_bag | BLOB | BLOB |
| 8 | dissociation_constants | BLOB | BLOB |
| 9 | group_vector | BLOB | BLOB |

### magnesium_dissociation_constant

```sql
CREATE TABLE magnesium_dissociation_constant (
	created_on DATETIME NOT NULL, 
	updated_on DATETIME, 
	id INTEGER NOT NULL, 
	compound_id INTEGER NOT NULL, 
	number_protons INTEGER NOT NULL, 
	number_magnesiums INTEGER NOT NULL, 
	dissociation_constant FLOAT NOT NULL, 
	PRIMARY KEY (id), 
	FOREIGN KEY(compound_id) REFERENCES compounds (id)
)
```

| # | 列名 | 声明类型 | 亲和性 |
|---|---|---|---|
| 0 | created_on | DATETIME | NUMERIC |
| 1 | updated_on | DATETIME | NUMERIC |
| 2 | id | INTEGER | INTEGER |
| 3 | compound_id | INTEGER | INTEGER |
| 4 | number_protons | INTEGER | INTEGER |
| 5 | number_magnesiums | INTEGER | INTEGER |
| 6 | dissociation_constant | FLOAT | REAL |

### registries

```sql
CREATE TABLE registries (
	created_on DATETIME NOT NULL, 
	updated_on DATETIME, 
	id INTEGER NOT NULL, 
	name VARCHAR, 
	namespace VARCHAR NOT NULL, 
	pattern VARCHAR NOT NULL, 
	identifier VARCHAR, 
	url VARCHAR, 
	is_prefixed BOOLEAN NOT NULL, 
	access_url VARCHAR, 
	PRIMARY KEY (id), 
	CHECK (is_prefixed IN (0, 1))
)
```

| # | 列名 | 声明类型 | 亲和性 |
|---|---|---|---|
| 0 | created_on | DATETIME | NUMERIC |
| 1 | updated_on | DATETIME | NUMERIC |
| 2 | id | INTEGER | INTEGER |
| 3 | name | VARCHAR | TEXT |
| 4 | namespace | VARCHAR | TEXT |
| 5 | pattern | VARCHAR | TEXT |
| 6 | identifier | VARCHAR | TEXT |
| 7 | url | VARCHAR | TEXT |
| 8 | is_prefixed | BOOLEAN | BOOLEAN |
| 9 | access_url | VARCHAR | TEXT |

## 5. 数据扫描统计

| 表 | 模式 | 读取行数 | 列数 | RowId 单调 | RowId 区间 |
|---|---|---|---|---|---|
| compound_identifiers | 抽样 | 200,000 | 6 | 是 | [1, 381296] |
| compound_microspecies | 抽样 | 200,000 | 9 | 是 | [1, 200000] |
| compounds | 抽样 | 5,000 | 10 | 是 | [1, 5000] |
| magnesium_dissociation_constant | 全量 | 1,442 | 7 | 是 | [1, 1442] |
| registries | 全量 | 14 | 10 | 是 | [1, 14] |

### 表 compound_identifiers 列统计

| 列 | 声明类型 | 亲和性 | NULL 数 | 非空数 | 观测到的 CLR 类型 | 最大文本长度 | 最大 BLOB 长度 |
|---|---|---|---|---|---|---|---|
| created_on | DATETIME | NUMERIC | 0 | 200,000 | String | 26 | - |
| updated_on | DATETIME | NUMERIC | 200,000 | 0 |  | - | - |
| id | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| compound_id | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| registry_id | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| accession | VARCHAR | TEXT | 0 | 200,000 | String | 82 | - |

样例数据(最多 5 行):

```text
RowId=1: "2019-01-16 11:21:20.553397" (String), <NULL>, 1 (Int64), 1 (Int64), 14 (Int64), "cpd11416" (String)
RowId=3: "2019-01-16 11:21:20.553407" (String), <NULL>, 3 (Int64), 2 (Int64), 7 (Int64), "CHEBI:23367" (String)
RowId=4: "2019-01-16 11:21:20.553408" (String), <NULL>, 4 (Int64), 2 (Int64), 7 (Int64), "CHEBI:59999" (String)
RowId=5: "2019-01-16 11:21:20.553410" (String), <NULL>, 5 (Int64), 2 (Int64), 5 (Int64), "7" (String)
RowId=6: "2019-01-16 11:21:20.553411" (String), <NULL>, 6 (Int64), 2 (Int64), 14 (Int64), "cpd00000" (String)
```

### 表 compound_microspecies 列统计

| 列 | 声明类型 | 亲和性 | NULL 数 | 非空数 | 观测到的 CLR 类型 | 最大文本长度 | 最大 BLOB 长度 |
|---|---|---|---|---|---|---|---|
| created_on | DATETIME | NUMERIC | 0 | 200,000 | String | 26 | - |
| updated_on | DATETIME | NUMERIC | 200,000 | 0 |  | - | - |
| id | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| compound_id | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| charge | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| number_protons | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| number_magnesiums | INTEGER | INTEGER | 0 | 200,000 | Int64 | - | - |
| is_major | BOOLEAN | BOOLEAN | 0 | 200,000 | Boolean | - | - |
| ddg_over_rt | FLOAT | REAL | 0 | 200,000 | Double | - | - |

样例数据(最多 5 行):

```text
RowId=1: "2020-05-15 16:43:59.725064" (String), <NULL>, 1 (Int64), 3 (Int64), 0 (Int64), 0 (Int64), 0 (Int64), True (Boolean), 0 (Double)
RowId=2: "2020-05-15 16:43:59.725073" (String), <NULL>, 2 (Int64), 4 (Int64), 0 (Int64), 0 (Int64), 0 (Int64), True (Boolean), 0 (Double)
RowId=3: "2020-05-15 16:43:59.725076" (String), <NULL>, 3 (Int64), 5 (Int64), 0 (Int64), 2 (Int64), 0 (Int64), True (Boolean), 0 (Double)
RowId=4: "2020-05-15 16:43:59.725078" (String), <NULL>, 4 (Int64), 6 (Int64), -5 (Int64), 11 (Int64), 0 (Int64), False (Boolean), 46.097753561740795 (Double)
RowId=5: "2020-05-15 16:43:59.725080" (String), <NULL>, 5 (Int64), 6 (Int64), -4 (Int64), 12 (Int64), 0 (Int64), False (Boolean), 17.08518139001582 (Double)
```

### 表 compounds 列统计

| 列 | 声明类型 | 亲和性 | NULL 数 | 非空数 | 观测到的 CLR 类型 | 最大文本长度 | 最大 BLOB 长度 |
|---|---|---|---|---|---|---|---|
| created_on | DATETIME | NUMERIC | 0 | 5,000 | String | 26 | - |
| updated_on | DATETIME | NUMERIC | 4,988 | 12 | String | 26 | - |
| id | INTEGER | INTEGER | 0 | 5,000 | Int64 | - | - |
| inchi_key | VARCHAR | TEXT | 810 | 4,190 | String | 28 | - |
| inchi | VARCHAR | TEXT | 763 | 4,237 | String | 1,487 | - |
| smiles | VARCHAR | TEXT | 505 | 4,495 | String | 747 | - |
| mass | FLOAT | REAL | 686 | 4,314 | Double | - | - |
| atom_bag | BLOB | BLOB | 761 | 4,239 | Byte[] | - | 67 |
| dissociation_constants | BLOB | BLOB | 761 | 4,239 | Byte[] | - | 88 |
| group_vector | BLOB | BLOB | 505 | 4,495 | Byte[] | - | 342 |

样例数据(最多 5 行):

```text
RowId=1: "2019-01-16 11:19:47.970825" (String), <NULL>, 1 (Int64), <NULL>, <NULL>, <NULL>, <NULL>, <NULL>, <NULL>, <NULL>
RowId=2: "2019-01-16 11:19:47.970838" (String), <NULL>, 2 (Int64), <NULL>, <NULL>, <NULL>, <NULL>, <NULL>, <NULL>, <NULL>
RowId=3: "2019-01-16 11:19:47.970840" (String), "2020-05-15 16:42:52.898220" (String), 3 (Int64), "GPRLSGONYQIRFK-UHFFFAOYSA-N" (String), "InChI=1S/p+1" (String), <NULL>, 1.0070000000000001 (Double), <blob 21 bytes>, <blob 5 bytes>, <NULL>
RowId=4: "2019-01-16 11:19:47.970842" (String), "2020-05-15 16:42:52.898228" (String), 4 (Int64), "GPRLSGONYQIRFK-UHFFFAOYSA-N" (String), "InChI=1S/p+1" (String), <NULL>, 1.0070000000000001 (Double), <blob 21 bytes>, <blob 5 bytes>, <NULL>
RowId=5: "2019-01-16 11:19:47.970843" (String), <NULL>, 5 (Int64), "XLYOFNOQVPJJNP-UHFFFAOYSA-N" (String), "InChI=1S/H2O/h1H2" (String), "O" (String), 18.015 (Double), <blob 35 bytes>, <blob 14 bytes>, <blob 14 bytes>
```

### 表 magnesium_dissociation_constant 列统计

| 列 | 声明类型 | 亲和性 | NULL 数 | 非空数 | 观测到的 CLR 类型 | 最大文本长度 | 最大 BLOB 长度 |
|---|---|---|---|---|---|---|---|
| created_on | DATETIME | NUMERIC | 0 | 1,442 | String | 26 | - |
| updated_on | DATETIME | NUMERIC | 1,442 | 0 |  | - | - |
| id | INTEGER | INTEGER | 0 | 1,442 | Int64 | - | - |
| compound_id | INTEGER | INTEGER | 0 | 1,442 | Int64 | - | - |
| number_protons | INTEGER | INTEGER | 0 | 1,442 | Int64 | - | - |
| number_magnesiums | INTEGER | INTEGER | 0 | 1,442 | Int64 | - | - |
| dissociation_constant | FLOAT | REAL | 0 | 1,442 | Double | - | - |

样例数据(最多 5 行):

```text
RowId=1: "2020-05-15 16:43:00.128748" (String), <NULL>, 1 (Int64), 467 (Int64), 5 (Int64), 1 (Int64), 0.247769869369108 (Double)
RowId=2: "2020-05-15 16:43:00.128754" (String), <NULL>, 2 (Int64), 467 (Int64), 4 (Int64), 1 (Int64), 2.28626058683202 (Double)
RowId=3: "2020-05-15 16:43:00.128757" (String), <NULL>, 3 (Int64), 911 (Int64), 9 (Int64), 1 (Int64), 0.052756897705979 (Double)
RowId=4: "2020-05-15 16:43:00.128759" (String), <NULL>, 4 (Int64), 911 (Int64), 8 (Int64), 1 (Int64), 1.76697109952452 (Double)
RowId=5: "2020-05-15 16:43:00.128762" (String), <NULL>, 5 (Int64), 911 (Int64), 7 (Int64), 1 (Int64), 3.7445643956612003 (Double)
```

### 表 registries 列统计

| 列 | 声明类型 | 亲和性 | NULL 数 | 非空数 | 观测到的 CLR 类型 | 最大文本长度 | 最大 BLOB 长度 |
|---|---|---|---|---|---|---|---|
| created_on | DATETIME | NUMERIC | 0 | 14 | String | 26 | - |
| updated_on | DATETIME | NUMERIC | 14 | 0 |  | - | - |
| id | INTEGER | INTEGER | 0 | 14 | Int64 | - | - |
| name | VARCHAR | TEXT | 0 | 14 | String | 39 | - |
| namespace | VARCHAR | TEXT | 0 | 14 | String | 17 | - |
| pattern | VARCHAR | TEXT | 0 | 14 | String | 63 | - |
| identifier | VARCHAR | TEXT | 3 | 11 | String | 12 | - |
| url | VARCHAR | TEXT | 3 | 11 | String | 40 | - |
| is_prefixed | BOOLEAN | BOOLEAN | 0 | 14 | Boolean | - | - |
| access_url | VARCHAR | TEXT | 2 | 12 | String | 77 | - |

样例数据(最多 5 行):

```text
RowId=1: "2019-01-16 11:19:42.298104" (String), <NULL>, 1 (Int64), "enviPath" (String), "envipath" (String), "^.+$" (String), <NULL>, <NULL>, False (Boolean), "https://envipath.org/package/{$id}" (String)
RowId=2: "2019-01-16 11:19:42.513024" (String), <NULL>, 2 (Int64), "Component-Contribution Metabolite" (String), "coco" (String), "^COCOM\d+$" (String), <NULL>, <NULL>, False (Boolean), <NULL>
RowId=3: "2019-01-16 11:19:42.513968" (String), <NULL>, 3 (Int64), "Synonyms" (String), "synonyms" (String), "^.+$" (String), <NULL>, <NULL>, False (Boolean), <NULL>
RowId=4: "2019-01-16 11:19:42.835052" (String), <NULL>, 4 (Int64), "MetaNetX chemical" (String), "metanetx.chemical" (String), "^(MNXM\d+|BIOMASS)$" (String), "MIR:00000567" (String), "http://identifiers.org/metanetx.chemical" (String), False (Boolean), "http://www.metanetx.org/chem_info/{$id}" (String)
RowId=5: "2019-01-16 11:19:42.835946" (String), <NULL>, 5 (Int64), "SABIO-RK Compound" (String), "sabiork.compound" (String), "^\d+$" (String), "MIR:00000688" (String), "http://identifiers.org/sabiork.compound" (String), False (Boolean), "http://sabiork.h-its.org/newSearch?q=sabiocompoundid:{$id}" (String)
```

### 溢出页(长记录)探测

- 探测表: ``compounds``, 扫描行数: 300,000 (达到探测上限)
- 单页内联阈值 U-35: 4061 字节
- 观测到最大 TEXT 长度: 6,031 字节
- 观测到最大 BLOB 长度: 342 字节
- 是否命中溢出页: 是

## 6. 测试用例结果

| # | 用例 | 结果 | 耗时(ms) | 说明 |
|---|---|---|---|---|
| 1 | 文件头解析 | 通过 | 181 |  |
| 2 | 枚举 sqlite_master | 通过 | 0 |  |
| 3 | 表结构解析: compound_identifiers | 通过 | 2 |  |
| 4 | 表结构解析: compound_microspecies | 通过 | 2 |  |
| 5 | 表结构解析: compounds | 通过 | 0 |  |
| 6 | 表结构解析: magnesium_dissociation_constant | 通过 | 0 |  |
| 7 | 表结构解析: registries | 通过 | 0 |  |
| 8 | 扫描: compound_identifiers | 通过 | 1395 |  |
| 9 | 扫描: compound_microspecies | 通过 | 970 |  |
| 10 | 扫描: compounds | 通过 | 78 |  |
| 11 | 扫描: magnesium_dissociation_constant | 通过 | 10 |  |
| 12 | 扫描: registries | 通过 | 6 |  |
| 13 | 溢出页/长记录校验: compounds | 通过 | 17936 |  |
| 14 | 取值校验: compounds | 通过 | 0 |  |
| 15 | blobAsBase64 设置 | 通过 | 3 |  |
| 16 | 未知表异常处理 | 通过 | 0 |  |

## 7. 发现的问题与修复记录

| 编号 | 问题 | 根因 | 修复 | 状态 |
|---|---|---|---|---|
| I-01 | BOOLEAN 声明类型无法解析, GetTable 直接抛异常 | DataTypeParser.TryParse 仅映射 bool/[bool]/bit, 未覆盖 boolean; 且遇到未知声明类型直接抛 NotImplementedException。 | DataTypeParser 新增 boolean/bool/bit/date/datetime/numeric/real 等映射, 未知声明类型按 SQLite 亲和性规则回退, 不再抛异常。 | 已修复(复测通过) |
| I-02 | 可空列 NULL 值读取错误 | ParseRow 中记录头 serial type=0(NULL) 时仅注释、既不重置长度也不标记为空, 且 ColumnDataMeta 在行间共享复用。 | ParseRow 改为按记录头 serial type 逐列解码, serial type=0 明确返回 Nothing, 不再共享可变的列元数据。 | 已修复(复测通过) |
| I-03 | 按声明类型而非真实存储类型解码 | SQLite 为动态类型, 记录头 serial type 才是每列真实存储类型的唯一依据; 原实现使用 schema 声明类型决定读取方式。 | 改为按 serial type 解码: 0=NULL、1..6=整数、7=IEEE 浮点、8/9=0/1、偶数>=12=BLOB、奇数>=13=TEXT; 并依据声明亲和性做合理转换(如 FLOAT 列返回 Double, BOOLEAN 列返回 Boolean)。 | 已修复(复测通过) |
| I-04 | BOOLEAN 列恒为 True | 原实现 Select Case 中 Boolean1 分支直接赋值 True, 未读取真实值。 | 按 serial type 8/9 取 0/1 并转换为真实 Boolean。 | 已修复(复测通过) |
| I-05 | cellOffsets 排序打乱记录行序 | BTreePage.Parse 对 cell 指针数组执行 Array.Sort, 而 SQLite 的 cell 指针数组本已按 key(rowid) 有序, 按物理偏移重排会破坏行序。 | 移除 Array.Sort, 保持页内 cell 指针数组的 key 顺序遍历。 | 已修复(复测通过) |
| I-06 | 9 字节 VarInt 读取计数偏移 | ReadVarInt 在第 9 字节分支对 readBytes 多加 1。 | 修正 9 字节分支的字节计数。 | 已修复(间接验证) |
| I-07 | 记录列数与 schema 列数不一致时越界 | ParseRow 未对列索引做边界检查。 | 解码循环加入列数边界保护, 超出部分安全忽略。 | 已修复(间接验证) |
| I-08 | 表级 CHECK 约束被误判为数据列 | Schema.ParseColumns 仅跳过 UNIQUE/FOREIGN KEY/PRIMARY KEY 约束, 未处理 CHECK/CONSTRAINT 约束。 | Schema.ParseColumns 新增跳过 CHECK/CONSTRAINT 约束, 并为缺失类型声明的列按 BLOB 亲和性回退。 | 已修复(复测通过) |
| I-09 | INTEGER PRIMARY KEY(rowid 别名)列被读成 NULL | SQLite 把 INTEGER PRIMARY KEY 作为 rowid 的别名, 记录体之中该列存储为 NULL, 读取时需要用该行的 rowid 回填; 原实现直接返回 NULL。 | Schema 记录主键列名, Sqlite3Table 识别 INTEGER PRIMARY KEY 别名列, 并在解码完成后用 rowid 回填该列。 | 已修复(复测通过) |
| I-10 | FLOAT 列存储 0/1 时 CLR 类型不一致 | SQLite 对 0/1 使用 serial type 8/9; 解码后只按 BOOLEAN 处理, 未考虑 FLOAT 亲和性。 | ToDeclaredBoolean 对 FLOAT 亲和性列返回 Double, 保证数值列类型稳定。 | 已修复(复测通过) |

> 说明: 相关用例全部通过时状态记为 [已修复(复测通过)]; 存在失败用例时记为 [复现/待修复]。

## 8. 复测结论

全部测试用例通过, 读取模块可正确解析目标数据库的文件头、sqlite_master、各表结构以及数据行,
包含可空列 NULL、BOOLEAN、FLOAT、TEXT、BLOB、rowid 别名以及表级约束等场景。
溢出页路径已被实际覆盖(最大字段 6,031 字节 > 内联阈值 4061 字节), 未发现异常。

