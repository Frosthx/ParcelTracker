using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net;
using System.Json;
using Android.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Views;            //引用的库

namespace ParcelTracker
{
    [Activity(Label = "ParcelDetail")]
    public class ParcelDetailActivity : ListActivity            //activity继承自ListActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            string strData = Stock.Query(Intent.GetStringExtra("parcelNumber"));    //查询快递单号是否已在数据库中
            if (!string.IsNullOrEmpty(strData))
            {
                string[] data = strData.Split('&');
                ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, data);    //用ListAdapter显示物流信息
            }

            string url = string.Format("http://api.kuaidi100.com/api?id=29833628d495d7a5&com={1}&nu={0}", Intent.GetStringExtra("parcelNumber"), Intent.GetStringExtra("parcelCompany"));   //快递api
            GetData(url);       //函数调用
        }

        public async void GetData(string url)
        {
            try   //try/catch
            {
                var httpReq = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));  //定义httpwebrequest
                var httpRes = (HttpWebResponse)await httpReq.GetResponseAsync();    //获取httpwebresponse（异步）
                if (httpRes.StatusCode == HttpStatusCode.OK)                        //若状态码ok
                {
                    var text = (JsonObject)JsonObject.Load(httpRes.GetResponseStream());    //将response流解析为json对象

                    //var result = ConvertData(text);

                    var data = (from item in (JsonArray)text["data"]            //获取json对象里的data
                                select item.ToString()).ToArray();

                    string[] result = new string[data.Length];                  //获取data中的物流信息和相应时间
                    for (int i = 0; i < data.Length; i++)
                    {
                        string jsonText = data[i];
                        JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
                        result[i] = jo["context"].ToString() + "\r\n" + jo["time"].ToString();
                    }

                    Stock.Insert(Intent.GetStringExtra("parcelNumber"), Intent.GetStringExtra("parcelCompany"), string.Join("&",result));   //更新数据库

                    ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, result);      //用ListAdapter显示物流信息
                }
            }
            catch
            {
                //Toast.MakeText(this, e.Message, ToastLength.Long).Show();
                Toast.MakeText(this, "Please check the Parcel Number and Company or try again later!", ToastLength.Long).Show();    //若发生错误，弹出提示
            }
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)      //ListItem点击事件
        {
            //base.OnListItemClick(l, v, position, id);
            AlertDialog.Builder builder = new AlertDialog.Builder(this);                        //创建选择对话框
            AlertDialog alertDialog = builder.Create();                                         
            alertDialog.SetTitle("Warning");                                                    //Title
            //alertDialog.SetIcon(Resource.Drawable.Icon);
            alertDialog.SetMessage("Are you going to delete this parcel record?");              //Message内容
            alertDialog.SetButton("Yes", (sender, args) =>                                      //按钮
            {
                Stock.Delete(Intent.GetStringExtra("parcelNumber"));                            //删除记录
                Finish();                                                                       //activity结束
            });
            alertDialog.SetButton2("No", (sender, args) =>                                      //按钮2
            {
                //做取消的事  
            });
            alertDialog.Show();                                                                 //显示选择对话框
        }

        //public static void CheckUpdate()
        //{
        //    string[] nums = Stock.QueryNum().Split('&');
        //    string[] coms = Stock.QueryCom().Split('&');
        //    for(int i=0;i<nums.Length;i++)
        //    {
        //        string url = string.Format("http://api.kuaidi100.com/api?id=29833628d495d7a5&com={1}&nu={0}", nums[i], coms[i]);
        //        try
        //        {
        //            var httpReq = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
        //            var httpRes = (HttpWebResponse)httpReq.GetResponse();
        //            if (httpRes.StatusCode == HttpStatusCode.OK)
        //            {
        //                var text = (JsonObject)JsonObject.Load(httpRes.GetResponseStream());

        //                var state = (int)text["state"];
        //                if(state == 3)
        //                {
        //                    // Instantiate the builder and set notification elements:
        //                    Notification.Builder builder = new Notification.Builder(this)
        //                        .SetContentTitle("Sample Notification")
        //                        .SetContentText("Hello World! This is my first notification!")
        //                        .SetSmallIcon(Resource.Drawable.notification_icon_background);

        //                    // Build the notification:
        //                    Notification notification = builder.Build();

        //                    // Get the notification manager:
        //                    NotificationManager notificationManager =
        //                        GetSystemService(Context.NotificationService) as NotificationManager;

        //                    // Publish the notification:
        //                    const int notificationId = 0;
        //                    notificationManager.Notify(notificationId, notification);
        //                }

        //                var data = (from item in (JsonArray)text["data"]
        //                            select item.ToString()).ToArray();

        //                string[] result = new string[data.Length];
        //                for (int j = 0; j < data.Length; j++)
        //                {
        //                    string jsonText = data[i];
        //                    JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
        //                    result[j] = jo["context"].ToString() + "\r\n" + jo["time"].ToString();
        //                }

        //                Stock.Insert(nums[i], coms[i], string.Join("&", result));
        //            }
        //        }
        //        catch
        //        {

        //        }
        //    }   
        //}

        //public static string[] ConvertData(JsonObject data)
        //{
        //    var Data = (from item in (JsonArray)data["data"]
        //                select item.ToString()).ToArray();

        //    string[] result = new string[Data.Length];
        //    for (int i = 0; i < Data.Length; i++)
        //    {
        //        string jsonText = Data[i];
        //        JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
        //        result[i] = jo["context"].ToString() + "\r\n" + jo["time"].ToString();
        //    }
        //    return result;
        //}
    }
}