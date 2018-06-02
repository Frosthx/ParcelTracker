using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ParcelTracker
{
    [Activity(Label = "History")]
    public class HistoryActivity : ListActivity
    {
        protected string[] nums = null;     //定义快递单号数组
        protected string[] coms = null;     //定义快递公司数组

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, nums);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (Stock.IsParcelExist())      //查询是否存在快递
            {
                nums = Stock.QueryNum().Split('&');     //解析快递单号及公司
                Array.Reverse(nums);
                coms = Stock.QueryCom().Split('&');
                Array.Reverse(coms);
                ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, nums);    //用ListAdapter显示存在的快递
            }
            else
                Finish();                   //activity结束
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)      //ListItem点击事件
        {
            //base.OnListItemClick(l, v, position, id);
            string parcelNum = nums[position];                                  //获取点击的快递单号
            string parcelCom = coms[position];                                  //获取点击的快递公司
            Intent intent = new Intent(this, typeof(ParcelDetailActivity));     //定义意向
            intent.PutExtra("parcelNumber", parcelNum);                         //传递快递单号
            intent.PutExtra("parcelCompany", parcelCom);                        //传递快递公司
            StartActivity(intent);                                              //开始activity
        }
    }
}