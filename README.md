WindowsDOC 第一个C# WPF程序

之前用易语言写了一版，后来了解到`C#是微软亲儿子`，开始尝试写一个C#程序，最开始也是跟着网上教程写了一个`hello world`，然后就开始写用这个程序了，写到中途界面自定义总有些美中不足，才发现写的是Winform，接着又了解到WPF，又开始重写之旅...发现自定义界面相当高，但是坑也是相当多，本着`吃一堑，长一智`的理念，断断续续过了几个月才有了点成就。写到最后打包又发现还分.net、.net framework...，为了绿色最终还是把框架集成在软件目录了。话不多说看成品吧。

![WindowsDOC演示.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/736089716.gif)

# 一、界面功能

一款软件启动器、批处理命令执行、查看电脑配置、查看编辑IP地址、查看编辑系统文件夹与已共享路径、内置浏览器、软件主题设置等多功能集聚一身的工具。

## 启动器

* 支持程序、文件、文件夹的打开操作，支持相对路径、可区分x86/x64程序；![编辑项目.png](https://www4.iceyer.cn:444/usr/uploads/2024/05/3214826753.png)
* 支持CMD命令的运行，支持CMD窗口隐藏；![编辑CMD命令.png](https://www4.iceyer.cn:444/usr/uploads/2024/05/1409708045.png)
* 支持项目的移动，鼠标长按左键等待弹出缩略图即可进入移动模式；![移动项目.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/991789507.gif)
* 支持项目的分组添加、删除、移动等操作；![分组操作.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/2067767878.gif)
* 支持拖拽外部程序可自动解析项目名称、图标并添加项目的操作。![外部推拽.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/1976711655.gif)

## 系统配置

* 支持读取当前CPU、内存、磁盘占用率；![状态监控.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/1254085978.gif)
* 支持读取电脑名称、型号、系统、主板、CPU、内存、磁盘、显卡、显示器、声卡、网卡信息。![系统配置.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/808425872.gif)

## 网络配置

* 支持读取网络适配器的信息；![读取IP地址.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/985310902.gif)
* 支持修改IP、MAC、DNS地址信息。![修改IP地址.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/2095720625.gif)

## 链接配置

* 支持读取系统桌面、下载、文档、图片、音乐、视频文件夹路径，快速打开其文件夹属性；![系统文件夹属性.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/1193291832.gif)
* 支持读取当前已共享的文件夹，快速打开其文件夹属性。![共享文件夹属性.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/3835458082.gif)

## 浏览器

* 支持内置CEF浏览器简易操作，如后退、刷新、主页、跳转；![浏览器按钮讲解.png](https://www4.iceyer.cn:444/usr/uploads/2024/05/2354744986.png)
* 支持多标签页；![新标签页.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/2976858691.gif)
* 支持下载管理器，可查看进度，允许暂停、恢复、删除、打开文件位置操作；![下载文件.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/3464230833.gif)
* 支持设置主页、UA的操作，UA可管理多条。![浏览器设置.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/3188047845.gif)

## 软件设置

* 支持全局颜色的修改；![全局颜色.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/2552987885.gif)
* 支持预设颜色，可在JSON配置修改；![预设颜色.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/2131610371.gif)
* 支持查看软件各功能存储位置及配置位置，可快速打开。![软件目录配置.gif](https://www4.iceyer.cn:444/usr/uploads/2024/05/3502697570.gif)

## 其他功能

* 支持软件设置保存、刷新配置等操作。
