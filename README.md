# JumperHelper

微信小程序 跳一跳 辅(wai)助(gua)工具

> 本程序仅供娱乐，部分代码来自社区

## 原理

投屏游戏画面到PC，人工标注起点及终点，计算距离并给出触摸事件时长，通过自动 ADB 模拟触摸发送到手机，完成跳跃

## 环境

* Android 手机，打开 USB 调试
* Windows PC，已安装 ADB

## 步骤

1. 编译或者下载已编译版本 [下载地址](https://raw.githubusercontent.com/Nihiue/JumpHelper/master/static/JumpHelper.exe)（若报毒请参阅已知问题）
2. 打开手机的 Debug 调试功能，并使用 USB 连接电脑
3. 使用任何手段将手机投屏到电脑。推荐使用 Win10 的 【投影到此电脑】，AirDruid 在 Web 端的截图功能也可以实现投屏。
4. 运行程序，点击 【ADB】 【Test】，查看结果。 如果结果不是 Success，请参阅已知问题
5. 在手机端打开【跳一跳】，点击辅助工具 Start，在 PC 的投屏上用鼠标指向起跳点，按下数字键1，再鼠标指向目标点，按下数字键2
6. 如果一切正常，程序会通过 ADB 发送模拟点击事件，完成一次跳跃

## 调整参数

> duration = delta * ratio

* duration 模拟的触摸事件的持续时间
* delta 在 PC 屏幕上标注的两点间距离，单位是像素
* ratio 转换系数，可调整

因为屏幕分辨率及投屏大小不同，可能需要根据实际运行时的效果调整 Conversion Ratio.

1080p 屏幕下的典型值是2.5


## 已知问题

### MSE 报毒

因为快捷键 Hook 了全局键盘事件，下载的 exe 文件会被 MSE(Microsoft Security Essentials) 误报为病毒

如果仍觉得有问题请自行审阅代码并编译

附上其他杀毒引擎的扫描结果 http://r.virscan.org/report/9e2b82910304501137842fbeef6bfbd1

### ADB问题

#### ADB not found

1. [下载](https://raw.githubusercontent.com/Nihiue/JumpHelper/master/static/platform-tools-latest-windows.zip) ADB，并解压
2. 在程序中 【ADB】 【Select Path】 中选择解压出的adb.exe

#### Please click accept on your phone
重新插拔手机，点击 Test，并在手机上接受调试


#### Fail
可能是手机未连接到电脑，或者未打开 USB 调试

## 截图

![run](https://raw.githubusercontent.com/Nihiue/JumpHelper/master/static/run.png)

![game](https://raw.githubusercontent.com/Nihiue/JumpHelper/master/static/game.jpg)




> Have Fun And Happy New Year!
