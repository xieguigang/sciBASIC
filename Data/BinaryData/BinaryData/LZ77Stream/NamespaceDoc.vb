Namespace LZ77Stream

    ''' <summary>
    ''' # LZ4 / LZMA 解压算法详解
    ''' 两者同属 **LZ77 家族**——核心思想都是“把重复内容替换为（距离， 长度）的反向引用，不重复的部分作为字面量保留”。但它们的取舍截然相反：
    ''' - **LZ4**：指令原样写入字节流，**零熵编码**，换取极致解压速度（GB/s 级）
    ''' - **LZMA**：指令流先经过**自适应概率模型**，再用**范围编码** 压成比特流，换取极致压缩率
    ''' ---
    ''' ## 一、LZ4 解压
    ''' ### 1.1 块格式
    ''' LZ4 压缩块是一串“序列”，每个序列的结构：
    ''' ```
    ''' ┌───────┬──────────────┬───────────────┬────────┬──────────────┐
    ''' │ token │ 字面量长度扩展 │ 字面量数据      │ 偏移    │ 匹配长度扩展   │
    ''' │ 1字节 │  (可选)       │ (literal)     │ 2字节  │  (可选)       │
    ''' └───────┴──────────────┴───────────────┴────────┴──────────────┘
    ''' ```
    ''' - **token 高 4 位**：字面量长度（0–15）；若为 15，后续追加扩展字节——每个字节累加到长度上，遇到非 0xFF 的字节终止
    ''' - **token 低 4 位**：匹配长度 − 4（**最小匹配长度是 4**，因为一个匹配至少要花掉 token + 2 字节偏移）；若为 15 同样追加扩展字节
    ''' - **偏移**：2 字节小端，指从当前输出位置**往前**数多少字节找到匹配（1–65535）
    ''' - **最后一个序列只含字面量**（无偏移和匹配）——这是格式规定，编码器保证块尾 5 字节是字面量
    ''' ### 1.2 解压伪代码
    ''' ```python
    ''' def lz4_decompress(src, dst):
    ''' ip = 0                      # 输入指针
    ''' while True:
    ''' token = src[ip]; ip += 1
    ''' # ① 解析字面量长度（高4位 + 可选扩展）
    ''' lit_len = token >> 4
    ''' if lit_len == 15:
    ''' while True:
    ''' b = src[ip]; ip += 1
    ''' lit_len += b
    ''' if b != 255: break
    ''' # ② 拷贝字面量（可整块 memcpy，最快路径）
    ''' dst += src[ip : ip + lit_len]
    ''' ip += lit_len
    ''' if ip == len(src):      # 最后一个序列：只有字面量
    ''' break
    ''' # ③ 匹配偏移（2字节小端）
    ''' offset = src[ip] | (src[ip+1] &lt;&lt; 8); ip += 2
    ''' # ④ 匹配长度（低4位 + 4，可选扩展）
    ''' mlen = token &amp; 0x0F
    ''' if mlen == 15:
    ''' while True:
    ''' b = src[ip]; ip += 1
    ''' mlen += b
    ''' if b != 255: break
    ''' mlen += 4
    ''' # ⑤ 复制匹配 —— 偏移小于长度时必须逐字节复制！
    ''' for _ in range(mlen):
    ''' dst += dst[-offset]
    ''' ```
    ''' ### 1.3 关键细节：重叠匹配
    ''' 第 ⑤ 步是 LZ4 唯一的“陷阱”：**偏移可以小于匹配长度**。例如偏移=1、长度=15，意思是“把前一个字节重复 15 次”（相当于 RLE）。此时不能 `memcpy`，必须逐字节复制，因为源区域会被写入过程不断改写——也正是这个特性让短距离重复极其高效。
    ''' **示例**：`"A" × 21` 压缩为：
    ''' ```
    ''' [0x1B] 'A' [01 00]   ← 1个字面量 + 偏移1 + 匹配15
    ''' [0x50] "AAAAA"       ← 收尾的纯字面量序列
    ''' ```
    ''' ### 1.4 帧格式（Frame Format）
    ''' 块格式之上还有一层封装：
    ''' - 魔数 `0x184D2204`，FLG/BD 描述字节
    ''' - 每个块带 4 字节长度前缀（**最高位=1 表示该块未压缩直接存储**）
    ''' - 结束标记 `0x00000000`，可选 XXH32 内容校验
    ''' - 块可设置为相互独立 → 支持**多线程并行解压**
    ''' 另外：LZ4 的 fast/HC 是两种压缩器，但**解压端只认格式**，完全一致。处理不可信数据时必须校验 `offset ≤ 已输出长度`、匹配不越界，否则会有越界读写风险（LZ4 历史上出过此类 CVE）。
    ''' ---
    ''' ## 二、LZMA 解压
    ''' ### 2.1 架构：三层结构
    ''' ```
    ''' 压缩比特流 ──→ 范围解码器 ──→ (决策序列) ──→ LZ77重建 ──→ 原始数据
    ''' ↑ 更新概率
    ''' 上下文概率模型
    ''' (state / posState / rep距离 / …)
    ''' ```
    ''' 核心区别在于：LZMA 把所有决策——*“这是字面量还是匹配？长度多少？距离多少？”*——都转化为**一串二进制判决**，每个判决的概率取决于上下文（这就是名字里 "Markov" 的含义），并随数据自适应调整。
    ''' ### 2.2 流头部
    ''' ```
    ''' 1字节 props = (pb×5 + lp)×9 + lc     ← 默认 0x5D(93): lc=3, lp=0, pb=2
    ''' 4字节 词典大小（小端）
    ''' 8字节 原始大小（小端；全 1 = 未知）
    ''' 之后：首字节忽略（范围编码器固定输出一个前导零字节），再读 4 字节大端作为解码器初值
    ''' ```
    ''' - `lc`（literal context）：用前一字节的高 lc 位给字面量分上下文
    ''' - `lp`（literal position）：用当前位置的低位给字面量分上下文
    ''' - `pb`（pos bits）：用位置低位给匹配类决策分上下文（利用数据对齐结构）
    ''' ### 2.3 范围解码器（核心机制）
    ''' 维护一个“预算区间” `range` 和“当前位置” `code`，**每个二进制判决按概率切分区间**：
    ''' ```python
    ''' range = 0xFFFFFFFF
    ''' code  = 大端读4字节
    ''' def decode_bit(p):              # p: 11位概率, 1024 ≈ P(0)=0.5
    ''' global range, code
    ''' bound = (range >> 11) * p   # 按概率切一刀
    ''' if code &lt; bound:            # 落在前段 → 判 0
    ''' range = bound
    ''' p += (2048 - p) >> 5    # 概率自适应：往0方向修正
    ''' bit = 0
    ''' else:                       # 落在后段 → 判 1
    ''' code -= bound
    ''' range -= bound
    ''' p -= p >> 5
    ''' bit = 1
    ''' if range &lt; (1 &lt;&lt; 24):       # 重归一化：区间太小就补充新字节
    ''' range &lt;&lt;= 8
    ''' code = (code &lt;&lt; 8) | read_byte()
    ''' return bit
    ''' ```
    ''' 几个要点：
    ''' - **直觉**：p=1024 时 `bound = (0xFFFFFFFF >> 11) × 1024 ≈ 2^31`，即把区间几乎对半分——正是“五五开”的体现。概率越偏，切分越不对称
    ''' - **压缩来源**：概率接近 1/2 时约每 8 次判决消耗 1 字节；模型预测越准、概率越极端，每次判决消耗的区间越少。**省掉的就是“可预测性”**
    ''' - 概率用 11 位整数而非浮点，`(range >> 11) × p` 用一次乘法替代除法；每次判决后以约 1/32 的步长自适应
    ''' - 另有 `decode_direct_bits(n)`：无概率、按 1/2 解码 n 个裸比特，用于大距离的高位
    ''' ### 2.4 上下文模型（解压器的“记忆”）
    ''' | 变量 | 含义 |
    ''' |---|---|
    ''' | `posState = pos &amp; ((1&lt;&lt;pb)-1)` | 当前输出位置的低位 |
    ''' | `state`（0–11） | 有限状态机：字面量连续到了第几步、上个符号是匹配/重复匹配/短匹配 |
    ''' | `rep0..rep3` | 最近 4 个匹配距离（LRU） |
    ''' | 各概率数组 | `isMatch`、`isRep`、`isRepG0/G1/G2`、`isRep0Long`、字面量位树、长度模型×2、posSlot 位树×4、specPos、align 等 |
    ''' 状态转移规则（解出符号后更新）：
    ''' ```
    ''' 字面量后:  state&lt;4 → 0;  4..9 → state-3;  10..11 → state-6
    ''' 新匹配后:  state&lt;7 → 7, 否则 10
    ''' rep匹配后: state&lt;7 → 8, 否则 11
    ''' 短rep后:   → 9
    ''' ```
    ''' ### 2.5 主循环
    ''' ```python
    ''' while True:
    ''' pos_state = len(out) &amp; pos_mask
    ''' if decode_bit(is_match[state][pos_state]) == 0:
    ''' # ── 字面量 ──（紧跟匹配后的第一个字面量走"匹配字面量"模式）
    ''' out.append(decode_literal(state, rep0))
    ''' state = literal_next(state)
    ''' elif decode_bit(is_rep[state]) == 0:
    ''' # ── 全新匹配 ──
    ''' rep3, rep2, rep1 = rep2, rep1, rep0      # 距离入队
    ''' length = decode_length(len_dec)          # 2..273
    ''' dist = decode_distance(length)
    ''' if dist == 0xFFFFFFFF: break             # 流结束标记！
    ''' rep0 = dist
    ''' copy_match(out, dist, length)
    ''' state = 7 if state &lt; 7 else 10
    ''' elif decode_bit(is_rep_g0[state]) == 0:
    ''' if decode_bit(is_rep0_long[state][pos_state]) == 0:
    ''' out.append(out[-(rep0 + 1)])         # 短rep：复用rep0距离,只输出1字节
    ''' state = 9
    ''' else:
    ''' copy_match(out, rep0, decode_length(rep_len_dec))
    ''' state = 8 if state &lt; 7 else 11
    ''' else:
    ''' # ── rep1 / rep2 / rep3：从历史距离中选一个,并按LRU提到rep0 ──
    ''' d = 选中的repN; 调整rep数组
    ''' copy_match(out, d, decode_length(rep_len_dec))
    ''' state = 8 if state &lt; 7 else 11
    ''' ```
    ''' 其中匹配复制天然支持重叠：
    ''' ```python
    ''' def copy_match(out, dist, length):   # dist 为 0 基距离(实际回退 dist+1 字节)
    ''' for _ in range(length):
    ''' out.append(out[-(dist + 1)])
    ''' ```
    ''' **为什么要有 rep 匹配**？数据里同一距离常被反复使用（结构化数据、周期性内容），编码“复用旧距离”只需几个比特，比完整编码一个距离便宜得多。
    ''' ### 2.6 字面量解码：普通模式与匹配模式
    ''' **普通模式**：按上下文选一棵 256 叶的位树，从高位到低位逐位判决：
    ''' ```python
    ''' ctx = ((pos &amp; lp_mask) &lt;&lt; lc) + (prev_byte >> (8 - lc))
    ''' symbol = 1
    ''' while symbol &lt; 0x100:
    ''' symbol = (symbol &lt;&lt; 1) | decode_bit(lit_probs[ctx][symbol])
    ''' byte = symbol &amp; 0xFF
    ''' ```
    ''' **匹配模式**（仅紧跟匹配后的第一个字面量）：匹配刚结束时，下一个字面量往往“长得像”匹配本该继续输出的下一个字节 `match_byte`（比如匹配了 `"the "` 之后，下一个字面量很常以 `t`、`m` 等开头）。LZMA 逐位对比：
    ''' ```python
    ''' while symbol &lt; 0x100:
    ''' match_bit = (match_byte >> 7) &amp; 1
    ''' bit = decode_bit(probs[ctx][0x100 + (match_bit &lt;&lt; 8) + symbol])
    ''' symbol = (symbol &lt;&lt; 1) | bit
    ''' if bit != match_bit:
    ''' break                      # 一旦"分岔",剩余位切回普通模式
    ''' # （分岔后继续用普通位树解完剩余位）
    ''' ```
    ''' 与 match_byte 相同的前缀位用专用紧概率解码，第一位不同后切回普通模式——这是 LZMA 压缩率的精妙来源之一。
    ''' ### 2.7 长度与距离解码
    ''' **长度**（2–273）：层级结构，小长度用更少的比特：
    ''' ```python
    ''' if decode_bit(choice)  == 0: return 2  + bit_tree(3, low)   # 2..9
    ''' if decode_bit(choice2) == 0: return 10 + bit_tree(3, mid)   # 10..17
    ''' return 18 + bit_tree(8, high)                               # 18..273
    ''' ```
    ''' **距离**：先解 6 位 posSlot（0–63，按 length&lt;4 与否选两套位树），再分档：
    ''' ```python
    ''' slot = bit_tree(6, pos_slot[3 if length >= 4 else length])
    ''' if slot &lt; 4:
    ''' dist = slot                                # 0基:回退1~4字节
    ''' else:
    ''' n = (slot >> 1) - 1                        # 额外位数
    ''' dist = (2 | (slot &amp; 1)) &lt;&lt; n               # 基数
    ''' if slot &lt; 14:
    ''' dist += bit_tree_reverse(n, spec_pos)  # 中档距离:带概率
    ''' else:
    ''' dist += direct_bits(n - 4) &lt;&lt; 4        # 高位:裸比特
    ''' dist += bit_tree_reverse(4, align)     # 低4位:align概率
    ''' ```
    ''' 设计逻辑：小距离高频 → 用细粒度概率模型；大距离低位常体现数据对齐（4/8 字节结构）→ 低 4 位单独用 `align` 概率；中间的“无信息”位直接裸编码。
    ''' ### 2.8 结束条件与容器格式
    ''' - 头部 8 字节已知原始大小 → 按字节计数终止
    ''' - 未知大小时，靠解出**距离 0xFFFFFFFF** 作为流结束标记
    ''' - `.xz` 容器用的是 **LZMA2**：在 LZMA 外再包一层分块（支持未压缩块、字典重置、并行压缩），加上过滤器（delta/BCJ）和 CRC 校验；解压核心仍是上述过程
    ''' **为什么 LZMA 解压慢**：每输出一字节平均要做 8+ 次二进制判决，每次一乘、两次概率表访问、一个数据依赖分支；概率表访问是跳跃式的，cache 不友好。解压内存主要是词典缓冲（典型几 MB），概率表本身只有几十 KB。
    ''' ---
    ''' ## 三、对比总结
    ''' | | LZ4 | LZMA（xz） | zstd（参照） |
    ''' |---|---|---|---|
    ''' | 压缩率（典型） | ~2× | ~3–4×，可更高 | 2.5–3.5× |
    ''' | 解压速度 | **数 GB/s/核** | 数十 MB/s/核 | ~1–2 GB/s |
    ''' | 解压内存 | 输出缓冲 | 词典 + 小概率表 | 窗口 + 表 |
    ''' | 匹配范围 | 64 KB | 词典大小（MB 级） | 窗口可调 |
    ''' | 熵编码 | 无 | 范围编码 + 自适应模型 | FSE/哈夫曼 |
    ''' | 典型场景 | zram、ZFS/btrfs、数据库、实时传输、内核 squashfs | 软件包、固件镜像、长期归档 | 通用中间路线 |
    ''' **一句话总结**：
    ''' - LZ4 的解压器本质是一个 **200 行左右的格式解析循环**——线性扫 token、memcpy 字面量、逐字节抄匹配，瓶颈只在内存带宽；
    ''' - LZMA 的解压器是一台 **“范围解码器 + 自适应状态机”驱动的虚拟机**——把整个 LZ77 指令流当作一串带上下文的二进制判决来重建，用计算量换压缩率。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace