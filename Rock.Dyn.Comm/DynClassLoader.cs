using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rock.Dyn.Core;

namespace Rock.Dyn.Comm
{
    class DynClassLoader
    {
        public static void LoadDynClass()
        {
            //已经注册过，直接返回
            if (DynTypeManager.GetClass("FileFragment") != null) { return; }

            //FileFragment对象
            DynClass fileFragment = new DynClass("FileFragment");

            DynProperty state = new DynProperty(0, "State", DynType.Byte);
            DynProperty msgID = new DynProperty(1, "MsgID", DynType.I32);
            DynProperty dataLength = new DynProperty(2, "DataLength", DynType.I32);
            DynProperty fileLength = new DynProperty(3, "FileLength", DynType.I64);
            DynProperty path = new DynProperty(4, "Path", DynType.String);
            DynProperty fileName = new DynProperty(5, "FileName", DynType.String);
            DynProperty extension = new DynProperty(6, "Extension", DynType.String);
            DynProperty mD5 = new DynProperty(7, "MD5", DynType.String);
            DynProperty excepMsg = new DynProperty(8, "ExcepMsg", DynType.String);
            DynProperty data = new DynProperty(9, "Data", DynType.Binary);

            fileFragment.AddProperty(state);
            fileFragment.AddProperty(msgID);
            fileFragment.AddProperty(dataLength);
            fileFragment.AddProperty(fileLength);
            fileFragment.AddProperty(path);
            fileFragment.AddProperty(fileName);
            fileFragment.AddProperty(extension);
            fileFragment.AddProperty(mD5);
            fileFragment.AddProperty(excepMsg);
            fileFragment.AddProperty(data);

            DynTypeManager.RegistClass(fileFragment);
        }
    }
}
