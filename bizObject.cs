﻿using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CPUFramework
{
    public class bizObject<T> : INotifyPropertyChanged where T : bizObject<T>, new()
    {
        string _typename = ""; string _tablename = ""; string _getsproc = ""; string _updatesproc = ""; string _deletesproc = "";
        string _primarykeyname = ""; string _primarykeyparamname = "";
        DataTable _datatable = new();
        List<PropertyInfo> _properties = new();

        public event PropertyChangedEventHandler? PropertyChanged;


        public bizObject()
        {
            Type t = this.GetType();
            _typename = t.Name;
            _tablename = _typename;
            if (_tablename.ToLower().StartsWith("biz")) { _tablename = _tablename.Substring(3); }
            _getsproc = _tablename + "Get";
            _updatesproc = _tablename + "Update";
            _deletesproc = _tablename + "Delete";
            _primarykeyname = _tablename + "Id";
            _primarykeyparamname = "@" + _primarykeyname;
            _properties = t.GetProperties().ToList<PropertyInfo>();

        }
        public DataTable Load(int primarykeyvalue)
        {
            DataTable dt = new();
            SqlCommand cmd = SQLUtility.GetSqlCommand(_getsproc);
            SQLUtility.SetParamValue(cmd, _primarykeyparamname, primarykeyvalue);
            dt = SQLUtility.GetDataTable(cmd);
            if (dt.Rows.Count > 0)
            {
                LoadProps(dt.Rows[0]);
            }

            _datatable = dt;
            return dt;
        }

        protected List<T> GetListFromDataTable(DataTable dt)
        {
            List<T> lst = new();
            foreach (DataRow dr in dt.Rows)
            {
                T obj = new T();
                obj.LoadProps(dr);
                lst.Add(obj);
            }
            return lst;
        }
        public List<T> GetList(bool includeblank = false)
        {
            List<T> lst = new();

            SqlCommand cmd = SQLUtility.GetSqlCommand(_getsproc);

            if (cmd.Parameters.Contains("@All"))
            {
                SQLUtility.SetParamValue(cmd, "@All", 1);
            }


            if (cmd.Parameters.Contains("@IncludeBlank"))
            {
                SQLUtility.SetParamValue(cmd, "@IncludeBlank", includeblank);
            }

            var dt = SQLUtility.GetDataTable(cmd);

            return GetListFromDataTable(dt);
        }

        private void LoadProps(DataRow dr)
        {
            foreach (DataColumn col in dr.Table.Columns)
            {
                var prop = GetProp(col.ColumnName, false, true);
                SetProp(col.ColumnName, dr[col.ColumnName]);
            }
        }

        public void Delete(int id)
        {
            SqlCommand cmd = SQLUtility.GetSqlCommand(_deletesproc);
            SQLUtility.SetParamValue(cmd, _primarykeyparamname, id);
            SQLUtility.ExecuteSQL(cmd);
        }
        public void Delete(DataTable datatable)
        {
            int id = (int)datatable.Rows[0][_primarykeyname];
            this.Delete(id);

        }

        public void Delete()
        {
            PropertyInfo? prop = GetProp(_primarykeyname, true, false);
            if (prop != null)
            {
                object? id = prop.GetValue(this);

                if (id != null)
                {
                    this.Delete((int)id);
                }
            }
        }

        public void Save()
        {
            SqlCommand cmd = SQLUtility.GetSqlCommand(_updatesproc);
            foreach (SqlParameter param in cmd.Parameters)
            {
                var prop = GetProp(param.ParameterName, true, false);
                if (prop != null)
                {
                    object? val = prop.GetValue(this);
                    if (val == null) { val = DBNull.Value; }
                    param.Value = val;
                }
            }
            SQLUtility.ExecuteSQL(cmd);
            foreach (SqlParameter param in cmd.Parameters)
            {
                if (param.Direction == ParameterDirection.InputOutput)
                {
                    SetProp(param.ParameterName, param.Value);
                }
            }
        }

        public void Save(DataTable datatable)
        {
            if (datatable.Rows.Count == 0)
            {
                throw new Exception($"Cannot call {_tablename} method because there are no rows in the table");
            }

            DataRow r = datatable.Rows[0];
            SQLUtility.SaveDataRow(r, _updatesproc);

        }

        private void SetProp(string propname, object? value)
        {
            var prop = GetProp(propname, false, true);
            if (prop != null)
            {
                if (value == DBNull.Value) { value = null; }
                try
                {
                    prop.SetValue(this, value);
                }
                catch (Exception ex)
                {
                    string msg = $"{_typename}.{prop.Name} is being set to {value?.ToString()} and that is wrong data type. {ex.Message}";
                    throw new CPUDevException(msg, ex);
                }
            }
        }
        private PropertyInfo? GetProp(string propname, bool forread, bool forwrite)
        {
            propname = propname.ToLower();
            if (propname.StartsWith("@")) { propname = propname.Substring(1); }
            PropertyInfo? prop = _properties.FirstOrDefault(p =>
            p.Name.ToLower() == propname
            && (forread == false || p.CanRead == true)
            && (forwrite == false || p.CanWrite == true)
            );
            return prop;
        }

        protected string GetSprocName { get => _getsproc; }
        protected void InvokePropertyChanged([CallerMemberName] string propertyname = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
