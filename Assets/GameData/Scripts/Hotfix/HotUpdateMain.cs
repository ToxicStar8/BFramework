/*********************************************
 * BFramework
 * 热更入口
 * 创建时间：2023/01/08 20:40:23
 *********************************************/
using HybridCLR;
using MainPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// 挂载脚本加载热更
    /// </summary>
    public class HotUpdateMain : MonoBehaviour
    {
        void Start()
        {
            GameEntry.Instance.Log(E_Log.Framework, "热更代码", "进入成功");
            //初始化表格
            //GameEntry.Instance.TableManager.Init(TableTypes.TableCtrlTypeArr);
            //初始化存档
            //new GameObject("[Data]").AddComponent<DataManager>();
            //背景音乐
            //GameEntry.Instance.AudioManager.PlayBackground("RetroComedy.ogg");
            //游戏管理器
            //GameManager.CreateGameManager();
            //正式启动
            //GameEntry.Instance.UIManager.OpenUI<UIMainMenu>(E_UILevel.Common);
        }

        private void Update()
        {
            GameEntry.Instance.Log(E_Log.Framework, "热更");
        }
    }
}
