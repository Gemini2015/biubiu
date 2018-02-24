using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiuBiu
{
    interface INet
    {
        void SendProtoData(Type messageType, byte[] protoData);
    }
}
