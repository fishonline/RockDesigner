using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Dyn.Comm
{
    /// <summary>
    /// 虚拟IP地址协议
    /// </summary>
    public class VirtuaIP
    {
        //0x00FFFFFF-->得到node的掩码
        //0xFF000000-->得到router的掩码
        private uint _vIP;

        public VirtuaIP()
        {
            _vIP = 0;
        }

        public VirtuaIP(ushort router, uint node)
        {
            //int 左移8位，然后再取左边24位
            //node & 0x00FFFFFF 运算后得到 node 的二进制值
            //router << 24 运算后相当于 router*2的24次方
            _vIP = (uint)((uint)(router << 24) + (node & 0x00FFFFFF));
        }

        public VirtuaIP(uint vIP)
        {
            _vIP = vIP;
        }

        public VirtuaIP(byte[] vIP)
        {
            _vIP = (uint)BitConverter.ToInt32(vIP, 0);
        }

        public VirtuaIP(string address)
        {
            string[] addressPartials = address.Split('.');
            if (addressPartials.Length != 2)
                throw new ArgumentException("地址格式不正确  '路由号.节点号',eg:1.2");

            ushort router = ushort.Parse(addressPartials[0]);
            uint node = uint.Parse(addressPartials[1]);
            _vIP = (uint)((uint)(router << 24) + (node & 0x00FFFFFF));
        }

        public ushort Router
        {
            get { return (ushort)(_vIP >> 24); }
            set { _vIP = ((_vIP & 0x00FFFFFF) + (uint)(value << 24)); }
        }

        //value & 0x00FFFFFF 最大值是1677215
        public uint Node
        {
            get { return (uint)(_vIP & 0x00FFFFFF); }
            set { _vIP = (_vIP & 0xFF000000) + (value & 0x00FFFFFF); }
        }

        public override string ToString()
        {
            return Router.ToString() + "." + Node.ToString();
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(_vIP);
        }
    }
}
