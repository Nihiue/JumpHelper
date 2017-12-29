# JumperHelper

微信小程序 跳一跳 辅(zuo)助(bi)工具
> 本程序仅供娱乐，部分代码来自社区

## 原理
投屏游戏画面到PC，人工标注起点及终点，计算距离并给出触摸事件时长，通过自动ADB模拟触摸发送到手机，完成跳跃

## 环境
* Android 手机，打开 USB 调试
* Windows PC，已安装 ADB

## 步骤
1. 编译或者下载已编译版本 https://pan.baidu.com/s/1qXJIyuw
2. 打开手机的 Debug 调试功能，并使用 USB 连接电脑
3. 使用任何手段将手机投屏到电脑，推荐使用 Win10 的 【投影到此电脑】
4. 运行程序并点击 Start
5. 在手机端打开【跳一跳】，在PC的投屏上用鼠标指向起跳点，按下数字键1，再鼠标指向目标点，按下数字键2
6. 如果一切正常，程序会通过 ADB 发送模拟点击事件，完成一次跳跃

## 参数
因为屏幕分辨率及投屏大小不同，可能需要调整 Conversion Ratio。 1080p 屏幕下全屏的典型值是2.5

## 截图
![run](https://raw.githubusercontent.com/Nihiue/JumpHelper/master/pics/run.png)

![game](https://raw.githubusercontent.com/Nihiue/JumpHelper/master/pics/game.jpg)




> Have Fun And Happy New Year!
