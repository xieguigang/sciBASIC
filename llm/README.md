# LLM：decoder-only 语言模型（MoE / KV Cache / Function Calling）

## 一、这个模块做什么

在既有 `Transformer` 命名空间（编码器-解码器翻译模型）之外，提供一套 **decoder-only 语言模型**的纯托管实现，覆盖三条主线：

| 主线 | 类型 | 说明 |
|---|---|---|
| **混合专家** | `MoELayer` | DeepSeekMoE：细粒度专家分割 + 共享专家隔离 + Sigmoid Top-K 路由 + 无辅助损失负载均衡 + 节点受限路由 |
| **KV Cache** | `KVCache` / `CausalSelfAttention` | prefill + 增量解码，把单步注意力开销从 `O(t²)` 降到 `O(t)` |
| **函数调用** | `ToolCallProtocol` / `JsonSchema` / `ConstrainedDecoder` / `ToolRegistry` / `AgentLoop` | 工具 Schema 注入 → 生成调用片段 → 约束解码 → 解析 → 执行 → 回填 → 多轮循环 |

以及支撑它们的基础模块：`RmsNorm`、`RotaryEmbedding`（RoPE）、`SwiGLUFeedForward`、`LLMBlock`、`LLMModel`、`Sampler`、`LMTrainer`、`AdamW`、`ParameterSet`、`TokenStream`、`TokenizerVocabulary`。

## 二、工程约定

刻意与既有 `Transformer` 命名空间保持完全一致的约定，**不引入自动微分**：

* 每个组件都暴露 `LastCache` 与 `Cache` 类型；
* 反向传播显式接收当步的缓存快照，手写求导公式；
* 梯度用「与参数同形的张量 + 原地 `+=`」累加；
* `MakeTrainingStep(lr, [step])` 完成更新并清零梯度。

每条前向路径的形状约定统一为：

```
x          : [B, S, dModel]
权重        : [in, out]            （与 BatchedMatMul 的语义一致）
Q/K        : [B, S, nHeads  * headDim]
V          : [B, S, nKvHeads * headDim]
MoE 输出    : [B, S, dModel]        （内部按 [B*S, dModel] 展平运算）
```

## 三、三条主线要点

### MoE（`MoELayer.vb`）

层输出对应理论公式：

```
h_t = Σ_{i≤Ks} FFN_i(u_t)  +  Σ_{i>Ks} g_{i,t}·FFN_i(u_t)  +  u_t
      └ 共享专家，无条件  ┘     └ Top-K 路由专家，稀疏  ┘    └ 残差由 LLMBlock 加 ┘
```

* **门控**：`s = sigmoid(u·Wr)`，Top-K 依据 `s + b`，权重 `g = s/Σs`（在被选中的 K 个之间归一化）。
* **负载均衡**：`b_k ← b_k + ε_k·(L − A_k)`，`ε_k = u/|L − A_k|`（等价于固定步长的符号更新）；
  偏置只影响"选择"，不参与反向传播，因此**不给主损失加任何辅助项**。
* **节点受限路由**：按"组内最大的若干打分之和"给节点打分，只保留得分最高的 M 个节点，
  其余专家的选择打分置 `-inf` 后再做 Top-K。
* **反向**：只对命中的 Top-K 专家回传，与稀疏前向对称；门控权重的梯度是
  「专家输出 · 上游梯度」的内积，再经 `g = s/Σs` 的雅可比与 sigmoid 导数回到路由器。

### KV Cache（`KVCache.vb` + `CausalSelfAttention.vb`）

* 缓存布局 `[maxSeq, nKvHeads, headDim]`：**位置优先**，因此前缀在内存里是连续的一段，
  追加退化为整块 `Array.Copy`，前缀视图可以零拷贝暴露。
* 前向两条路径：`caches = Nothing` 走全序列因果掩码（训练，支持反向）；
  传入缓存则走 prefill / 增量解码（推理，不保存反向中间量）。
* `nKvHeads < nHeads` 即为 GQA / MQA，缓存按 `groupSize` 缩小。
* `LLMModel` 额外缓存了输出层权重的转置：它是常量，但每步重算的代价足以掩盖
  KV Cache 带来的复杂度收益。

### Function Calling（`ConstrainedDecoder.vb` 等）

* `ConstrainedDecoder` 把 `JsonSchema` 编译成有限状态机，逐字符推进；
  键必须按 schema 顺序出现、必填键一个不能少、枚举值只能是候选之一。
* 每步枚举"合法 token"的两级索引：**首字符分桶**（`TokenizerVocabulary`）先整体跳过绝大多数桶，
  桶内再用状态机逐字符验证；自由字符串状态有一条 `O(len)` 的快路径。
* 非法 token 的 logits 被置为 `-inf`，因此"生成结构非法的参数"在物理上不可达。
  但它**只保证语法**：语义是否合理（城市名对不对）仍由 `ToolRegistry` 校验并回填错误。
* `AgentLoop` 负责协议层：识别信号灯 → 补齐调用头部 → 约束解码参数 → 解析 →
  执行 → 结果回填（复用 KV Cache 前缀）→ 循环，直到模型不再输出调用标记。

## 四、与 `TalkBuddy/readme.md` 的对应

| readme 里的说法 | 本模块的落地 |
|---|---|
| 文本 → token → 嵌入加位置信息 → 多层「因果自注意力 + FFN」→ 词表概率 → 采样 | `LLMModel.Forward` + `Sampler` |
| RoPE 让 `q_m·k_n` 只依赖相对距离 | `RotaryEmbedding.Apply` |
| 现代模型多用 RMSNorm 且采用 Pre-Norm | `RmsNorm` + `LLMBlock` |
| FFN 多用 SwiGLU 等门控激活 | `SwiGLUFeedForward` |
| KV Cache 把单步开销从 `O(t²)` 降到 `O(t)` | `KVCache` + `CausalSelfAttention` |
| MQA/GQA 可压缩缓存 | `nKvHeads` 参数 |
| DeepSeekMoE 的细粒度分割 + 共享专家隔离 | `MoELayer` |
| Sigmoid 归一化 + Top-K 门控 | `MoELayer.Forward` |
| 无辅助损失的动态偏置 | `MoELayer.UpdateBalanceBias` |
| 节点受限路由 | `MoELayer.ApplyNodeLimitedRouting` |
| 特殊 token 是被加进词表的保留 token | `ToolCallProtocol` |
| 约束解码把 Schema 编译成 FSM 并掩码 logits | `ConstrainedDecoder` |
| 闭式调用闭环 / Agent Loop / 最大轮次兜底 | `AgentLoop` |
| SFT 时工具结果 token 不计损失 | `LLMTensorOps.MaskedCrossEntropy` |
| 权重绑定、AdamW、warmup + cosine、梯度裁剪 | `LLMModel` / `AdamW` / `LMTrainer` |

## 五、使用方式

见 `TalkBuddy` 项目中的装配层与 `TalkBuddy/test/Program.vb` 的完整演示：
它会依次跑通「预训练 → 指令 SFT → 工具调用 SFT → MoE 路由统计 → 采样策略对比 →
KV Cache 一致性验证 → 多轮工具调用 → 约束解码 → 权重持久化」。

## 六、许可证

GPL-3.0-or-later
