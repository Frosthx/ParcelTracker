using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Net;
using System;
using System.IO;
using System.Json;
using System.Linq;
using System.Collections.Generic;
using ZXing.Mobile;
using Android.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;     //引用的库

namespace ParcelTracker
{
    [Activity(Label = "ParcelTracker", MainLauncher = true, Icon = "@drawable/icon")]   //设置activity标签等属性
    public class MainActivity : Activity                //主activity继承自activity
    {
        private List<KeyValuePair<string, string>> companies;   //定义快递公司代码键值对
        private int companySelected = 0;    //已选择的spinner公司编号

        protected override void OnCreate(Bundle savedInstanceState)     //oncreate函数
        {
            base.OnCreate(savedInstanceState);      //创建方法

            SetContentView(Resource.Layout.Main);   //设置内容界面

            EditText parcelNumberText = FindViewById<EditText>(Resource.Id.ParcelNumberText);   //定位快递单号控件
            Button trackButton = FindViewById<Button>(Resource.Id.TrackButton);                 //定位追踪按钮控件
            Button scanButton = FindViewById<Button>(Resource.Id.ScanButton);                   //定位扫描按钮控件
            Button historyButton = FindViewById<Button>(Resource.Id.HistoryButton);             //定位历史按钮控件
            Spinner companySpinner = FindViewById<Spinner>(Resource.Id.CompanySpinner);         //定位spinner控件

            companySpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(companySpinner_ItemSelected);    //spinner选择事件
            
            companies = new List<KeyValuePair<string, string>>      //对快递公司代码键值对赋值
            {
                new KeyValuePair<string, string>("顺丰", "shunfeng"),
                new KeyValuePair<string, string>("中通速递", "zhongtong"),
                new KeyValuePair<string, string>("申通", "shentong"),
                new KeyValuePair<string, string>("圆通速递", "yuantong"),
                new KeyValuePair<string, string>("韵达快递" , "yunda"),
                new KeyValuePair<string, string>("天天快递", "tiantian"),
                new KeyValuePair<string, string>("百世汇通", "huitongkuaidi"),
                new KeyValuePair<string, string>("德邦物流", "debangwuliu"),
                new KeyValuePair<string, string>("EMS", "ems"),
                new KeyValuePair<string, string>("Fedex中国", "lianbangkuaidi"),
                new KeyValuePair<string, string>("快捷速递", "kuaijiesudi"),
                new KeyValuePair<string, string>("全一快递", "quanyikuaidi"),
                new KeyValuePair<string, string>("全峰快递" , "quanfengkuaidi"),
                new KeyValuePair<string, string>("优速物流", "yousuwuliu"),
                new KeyValuePair<string, string>("UPS", "ups"),
                new KeyValuePair<string, string>("宅急送", "zhaijisong") 
            };

            List<string> companyNames = new List<string>();         //定义快递公司名List
            foreach (var item in companies)                         //赋值
                companyNames.Add(item.Key);

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, companyNames);  //定义Spinner的ArrayAdapter

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);     //设置adapter的下拉界面方法
            companySpinner.Adapter = adapter;                                                       //修改spinner的adapter方法

            if (Stock.IsParcelExist())          //检查是否有快递存在
            {
                CheckUpdate();                  //若存在，检查快递物流信息更新
            }

            scanButton.Click += (sender, e) =>      //扫描按钮点击
            {
                StartActivityForResult(typeof(ScanActivity),1);     //开始扫描条形码activity
            };

            historyButton.Click += (sender, e) =>   //记录按钮点击
            {
                Intent intent = new Intent(this, typeof(HistoryActivity));      //定义意向
                StartActivity(intent);                                          //开始记录activity
            };

            trackButton.Click += (sender, e) =>     //追踪按钮点击
            {
                if (string.IsNullOrEmpty(parcelNumberText.Text))                //检查快递单号是否为空
                    Toast.MakeText(this,"Please enter the Parcel Number",ToastLength.Long).Show();  //若为空，弹出提示
                else
                {
                    Intent intent = new Intent(this, typeof(ParcelDetailActivity));         //定义意向
                    intent.PutExtra("parcelNumber", parcelNumberText.Text.Trim());          //传递快递单号
                    intent.PutExtra("parcelCompany", companies[companySelected].Value);     //传递快递公司代码
                    StartActivity(intent);                                                  //开始追踪activity
                }
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)   //activity返回
        {
            if(requestCode==1)
            {
                if (resultCode == Result.Ok)
                {
                    FindViewById<EditText>(Resource.Id.ParcelNumberText).Text = data.GetStringExtra("parcelNum");   //修改为扫描获取的快递单号
                }
            }
        }

        protected override void OnStart()           //activity start时执行
        {
            //base.OnRestart();
            base.OnStart();                         //activity start
            if (!Stock.IsParcelExist())             //检查快递是否存在
                FindViewById<Button>(Resource.Id.HistoryButton).Enabled = false;    //disable掉记录按钮
            else
                FindViewById<Button>(Resource.Id.HistoryButton).Enabled = true;     //enable记录按钮
        }

        private void companySpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)    //spinnerItem选择
        {
            companySelected = e.Position;       //修改选择的快递公司编号
            //Spinner spinner = (Spinner)sender;
            //string toast = string.Format("The company you choose is {0}, code {1}.",
            //    spinner.GetItemAtPosition(e.Position), companies[e.Position].Value);
            //Toast.MakeText(this, toast, ToastLength.Long).Show();
        }

        public async void CheckUpdate()     //检查快递更新（异步）
        {
            string[] nums = Stock.QueryNum().Split('&');    //从数据库获取快递单号
            string[] coms = Stock.QueryCom().Split('&');    //从数据库获取快递公司
            for (int i = 0; i < nums.Length; i++)
            {
                string url = string.Format("http://api.kuaidi100.com/api?id=29833628d495d7a5&com={1}&nu={0}", nums[i], coms[i]);    //快递api
                try                                                                         //try/catch
                {
                    var httpReq = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));      //定义httpwebrequest
                    var httpRes = (HttpWebResponse)await httpReq.GetResponseAsync();        //获取httpwebresponse（异步）
                    if (httpRes.StatusCode == HttpStatusCode.OK)                            //若状态码ok
                    {
                        var text = (JsonObject)JsonObject.Load(httpRes.GetResponseStream());    //将response流解析为json对象

                        var data = (from item in (JsonArray)text["data"]                        //获取json对象里的data
                                    select item.ToString()).ToArray();

                        string[] result = new string[data.Length];                              //获取data中的物流信息和相应时间
                        for (int j = 0; j < data.Length; j++)
                        {
                            string jsonText = data[i];
                            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
                            result[j] = jo["context"].ToString() + "\r\n" + jo["time"].ToString();
                        }

                        Stock.Insert(nums[i], coms[i], string.Join("&", result));               //数据库更新

                        var state = (int)text["state"];     //获取快递信息中的状态码
                        if (state == 3)                     //若正在派送
                        {
                            // Set up an intent so that tapping the notifications returns to this app:
                            Intent intent = new Intent(this, typeof(ParcelDetailActivity));     //定义意向

                            intent.PutExtra("parcelNumber", nums[i]);                           //传递快递单号
                            intent.PutExtra("parcelCompany", coms[i]);                          //传递快递公司

                            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
                            int pendingIntentId = i;                                            //pendingIntendId
                            PendingIntent pendingIntent =                                       //设置PendingIntent
                                PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.OneShot);

                            // Instantiate the builder and set notification elements:
                            Notification.Builder builder = new Notification.Builder(this)       //定义notification
                                .SetContentIntent(pendingIntent)                                //pendingIntent
                                .SetContentTitle("Delivery Notification")                       //Title
                                .SetAutoCancel(true)                                            //点击后自动cancel
                                .SetContentText(string.Format("Your parcel {0} is in delivery.", nums[i]))  //notification内容
                                .SetSmallIcon(Resource.Drawable.notification_icon_background);  //Icon

                            // Build the notification:
                            Notification notification = builder.Build();                        //创建notification

                            // Get the notification manager:
                            NotificationManager notificationManager =                           //设置NotificationManager
                                GetSystemService(Context.NotificationService) as NotificationManager;

                            // Publish the notification:
                            int notificationId = i;                                             //notificationId
                            notificationManager.Notify(notificationId, notification);
                        }
                    }
                }
                catch
                {

                }
            }
        }

    }
}

