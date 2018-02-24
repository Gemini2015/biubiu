using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace BiuBiu
{
    public enum PropertyType
    {
        None,
        Int,
        Float,
        String,
        Object,
        RepeatedInt,
        RepeatedFloat,
        RepeatedString,
        RepeatedObject,
    }


    abstract class BaseProperty
    {

        /// <summary>
        /// 字段名
        /// </summary>
        public string name;

        /// <summary>
        /// 字段类型名
        /// </summary>
        public string typeName;

        public virtual void Build(Type t)
        {
        }

        public virtual void Transfer(object targetValue)
        {
        }

        public string GetLabel()
        {
            return string.Format("[{0}] {1}", typeName, name);
        }

        public abstract void Draw();

        public virtual object GetValue()
        {
            return null;
        }
    }

    class IntProperty: BaseProperty
    {
        public int value;
        public int min;
        public int max;
        public bool random = false;
        public PropertyType propertyType = PropertyType.Int;

        public override void Draw()
        {
            using (var h = new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.IntField(GetLabel(), value);

                EditorGUILayout.BeginVertical();
                using (var t = new EditorGUILayout.ToggleGroupScope("Random", random))
                {
                    random = t.enabled;
                    min = EditorGUILayout.IntField("Min", min);
                    max = EditorGUILayout.IntField("Max", max);
                }
                EditorGUILayout.EndVertical();
            }
        }

        public override object GetValue()
        {
            if(random)
            {
                if(min >= max)
                {
                    Debug.LogFormat("Field {0} Value Invalid <{1}, {2}>", name, min, max);
                    return min;
                }
                return UnityEngine.Random.Range(min, max);
            }
            else
            {
                return value;
            }
        }
    }

    class FloatProperty: BaseProperty
    {
        public float value;
        public float min;
        public float max;
        public bool random = false;
        public PropertyType propertyType = PropertyType.Float;

        public override void Draw()
        {
            using (var h = new EditorGUILayout.HorizontalScope())
            {
                value = EditorGUILayout.FloatField(GetLabel(), value);

                EditorGUILayout.BeginVertical();
                using (var t = new EditorGUILayout.ToggleGroupScope("Random", random))
                {
                    random = t.enabled;
                    min = EditorGUILayout.FloatField("Min", min);
                    max = EditorGUILayout.FloatField("Max", max);
                }
                EditorGUILayout.EndVertical();
            }
        }

        public override object GetValue()
        {
            if (random)
            {
                if (min >= max)
                {
                    Debug.LogFormat("Field {0} Value Invalid <{1}, {2}>", name, min, max);
                    return min;
                }
                return UnityEngine.Random.Range(min, max);
            }
            else
            {
                return value;
            }
        }
    }

    class StringProperty : BaseProperty
    {
        public string value;
        public int min;
        public int max;
        public bool random = false;
        public PropertyType propertyType = PropertyType.String;

        public override void Draw()
        {
            using (var h = new EditorGUILayout.HorizontalScope())
            {
                //EditorGUILayout.BeginVertical();
                //EditorGUILayout.LabelField(GetLabel());
                EditorGUILayout.PrefixLabel(GetLabel());
                value = EditorGUILayout.TextField(value);
                //EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                using (var t = new EditorGUILayout.ToggleGroupScope("Random", random))
                {
                    random = t.enabled;
                    min = EditorGUILayout.IntField("Min", min);
                    max = EditorGUILayout.IntField("Max", max);
                }
                EditorGUILayout.EndVertical();
            }                
        }

        public override object GetValue()
        {
            if (random)
            {
                if (min >= max)
                {
                    Debug.LogFormat("Field {0} Value Invalid <{1}, {2}>", name, min, max);
                    return value;
                }
                return string.Format("{0}_{1}", value, UnityEngine.Random.Range(min, max));
            }
            else
            {
                return value;
            }
        }
    }

    class NestedProperty : BaseProperty
    {
        public List<BaseProperty> value;
        public PropertyType propertyType = PropertyType.Object;

        public override void Build(Type t)
        {
            typeName = t.FullName;
            value = new List<BaseProperty>();

            var props = t.GetProperties();
            foreach (var prop in props)
            {
                ProcessProperty(prop);
            }
        }

        public override void Transfer(object targetValue)
        {
            if (targetValue == null || value == null)
                return;

            var targetType = targetValue.GetType();
            var props = targetType.GetProperties();
            foreach(var prop in props)
            {
                var propName = prop.Name;
                foreach(var p in value)
                {
                    if(p.name == propName)
                    {
                        if((p is IntProperty)
                            || (p is FloatProperty)
                            || (p is StringProperty))
                        {
                            var convValue = Convert.ChangeType(p.GetValue(), prop.PropertyType);
                            prop.SetValue(targetValue, convValue, null);
                        }
                        else if((p is RepeatedIntProperty)
                            || (p is RepeatedFloatProperty)
                            || (p is RepeatedStringProperty))
                        {
                            var listValue = prop.GetValue(targetValue, null);
                            p.Transfer(listValue);
                        }
                        else if(p is NestedProperty)
                        {
                            Debug.LogFormat("Current Not Support {0}", p.GetType().Name);
                            //var subValue = prop.PropertyType.Assembly.CreateInstance(prop.PropertyType.FullName);
                            //prop.SetValue(targetValue, subValue, null);
                            //p.Transfer(subValue);
                        }
                        else
                        {
                            Debug.LogFormat("Current Not Support {0}", p.GetType().Name);
                        }                        
                    }
                }
            }
        }

        void ProcessProperty(PropertyInfo prop)
        {
            var pt = prop.PropertyType;
            if (pt == typeof(UInt32)
                    || pt == typeof(UInt64)
                    || pt == typeof(Int32)
                    || pt == typeof(Int64))
            {
                IntProperty ip = new IntProperty();
                ip.name = prop.Name;
                ip.typeName = MessageData.GetTypeName(pt);

                value.Add(ip);
            }
            else if (pt == typeof(Single))
            {
                FloatProperty fp = new FloatProperty();
                fp.name = prop.Name;
                fp.typeName = MessageData.GetTypeName(pt);

                value.Add(fp);
            }
            else if (pt == typeof(String))
            {
                StringProperty sp = new StringProperty();
                sp.name = prop.Name;
                sp.typeName = MessageData.GetTypeName(pt);

                value.Add(sp);
            }
            else if (pt.IsGenericType)
            {
                var gas = pt.GetGenericArguments();
                var ft = gas[0];

                if (ft == typeof(UInt32)
                    || ft == typeof(UInt64)
                    || ft == typeof(Int32)
                    || ft == typeof(Int64))
                {
                    RepeatedIntProperty ip = new RepeatedIntProperty();
                    ip.name = prop.Name;
                    ip.typeName = MessageData.GetTypeName(ft);
                    ip.Build(ft);

                    value.Add(ip);
                }
                else if (ft == typeof(Single))
                {
                    RepeatedFloatProperty fp = new RepeatedFloatProperty();
                    fp.name = prop.Name;
                    fp.typeName = MessageData.GetTypeName(ft);
                    fp.Build(ft);

                    value.Add(fp);
                }
                else if (ft == typeof(String))
                {
                    RepeatedStringProperty sp = new RepeatedStringProperty();
                    sp.name = prop.Name;
                    sp.typeName = MessageData.GetTypeName(ft);
                    sp.Build(ft);

                    value.Add(sp);
                }
                else if (ft.IsClass)
                {
                    RepeatedNestedProperty np = new RepeatedNestedProperty();
                    np.name = prop.Name;
                    np.Build(ft);

                    value.Add(np);
                }
            }
            else if (pt.IsClass)
            {
                NestedProperty np = new NestedProperty();
                np.name = prop.Name;
                np.Build(pt);

                value.Add(np);
            }
            
        }

        Vector2 scrollPos;

        public override void Draw()
        {
            if (value == null)
                return;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for(int i = 0; i < value.Count; ++i)
            {
                var item = value[i];
                item.Draw();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    abstract class RepeatedBaseProperty<T>: BaseProperty where T : BaseProperty
    {
        public List<T> value;
        Type targetType;

        public override void Draw()
        {
            if (value == null)
                return;

            using (var v = new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(string.Format("List[{0}] {1}", typeName, name));

                using (var t = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(23);
                    using (var vv = new EditorGUILayout.VerticalScope())
                    {
                        for (int i = 0; i < value.Count; ++i)
                        {
                            var item = value[i];
                            item.Draw();
                        }

                        using (var tt = new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Add Item"))
                            {
                                AddItem();
                            }
                            if (GUILayout.Button("Delete Item"))
                            {
                                DeleteItem();
                            }
                        }
                    }
                }   
            }                
        }

        public override void Transfer(object targetValue)
        {
            var m = targetValue.GetType().GetMethod("Add");
            
            for (int i = 0; i < value.Count; ++i)
            {
                var item = value[i];
                var convValue = Convert.ChangeType(item.GetValue(), targetType);
                m.Invoke(targetValue, new object[] { convValue });
            }
        }

        public override void Build(Type t)
        {
            value = new List<T>();
            targetType = t;            
        }

        public virtual void AddItem()
        {
            T item = Activator.CreateInstance<T>();
            item.name = string.Format("{0}", value.Count);
            item.typeName = typeName;
            value.Add(item);
        }

        public virtual void DeleteItem()
        {
            if(value != null && value.Count > 0)
            {
                value.RemoveAt(value.Count - 1);
            }            
        }
    }

    class RepeatedIntProperty: RepeatedBaseProperty<IntProperty>
    {
    }

    class RepeatedFloatProperty: RepeatedBaseProperty<FloatProperty>
    {
    }

    class RepeatedStringProperty : RepeatedBaseProperty<StringProperty>
    {
    }

    class RepeatedNestedProperty: RepeatedBaseProperty<NestedProperty>
    {
    }

    class MessageData
    {
        BaseProperty root;
        Type type;

        public MessageData(Type type)
        {
            this.type = type;
            Build();
        }

        private void Build()
        {
            root = new NestedProperty();
            root.Build(type);
        }

        public void Transfer(object targetValue)
        {
            if (root == null)
                return;
            root.Transfer(targetValue);
        }

        public void Draw()
        {
            if(root == null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Build Message Data First");
                EditorGUILayout.EndVertical();
            }
            else
            {
                root.Draw();
            }
        }

        static Dictionary<Type, string> typeNameDic = new Dictionary<Type, string>()
        {
            { typeof(UInt32), "uint32" },
            { typeof(UInt64), "uint64" },
            { typeof(Int32), "int32" },
            { typeof(Int64), "uint64" },
            { typeof(Single), "float" },
            { typeof(String), "string" },
        };

        public static string GetTypeName(Type t)
        {
            var typeName = string.Empty;
            if(typeNameDic.TryGetValue(t, out typeName))
            {
                return typeName;
            }
            return t.Name;
        }
    }
}
