/*********************************************
 * BFramework
 * 游戏入口
 * 创建时间：2023/06/16 16:54:23
 *********************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MainPackage
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        /// <summary>
        /// UI根节点
        /// </summary>
        [SerializeField]
        public GameObject UIRoot;

        /// <summary>
        /// UI根节点下的层级
        /// </summary>
        private static Dictionary<E_UILevel, RectTransform> _uiRootDic;

        [SerializeField]
        public Camera UICamera;

        [SerializeField]
        public Transform ObjPool;

        [SerializeField]
        public Transform GameStart;

        [Header("是否编辑器模式")]
        [SerializeField]
        public bool IsEditorMode;

        [Header("是否使用AB包运行，编辑器模式下才有意义")]
        [SerializeField]
        public bool IsRunABPackage;

        [Header("加载界面")]
        [SerializeField]
        public WinLoading WinLoading;

#if UNITY_EDITOR
        [Header("时间倍率")]
        [SerializeField]
        public float TimeScale;
#endif

        /// <summary>
        /// 下载管理器
        /// </summary>
        public DowloadManager DowloadManager;

        /// <summary>
        /// 热更的DLL名
        /// </summary>
        private string _hotfixDllName = "Assembly-CSharp.dll";

        /// <summary>
        /// Update回调
        /// </summary>
        public Action UpdateCallback;

        /// <summary>
        /// 退出回调回调
        /// </summary>
        public Action DisposeCallback;

        public static GameEntry Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
            _uiRootDic = new Dictionary<E_UILevel, RectTransform>();
            DontDestroyOnLoad(Instance);
            DontDestroyOnLoad(UIRoot);
            DontDestroyOnLoad(ObjPool);
        }

        private void Start()
        {
            //限定60fps
            Application.targetFrameRate = 60;

            DowloadManager = new DowloadManager();

            StartCoroutine(DownloadABPackage());
        }

        /// <summary>
        /// 开始下载AB包
        /// </summary>
        private IEnumerator DownloadABPackage()
        {
            DowloadManager.StartDowload();

            yield return new WaitUntil(() => DowloadManager.IsDowloadEnd);

            Log(E_Log.Framework, "热更代码", "启动中");
            Assembly ass = null;
            if (!IsEditorMode || IsRunABPackage)
            {
                //下载完资源后 直接本地加载
                byte[] assemblyData = File.ReadAllBytes(DowloadManager.SavePath + _hotfixDllName);
                ass = Assembly.Load(assemblyData);
                Log(E_Log.Framework, "热更代码", "DLL加载完毕");
            }

//#if UNITY_WEBGL && !UNITY_EDITOR
//            WebGL目前在2021版本不支持脚本挂载在资源上加载热更的方式 使用反射特殊处理（最新版本已修复，直接使用原生加载即可）
//            Type entryType = ass.GetType("GameData.HotUpdateMainByMethod");
//            MethodInfo method = entryType.GetMethod("Start");
//            method.Invoke(null, null);
//#else
            //原生加载热更
            var abPackage = AssetBundle.LoadFromFile(DowloadManager.SavePath + "hotfix");
            var hotfixObj = abPackage.LoadAsset<GameObject>("HotUpdatePrefab.prefab");
            GameObject hotfixPrefab = Instantiate(hotfixObj, transform);
            hotfixPrefab.name = "[Hotfix]";
            //abPackage.Unload(true);
//#endif
        }

        private void Update()
        {
            UpdateCallback?.Invoke();

#if UNITY_EDITOR
            Time.timeScale = TimeScale;
#endif
        }

        private void OnApplicationQuit()
        {
            //先执行
        }

        private void OnDestroy()
        {
            //再执行
            DisposeCallback?.Invoke();
        }

        /// <summary>
        /// 获得UI根节点下的层级节点
        /// </summary>
        /// <param name="uiLevel"></param>
        /// <returns></returns>
        public RectTransform GetUILevelTrans(E_UILevel uiLevel)
        {
            if (!_uiRootDic.TryGetValue(uiLevel, out var rect))
            {
                rect = Instance.UIRoot.transform.Find(uiLevel.ToString()) as RectTransform;
                _uiRootDic[uiLevel] = rect;
            }
            return rect;
        }

        public void Log(E_Log logType, string title = null, string content = null)
        {
            string tempStr = string.Empty;
            if (title == null || content == null)
            {
                tempStr = "<color={0}>{1}</color>";
            }
            else
            {
                tempStr = "<color={0}>{1}</color>===><color={0}>{2}</color>";
            }

            switch (logType)
            {
                case E_Log.Log:
                    Debug.Log(string.Format(tempStr, "white", title, content));
                    break;
                case E_Log.Framework:
                    Debug.Log(string.Format(tempStr, "magenta", title, content));
                    break;
                case E_Log.Proto:
                    Debug.Log(string.Format(tempStr, "yellow", title, content));
                    break;
                case E_Log.Error:
                    Debug.Log(string.Format(tempStr, "red", title, content));
                    break;
            }
        }
    }

    /// <summary>
    /// UI层级
    /// </summary>
    public enum E_UILevel
    {
        Background,
        Common,
        Pop,
        Loading,
        Tips,
    }

    /// <summary>
    /// Log类型
    /// </summary>
    public enum E_Log
    {
        Log,        //普通Log
        Framework,  //框架Log
        Proto,      //联网Log
        Error,      //错误Log
    }
}