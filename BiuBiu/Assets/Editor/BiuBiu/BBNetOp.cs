using CC.Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BiuBiu
{
    internal class BBNetOp: TSingleton<BBNetOp>
    {
        private INet net;

        public void SetNet(INet net)
        {
            this.net = net;
        }

        private Type currentType = null;
        private MessageData currentMessageData = null;
        private int times = 1;
        private float timeSpan = 0.0f;

        private int counter = 0;

        public void Send(Type type, MessageData messageData, int times = 1, float timeSpan = 0.0f)
        {
            currentType = type;
            currentMessageData = messageData;
            this.times = times;
            this.timeSpan = timeSpan;

            StopUpdate();

            if(times <= 1)
            {
                SendInternal();
            }
            else
            {
                if(timeSpan <= 0.0f)
                {
                    for(int i = 0; i < times; ++i)
                    {
                        SendInternal();
                    }
                }
                else
                {
                    StartUpdate();
                }
            }
        }

        public void SendInternal()
        {
            if(net == null)
                return;

            var bytes = CreateProtoData();
            net.SendProtoData(currentType, bytes);
        }

        private byte[] CreateProtoData()
        {
            var obj = currentType.Assembly.CreateInstance(currentType.FullName);
            currentMessageData.Transfer(obj);

            MemoryStream ms = new MemoryStream();
            var m = typeof(Serializer).GetMethod("Serialize");
            m.MakeGenericMethod(currentType)
            .Invoke(null, new object[] { ms, obj });
            return ms.ToArray();
        }

        #region Update

        private float lastTimeStamp = 0;

        private void StartUpdate()
        {
            lastTimeStamp = Time.realtimeSinceStartup;
            counter = 0;
            EditorApplication.update = EditorUpdate;
        }

        private void StopUpdate()
        {
            EditorApplication.update = null;
        }

        private void EditorUpdate()
        {
            var nowTime = Time.realtimeSinceStartup;
            if((nowTime - lastTimeStamp) < timeSpan)
                return;
            lastTimeStamp = nowTime;

            if(counter >= times)
            {
                StopUpdate();
                return;
            }

            SendInternal();
            counter++;
        }

        #endregion Update
    }
}
