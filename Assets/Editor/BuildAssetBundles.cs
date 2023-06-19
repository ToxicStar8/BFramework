///*********************************************
// * BFramework
// * 打AB包工具
// * 创建时间：2022/12/29 15:23:46
// *********************************************/
//using LitJson;
//using MainPackage;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//namespace Framework
//{
//    /// <summary>
//    /// AB包生成
//    /// </summary>
//    public class BuildAssetBundles : Editor
//    {
//        /// <summary>
//        /// AB包输出路径
//        /// </summary>
//        public static string ABOutPath = Application.dataPath.Substring(0,Application.dataPath .Length - "Assets".Length) + "AssetBundle";
//        /// <summary>
//        /// 脚本DLL输出路径
//        /// </summary>
//        public static string ScriptOutPath = Application.dataPath.Substring(0,Application.dataPath .Length - "Assets".Length) + "HybridCLRData/HotUpdateDlls/" + EditorUserBuildSettings.activeBuildTarget .ToString();
//        //依赖信息Json的路径
//        private static string _jsoninformationPath => ConstDefine.JsoninformationPath;
//        private static ABConfig _abConfig => AssetDatabase.LoadAssetAtPath<ABConfig>(ConstDefine.ABConfigPath);

//        [MenuItem("BFramework/Build AssetBundles")]
//        public static void BuildAsset()
//        {
//            //1.设置AB包路径和AB包名
//            _abConfig.SetABNameAndPath();
//            //2.清空文件夹
//            if (Directory.Exists(ABOutPath))
//            {
//                Directory.Delete(ABOutPath, true);
//            }
//            Directory.CreateDirectory(ABOutPath);
//            //3.设置打包名通过路径
//            SetABNameByPath();
//            //4.存储依赖关系表（如果用Unity自带的依赖管理可以忽略）
//            SaveABPackageToJson();
//            //5.真正打ab包的地方，打包实际上就是一句话
//            BuildPipeline.BuildAssetBundles(ABOutPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
//            //6.把不需要的依赖文件删除
//            DeleteRelyOnFile();
//            //7.生成MD5校验文件
//            SaveABMD5ToXML();
//            //8.清除AB包标签
//            ClearABName();
//            //打开文件夹
//            OpenFileTools.OpenFile(ABOutPath);
//            //回收资源
//            System.GC.Collect();
//            //刷新编辑器
//            AssetDatabase.Refresh();
//        }

//        /// <summary>
//        /// 设置AB包路径的AB包名
//        /// </summary>
//        /// <param name="abConfig"></param>
//        private static void SetABNameByPath()
//        {
//            var list = _abConfig.TrueABList;
//            foreach (var item in list)
//            {
//                AssetImporter.GetAtPath(item.path).assetBundleName = item.abName;
//            }
//        }

//        /// <summary>
//        /// 存储依赖关系为Json格式
//        /// </summary>
//        private static void SaveABPackageToJson()
//        {
//            Directory.CreateDirectory(ConstDefine.JsoninformationDirPath);
//            File.Delete(_jsoninformationPath);

//            var list = _abConfig.TrueABList;
//            var abInfo = new ABInfo();
//            foreach (var ab in list)
//            {
//                //当前ab包依赖的列表
//                var relyOnList = AssetDatabase.GetAssetBundleDependencies(ab.abName, true).ToList();
//                abInfo.ABRelyInfoList.Add(new ABInfo.ABRelyInfo()
//                {
//                    ABName = ab.abName,
//                    //ABPath = ab.path,
//                    ABRelyOnNameList = relyOnList,
//                });
//                //当前ab包包括的资源路径
//                foreach (var item in AssetDatabase.GetAssetPathsFromAssetBundle(ab.abName))
//                {
//                    abInfo.ABFileDic.Add(item.Substring(item.LastIndexOf("/") + 1), ab.abName);
//                }
//            }
//            //存储完毕后把这个文件也打个包 读取时首先读取这个AB包
//            using (var assetInformation = File.CreateText(_jsoninformationPath))
//            {
//                assetInformation.Write(JsonMapper.ToJson(abInfo));
//            }
//        }

//        /// <summary>
//        /// 删除不需要的依赖文件
//        /// </summary>
//        private static void DeleteRelyOnFile()
//        {
//            var dir = new DirectoryInfo(ABOutPath);
//            foreach (var item in dir.GetFileSystemInfos("*", SearchOption.AllDirectories))
//            {
//                if(item.Extension == ".manifest")
//                {
//                    File.Delete(item.FullName);
//                }
//            }

//            File.Delete(ABOutPath + "/AssetBundle");
//        }

//        /// <summary>
//        /// 生成AB包的MD5
//        /// </summary>
//        private static void SaveABMD5ToXML()
//        {
//            var list = new List<ABMd5Info>();
//            var dir = new DirectoryInfo(ABOutPath);
//            foreach (var item in dir.GetFileSystemInfos("*", SearchOption.AllDirectories))
//            {
//                var path = ABOutPath + "/" + item.Name;
//                list.Add(new ABMd5Info()
//                {
//                    ABName = item.Name,
//                    ABSize = File.ReadAllBytes(path).Length,
//                    ABMd5 = Md5Util.GetMd5ByPath(path),
//                });
//            }
//            Debug.Log("ScriptOutPath="+ ScriptOutPath);
//            //添加脚本热更的信息
//            var dllPath = ScriptOutPath + "/" + ConstDefine.HotfixDllName;
//            if (File.Exists(dllPath))
//            {
//                list.Add(new ABMd5Info()
//                {
//                    ABName = ConstDefine.HotfixDllName,
//                    ABSize = File.ReadAllBytes(dllPath).Length,
//                    ABMd5 = Md5Util.GetMd5ByPath(dllPath),
//                });
//                File.Move(dllPath, ABOutPath + "/" + ConstDefine.HotfixDllName);
//            }
//            else
//            {
//                Debug.LogError("DLL文件为空");
//            }
//            //输出资源索引信息
//            using (var fileUpdateInfo = File.CreateText(ABOutPath + "/" + ConstDefine.ABMd5InfoName))
//            {
//                var json = JsonMapper.ToJson(list);
//                fileUpdateInfo.Write(json);
//            }
//        }

//        /// <summary>
//        /// 清除AB包标签
//        /// </summary>
//        private static void ClearABName()
//        {
//            foreach (var item in _abConfig.TrueABList)
//            {
//                AssetImporter.GetAtPath(item.path).assetBundleName = null;
//            }
//            AssetDatabase.RemoveUnusedAssetBundleNames();
//            foreach (var item in AssetDatabase.GetAllAssetBundleNames())
//            {
//                Debug.LogError("AB 标签存在未移除情况：" + item);
//            }
//        }
//    }
//}
