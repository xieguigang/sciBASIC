#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' =========================================================================
' Spiking neural network tutorial (standard library edition)
'
'   ml.snnTrain trains a LIF network on 3 gaussian clusters and returns a
'   report object (accuracy, per epoch loss / accuracy, spike rasters), so the
'   whole training pipeline lives in the standard library and the script only
'   prints the result — exactly like spiking_nn.js does on the njs engine.
' =========================================================================

call console.WriteLine("  Spiking Neural Network  ·  LIF + SurrogateGrad + Adam")
call console.WriteLine()

dim result = ml.snnTrain(
    perClass:=120,
    epochs:=40,
    seed:=2024,
    hidden:=64,
    timeSteps:=30,
    batchSize:=36)

call console.WriteLine($"  数据: 3 类 × 8 维高斯团簇（σ=0.09）, 训练 {result.trainSamples}, 测试 {result.testSamples}")
call console.WriteLine($"  网络结构: 8 → [LIF×{result.hidden} β=0.85 θ=1.0] → [LIF×3], T={result.timeSteps}")
call console.WriteLine("  训练配置: Adam(lr=0.002) + 梯度裁剪(1.0) + FastSigmoid 替代梯度(α=2)")
call console.WriteLine()

' ---- 输入层脉冲栅格（样本 #0 的频率编码）----
call console.WriteLine("  样本 #0 的频率编码脉冲栅格（行=输入神经元, 列=时间步, █=脉冲）:")
call console.WriteLine(result.inputRaster)
call console.WriteLine()

' ---- 训练过程 ----
call console.WriteLine("  epoch |   loss   | test-acc")

for i = 0 to result.lossHistory.length - 1
    if i = 0 orelse (i + 1) mod 5 = 0 then
        call console.WriteLine($"   {i + 1,4} | {result.lossHistory(i),8:F4} | {result.accuracyHistory(i),8:P1}")
    end if
next

call console.WriteLine()
call console.WriteLine($"  loss 曲线: {ml.sparkline(result.lossHistory)}")
call console.WriteLine($"  准确率曲线: {ml.sparkline(result.accuracyHistory)}")
call console.WriteLine()
call console.WriteLine($"  [结果] 测试集准确率: {result.accuracy:P1} ({CInt(result.accuracy * result.testSamples)}/{result.testSamples})")

' ---- 单样本推理轨迹 ----
call console.WriteLine()
call console.WriteLine("  —— 单样本推理轨迹（测试样本 #0）——")
call console.WriteLine("  输入脉冲栅格:")
call console.WriteLine(result.inputRaster)
call console.WriteLine()
call console.WriteLine(result.outputRaster)

' ---- 用标准报告 api 输出一份图文报告 ----
dim doc = report.document("Spiking neural network", "sciBASIC standard library")

call doc.add(report.heading("1. 训练配置"))
call doc.add(report.list(New String() {
    $"3 类 × 8 维高斯团簇（σ=0.09），训练 {result.trainSamples} / 测试 {result.testSamples} 样本",
    $"LIF×{result.hidden} → LIF×3，仿真步数 T={result.timeSteps}，β=0.85，θ=1.0",
    "Adam(lr=0.002) + 梯度裁剪 1.0 + FastSigmoid 替代梯度"
}))
call doc.add(report.heading("2. 测试集准确率"))
call doc.add(report.paragraph($"{result.accuracy:P1}，{result.epochs} 轮训练后的 loss = {result.lossHistory(result.lossHistory.length - 1):F4}"))
call doc.add(report.heading("3. 推理轨迹"))
call doc.add(report.code(result.inputRaster, "text"))
call doc.add(report.code(result.outputRaster, "text"))

call console.WriteLine()
call console.WriteLine("md  : " & doc.save(here("spiking-nn.md")))
call console.WriteLine("html: " & doc.save(here("spiking-nn.html")))

call console.WriteLine("done: spiking-nn")
