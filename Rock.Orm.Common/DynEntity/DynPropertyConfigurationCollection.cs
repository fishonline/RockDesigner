using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Linq;

namespace Rock.Orm.Common
{
    public class DynPropertyConfigurationCollection : IList<DynPropertyConfiguration>
    {
        protected DynEntityType _dynEntityType;
        protected List<DynPropertyConfiguration> _dynPropertyConfigurationList = new List<DynPropertyConfiguration>();

        public DynPropertyConfigurationCollection(DynEntityType dynEntityType)
        {
            this._dynEntityType = dynEntityType;
        }

        public DynEntityType ObjType
        {
            get
            {
                return _dynEntityType;
            }
        }

        #region IList<EntityAttribute> Members

        public int IndexOf(DynPropertyConfiguration item)
        {
            return _dynPropertyConfigurationList.IndexOf(item);
        }

        public void Insert(int index, DynPropertyConfiguration item)
        {
            _dynPropertyConfigurationList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _dynPropertyConfigurationList.RemoveAt(index);
        }

        public DynPropertyConfiguration this[int index]
        {
            get
            {
                return _dynPropertyConfigurationList[index];
            }
            set
            {
                _dynPropertyConfigurationList[index] = value;
            }
        }

        public DynPropertyConfiguration this[string name]
        {
            get
            {
                foreach (DynPropertyConfiguration oEntityAttribute in this._dynPropertyConfigurationList)
                {
                    if (oEntityAttribute.Name == name)
                        return oEntityAttribute;
                }

                return null;
            }
            set
            {
                DynPropertyConfiguration temp = null;
                foreach (DynPropertyConfiguration oEntityAttribute in this._dynPropertyConfigurationList)
                {
                    if (oEntityAttribute.Name == name)
                        temp = oEntityAttribute;
                }

                if (temp != null)
                    temp = value;
            }
        }

        #endregion

        #region ICollection<EntityAttribute> Members

        public void Add(DynPropertyConfiguration item)
        {
            DynPropertyConfiguration oEntityAttribute = this[item.Name];
            if (oEntityAttribute != null)
                throw new ApplicationException("已存在同名属性，无法添加！");

            _dynPropertyConfigurationList.Add(item);
        }

        public void AddRange(IEnumerable<DynPropertyConfiguration> items)
        {
            foreach (DynPropertyConfiguration item in items)
            {
                DynPropertyConfiguration oEntityAttribute = this[item.Name];
                if (oEntityAttribute != null)
                    throw new ApplicationException("已存在同名属性，无法添加！");
            }

            _dynPropertyConfigurationList.AddRange(items);
        }

        public void Clear()
        {
            _dynPropertyConfigurationList.Clear();
        }

        public bool Contains(DynPropertyConfiguration item)
        {
            return _dynPropertyConfigurationList.Contains(item);
        }

        public bool Contains(string propertyName)
        {
            return _dynPropertyConfigurationList.Exists(item => item.Name == propertyName);
        }

        public void CopyTo(DynPropertyConfiguration[] array, int arrayIndex)
        {
            _dynPropertyConfigurationList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _dynPropertyConfigurationList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(DynPropertyConfiguration item)
        {
            return _dynPropertyConfigurationList.Remove(item);
        }

        public bool Remove(string name)
        {
            DynPropertyConfiguration oEntityAttribute = this[name];
            if (oEntityAttribute == null)
                return false;

            return this._dynPropertyConfigurationList.Remove(oEntityAttribute);
        }

        #endregion

        #region IEnumerable<EntityAttribute> Members

        public IEnumerator<DynPropertyConfiguration> GetEnumerator()
        {
            return _dynPropertyConfigurationList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._dynPropertyConfigurationList.GetEnumerator();
        }

        #endregion
    }
}
