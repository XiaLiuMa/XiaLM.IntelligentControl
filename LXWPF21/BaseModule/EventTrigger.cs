using System;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace BaseModule
{
    /// <summary>
    /// 时间触发器
    /// </summary>
    static class EventTrigger
    {
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="sender">触发的对象</param>
        /// <param name="eventDelegate">事件</param>
        /// <param name="eventName">事件名</param>
        /// <param name="paramValues">事件参数</param>
        public static void FireEvent(object sender, Delegate eventDelegate, string eventName, params object[] paramValues)
        {
            if (sender == null || eventDelegate == null) return;
            EventInfo eventInfo = sender.GetType().GetEvent(eventName);
            foreach (Delegate del in eventDelegate.GetInvocationList())
            {
                object target = del.Target;
                try
                {
                    if (target is Control)
                    {
                        Control ctrl = target as Control;
                        if (ctrl.InvokeRequired)
                        {
                            ctrl.Invoke(del, paramValues);
                        }
                        else
                        {
                            del.DynamicInvoke(paramValues);
                        }
                    }
                    else if (target is UIElement)
                    {
                        UIElement element = target as UIElement;
                        element.Dispatcher.Invoke(del, paramValues);
                    }
                    else
                    {
                        del.DynamicInvoke(paramValues);
                    }
                }
                catch
                {
                    eventInfo.RemoveEventHandler(sender, del);
                }
            }
        }
    }

}