# BFramework
使用<a href="https://github.com/focus-creative-games/hybridclr">HybridCLR</a>的Unity热更框架

# 打包流程
1.需要先安装Git和C++环境</br>
2.HybridCLR-Install-安装</br>
3.HybridCLR-Setting-Hot Update Assemblies-新增输入Assembly-CSharp-保存</br>
4.参考Editor目录下的热更说明</br>

# 常见错误
黑屏、报Null、元数据不匹配等，一般都是打包环节出了问题，如下操作</br>
1.检查HybridCLR-Setting-Hot Update Assemblies-参数是否为Assembly-CSharp</br>
2.检查DowloadManager的下载地址是否正确</br>
3.打包(因为ab包存放包体内部所以需要先打包，如果ab包存放云端可以忽略)</br>
4.参考Editor目录下的热更说明</br>
5.将打好的AB包全部放到StreamingAssets</br>
6.再打包</br>
