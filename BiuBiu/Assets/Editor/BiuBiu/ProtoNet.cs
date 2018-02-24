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
    internal class ProtoNet: INet
    {
        public void SendProtoData(Type messageType, byte[] protoData)
        {
            if(messageType == null || protoData == null)
                return;

            var messageFullName = messageType.FullName;
            var messageName = messageType.Name;
            Debug.Log("ProtoNet Send: " + messageName + " Length " + protoData.Length);
            //if(EditorApplication.isPlaying)
            //{
            //    RawDatagram datagram = RawDatagram.Get();

            //    datagram.StartWrite();
            //    byte length = (byte)messageName.Length;
            //    datagram.WriteByte(length);
            //    datagram.WriteString(messageName, length);
            //    datagram.WriteBytes(protoData);
            //    datagram.EndWrite();

            //    NetManager.Instance.Send(datagram);
            //}
        }
    }
}
