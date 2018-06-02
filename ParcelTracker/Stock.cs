using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SQLite;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ParcelTracker
{
    [Table("Parcel")]
    public class Stock
    {
        [PrimaryKey, Unique, MaxLength(30)]             //设置为主键，唯一性，限制最大长度
        public string ParcelNum { get; set; }           //定义快递单号字符串
        [MaxLength(20)]                                 //限制最大长度
        public string ParcelCompany { get; set; }       //定义快递公司字符串
        [MaxLength(1000)]                               //限制最大长度
        public string Detail { get; set; }              //定义物流信息字符串

        public static bool IsParcelExist()              //查询是否存在快递
        {
            string dbPath = Path.Combine(               //数据库文件路径
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "database.db3");
            var db = new SQLiteConnection(dbPath);      //定义数据库链接
            db.CreateTable<Stock>();                    //若不存在Table，创建
            if (db.Table<Stock>().Count() == 0)         //若Table中数据量为0
                return false;
            else
                return true;
        }

        public static void Insert(string parcelNum, string parcelCompany, string parcelDetail)      //插入数据
        {
            string dbPath = Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "database.db3");
            var db = new SQLiteConnection(dbPath);
            if (db.Find<Stock>(parcelNum) != null)          //更新数据库
                db.Update(db.Get<Stock>(parcelNum));
            else
                db.Insert(new Stock                         //新增数据
                {
                    ParcelNum = parcelNum,
                    ParcelCompany = parcelCompany,
                    Detail = parcelDetail
                });
        }

        public static string Query(string parcelNum)        //查询数据
        {
            string dbPath = Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "database.db3");
            var db = new SQLiteConnection(dbPath);
            try
            {
                return db.Get<Stock>(parcelNum).Detail;     //返回快递物流信息
            }
            catch
            {
                return null;
            }
        }

        public static string QueryNum()                     //查询快递单号
        {
            string dbPath = Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "database.db3");
            var db = new SQLiteConnection(dbPath);
            var data = db.Table<Stock>();
            string res = null;
            foreach (var d in data)                         //返回为字符串
                res += d.ParcelNum + "&";
            res = res.Substring(0, res.Length - 1);
            return res;
        }

        public static string QueryCom()                     //查询快递公司
        {
            string dbPath = Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "database.db3");
            var db = new SQLiteConnection(dbPath);
            var data = db.Table<Stock>();
            string res = null;
            foreach (var d in data)                         //返回为字符串
                res += d.ParcelCompany + "&";
            res = res.Substring(0, res.Length - 1);
            return res;
        }

        public static void Delete(string parcelNum)         //删除数据
        {
            string dbPath = Path.Combine(
                 System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                 "database.db3");
            var db = new SQLiteConnection(dbPath);
            db.Delete<Stock>(parcelNum);                    //删除主键为该参数的数据
        }
    }
}