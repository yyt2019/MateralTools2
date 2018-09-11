﻿using MateralTools.Base;
using MateralTools.MConvert;
using MateralTools.MVerify;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MateralTools.Base.Manager;

namespace MateralTools.MEntityFramework
{
    /// <summary>
    /// 业务处理类父类
    /// </summary>
    /// <typeparam name="TDAL">对应的数据操作类</typeparam>
    /// <typeparam name="TModel">对应的数据模型</typeparam>
    /// <typeparam name="VModel">对应的视图模型</typeparam>
    public abstract class EFBaseBLL<TDAL, TModel, VModel> : EFBaseBLL<TDAL, TModel>
    {
        /// <summary>
        /// 根据唯一标识获得视图信息
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <returns></returns>
        /// <exception cref="MEntityFrameworkException"></exception>
        public virtual VModel GetDBModelViewInfoByID(object id)
        {
            MethodInfo method = (typeof(TDAL)).GetMethod("GetDBModelViewInfoByID");
            if (method != null)
            {
                return (VModel)method.Invoke(_dal, new object[] { id });
            }
            else
            {
                throw new MEntityFrameworkException("未实现该方法，需重写");
            }
        }
    }
    /// <summary>
    /// 业务处理类父类
    /// </summary>
    /// <typeparam name="TDAL">对应的数据操作类</typeparam>
    /// <typeparam name="TModel">对应的数据模型</typeparam>
    public abstract class EFBaseBLL<TDAL, TModel>
    {
        /// <summary>
        /// 不修改列表
        /// </summary>
        public List<string> NotUpdateList = new List<string>();
        /// <summary>
        /// 构造方法
        /// </summary>
        public EFBaseBLL()
        {
            PropertyInfo logicDeletePi = GetLogicDeletePropertyInfo();
            if (logicDeletePi != null)
            {
                NotUpdateList.Add(logicDeletePi.Name);
            }
            PropertyInfo pi = EFBaseDAL.GetKeyPropertyInfo<TModel>();
            if (pi != null)
            {
                NotUpdateList.Add(pi.Name);
            }
            _dal = _dal.MGetDefultObject<TDAL>();
        }
        /// <summary>
        /// 数据操作对象
        /// </summary>
        public readonly TDAL _dal;
        /// <summary>
        /// 验证模型
        /// </summary>
        /// <param name="model">要验证的模型</param>
        /// <param name="msg">提示信息</param>
        /// <returns>验证结果</returns>
        protected virtual bool Verification(TModel model, out string msg)
        {
            List<string> msgs = new List<string>();
            Type tType = typeof(TModel);
            PropertyInfo[] pis = tType.GetProperties();
            string Description;
            foreach (PropertyInfo pi in pis)
            {
                object attr = pi.GetCustomAttribute(typeof(RequiredAttribute), false);
                if (attr != null)
                {
                    if (pi.GetValue(model).MIsNullOrEmptyStr())
                    {
                        try
                        {
                            Description = pi.MGetDescription();
                        }
                        catch
                        {
                            Description = pi.Name;
                        }
                        msgs.Add($"{Description}不能为空");
                    }
                }
            }
            return GetVeificationOutMsg(out msg, msgs);
        }
        /// <summary>
        /// 获得验证返回信息
        /// </summary>
        /// <param name="msg">返回信息</param>
        /// <param name="msgs">信息列表</param>
        /// <returns>验证结果</returns>
        public static bool GetVeificationOutMsg(out string msg, List<string> msgs)
        {
            if (msgs.Count == 0)
            {
                msg = "验证通过。";
                return true;
            }
            else
            {
                msg = "验证未通过：" + string.Join(",", msgs) + "。";
                return false;
            }
        }

        /// <summary>
        /// 验证添加模型
        /// </summary>
        /// <param name="model">要验证的模型</param>
        /// <param name="msg">提示信息</param>
        /// <returns>验证结果</returns>
        protected virtual bool VerificationAdd(TModel model, out string msg)
        {
            return Verification(model, out msg);
        }
        /// <summary>
        /// 验证修改模型
        /// </summary>
        /// <param name="model">要验证的模型</param>
        /// <param name="msg">提示信息</param>
        /// <returns>验证结果</returns>
        protected virtual bool VerificationUpdate(TModel model, out string msg)
        {
            return Verification(model, out msg);
        }
        /// <summary>
        /// 根据唯一标识获得信息
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <returns></returns>
        /// <exception cref="MEntityFrameworkException"></exception>
        public virtual TModel GetDBModelInfoByID(object id)
        {
            MethodInfo method = (typeof(TDAL)).GetMethod("GetDBModelInfoByID");
            if (method != null)
            {
                return (TModel)method.Invoke(_dal, new object[] { id });
            }
            else
            {
                throw new MEntityFrameworkException("未实现该方法，需重写");
            }
        }
        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <param name="model">要添加的对象</param>
        /// <exception cref="MEntityFrameworkException"></exception>
        public virtual TModel Add(TModel model)
        {
            MethodInfo method = (typeof(TDAL)).GetMethod("Insert");
            if (method != null)
            {
                if (VerificationAdd(model, out string msg))
                {
                    model = (TModel)method.Invoke(_dal, new object[] { model });
                    return model;
                }
                else
                {
                    throw new MEntityFrameworkException(msg);
                }
            }
            else
            {
                throw new MEntityFrameworkException("未实现该方法，需重写");
            }
        }
        /// <summary>
        /// 删除一个对象
        /// </summary>
        /// <param name="id">对象ID</param>
        /// <exception cref="MEntityFrameworkException"></exception>
        public virtual void Delete(object id)
        {
            Type TType = typeof(TModel);
            PropertyInfo pi = GetLogicDeletePropertyInfo();
            if (pi == null)
            {
                MethodInfo method = (typeof(TDAL)).GetMethod("Delete");
                if (method != null)
                {
                    method.Invoke(_dal, new object[] { id });
                }
                else
                {
                    throw new MEntityFrameworkException("未实现该方法，需重写");
                }
            }
            else
            {
                TModel DBModel = GetDBModelInfoByID(id);
                pi.SetValue(DBModel, true);
                MethodInfo method = (typeof(TDAL)).GetMethod("SaveChange");
                if (method != null)
                {
                    method.Invoke(_dal, new object[] { });
                }
                else
                {
                    throw new MEntityFrameworkException("未实现保存方法，需重写");
                }
            }
        }
        /// <summary>
        /// 修改一个对象
        /// </summary>
        /// <param name="model">要修改的对象</param>
        /// <exception cref="MEntityFrameworkException"></exception>
        public virtual TModel Update(TModel model)
        {
            Type TType = model.GetType();
            PropertyInfo[] pis = TType.GetProperties();
            PropertyInfo pi = EFBaseDAL.GetKeyPropertyInfo<TModel>();
            TModel DBModel = GetDBModelInfoByID(pi.GetValue(model));
            if (DBModel != null)
            {
                foreach (PropertyInfo item in pis)
                {
                    if (!NotUpdateList.Contains(item.Name))
                    {
                        item.SetValue(DBModel, item.GetValue(model));
                    }
                }
                if (VerificationUpdate(DBModel, out string msg))
                {
                    MethodInfo method = (typeof(TDAL)).GetMethod("SaveChange");
                    if (method != null)
                    {
                        method.Invoke(_dal, new object[] { });
                        return DBModel;
                    }
                    else
                    {
                        throw new MEntityFrameworkException("未实现保存方法，需重写");
                    }
                }
                else
                {
                    throw new MEntityFrameworkException(msg);
                }
            }
            else
            {
                throw new MEntityFrameworkException("修改失败，该对象不存在于数据库中");
            }
        }
        /// <summary>
        /// 获得逻辑删除属性
        /// </summary>
        /// <typeparam name="TM">类型</typeparam>
        /// <returns>类型</returns>
        public PropertyInfo GetLogicDeletePropertyInfo()
        {
            Type tType = typeof(TModel);
            PropertyInfo[] pis = tType.GetProperties();
            LogicDeleteAttribute ka = null;
            PropertyInfo pi = null;
            foreach (PropertyInfo item in pis)
            {
                ka = item.GetCustomAttribute<LogicDeleteAttribute>();
                if (ka != null)
                {
                    pi = item;
                    break;
                }
            }
            return pi;
        }
    }
}
