using System;
using System.Collections.Generic;
using System.Reflection;
namespace ZGameFrameWork.Core
{

    using UnityEngine;

    /// <summary>
    /// 事件中心，通过事件ID（int类型，建议用枚举转换）来注册、触发和移除事件，支持0-7个参数的事件，参数类型不限
    /// 调用时需要注意事件ID和参数类型要匹配，否则会有错误提示
    /// 通过AddEventListener,TriggerEvent, RemoveEventListener等方法来操作事件
    /// RemoveAllListener可以移除某个事件的所有监听，ClearAllListener可以移除所有事件的所有监听
    /// 需要手动取消订阅
    /// </summary>
    public static class EventCenter
    {
        /// <summary>
        /// 调试模式开关，开启后触发没有监听的事件会有警告提示
        /// </summary>
        public static bool EnableDebugMode = true;

        #region 内部类
        private interface IEventInfo
        {
            void RemoveAllAction();
        }

        private class EventInfo : IEventInfo
        {
            private Action action;

            public void AddAction(Action action)
            {
                this.action += action;
            }

            public void RemoveAction(Action action)
            {
                this.action -= action;
            }

            public void RemoveAllAction()
            {
                action = null;
                ClassPool<EventInfo>.Recycle(this);
            }

            public void Trigger()
            {
                action?.Invoke();
            }
        }

        private class EventInfo<T> : IEventInfo
        {
            private Action<T> action;

            public void AddAction(Action<T> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T> action)
            {
                this.action -= action;
            }
            public void Trigger(T arg)
            {
                action?.Invoke(arg);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T>>.Recycle(this);
            }

        }
        private class EventInfo<T0, T1> : IEventInfo
        {
            private Action<T0, T1> action;

            public void AddAction(Action<T0, T1> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T0, T1> action)
            {
                this.action -= action;
            }
            public void Trigger(T0 arg, T1 arg2)
            {
                action?.Invoke(arg, arg2);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T0, T1>>.Recycle(this);
            }
        }

        private class EventInfo<T0, T1, T2> : IEventInfo
        {
            private Action<T0, T1, T2> action;

            public void AddAction(Action<T0, T1, T2> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T0, T1, T2> action)
            {
                this.action -= action;
            }
            public void Trigger(T0 arg, T1 arg1, T2 arg2)
            {
                action?.Invoke(arg, arg1, arg2);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T0, T1, T2>>.Recycle(this);
            }
        }

        private class EventInfo<T0, T1, T2, T3> : IEventInfo
        {
            private Action<T0, T1, T2, T3> action;

            public void AddAction(Action<T0, T1, T2, T3> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T0, T1, T2, T3> action)
            {
                this.action -= action;
            }
            public void Trigger(T0 arg, T1 arg1, T2 arg2, T3 arg3)
            {
                action?.Invoke(arg, arg1, arg2, arg3);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T0, T1, T2, T3>>.Recycle(this);
            }
        }

        private class EventInfo<T0, T1, T2, T3, T4> : IEventInfo
        {
            private Action<T0, T1, T2, T3, T4> action;

            public void AddAction(Action<T0, T1, T2, T3, T4> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T0, T1, T2, T3, T4> action)
            {
                this.action -= action;
            }
            public void Trigger(T0 arg, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                action?.Invoke(arg, arg1, arg2, arg3, arg4);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T0, T1, T2, T3, T4>>.Recycle(this);
            }
        }

        private class EventInfo<T0, T1, T2, T3, T4, T5> : IEventInfo
        {
            private Action<T0, T1, T2, T3, T4, T5> action;

            public void AddAction(Action<T0, T1, T2, T3, T4, T5> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T0, T1, T2, T3, T4, T5> action)
            {
                this.action -= action;
            }
            public void Trigger(T0 arg, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                action?.Invoke(arg, arg1, arg2, arg3, arg4, arg5);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T0, T1, T2, T3, T4, T5>>.Recycle(this);
            }
        }

        private class EventInfo<T0, T1, T2, T3, T4, T5, T6> : IEventInfo
        {
            private Action<T0, T1, T2, T3, T4, T5, T6> action;

            public void AddAction(Action<T0, T1, T2, T3, T4, T5, T6> action)
            {
                this.action += action;
            }
            public void RemoveAction(Action<T0, T1, T2, T3, T4, T5, T6> action)
            {
                this.action -= action;
            }
            public void Trigger(T0 arg, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                action?.Invoke(arg, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            public void RemoveAllAction()
            {
                this.action = null;
                ClassPool<EventInfo<T0, T1, T2, T3, T4, T5, T6>>.Recycle(this);
            }
        }
        #endregion

        private static Dictionary<int, IEventInfo> eventDic = new Dictionary<int, IEventInfo>();

        #region 注册事件
        /// <summary>
        /// 注册事件，事件名需枚举转换为int类型
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public static void AddEventListener(int eventId, Action action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo).AddAction(action);
            }
            else
            {
                EventInfo newEventInfo = ClassPool<EventInfo>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T>(int eventId, Action<T> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T>).AddAction(action);
            }
            else
            {
                EventInfo<T> newEventInfo = ClassPool<EventInfo<T>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T0, T1>(int eventId, Action<T0, T1> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T0, T1>).AddAction(action);
            }
            else
            {
                EventInfo<T0, T1> newEventInfo = ClassPool<EventInfo<T0, T1>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T0, T1, T2>(int eventId, Action<T0, T1, T2> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T0, T1, T2>).AddAction(action);
            }
            else
            {
                EventInfo<T0, T1, T2> newEventInfo = ClassPool<EventInfo<T0, T1, T2>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T0, T1, T2, T3>(int eventId, Action<T0, T1, T2, T3> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T0, T1, T2, T3>).AddAction(action);
            }
            else
            {
                EventInfo<T0, T1, T2, T3> newEventInfo = ClassPool<EventInfo<T0, T1, T2, T3>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T0, T1, T2, T3, T4>(int eventId, Action<T0, T1, T2, T3, T4> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T0, T1, T2, T3, T4>).AddAction(action);
            }
            else
            {
                EventInfo<T0, T1, T2, T3, T4> newEventInfo = ClassPool<EventInfo<T0, T1, T2, T3, T4>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T0, T1, T2, T3, T4, T5>(int eventId, Action<T0, T1, T2, T3, T4, T5> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T0, T1, T2, T3, T4, T5>).AddAction(action);
            }
            else
            {
                EventInfo<T0, T1, T2, T3, T4, T5> newEventInfo = ClassPool<EventInfo<T0, T1, T2, T3, T4, T5>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        public static void AddEventListener<T0, T1, T2, T3, T4, T5, T6>(int eventId, Action<T0, T1, T2, T3, T4, T5, T6> action)
        {
            if (eventDic.TryGetValue(eventId, out IEventInfo eventInfo))
            {
                (eventInfo as EventInfo<T0, T1, T2, T3, T4, T5, T6>).AddAction(action);
            }
            else
            {
                EventInfo<T0, T1, T2, T3, T4, T5, T6> newEventInfo = ClassPool<EventInfo<T0, T1, T2, T3, T4, T5, T6>>.Get();
                newEventInfo.AddAction(action);
                eventDic.Add(eventId, newEventInfo);
            }
        }

        #endregion

        #region 触发事件
        public static void TriggerEvent(int eventID)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo eventInfo)
                {
                    eventInfo.Trigger();
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T>(int eventID, T arg)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T> eventInfo)
                {
                    eventInfo.Trigger(arg);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T0, T1>(int eventID, T0 arg0, T1 arg1)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1> eventInfo)
                {
                    eventInfo.Trigger(arg0, arg1);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T0, T1, T2>(int eventID, T0 arg0, T1 arg1, T2 arg2)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2> eventInfo)
                {
                    eventInfo.Trigger(arg0, arg1, arg2);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T0, T1, T2, T3>(int eventID, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3> eventInfo)
                {
                    eventInfo.Trigger(arg0, arg1, arg2, arg3);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T0, T1, T2, T3, T4>(int eventID, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3, T4> eventInfo)
                {
                    eventInfo.Trigger(arg0, arg1, arg2, arg3, arg4);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T0, T1, T2, T3, T4, T5>(int eventID, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3, T4, T5> eventInfo)
                {
                    eventInfo.Trigger(arg0, arg1, arg2, arg3, arg4, arg5);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        public static void TriggerEvent<T0, T1, T2, T3, T4, T5, T6>(int eventID, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3, T4, T5, T6> eventInfo)
                {
                    eventInfo.Trigger(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                }
                else
                {
                    Debug.LogError("事件ID:" + eventID + "对应的事件参数不匹配");
                }
            }
            else
            {
                if (EnableDebugMode)
                {
                    Debug.LogWarning($"[EventCenter] 触发了事件 {(EventID)eventID}，但没有任何监听");
                }
            }
        }

        #endregion

        #region 移除事件
        public static void RemoveEventListener(int eventID, Action action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T>(int eventID, Action<T> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T0, T1>(int eventID, Action<T0, T1> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T0, T1, T2>(int eventID, Action<T0, T1, T2> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T0, T1, T2, T3>(int eventID, Action<T0, T1, T2, T3> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T0, T1, T2, T3, T4>(int eventID, Action<T0, T1, T2, T3, T4> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3, T4> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T0, T1, T2, T3, T4, T5>(int eventID, Action<T0, T1, T2, T3, T4, T5> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3, T4, T5> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        public static void RemoveEventListener<T0, T1, T2, T3, T4, T5, T6>(int eventID, Action<T0, T1, T2, T3, T4, T5, T6> action)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                if (baseInfo is EventInfo<T0, T1, T2, T3, T4, T5, T6> info)
                {
                    info.RemoveAction(action);
                }
            }
        }

        #endregion

        public static void RemoveAllListener(int eventID)
        {
            if (eventDic.TryGetValue(eventID, out IEventInfo baseInfo))
            {
                baseInfo.RemoveAllAction();
                eventDic.Remove(eventID);
            }
        }

        public static void ClearAllListener()
        {
            foreach (var eventInfo in eventDic.Values)
            {
                eventInfo.RemoveAllAction();
            }
            eventDic.Clear();
        }


        /// <summary>
        /// 打印出当前所有正在运行的事件，以及它们被监听的情况
        /// </summary>
        public static void PrintAllEventsStatus()
        {
            Debug.Log("========== 当前 EventCenter 状态 ==========");
            foreach (var kvp in eventDic)
            {
                // 把 int 强转回枚举名字，方便阅读
                string eventName = ((EventID)kvp.Key).ToString();
                Debug.Log($"频道 [{eventName}] : 正在被监听中...");
            }
            Debug.Log("=========================================");
        }
    }
}